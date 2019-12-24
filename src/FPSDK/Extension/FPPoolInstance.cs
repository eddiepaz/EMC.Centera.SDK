﻿/*****************************************************************************

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

using System.Collections;
using System.Text;

namespace EMC.Centera.SDK.Extension
{
    public sealed class FPPoolInstance : FPPool
    {
        private static readonly Hashtable connectionString2PoolConnection = Hashtable.Synchronized(new Hashtable());

        public static IFPPool Get(string connectionString)
        {
            IFPPool myPool;
            if (FPPoolInstance.connectionString2PoolConnection.Contains(connectionString))
            {
                myPool = (IFPPool)FPPoolInstance.connectionString2PoolConnection[connectionString];
            }
            else
            {
                myPool = new FPPoolInstance(connectionString);

                FPPoolInstance.connectionString2PoolConnection.Add(connectionString, myPool);
            }
            return myPool;
        }

        public static IFPPool Get(byte[] theBytes)
        {
            var connectionString = Encoding.Unicode.GetString(theBytes);

            return FPPoolInstance.Get(connectionString);
        }

        private FPPoolInstance(string poolConnectionString) : base(poolConnectionString)
        {
        }

        ~FPPoolInstance()
        {
          this.Dispose(false);
        }
    }
}
