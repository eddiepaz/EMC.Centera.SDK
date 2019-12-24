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
using EMC.Centera.SDK.FPTypes;

namespace EMC.Centera.SDK
{

  /// <summary>
  ///An object representing a query to an existing pool.
  ///@author Graham Stuart
  ///@version
  /// </summary>
  public class FPQuery : FPObject
  {
    readonly FPPoolRef thePool;
    FPPoolQueryRef theQuery;
    FPQueryExpressionRef theExpression;

    /// <summary>
    ///Construct a Query for an existing Pool. Default values set for an unbounded time query on existing objects.
    ///
    ///@param p The existing Pool object.
    /// </summary>
    public FPQuery( FPPool p )
    {
      this.thePool = p;
      this.theQuery = 0;
      this.theExpression = Native.QueryExpression.Create();

      // Set default values - unbounded query on existing objects
      Native.QueryExpression.SetStartTime( this.theExpression, 0 );
      Native.QueryExpression.SetEndTime( this.theExpression, (FPLong) (-1) );
      Native.QueryExpression.SetType( this.theExpression, (FPInt) FPMisc.QUERY_TYPE_EXISTING );
    }

    /// <summary>
    ///Construct a Query using an existing FPPoolQueryRef. Used internally when implicitly converting
    ///an FPPoolQueryRef to a Query.
    ///
    ///@param q The existing FPPoolQueryRef
    /// </summary>

    internal FPQuery( FPPoolQueryRef q )
    {
      this.thePool = Native.PoolQuery.GetPoolRef( q );
      this.theQuery = q;
      this.theExpression = 0;
    }

    /// <summary>
    ///Implicit conversion between a Query and an FPPoolQueryRef
    ///
    ///@param q The Query.
    ///@return  The FPPoolQueryRef associated with the object
    /// </summary>
    public static implicit operator FPPoolQueryRef( FPQuery q ) => q.theQuery;

    /// <summary>
    ///Explicitly close this Query. See API Guide: FPPoolQuery_Close
    /// </summary>
    public override void Close()
    {
      if( this.theQuery != 0 )
      {
        Native.PoolQuery.Close( this.theQuery );
        this.theQuery = 0;
      }

      if( this.theExpression != 0 )
      {
        Native.QueryExpression.Close( this.theExpression );
        this.theExpression = 0;
      }
    }

    /// <summary>
    ///Executes this Query on the associated Pool based on the associated QueryExpression.
    ///See API Guide: FPPoolQuery_Open
    ///
    /// </summary>
    public void Execute()
    {
      this.theQuery = Native.PoolQuery.Open( this.thePool, this.theExpression );
    }


    /// <summary>
    ///The Pool object associated with this Query.
    ///See API Guide: FPPoolQuery_GetPoolRef
    ///
    /// </summary>
    public FPPool FPPool => Native.PoolQuery.GetPoolRef( this );

    /// <summary>
    ///Retrieve the next member of the result set for the current open Query. See API Guide: FPPoolQuery_FetchResult
    ///
    ///@param   outResult The next available FPQueryResult in the FPQuery.
    ///@param inTimeout The timeout value to wait for the next result.
    ///@return  The ResultCode of the operation.
    /// </summary>
    public int FetchResult( ref FPQueryResult outResult, int inTimeout )
    {
      outResult.Result = Native.PoolQuery.FetchResult( this.theQuery, (FPInt) inTimeout );
      return (int) Native.QueryResult.GetResultCode( outResult );
    }

    /// <summary>
    ///The Start Time for the Query to be executed.
    ///
    /// </summary>
    public DateTime StartTime
    {
      get => FPMisc.GetDateTime( Native.QueryExpression.GetStartTime( this.theExpression ) );
      set => Native.QueryExpression.SetStartTime( this.theExpression, FPMisc.GetTime( value ) );
    }

    /// <summary>
    ///The End Time for the Query to be executed.
    ///
    /// </summary>
    public DateTime EndTime
    {
      get =>
        this.UnboundedEndTime
          ? this.FPPool.ClusterTime
          : FPMisc.GetDateTime( Native.QueryExpression.GetEndTime( this.theExpression ) );
      set => Native.QueryExpression.SetEndTime( this.theExpression, FPMisc.GetTime( value ) );
    }

    /// <summary>
    ///The Start Time for the Query to be executed is unbounded i.e. the Java Epoch.
    ///
    /// </summary>
    public bool UnboundedStartTime
    {
      get => Native.QueryExpression.GetStartTime( this.theExpression ) == 0;
      set => Native.QueryExpression.SetStartTime( this.theExpression, 0 );
    }

    /// <summary>
    ///The End Time for the Query to be executed is unbounded i.e. the current cluster time.
    ///
    /// </summary>
    public bool UnboundedEndTime
    {
      get => (Native.QueryExpression.GetEndTime( this.theExpression ) == (FPLong) (-1));
      set => Native.QueryExpression.SetEndTime( this.theExpression, (FPLong) (-1) );
    }

    /// <summary>
    ///The Type of Query to be executed i.e. the type of clips to query for:
    ///
    ///FPMisc.QUERY_TYPE_DELETED
    ///FPMisc.QUERY_TYPE_EXISTING
    ///FPMisc.QUERY_TYPE_DELETED | FPMisc.QUERY_TYPE_EXISTING
    ///
    /// </summary>
    public int Type
    {
      get => (int) Native.QueryExpression.GetType( this.theExpression );
      set => Native.QueryExpression.SetType( this.theExpression, (FPInt) value );
    }


    /// <summary>
    ///Add a Clip level attribute to the list of attributes to be retrieved in the Query to be executed.
    ///See API Guide: FPQueryExpression_SelectField
    ///
    ///@param inFieldName The Attribute Name.
    /// </summary>
    public void SelectField( string inFieldName )
    {
      Native.QueryExpression.SelectField( this.theExpression, inFieldName );
    }

    /// <summary>
    ///Remove a Clip level attribute from the list of attributes to be retrieved in the Query to be executed.
    ///See API Guide: FPQueryExpression_DeselectField
    ///
    ///@param inFieldName The Attribute Name.
    /// </summary>
    public void DeselectField( string inFieldName )
    {
      Native.QueryExpression.DeselectField( this.theExpression, inFieldName );
    }

    /// <summary>
    ///Determine if an Attribute is in the list of selected attributes to be retrieved in the Query to be executed.
    ///See API Guide: FPQueryExpression_IsFieldSelected
    ///
    ///@param inFieldName The Attribute Name.
    ///@return  Boolean representing the Selected state for the attributes in the Query Expression.
    /// </summary>
    public bool IsSelected( string inFieldName )
    {
      return Native.QueryExpression.IsFieldSelected( this.theExpression, inFieldName ) == FPBool.True;
    }
  }
}
