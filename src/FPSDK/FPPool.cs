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
using System.Text;
using EMC.Centera.SDK.FPTypes;

namespace EMC.Centera.SDK
{

  /// <summary>
  ///A Pool connection object.
  ///@author Graham Stuart
  ///@version
  /// </summary>
  public class FPPool : FPObject, IFPPool
  {
    private FPPoolRef thePool;

    /// <summary>
    ///Creates a pool connection with the clusters specified in the poolConnectionString. See API Guide: FPPool_Open
    ///
    ///@param poolConnectionString A Pool connection string containing IP (or DNS names) of
    ///                      the target clusters and associated authorization / options.
    /// </summary>
    public FPPool( string poolConnectionString )
    {
      this.thePool = Native.Pool.Open( poolConnectionString );
      this.AddObject( this.thePool, this );
    }


    /// <summary>
    ///Implicit cast between an existing Pool object and raw FPPoolRef.
    ///
    ///@param p A Pool object..
    ///@return The FPPoolRef associated with this Pool.
    /// </summary>
    public static implicit operator FPPoolRef( FPPool p ) => p.thePool;

    /// <summary>
    ///Implicit cast between a raw FPPoolRef and a new Pool object.
    ///
    ///@param poolRef FPPoolRef.
    ///@return The FPPool object associated with the FPPoolRef.
    /// </summary>
    public static implicit operator FPPool( FPPoolRef poolRef )
    {
      // Find the relevant Pool object in the hashtable for this FPPoolRef
      FPPool poolObject;

      if( FPObject.SDKObjects.ContainsKey( poolRef ) )
      {
        poolObject = (FPPool) FPObject.SDKObjects[poolRef];
      }
      else
      {
        throw new FPLibraryException( "FPPoolRef is not associated with an FPPool object", FPMisc.WRONG_REFERENCE_ERR );
      }

      return poolObject;
    }


    private PoolInfo PoolInfo
    {
      get
      {
        var outPoolInfo = new FPPoolInfo();

        Native.Pool.GetPoolInfo( this.thePool, ref outPoolInfo );
        return new PoolInfo( outPoolInfo );
      }
    }


    /// <summary>
    ///Explicitly close the FPPoolRef belonging to this Pool object.
    ///
    /// </summary>
    public override void Close()
    {
      if( this.thePool != 0 )
      {
        this.RemoveObject( this.thePool );
        Native.Pool.Close( this );
        this.thePool = 0;
      }
    }


    /// <summary>
    ///The size of the buffer to use when creating a CDF before overflowing to a
    ///temporary file. Set this value to the typical maximum size of your application
    ///CDFs including the size of any base64 embedded blob data.
    /// </summary>
    public int ClipBufferSize
    {
      get => (int) Native.Pool.GetIntOption( this, FPMisc.OPTION_BUFFERSIZE );
      set => Native.Pool.SetIntOption( this, FPMisc.OPTION_BUFFERSIZE, (FPInt) value );
    }

    /// <summary>
    ///The TCP/IP connection timeout, in milliseconds. Default is 2 minutes (12000ms),
    ///maximum is 10 minutes (600000ms).
    /// </summary>
    public int Timeout
    {
      get => (int) Native.Pool.GetIntOption( this, FPMisc.OPTION_TIMEOUT );
      set => Native.Pool.SetIntOption( this, FPMisc.OPTION_TIMEOUT, (FPInt) value );
    }

    /// <summary>
    ///Is FailOver to Replica / Secondary cluster enabled?
    /// </summary>
    public bool MultiClusterFailOverEnabled
    {
      get => Native.Pool.GetIntOption( this, FPMisc.OPTION_ENABLE_MULTICLUSTER_FAILOVER ) == (FPInt) 1;
      set
      {
        if( value )
        {
          Native.Pool.SetIntOption( this, FPMisc.OPTION_ENABLE_MULTICLUSTER_FAILOVER, (FPInt) 1 );
        }
        else
        {
          Native.Pool.SetIntOption( this, FPMisc.OPTION_ENABLE_MULTICLUSTER_FAILOVER, 0 );
        }
      }
    }

    /// <summary>
    ///Does the SDK request that extended naming schemes are used in order to guarantee
    ///that no collisions can take place?
    ///
    ///If set to true and StorageStrategyCapacity is active on the Cluster, then
    ///Single Instancing is disabled and each piece of content is stored separately
    ///with the addition of a GUID to the standard ContentAddress.
    ///
    ///If StorageStrategyPerformance is active on the Cluster then an additional
    ///discriminator is added to the Content Address.
    /// </summary>
    public bool CollisionAvoidanceEnabled
    {
      get => Native.Pool.GetIntOption( this, FPMisc.OPTION_DEFAULT_COLLISION_AVOIDANCE ) == (FPInt) 1;
      set
      {
        if( value )
        {
          Native.Pool.SetIntOption( this, FPMisc.OPTION_DEFAULT_COLLISION_AVOIDANCE, (FPInt) 1 );
        }
        else
        {
          Native.Pool.SetIntOption( this, FPMisc.OPTION_DEFAULT_COLLISION_AVOIDANCE, 0 );
        }
      }
    }

    /// <summary>
    ///The size of the PrefetchBuffer. This should be set to be no less than
    ///the EmbeddedBlob threshold (if one is used).
    /// </summary>
    public int PrefetchBufferSize
    {
      get => (int) Native.Pool.GetIntOption( this, FPMisc.OPTION_PREFETCH_SIZE );
      set => Native.Pool.SetIntOption( this, FPMisc.OPTION_PREFETCH_SIZE, (FPInt) value );
    }

    /// <summary>
    ///The maximum number of sockets the SDK will allocate for the client application.
    /// </summary>
    public static int MaxConnections
    {
      get => (int) Native.Pool.GetGlobalOption( FPMisc.OPTION_MAXCONNECTIONS );
      set => Native.Pool.SetGlobalOption( FPMisc.OPTION_MAXCONNECTIONS, (FPInt) value );
    }

    /// <summary>
    ///The maximum number of retries that will be made after a failed operation before
    ///returning a failure to the client application.
    /// </summary>
    public static int RetryLimit
    {
      get => (int) Native.Pool.GetGlobalOption( FPMisc.OPTION_RETRYCOUNT );
      set => Native.Pool.SetGlobalOption( FPMisc.OPTION_RETRYCOUNT, (FPInt) value );
    }

    /// <summary>
    ///The threshold for how long an application probe is allowed to attempt communication with an
    ///Access Node. The maximum value is 3600 seconds (1 hour)
    /// </summary>
    public static int ProbeLimit
    {
      get => (int) Native.Pool.GetGlobalOption( FPMisc.OPTION_PROBE_LIMIT );
      set => Native.Pool.SetGlobalOption( FPMisc.OPTION_PROBE_LIMIT, (FPInt) value );
    }

    /// <summary>
    ///The time to wait before retrying a failed API call.
    /// </summary>
    public static int RetrySleepTime
    {
      get => (int) Native.Pool.GetGlobalOption( FPMisc.OPTION_RETRYSLEEP );
      set => Native.Pool.SetGlobalOption( FPMisc.OPTION_RETRYSLEEP, (FPInt) value );
    }

    /// <summary>
    ///After a failure, the amount of time a Cluster is marked as Non Available
    ///before retrying.
    /// </summary>
    public static int ClusterNonAvailableTime
    {
      get => (int) Native.Pool.GetGlobalOption( FPMisc.OPTION_CLUSTER_NON_AVAIL_TIME );
      set => Native.Pool.SetGlobalOption( FPMisc.OPTION_CLUSTER_NON_AVAIL_TIME, (FPInt) value );
    }


    /// <summary>
    ///The strategy used for opening the Pool connection.
    /// </summary>
    public static int OpenStrategy
    {
      get => (int) Native.Pool.GetGlobalOption( FPMisc.OPTION_OPENSTRATEGY );
      set => Native.Pool.SetGlobalOption( FPMisc.OPTION_OPENSTRATEGY, (FPInt) value );
    }

    /// <summary>
    ///The Threshold value to stop embedding data within the CDF.
    /// </summary>
    public static int EmbeddedBlobThreshold
    {
      get => (int) Native.Pool.GetGlobalOption( FPMisc.OPTION_EMBEDDED_DATA_THRESHOLD );
      set => Native.Pool.SetGlobalOption( FPMisc.OPTION_EMBEDDED_DATA_THRESHOLD, (FPInt) value );
    }


    /*
    ///3.0
    public static int ProbeLimit
    {
      get
      {
        return (int) EMC.Centera.SDK.Native.Pool.GetGlobalOption(FPMisc.OPTION_PROBE_LIMIT);
      }
      set
      {
        EMC.Centera.SDK.Native.Pool.SetGlobalOption(FPMisc.OPTION_PROBE_LIMIT, (FPInt) value);
      }
    }
    /// </summary>
    */

    /// <summary>
    ///The FailOver strategy for Read operations.
    /// </summary>
    public static int MultiClusterReadStrategy
    {
      get => (int) Native.Pool.GetGlobalOption( FPMisc.OPTION_MULTICLUSTER_READ_STRATEGY );
      set => Native.Pool.SetGlobalOption( FPMisc.OPTION_MULTICLUSTER_READ_STRATEGY, (FPInt) value );
    }

    /// <summary>
    ///The FailOver strategy for Write operations.
    /// </summary>
    public static int MultiClusterWriteStrategy
    {
      get => (int) Native.Pool.GetGlobalOption( FPMisc.OPTION_MULTICLUSTER_WRITE_STRATEGY );
      set => Native.Pool.SetGlobalOption( FPMisc.OPTION_MULTICLUSTER_WRITE_STRATEGY, (FPInt) value );
    }

    /// <summary>
    ///The FailOver strategy for Delete operations.
    /// </summary>
    public static int MultiClusterDeleteStrategy
    {
      get => (int) Native.Pool.GetGlobalOption( FPMisc.OPTION_MULTICLUSTER_DELETE_STRATEGY );
      set => Native.Pool.SetGlobalOption( FPMisc.OPTION_MULTICLUSTER_DELETE_STRATEGY, (FPInt) value );
    }

    /// <summary>
    ///The FailOver strategy for Exists operations.
    /// </summary>
    public static int MultiClusterExistsStrategy
    {
      get => (int) Native.Pool.GetGlobalOption( FPMisc.OPTION_MULTICLUSTER_EXISTS_STRATEGY );
      set => Native.Pool.SetGlobalOption( FPMisc.OPTION_MULTICLUSTER_EXISTS_STRATEGY, (FPInt) value );
    }

    /// <summary>
    ///The FailOver strategy for Query operations.
    /// </summary>
    public static int MultiClusterQueryStrategy
    {
      get => (int) Native.Pool.GetGlobalOption( FPMisc.OPTION_MULTICLUSTER_QUERY_STRATEGY );
      set => Native.Pool.SetGlobalOption( FPMisc.OPTION_MULTICLUSTER_QUERY_STRATEGY, (FPInt) value );
    }

    /// <summary>
    ///The Cluster types that are available for Read fail over.
    /// </summary>
    public static int MultiClusterReadClusters
    {
      get => (int) Native.Pool.GetGlobalOption( FPMisc.OPTION_MULTICLUSTER_READ_CLUSTERS );
      set => Native.Pool.SetGlobalOption( FPMisc.OPTION_MULTICLUSTER_READ_CLUSTERS, (FPInt) value );
    }

    /// <summary>
    ///The Cluster types that are available for Write fail over.
    /// </summary>
    public static int MultiClusterWriteClusters
    {
      get => (int) Native.Pool.GetGlobalOption( FPMisc.OPTION_MULTICLUSTER_WRITE_CLUSTERS );
      set => Native.Pool.SetGlobalOption( FPMisc.OPTION_MULTICLUSTER_WRITE_CLUSTERS, (FPInt) value );
    }

    /// <summary>
    ///The Cluster types that are available for Delete fail over.
    /// </summary>
    public static int MultiClusterDeleteClusters
    {
      get => (int) Native.Pool.GetGlobalOption( FPMisc.OPTION_MULTICLUSTER_DELETE_CLUSTERS );
      set => Native.Pool.SetGlobalOption( FPMisc.OPTION_MULTICLUSTER_DELETE_CLUSTERS, (FPInt) value );
    }

    /// <summary>
    ///The Cluster types that are available for Exists fail over.
    /// </summary>
    public static int MultiClusterExistsClusters
    {
      get => (int) Native.Pool.GetGlobalOption( FPMisc.OPTION_MULTICLUSTER_EXISTS_CLUSTERS );
      set => Native.Pool.SetGlobalOption( FPMisc.OPTION_MULTICLUSTER_EXISTS_CLUSTERS, (FPInt) value );
    }

    /// <summary>
    ///The Cluster types that are available for Query fail over.
    /// </summary>
    public static int MultiClusterQueryClusters
    {
      get => (int) Native.Pool.GetGlobalOption( FPMisc.OPTION_MULTICLUSTER_QUERY_CLUSTERS );
      set => Native.Pool.SetGlobalOption( FPMisc.OPTION_MULTICLUSTER_QUERY_CLUSTERS, (FPInt) value );
    }

    /// <summary>
    ///The capacity of the Cluster.
    /// </summary>
    public long Capacity => this.PoolInfo.capacity;

    /// <summary>
    ///The amount of FreeSpace on the Cluster.
    /// </summary>
    public long FreeSpace => this.PoolInfo.freeSpace;

    /// <summary>
    ///The ID string of the Cluster.
    /// </summary>
    public string ClusterID => this.PoolInfo.clusterID;

    /// <summary>
    ///The name of the Cluster.
    /// </summary>
    public string ClusterName => this.PoolInfo.clusterName;

    /// <summary>
    ///The version of CentraStar software on the Cluster.
    /// </summary>
    public string CentraStarVersion => this.PoolInfo.version;

    /// <summary>
    ///The ReplicaAddress of the Cluster.
    /// </summary>
    public string ReplicaAddress => this.PoolInfo.replicaAddress;

    /// <summary>
    ///The error status for the last SDK operation with any pool. See API Guide: FPPool_GetLastError
    ///
    /// </summary>
    public static int LastError => (int) Native.Pool.GetLastError();


    /// <summary>
    ///A structure containing information relating to the last SDK operation. See API Guide: FPPool_GetLastErrorInfo
    ///
    /// </summary>
    public static FPErrorInfo LastErrorInfo
    {
      get
      {
        var errorInfo = new FPErrorInfo();
        Native.Pool.GetLastErrorInfo( ref errorInfo );

        return errorInfo;
      }
    }

    /// <summary>
    ///The naming schemes that are available on the Cluster.
    /// </summary>
    public string BlobNamingSchemes => this.GetCapability( FPMisc.BLOBNAMING, FPMisc.SUPPORTED_SCHEMES );

    /// <summary>
    ///Is Query available in the Capabilities list of the Pool object?
    /// </summary>
    public bool QueryAllowed => this.GetCapability( FPMisc.CLIPENUMERATION, FPMisc.ALLOWED ).Equals( FPMisc.TRUE );

    /// <summary>
    ///A list of pools that have Query capability.
    /// </summary>
    public string QueryPools => this.GetCapability( FPMisc.CLIPENUMERATION, FPMisc.POOLS );
    /// <summary>
    ///Is Delete available in the Capabilities list of the Pool object?
    /// </summary>
    public bool DeleteAllowed => this.GetCapability( FPMisc.DELETE, FPMisc.ALLOWED ).Equals( FPMisc.TRUE );

    /// <summary>
    ///A list of pools that have Delete capability.
    /// </summary>
    public string DeletePools => this.GetCapability( FPMisc.DELETE, FPMisc.POOLS );
    /// <summary>
    ///Are Delete operations logged i.e. are Reflections created?
    /// </summary>
    public bool DeletionsLogged => this.GetCapability( FPMisc.DELETIONLOGGING, FPMisc.SUPPORTED ).Equals( FPMisc.TRUE );

    /// <summary>
    ///Is Exists available in the Capabilities list of the Pool object?
    /// </summary>
    public bool ExistsAllowed => this.GetCapability( FPMisc.EXIST, FPMisc.ALLOWED ).Equals( FPMisc.TRUE );

    /// <summary>
    ///A list of pools that have Exists capability.
    /// </summary>
    public string ExistsPools => this.GetCapability( FPMisc.EXIST, FPMisc.POOLS );
    /// <summary>
    ///Is PrivilegedDelete available in the Capabilities list of the Pool object?
    /// </summary>
    public bool PrivilegedDeleteAllowed => this.GetCapability( FPMisc.PRIVILEGEDDELETE, FPMisc.ALLOWED ).Equals( FPMisc.TRUE );

    /// <summary>
    ///A list of pools that have PrivilegedDelete capability.
    /// </summary>
    public string PrivilegedDeletePools => this.GetCapability( FPMisc.PRIVILEGEDDELETE, FPMisc.POOLS );
    /// <summary>
    ///Is Read available in the Capabilities list of the Pool object?
    /// </summary>
    public bool ReadAllowed => this.GetCapability( FPMisc.READ, FPMisc.ALLOWED ).Equals( FPMisc.TRUE );

    /// <summary>
    ///A list of pools that have Read capability.
    /// </summary>
    public string ReadPools => this.GetCapability( FPMisc.READ, FPMisc.POOLS );
    /// <summary>
    ///What is the Default retention Strategy of the cluster?
    /// </summary>
    public string DefaultRetentionScheme => this.GetCapability( FPMisc.RETENTION, FPMisc.DEFAULT );

    /// <summary>
    ///Is Write available in the Capabilities list of the Pool object?
    /// </summary>
    public bool WriteAllowed => this.GetCapability( FPMisc.WRITE, FPMisc.ALLOWED ).Equals( FPMisc.TRUE );

    /// <summary>
    ///A list of pools that have Write capability.
    /// </summary>
    public string WritePools => this.GetCapability( FPMisc.WRITE, FPMisc.POOLS );

    /// <summary>
    ///A list containing the Pool mappings for all Profiles / Pools.
    /// </summary>
    public string PoolMappings => this.GetCapability( FPMisc.RETENTION, FPMisc.POOLMAPPINGS );

    /// <summary>
    ///A list containing the Profiles for which a Pool mapping exists.
    /// </summary>
    public string PoolProfiles => this.GetCapability( FPMisc.RETENTION, FPMisc.PROFILES );

    /// <summary>
    ///The Edition of the Centera this pool is connected to i.e.
    ///
    ///basic  Basic Edition
    ///ce   Governance Edition (formerly known as Compliance Edition)
    ///ce+    Compliance Edition Plus
    /// </summary>
    public string CenteraEdition => this.GetCapability( FPMisc.COMPLIANCE, FPMisc.MODE );

    /// <summary>
    /// 
    /// </summary>
    /// <param name="inCapabilityName"></param>
    /// <param name="inCapabilityAttributeName"></param>
    /// <returns></returns>
    internal string GetCapability( string inCapabilityName, string inCapabilityAttributeName )
    {
      var outString = new StringBuilder();
      FPInt bufSize = 0;
      FPInt len = 0;

      do
      {
        bufSize += FPMisc.STRING_BUFFER_SIZE;
        len += FPMisc.STRING_BUFFER_SIZE;
        outString.EnsureCapacity( (int) bufSize );
        try
        {
          Native.Pool.GetCapability( this, inCapabilityName, inCapabilityAttributeName, outString, ref len );
        }
        catch( FPLibraryException e )
        {
          return e.ErrorInfo.Error == FPMisc.ATTR_NOT_FOUND_ERR ? "" : throw e;
        }

      } while( len > bufSize );

      return outString.ToString();
    }

    /// <summary>
    ///A DateTime object representing the time of the Cluster associated
    ///with this object
    /// </summary>
    public DateTime ClusterTime
    {
      get
      {
        var outClusterTime = new StringBuilder();
        FPInt bufSize = 0;
        FPInt ioClusterTimeLen = 0;

        do
        {
          bufSize += FPMisc.STRING_BUFFER_SIZE;
          ioClusterTimeLen += FPMisc.STRING_BUFFER_SIZE;
          outClusterTime.EnsureCapacity( (int) bufSize );
          Native.Pool.GetClusterTime( this, outClusterTime, ref ioClusterTimeLen );
        } while( ioClusterTimeLen > bufSize );

        return FPMisc.GetDateTime( outClusterTime.ToString() );
      }
    }

    /// <summary>
    ///A string containing the version information for the supplied component.
    ///See API Guide: FPPool_GetComponentVersion
    ///
    /// </summary>
    public static string SDKVersion
    {
      get
      {
        var outVersion = new StringBuilder();
        FPInt bufSize = 0;
        FPInt ioVersionLen = 0;

        do
        {
          bufSize += FPMisc.STRING_BUFFER_SIZE;
          ioVersionLen += FPMisc.STRING_BUFFER_SIZE;
          outVersion.EnsureCapacity( (int) bufSize );
          Native.Pool.GetComponentVersion( (FPInt) FPMisc.VERSION_FPLIBRARY_DLL, outVersion, ref ioVersionLen );
        } while( ioVersionLen > bufSize );

        return outVersion.ToString();
      }
    }

    /// <summary>
    ///The ID of the clip containing information about the
    ///profile being used to connect to the pool.
    ///
    /// </summary>
    public string ProfileClip
    {
      get
      {
        var profileClipID = new StringBuilder( FPMisc.STRING_BUFFER_SIZE );
        Native.Pool.GetClipID( this, profileClipID );
        return profileClipID.ToString();
      }
      set => Native.Pool.SetClipID( this, value );
    }


    /// <summary>
    ///Check for the existence of a given ClipID in the Pool associated with
    ///this object. See API Guide: FPClip_Exists
    ///
    ///@param inClipID  The clip to check for existence.
    ///@return A boolean value relating to existence.
    /// </summary>
    public bool ClipExists( string inClipID ) => Native.Clip.Exists( this, inClipID ) == FPBool.True;

    /// <summary>
    ///Delete the clip with the the supplied ID from the Pool associated with this
    ///object.  See API Guide: FPClip_Delete
    ///
    ///@param inClipID  The clip to delete.
    /// </summary>
    public void ClipDelete( string inClipID ) => Native.Clip.Delete( this, inClipID );

    /// <summary>
    ///Delete the clip (using DEFAULT_OPTIONS) with the the supplied ID from the Pool
    ///associated with this object and record the reason in an Audit Trail.
    ///See API Guide: FPClip_AuditedDelete
    ///
    ///@param inClipID  The clip to delete.
    ///@param inReason  A string containing free test relating to the reason for deletion.
    /// </summary>
    public void ClipAuditedDelete( string inClipID, string inReason ) => this.ClipAuditedDelete( inClipID, inReason, FPMisc.OPTION_DEFAULT_OPTIONS );

    /// <summary>
    ///Delete the clip with the the supplied ID from the Pool associated with this
    ///object and record the reason in an Audit Trail. See API Guide: FPClip_AuditedDelete
    ///
    ///@param inClipID  The clip to delete.
    ///@param inReason  A string containing free test relating to the reason for deletion.
    ///@param inOptions The type of deletion being performed (Normal or Privileged).
    /// </summary>
    public void ClipAuditedDelete( string inClipID, string inReason, long inOptions )
    {
      Native.Clip.AuditedDelete( this, inClipID, inReason, (FPLong) inOptions );
    }

    /// <summary>
    ///Create a new clip in the Pool. See API Guide: FPClip_Create
    ///
    ///@param inName The name of the clip to be created.
    ///@return The resulting Clip object.
    /// </summary>
    public FPClip ClipCreate( string inName ) => new FPClip( Native.Clip.Create( this, inName ) );

    /// <summary>
    ///Open a Clip that exists in the Pool. See API Guide: FPClip_Open
    ///
    ///@param inClipID    The Content Address string of the clip to be opened.
    ///@param inOpenMode  How the clip should be opened (Flat or Tree)
    ///@return The resulting Clip object.
    /// </summary>
    public FPClip ClipOpen( string inClipID, int inOpenMode ) => new FPClip( Native.Clip.Open( this, inClipID, (FPInt) inOpenMode ) );

    /// <summary>
    ///Create a new clip by reading a raw clip from a stream using DEFAULT_OPTIONS.
    ///See API Guide: FPClip_RawOpen
    ///
    ///@param inClipID  The ID of the Clip being read - must match the new Clip ID.
    ///@param inStream  The stream to read the clip from.
    ///return The resulting new Clip in this Pool.
    /// </summary>
    public FPClip ClipRawOpen( string inClipID, FPStream inStream ) => this.ClipRawOpen( inClipID, inStream, FPMisc.OPTION_DEFAULT_OPTIONS );

    /// <summary>
    ///Create a new clip by reading a raw clip from a stream.
    ///See API Guide: FPClip_RawOpen
    ///
    ///@param inClipID  The ID of the Clip being read - must match the new Clip ID.
    ///@param inStream  The stream to read the clip from.
    ///@param inOptions Options relating to how the Open is performed.
    ///return The resulting new Clip in this Pool.
    /// </summary>
    public FPClip ClipRawOpen( string inClipID, FPStream inStream, long inOptions ) => new FPClip( Native.Clip.RawOpen( this.thePool, inClipID, inStream, (FPLong) inOptions ) );

    public override string ToString() => this.PoolInfo.ToString();

    private FPRetentionClassCollection myRetentionClasses;

    /// <summary>
    ///The RetentionClasses configured within this pool. These are configured on the Centera so no modification
    ///should be made to the Collection.
    /// </summary>
    public FPRetentionClassCollection RetentionClasses =>
      this.myRetentionClasses ??
      (this.myRetentionClasses = new FPRetentionClassCollection( this ));

    // New in 3.1

    /// <summary>
    ///Registers the application with the Centera. This is not required but does allow the administrator to see which applications
    ///are using the Centera at any point in time.
    /// </summary>
    public static void RegisterApplication( string appName, string appVersion ) => Native.Pool.RegisterApplication( appName, appVersion );

    /// <summary>
    ///Is Event Based Retention supported on the Cluster.
    /// </summary>
    public bool EBRSupported => this.GetCapability( FPMisc.COMPLIANCE, FPMisc.EVENT_BASED_RETENTION ).Equals( FPMisc.SUPPORTED );

    /// <summary>
    ///Is Litigation / Retention Hold supported on the cluster.
    /// </summary>
    public bool HoldSupported => this.GetCapability( FPMisc.COMPLIANCE, FPMisc.RETENTION_HOLD ).Equals( FPMisc.SUPPORTED );

    /// <summary>
    ///Is Litigation / Retention Hold allowed on the Cluster. Ensures that it is supported before checking.
    /// </summary>
    public bool HoldAllowed => this.HoldSupported && this.GetCapability( FPMisc.RETENTION_HOLD, FPMisc.ALLOWED ).Equals( FPMisc.TRUE );

    /// <summary>
    ///A list of pools that have Retention Hold capability.
    /// </summary>
    public string HoldPools => this.GetCapability( FPMisc.RETENTION_HOLD, FPMisc.POOLS );
    /// <summary>
    ///The minimum period that may be specified for a normal (fixed) retention period.
    /// </summary>
    public TimeSpan FixedRetentionMin => new TimeSpan( 0, 0, int.Parse( this.GetCapability( FPMisc.RETENTION, FPMisc.FIXED_RETENTION_MIN ) ) );

    /// <summary>
    ///The maximum period that may be specified for a normal (fixed) retention period.
    /// </summary>
    public TimeSpan FixedRetentionMax => new TimeSpan( 0, 0, int.Parse( this.GetCapability( FPMisc.RETENTION, FPMisc.FIXED_RETENTION_MAX ) ) );

    /// <summary>
    ///The minimum period that may be specified for a variable (EBR) retention period.
    /// </summary>
    public TimeSpan VariableRetentionMin => new TimeSpan( 0, 0, int.Parse( this.GetCapability( FPMisc.RETENTION, FPMisc.VARIABLE_RETENTION_MIN ) ) );

    /// <summary>
    ///The maximum period that may be specified for a variable (EBR) retention period.
    /// </summary>
    public TimeSpan VariableRetentionMax => new TimeSpan( 0, 0, int.Parse( this.GetCapability( FPMisc.RETENTION, FPMisc.VARIABLE_RETENTION_MAX ) ) );

    /// <summary>
    ///The default retention period that may will be used if the application does not specifically set it.
    /// </summary>
    public TimeSpan RetentionDefault => new TimeSpan( 0, 0, int.Parse( this.GetCapability( FPMisc.RETENTION, FPMisc.RETENTION_DEFAULT ) ) );

    /// <summary>
    ///Are Retention Governors supported on the Centera i.e. Minimum / Maximum values for Fixed and Variable retention.
    /// </summary>
    public bool RetentionMinMax => this.GetCapability( FPMisc.COMPLIANCE, FPMisc.RETENTION_MIN_MAX ).Equals( "supported" );

    public static bool StrictStreamMode
    {
      get => Native.Pool.GetGlobalOption( FPMisc.OPTION_STREAM_STRICT_MODE ).Equals( FPMisc.TRUE );
      set
      {
        if( value )
        {
          Native.Pool.SetGlobalOption( FPMisc.OPTION_STREAM_STRICT_MODE, 0 );
        }
        else
        {
          Native.Pool.SetGlobalOption( FPMisc.OPTION_STREAM_STRICT_MODE, (FPInt) 1 );
        }
      }
    }
  }
}