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

using System.IO;

namespace EMC.Centera.SDK
{
  /// <summary> The FPPartialInputStream reads data from a section of an
  /// underlying stream using offset and size to determine the "section" boundaries.
  /// </summary>
  public class FPPartialInputStream : FPPartialStream
  {
    public FPPartialInputStream( Stream s, long o, long c ) : base( s, o, c ) { }

    public override bool CanRead => true;

    /// <inheritdoc />
    public override int Read( byte[] buffer, int offset, int count )
    {
      int bytesRead;

      lock( this.theStream )
      {
        this.theStream.Seek( this.Position, SeekOrigin.Begin );

        if( (this.Position + count) > this.end )
        {
          count = (int) (this.end - this.Position);
        }

        bytesRead = this.theStream.Read( buffer, offset, count );
        this.Position += bytesRead;
      }

      return bytesRead;
    }
  }
}