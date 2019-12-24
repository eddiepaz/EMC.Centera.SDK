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
using EMC.Centera.SDK.FPTypes;

namespace EMC.Centera.SDK
{
  public class FPStream : FPObject
  {
    /// <summary>
    ///StreamType indicator used when creating "special" FPStream objects.
    ///
    ///Members: Null / Stdio.
    /// </summary>
    public enum StreamType { Null, Stdio };

    /// <summary>
    ///The StreamDirection indicator used when creating buffer based FPStream objects.
    ///
    ///Members: InputToCentera / OutputFromCentera.
    /// </summary>
    public enum StreamDirection { OutputFromCentera = 0, InputToCentera = 1 };

    protected FPStreamRef theStream;

    /// <summary>
    ///Create a Stream using a buffer to write to (StreamType.InputToCentera)
    ///or read from (StreamType.OutputFromCentera) the Centera.
    ///See API Guide: FPStream_CreateBufferForInput, FPStream_CreateBufferForOutput
    ///
    ///@param streamBuffer  The buffer that the Stream will read from (for INPUT to the Centera)
    ///             or write to (for OUTPUT from the Centera).
    ///@param bufferSize  The size of the buffer.
    ///@param streamDirection The StreamDirection enum indicating input or output.
    /// </summary>
    public FPStream( IntPtr streamBuffer, int bufferSize, StreamDirection streamDirection )
    {
      this.theStream = streamDirection == StreamDirection.InputToCentera
                         ? Native.Stream.CreateBufferForInput( streamBuffer, bufferSize )
                         : Native.Stream.CreateBufferForOutput( streamBuffer, bufferSize );

      this.AddObject( this.theStream, this );
    }

    /// <summary>
    ///Create a Stream that reads from the Centera and writes the content to a file.
    ///See API Guide: FPStream_CreateFileForOutput
    ///
    ///@param fileName  The name of the file to write to.
    ///@param permissions The permissions to create the file with.
    /// </summary>
    public FPStream( string fileName, string permissions )
    {
      this.theStream = Native.Stream.CreateFileForOutput( fileName, permissions );
      this.AddObject( this.theStream, this );
    }

    /// <summary>
    ///Create a Stream that reads a file and writes the content to Centera.
    ///See API Guide: FPStream_CreateFileForInput
    ///
    ///@param fileName  The name of the file to read from.
    ///@param bufferSize  The size of the buffer to use for writing.
    /// </summary>
    public FPStream( string fileName, long bufferSize )
    {
      this.theStream = Native.Stream.CreateFileForInput( fileName, "rb", bufferSize );
      this.AddObject( this.theStream, this );
    }

    /// <summary>
    ///Create a Stream that reads part of a file and writes the content to Centera.
    ///See API Guide: FPStream_CreatePartialFileForInput
    ///
    ///@param fileName  The name of the file to read from.
    ///@param bufferSize  The size of the buffer to use for writing.
    ///@param offset    The position in the file to start reading from.
    ///@param length    The length of the file segment to read from.
    /// </summary>
    public FPStream( string fileName, long bufferSize, long offset, long length )
    {
      this.theStream = Native.Stream.CreatePartialFileForInput( fileName, "rb", bufferSize, offset, length );
      this.AddObject( this.theStream, this );
    }

    /// <summary>
    ///Create a Stream that reads from Centera and writes the content at an offset in a file.
    ///See API Guide: FPStream_CreatePartialFileForOutput
    ///
    ///@param fileName  The name of the file to read from.
    ///@param   permission  The write mode that the file is opened in.
    ///@param bufferSize  The size of the buffer to use for writing.
    ///@param offset    The position in the file to start writing to.
    ///@param length    The length of the file segment to write to.
    ///@param maxFileSize The maximum size that the output file max grow to.
    /// </summary>
    public FPStream( string fileName, string permission, long bufferSize, long offset, long length, long maxFileSize )
    {
      this.theStream = Native.Stream.CreatePartialFileForOutput( fileName, permission, bufferSize, offset, length, maxFileSize );
      this.AddObject( this.theStream, this );
    }
    /// <summary>
    ///Create a Stream that reads from the Centera and writes the content to stdio or
    ///a null stream.
    ///See API Guide: FPStream_CreateToStdio / FPStream_CreateToNull
    ///
    ///@param streamType  StreamType enum - Stdio or Null.
    /// </summary>
    public FPStream( StreamType streamType )
    {
      this.theStream = streamType == StreamType.Stdio
                         ? Native.Stream.FPStream_CreateToStdio()
                         : Native.Stream.FPStream_CreateToNull();

      this.AddObject( this.theStream, this );
    }

    /// <summary>
    ///Creates a Stream for temporary storage. If the length of the stream is greater than
    ///pMemBuffSize the overflow is flushed to a temporary file.
    ///See API Guide: FPStream_CreateTemporaryFile
    ///
    ///@param pMemBuffSize  The size of the in-memory buffer to use.
    /// </summary>
    public FPStream( long pMemBuffSize )
    {
      this.theStream = Native.Stream.FPStream_CreateTemporaryFile( pMemBuffSize );
      this.AddObject( this.theStream, this );
    }

    /// <summary>
    ///Implicit conversion between a Stream and an FPStreamRef
    ///
    ///@param s The Stream.
    ///@return  The FPStreamRef associated with this Stream.
    /// </summary>
    public static implicit operator FPStreamRef( FPStream s ) => s.theStream;

    /// <summary>
    ///Implicit conversion between an FPStreamRef and a  Stream
    ///
    ///@param streamRef The FPStreamRef.
    ///@return  The new Stream.
    /// </summary>
    public static implicit operator FPStream( FPStreamRef streamRef )
    {
      // Find the relevant Tag object in the hashtable for this FPTagRef
      FPStream streamObject = null;

      if( FPObject.SDKObjects.Contains( streamRef ) )
      {
        streamObject = (FPStream) FPObject.SDKObjects[streamRef];
      }
      else
      {
        throw new FPLibraryException( "FPStreamRef is not associated with an FPStream object", FPMisc.WRONG_REFERENCE_ERR );
      }

      return streamObject;
    }

    /// <summary>
    ///Explicitly close this Stream. See API Guide: FPStream_Close
    /// </summary>
    public override void Close()
    {
      if( this.theStream != 0 )
      {
        this.RemoveObject( this.theStream );
        Native.Stream.Close( this.theStream );
        this.theStream = 0;
      }
    }

    /// <inheritdoc />
    protected FPStream()
    {
    }
  }
}