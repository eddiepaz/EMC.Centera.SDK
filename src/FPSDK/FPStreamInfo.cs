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

using System.Runtime.InteropServices;

namespace EMC.Centera.SDK
{
  [StructLayout( LayoutKind.Sequential )]
  public unsafe struct FPStreamInfo
  {

    public short mVersion;         // current version of FPStreamInfo
    public void* mUserData;        // application-specific data, untouched by Generic Streams

    public long mStreamPos;       // current position
    public long mMarkerPos;       // position of marker
    public long mStreamLen;       // length of stream, if known, else -1

    public byte mAtEOF;           // have we reached the end of the stream?
    public byte mReadFlag;        // indicator for the direction of the transfer

    public void* mBuffer;          // databuffer supplied by application
    public long mTransferLen;     // number of bytes actually transferred
  }
}