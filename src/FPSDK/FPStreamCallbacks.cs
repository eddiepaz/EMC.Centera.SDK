/*****************************************************************************

Copyright � 2006 EMC Corporation. All Rights Reserved
 
This file is part of .NET wrapper for the Centera SDK.

.NET wrapper is free software; you can redistribute it and/or modify it under
the terms of the GNU General Public License as published by the Free Software
Foundation version 2.

In addition to the permissions granted in the GNU General Public License
version 2, EMC Corporation gives you unlimited permission to link the compiled
version of this file into combinations with other programs, and to distribute
those combinations without any restriction coming from the use of this file.
(The General Public License restrictions do apply in other respects; for
example, they cover modification of the file, and distribution when not linked
into a combined executable.)

.NET wrapper is distributed in the hope that it will be useful, but WITHOUT ANY
WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A
PARTICULAR PURPOSE. See the GNU General Public License version 2 for more
details.

You should have received a copy of the GNU General Public License version 2
along with .NET wrapper; see the file COPYING. If not, write to:

 EMC Corporation 
 Centera Open Source Initiative (COSI) 
 80 South Street
 1/W-1
 Hopkinton, MA 01748 
 USA

******************************************************************************/

using System;
using System.IO;
using System.Runtime.InteropServices;
using EMC.Centera.SDK.FPTypes;

namespace EMC.Centera.SDK
{
  /// <summary>
  ///This class encapsulates the Callback Methods that are used to manipulate a GenericStream, and
  ///data members that are required while doing so. You should derive from this class is you wish to
  ///populate the data buffer or process returned data in a way that does not make use of a Stream
  ///derived object or you wish do to things differently than the default behaviour.
  /// </summary>
  public class FPStreamCallbacks
  {
    protected int bufferSize;
    protected byte[] localBuffer;
    protected internal Stream userStream;

    protected FPStreamRef theStream;

    /// <summary>
    ///Creates an FPStreamCallbacks object.
    ///
    ///@param s     The Stream derived object to use for transferring the data between the application
    ///         and the SDK. This Stream should support Seek in order to allow Marker Support.
    /// </summary>
    public FPStreamCallbacks( Stream s )
    {
      this.BufferSize = 16 * 1024;
      this.userStream = s;
    }

    /// <summary>
    /// Zero parameter base class construtor for derivation purposes.
    /// </summary>
    protected FPStreamCallbacks() => this.userStream = null;

    /// <summary>
    ///Change the buffer size for the transfer of the data between the application and the SDK. As
    ///the SDK always uses 16k, this is the optimum value to use and is the initial default value
    ///is set when the FPStreamCallbacks object is created..
    /// </summary>
    public int BufferSize
    {
      get => this.bufferSize;
      set
      {
        this.bufferSize = value;
        this.localBuffer = new byte[this.bufferSize];
      }
    }

    /// <summary>
    ///The FPStreamRef object representing the GenericStream.
    /// </summary>
    public FPStreamRef StreamRef
    {
      get => this.theStream;
      set => this.theStream = value;
    }

    /// <summary>
    ///This function is only required when writing to the Centera.
    ///Allocate a buffer and populate it with the next chunk of data to be sent.
    ///
    ///@param info    The FPStreamInfo structure containing the data and control information.
    /// </summary>
    public virtual unsafe long PrepareBuffer( ref FPStreamInfo info )
    {
      //Console.WriteLine(this.GetHashCode() + " prepare buffer " + info.ToString());
      // We allocate the buffer if this is the first time through
      if( info.mBuffer == null )
      {
        // Create a heap memory buffer that the stream can use
        info.mBuffer = Marshal.AllocHGlobal( this.bufferSize ).ToPointer();
      }

      var dataSize = this.PopulateBuffer( ref this.localBuffer, this.bufferSize, this.userStream );

      // Copy the localBuffer into the Heap Allocated mBuffer for transfer.
      Marshal.Copy( this.localBuffer, 0, (IntPtr) info.mBuffer, dataSize );

      // Update the stream position and the amount transferred
      info.mStreamPos += dataSize;
      info.mTransferLen = dataSize;

      if( info.mStreamLen == -1 )
      {
        if( dataSize < this.bufferSize )
        {
          //Console.WriteLine("\tStream processed " + dataSize + "/" + bufferSize);
          info.mAtEOF = 1;
        }
      }
      else if( info.mStreamLen <= info.mStreamPos )
      {
        //Console.WriteLine("\tStream processed " + info.mStreamLen + "/" + info.mStreamPos);
        info.mAtEOF = 1;
      }
      else
      {
        //Console.WriteLine("\tStream not exhausted " + info.mStreamLen + "/" + info.mStreamPos);
      }

      return 0;
    }


    /// <summary>
    ///Populates the buffer that transfers data from the application to the SDK.
    ///
    ///@param localBuffer The area of local storage allocated as the transfer buffer.
    ///@param bufferSize  The size of this buffer.
    ///@param userData    User object for whatever purpose they deem fit.
    /// </summary>
    public virtual int PopulateBuffer( ref byte[] localBuffer, int bufferSize, object userData )
    {
      // The localBuffer is populated from the Stream that the user provides.
      var userStream = (Stream) userData;

      return userStream?.Read( localBuffer, 0, bufferSize ) ?? 0;
    }

    /// <summary>
    ///This function is called both for reading and writing to the Centera and it signifies
    ///that the block has been to the SDK (when writing) or received from the SDK (when reading).
    ///
    ///@param info    The FPStreamInfo structure containing the data and control information.
    /// </summary>
    public virtual long BlockTransferred( ref FPStreamInfo info )
    {
      //Console.WriteLine(this.GetHashCode() + " Block transferred OK");
      if( info.mReadFlag == (byte) FPStream.StreamDirection.OutputFromCentera )
      {
        if( info.mTransferLen > 0 )
        {
          if( info.mTransferLen > this.bufferSize )
          {
            this.localBuffer = new byte[info.mTransferLen];
            this.bufferSize = (int) info.mTransferLen;
          }

          unsafe
          {
            Marshal.Copy( (IntPtr) info.mBuffer, this.localBuffer, 0, (int) info.mTransferLen );
          }
        }

        this.ProcessReturnedData( this.localBuffer, (int) info.mTransferLen, this.userStream );
        info.mStreamPos += info.mTransferLen;
      }

      return 0;
    }

    /// <summary>
    ///Extract the data from the buffer returned from the Centera and send it on to the
    ///application.
    ///
    ///@param localBuffer The area of local storage allocated as the transfer buffer.
    ///@param bufferSize  The size of this buffer.
    ///@param userData    User object for whatever purpose they deem fit.
    /// </summary>
    public virtual void ProcessReturnedData( byte[] localBuffer, int bufferSize, object userData )
    {
      //Console.WriteLine(this.GetHashCode() + " Processing returned data");
      var userStream = (Stream) userData;

      if( userStream?.CanWrite == true )
      {
        userStream.Write( localBuffer, 0, bufferSize );
      }
    }

    /// <summary>
    ///Allows the SDK to set a marker at the current stream position. This effectively
    ///means that data up to this point has been successfully transferred to the Centera.
    ///
    ///@param info    The FPStreamInfo structure containing the data and control information.
    /// </summary>
    public virtual long SetMark( ref FPStreamInfo info )
    {
      //Console.WriteLine(this.GetHashCode() + " Mark set at " + info.mStreamPos);
      info.mMarkerPos = info.mStreamPos;

      return 0;
    }

    /// <summary>
    ///Allows the SDK to request that the application re-sends data from the previously set
    ///marker position due to problems in sending that data to the Centera. If using
    ///a "normal" GenericStream that uses a Stream based class for transfer, this will be
    ///achieved using the Seek member, and the derived class must implement this.
    ///
    ///Note: up to 100MB of data could be required to be resent!
    ///
    ///@param info    The FPStreamInfo structure containing the data and control information.
    /// </summary>
    public virtual long ResetMark( ref FPStreamInfo info )
    {
      // The stream the user supplies must override Seek in order for this to work!!
      ////Console.WriteLine(this.GetHashCode() + " Mark reset to " + info.mMarkerPos);

      if( this.userStream == null )
      {
        return -1;
      }

      this.userStream.Seek( info.mMarkerPos, SeekOrigin.Begin );
      info.mStreamPos = info.mMarkerPos;
      return 0;
    }

    /// <summary>
    ///The SDK has signaled that the operation is complete. Allocated buffers should now be freed
    ///up.
    ///
    ///@param info    The FPStreamInfo structure containing the data and control information.
    /// </summary>
    public virtual long TransferComplete( ref FPStreamInfo info )
    {
      // If we are writing to Centera, free the unmanaged buffer that we allocated
      //Console.WriteLine(this.GetHashCode() + " Transfer complete");
      if( info.mReadFlag == (byte) FPStream.StreamDirection.InputToCentera )
      {
        unsafe
        {
          Marshal.FreeHGlobal( (IntPtr) info.mBuffer );
          info.mBuffer = null;
        }
      }
      return 0;
    }
  }
}