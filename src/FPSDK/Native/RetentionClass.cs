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

using EMC.Centera.SDK.FPTypes;

namespace EMC.Centera.SDK.Native
{
    public class RetentionClass
    {

        public static void GetName(FPRetentionClassRef inClassRef, ref byte[] outName, ref FPInt ioNameLen)
        {
            SDK.FPRetentionClass_GetName8(inClassRef, outName, ref ioNameLen);
            SDK.CheckAndThrowError();
        }
        public static FPLong GetPeriod(FPRetentionClassRef inClassRef)
        {
            var retval = SDK.FPRetentionClass_GetPeriod(inClassRef);
            SDK.CheckAndThrowError();
            return retval;
        }
        public static void Close(FPRetentionClassRef inClassRef)
        {
            SDK.FPRetentionClass_Close(inClassRef);
            SDK.CheckAndThrowError();
        }

    }
}