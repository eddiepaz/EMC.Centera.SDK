/*****************************************************************************

Copyright © 2006 EMC Corporation. All Rights Reserved
 
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

namespace EMC.Centera.SDK
{
  /// <summary>The FPPartialStream is a convenience classes to utilise a section of an
  /// underlying stream using offset and size to determine the "section" boundaries.
  /// </summary>
  public abstract class FPPartialStream : Stream
  {
    protected Stream theStream;
    protected long start, end, position, length;

    protected FPPartialStream( Stream s, long offset, long size )
    {
      this.start = offset;
      this.position = offset;
      this.length = size;
      this.end = offset + this.length;
      this.theStream = s;
    }

    public override bool CanRead => false;

    public override bool CanWrite => false;

    public override int Read( byte[] buffer, int offset, int count ) => throw new Exception( "The method or operation is not implemented." );

    public override void Write( byte[] buffer, int offset, int count )
    {
      throw new Exception( "The method or operation is not implemented." );
    }

    public override bool CanSeek => throw new Exception( "The method or operation is not implemented." );

    public override long Length => this.length;

    public override long Position
    {
      get => this.position;
      set
      {
        if( (this.start <= value) && (value <= this.end) )
        {
          this.position = value;
        }
        else
        {
          throw new Exception( "Attempt to position to " + value + " - outside range of partial stream ("
                               + this.start + "/" + this.position + "/" + this.end + ")" );
        }
      }
    }

    public override void Flush()
    {
      throw new Exception( "The method or operation is not implemented." );
    }

    public override long Seek( long offset, SeekOrigin origin )
    {
      long newPosition = 0;

      switch( origin )
      {
        case SeekOrigin.Begin:
          newPosition = this.start + offset;
          break;
        case SeekOrigin.Current:
          newPosition = this.Position + offset;
          break;
        case SeekOrigin.End:
          newPosition = this.end - offset;
          break;
      }

      return this.Position = newPosition;
    }

    public override void SetLength( long value )
    {
      throw new Exception( "The method or operation is not implemented." );
    }
  }
}