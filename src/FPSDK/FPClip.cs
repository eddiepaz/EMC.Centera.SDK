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
using System.Text;
using EMC.Centera.SDK.FPTypes;

namespace EMC.Centera.SDK
{
  /// <summary>
  ///An object representing an FPClipRef.
  ///@author Graham Stuart
  ///@version
  /// </summary>

  public class FPClip : FPObject, IFPClip
  {
    private FPClipRef theClip;

    private readonly FPPool thePool;

    /// <summary>
    ///Create a new Clip object in the supplied Pool.
    ///See API Guide: FPClip_Create
    ///
    ///@param inPool  The Pool to create the Clip in.
    ///@param inName  The name of the clip.
    ///@return The int value of the option.
    /// </summary>
    public FPClip( FPPool inPool, string inName )
    {
      this.thePool = inPool;
      this.theClip = Native.Clip.Create( this.thePool, inName );
      this.AddObject( this.theClip, this );
    }


    /// <summary>
    ///Create a Clip object by opening an existing clip in the supplied Pool.
    ///See API Guide: FPClip_Open
    ///
    ///@param inPool  The Pool containing the Clip.
    ///@param inClipID  The ID of the clip to be opened.
    ///@param inOpenMode  The mode to open the clip in (Flat or Tree).
    /// </summary>
    public FPClip( FPPool inPool, string inClipID, int inOpenMode )
    {
      this.thePool = inPool;
      this.theClip = Native.Clip.Open( inPool, inClipID, (FPInt) inOpenMode );
      this.AddObject( this.theClip, this );
    }


    /// <summary>
    ///Create a new clip in the supplied Pool by reading a raw clip from a stream.
    ///See API Guide: FPClip_RawOpen
    ///
    ///@param inPool  The Pool to create the Clip in.
    ///@param inClipID  The ID of the Clip being read - must match the new Clip ID.
    ///@param inStream  The stream to read the clip from.
    ///@param inOptions A suitable option.
    /// </summary>
    public FPClip( FPPool inPool, string inClipID, FPStream inStream, long inOptions )
    {
      this.thePool = inPool;
      this.theClip = Native.Clip.RawOpen( this.thePool, inClipID, inStream, (FPLong) inOptions );
      this.AddObject( this.theClip, this );
    }


    /// <summary>
    ///Create a Clip using an existing FPClipRef.
    ///
    ///@param c The FPClipRef.
    /// </summary>
    internal FPClip( FPClipRef c )
    {
      this.theClip = c;
      this.thePool = Native.Clip.GetPoolRef( c );
      this.AddObject( this.theClip, this );
    }

    /// <summary>
    ///Copy constructor.
    ///
    ///@param c The FPClip to make a copy of.
    /// 
    /// </summary>
    public FPClip( FPClip c ) => this.theClip = c;

    /// <summary>
    ///Implicit cast between an existing Clip object and an FPClipRef.
    ///
    ///@param c A Clip object..
    ///@return The FPClipRef associated with this Clip.
    /// </summary>
    public static implicit operator FPClipRef( FPClip c ) => c.theClip;

    /// <summary>
    ///Implicit cast between an FPClipRef and an FPClip (which must already exist).
    ///
    ///@param clipRef An FPClipRef.
    ///@return A new Clip object.
    /// </summary>
    public static implicit operator FPClip( FPClipRef clipRef )
    {
      // Find the relevant Tag object in the hashtable for this FPTagRef
      FPClip clipObject;

      if( FPObject.SDKObjects.Contains( clipRef ) )
      {
        clipObject = (FPClip) FPObject.SDKObjects[clipRef];
      }
      else
      {
        throw new FPLibraryException( "FPClipRef is not associated with an FPClip object", FPMisc.WRONG_REFERENCE_ERR );
      }

      return clipObject;
    }

    /// <summary>
    ///Explicitly Close the Clip. See API Guide: FPClip_Close
    /// </summary>
    public override void Close()
    {
      if( this.myTopTag != null )
      {
        this.RemoveObject( this.myTopTag );
        this.myTopTag.Close();
      }

      if( this.theClip != 0 )
      {
        this.RemoveObject( this.theClip );
        Native.Clip.Close( this.theClip );
      }

      this.theClip = 0;
    }

    /// <summary>
    ///The Pool associated with this Clip. See API Guide: FPClip_GetPoolRef
    ///
    ///@return A Pool object.
    /// </summary>
    public FPPool FPPool => this.thePool;

    /// <inheritdoc />
    public FPTag AddTag( string tagName ) => new FPTag( this.TopTag, tagName );

    private FPTag myTopTag;

    /// <summary>
    ///The top tag within the Clip. See API Guide: FPClip_GetTopTag
    ///
    /// </summary>
    public FPTag TopTag
    {
      get
      {
        if( this.myTopTag == null )
        {
          var t = Native.Clip.GetTopTag( this );

          this.myTopTag = !FPObject.SDKObjects.Contains( t )
                            ? new FPTag( t )
                            : (FPTag) FPObject.SDKObjects[t];
        }

        return this.myTopTag;
      }
    }

    /// <summary>
    ///The number of blobs within this Clip. See API Guide: FPClip_GetNumBlobs
    ///
    /// </summary>
    public int NumBlobs => (int) Native.Clip.GetNumBlobs( this );

    /// <summary>
    ///The number of tags within this Clip. See API Guide: FPClip_GetNumTags
    ///
    /// </summary>
    public int NumTags => (int) Native.Clip.GetNumTags( this );

    /// <summary>
    ///The total size of this Clip. See API Guide: FPClip_GetTotalSize
    ///
    /// </summary>
    public long TotalSize => (long) Native.Clip.GetTotalSize( this );

    /// <summary>
    ///A string representing the ID of this Clip. See API Guide: FPClip_GetClipID
    ///
    /// </summary>
    public string ClipID
    {
      get
      {
        var outClipID = new StringBuilder( FPMisc.STRING_BUFFER_SIZE );

        Native.Clip.GetClipID( this, outClipID );
        return outClipID.ToString();
      }
    }

    /// <summary>
    ///The name of this Clip.
    ///
    /// </summary>
    public string Name
    {
      get
      {
        byte[] outString;
        FPInt bufSize = 0;
        FPInt len;

        do
        {
          bufSize += FPMisc.STRING_BUFFER_SIZE;
          len = bufSize;
          outString = new byte[(int) bufSize];
          Native.Clip.GetName( this, ref outString, ref len );
        } while( len > bufSize );

        return Encoding.UTF8.GetString( outString, 0, (int) len - 1 );
      }
      set => Native.Clip.SetName( this, value );
    }

    /// <summary>
    ///Return a string form of this Clip
    ///
    ///@return The string representing the ID of this Clip.
    /// </summary>
    public override string ToString() => this.ClipID;

    /// <summary>
    ///The creation date of this Clip using default buffer of size FPMisc.STRING_BUFFER_SIZE. See API Guide: FPClip_GetCreationDate
    ///
    /// </summary>
    public DateTime CreationDate
    {
      get
      {
        var outString = new StringBuilder();
        FPInt bufSize = 0;
        FPInt len;

        do
        {
          bufSize += FPMisc.STRING_BUFFER_SIZE;
          len = bufSize;
          outString.EnsureCapacity( (int) bufSize );
          Native.Clip.GetCreationDate( this, outString, ref len );
        } while( len > bufSize );

        return FPMisc.GetDateTime( outString.ToString() );
      }
    }

    /// <summary>
    ///The retention period represented by a TimeSpan value.
    ///Infinite Retention is represented a value of FPMisc.INFINITE_RETENTION_PERIOD ticks.
    ///To set this to the default RetentionPeriod of the cluster use a value of FPMisc.DEFAULT_RETENTION_PERIOD ticks.
    ///
    /// </summary>
    public TimeSpan RetentionPeriod
    {
      get
      {
        var period = (int) Native.Clip.GetRetentionPeriod( this );

        return period < 0 ? new TimeSpan( period ) : new TimeSpan( 0, 0, period );
      }

      set
      {
        if( value.Ticks > 0 )
        {
          Native.Clip.SetRetentionPeriod( this, (FPLong) value.TotalSeconds );
        }
        else
        {
          Native.Clip.SetRetentionPeriod( this, (FPLong) value.Ticks );
        }
      }
    }


    /// <summary>
    ///The retention expiry date.
    ///
    /// </summary>
    public DateTime RetentionExpiry
    {
      get
      {
        if( this.RetentionPeriod.Ticks < 0 )
        {
          return this.RetentionPeriod.Ticks == FPMisc.INFINITE_RETENTION_PERIOD
                   ? new DateTime( 9999, 12, 31 )
                   : this.CreationDate.Add( this.thePool.RetentionDefault );
        }

        return this.CreationDate.Add( this.RetentionPeriod );
      }

      set => this.RetentionPeriod = new TimeSpan( value.Ticks - this.FPPool.ClusterTime.Ticks );
    }

    /// <summary>
    ///The EBR expiry date.
    ///
    /// </summary>
    public DateTime EBRExpiry
    {
      get
      {
        // The EBR event has not been triggered so as it
        // is not yet in effect we can always deleted it
        var epoch = new DateTime( 1970, 1, 1 );
        if( this.EBREventTime == epoch )
        {
          return epoch;
        }

        return this.EBRPeriod.Ticks != FPMisc.INFINITE_RETENTION_PERIOD
          ? this.EBREventTime.Add( this.EBRPeriod )
          : new DateTime( 9999, 12, 31 );
      }

      set => this.EBRPeriod = new TimeSpan( value.Ticks - this.FPPool.ClusterTime.Ticks );
    }

    /// <summary>
    ///The time of the EBR Event
    ///
    /// </summary>
    public DateTime EBREventTime
    {
      get
      {
        var outString = new StringBuilder();
        FPInt bufSize = 0;
        FPInt len;

        do
        {
          bufSize += FPMisc.STRING_BUFFER_SIZE;
          len = bufSize;
          outString.EnsureCapacity( (int) bufSize );
          Native.Clip.GetEBREventTime( this, outString, ref len );
        } while( len > bufSize );

        return FPMisc.GetDateTime( outString.ToString() );
      }
    }

    /// <summary>
    ///The modification state of the clip. See API Guide: FPClip_IsModified
    ///
    /// </summary>
    public bool Modified => Native.Clip.IsModified( this ) == FPBool.True;

    /// <summary>
    ///The next tag within this Clip. See API Guide: FPClip_FetchNext
    ///
    /// </summary>
    public FPTag NextTag
    {
      get
      {
        var t = Native.Clip.FetchNext( this );

        var tag = !FPObject.SDKObjects.Contains( t )
                    ? new FPTag( t )
                    : (FPTag) FPObject.SDKObjects[t];

        return tag;
      }
    }

    /// <summary>
    ///Write the Clip to the Centera. See API Guide: FPClip_Write
    ///
    ///@return string representing the ID of the clip written to the Centera.
    /// </summary>
    public string Write()
    {
      var outClipID = new StringBuilder( FPMisc.STRING_BUFFER_SIZE );

      Native.Clip.Write( this, outClipID );
      return outClipID.ToString();
    }

    /// <summary>
    ///Read this Clip in raw form to a Stream. See API Guide: FPClip_RawRead
    ///
    ///@param inStream  The Stream object to read the clip into.
    /// </summary>
    public void RawRead( FPStream inStream )
    {
      Native.Clip.RawRead( this, inStream );
    }

    /// <summary>
    ///Set a description attribute in the Clip level metadata. See API Guide: FPClip_SetDescriptionAttribute
    ///
    ///@param inAttrName  The name of the description attribute.
    ///@param inAttrValue The value for the description attribute.
    /// </summary>
    public void SetAttribute( string inAttrName, string inAttrValue )
    {
      Native.Clip.SetDescriptionAttribute( this, inAttrName, inAttrValue );
    }

    /// <summary>
    ///Remove a description attribute from the Clip level metadata. See API Guide: FPClip_SetDescriptionAttribute
    ///
    ///@param inAttrName  The name of the description attribute.
    /// </summary>
    public void RemoveAttribute( string inAttrName )
    {
      Native.Clip.RemoveDescriptionAttribute( this, inAttrName );
    }

    /// <summary>
    ///Get a description attribute from the Clip level metadata. See API Guide: FPClip_GetDescriptionAttribute
    ///
    ///@param inAttrName  The name of the description attribute.
    ///@return The string value of the Description attribute.
    /// </summary>
    public string GetAttribute( string inAttrName )
    {
      byte[] outString;
      FPInt bufSize = 0;
      FPInt len;

      do
      {
        bufSize += FPMisc.STRING_BUFFER_SIZE;
        len = bufSize;
        outString = new byte[(int) bufSize];
        Native.Clip.GetDescriptionAttribute( this, inAttrName, ref outString, ref len );
      } while( len > bufSize );

      return Encoding.UTF8.GetString( outString, 0, (int) len - 1 );

    }

    /// <summary>
    ///Get a description attribute from the Clip level metadata using the index of the
    ///attribute. See API Guide: FPClip_GetDescriptionAttributeIndex
    ///
    ///@param inIndex   The index of the attribute to retrieve.
    ///@return The FPAttribute at the specified index position.
    /// </summary>
    public FPAttribute GetAttributeByIndex( int inIndex )
    {
      byte[] nameString;
      byte[] valueString;
      FPInt nameSize = 0, nameLen, valSize = 0, valLen;

      do
      {
        nameSize += FPMisc.STRING_BUFFER_SIZE;
        nameLen = nameSize;
        nameString = new byte[(int) nameSize];

        valSize += FPMisc.STRING_BUFFER_SIZE;
        valLen = valSize;
        valueString = new byte[(int) valSize];

        Native.Clip.GetDescriptionAttributeIndex( this, (FPInt) inIndex, ref nameString, ref nameLen, ref valueString, ref valLen );
      } while( nameLen > nameSize || valLen > valSize );

      return new FPAttribute( Encoding.UTF8.GetString( nameString, 0, (int) nameLen - 1 ), Encoding.UTF8.GetString( valueString, 0, (int) valLen - 1 ) );

    }

    /// <summary>
    ///The number of Description Attributes associated with this clip.
    ///
    /// </summary>
    public int NumAttributes => (int) Native.Clip.GetNumDescriptionAttributes( this );


    /// <summary>
    ///The retention class name associated with this clip.
    ///
    /// </summary>
    public string RetentionClassName
    {
      get
      {
        byte[] outString;
        FPInt bufSize = 0;
        FPInt len;

        do
        {
          bufSize += FPMisc.STRING_BUFFER_SIZE;
          len = bufSize;
          outString = new byte[(int) bufSize];
          Native.Clip.GetRetentionClassName( this, ref outString, ref len );
        } while( len > bufSize );

        return Encoding.UTF8.GetString( outString, 0, (int) len - 1 );
      }

      set
      {
        var rcRef = this.thePool.RetentionClasses.GetClass( value );
        Native.Clip.SetRetentionClass( this, rcRef );
      }
    }

    /// <summary>
    ///The retention class associated with this clip.
    ///
    /// </summary>
    public FPRetentionClass FPRetentionClass
    {
      get
      {
        var rcRef = this.thePool.RetentionClasses.GetClass( this.RetentionClassName );

        return rcRef;
      }
      set => Native.Clip.SetRetentionClass( this, value );
    }

    /// <summary>
    ///Remove any RetentionClass associated with the Clip. See API Guide: FPClip_RemoveRetentionClass
    ///
    /// </summary>
    public void RemoveRetentionClass()
    {
      Native.Clip.RemoveRetentionClass( this );
    }


    /// <summary>
    ///Validates that the Retention Class set on the Clip exists in the RetentionClassList supplied.
    ///See API Guide: FPClip_ValidateRetentionClass
    ///
    ///@return Boolean indicating Valid or Invalid Retention Class.
    /// </summary>
    public bool ValidateRetentionClass( FPRetentionClassCollection coll ) => coll.ValidateClass( this.Name );

    /// <summary>
    ///Validates that the Retention Class set on the Clip exists in the Pool for this Clip.
    ///
    ///@return Boolean indicating Valid or Invalid Retention Class.
    /// </summary>
    public bool ValidateRetentionClass() => Native.Clip.ValidateRetentionClass( Native.Pool.GetRetentionClassContext( this.thePool ), this ) == FPBool.True;

    /// <summary>
    ///Returns the (non-ASCII based) Canonical form of a Clip ID. Used for translating Clip IDs between platforms that use
    ///different character sets. See API Guide: FPClip_GetCanonicalForm
    ///
    ///@param inClipID The ClipID to convert to Canonical Form.
    ///@param bufSize The size of the buffer to allocate to hold the Canonical Form.
    ///@return The Canonical Form of the clip.
    /// </summary>
    public static byte[] GetCanonicalFormat( string inClipID, int bufSize )
    {
      var outClipID = new byte[bufSize];

      Native.Clip.GetCanonicalFormat( inClipID, outClipID );

      return outClipID;
    }

    /// <summary>
    ///Returns the (non-ASCII based) Canonical form of the ID of this Clip. Used for translating Clip IDs between platforms that use
    ///different character sets. See API Guide: FPClip_GetCanonicalForm
    ///
    ///@param bufSize The size of the buffer to allocate to hold the Canonical Form.
    ///@return The Canonical Form of the clip.
    /// </summary>
    public byte[] GetCanonicalFormat( int bufSize )
    {
      var outClipID = new byte[bufSize];

      Native.Clip.GetCanonicalFormat( this.ClipID, outClipID );

      return outClipID;
    }

    /// <summary>
    ///Returns the (non-ASCII based) Canonical form of a Clip ID. Used for translating Clip IDs between
    ///platforms that use different character sets. Buffer of FPMisc.STRING_BUFFER_SIZE is used.
    ///See API Guide: FPClip_GetCanonicalForm
    ///
    ///@param inClipID A standard string format Clip ID.
    ///@return The Canonical Form of the clip.
    /// </summary>
    public static byte[] GetCanonicalFormat( string inClipID ) => FPClip.GetCanonicalFormat( inClipID, FPMisc.STRING_BUFFER_SIZE );

    /// <summary>
    ///The (non-ASCII based) Canonical form of the ClipID of this Clip. Used for translating Clip IDs between
    ///platforms that use different character sets. Buffer of FPMisc.STRING_BUFFER_SIZE is used.
    ///See API Guide: FPClip_GetCanonicalForm
    ///
    /// </summary>
    public byte[] CanonicalForm => FPClip.GetCanonicalFormat( this.ClipID, FPMisc.STRING_BUFFER_SIZE );

    /// <summary>
    ///Get the standard (ASCII based) form of a Clip ID in Canonical Form. Used for translating Clip IDs between
    ///platforms that use different character sets. Buffer of FPMisc.STRING_BUFFER_SIZE is used.
    ///See API Guide: FPClip_GetStringForm
    ///
    ///@param inClipID A Clip ID in Canonical Form.
    ///@return The Canonical Form of the clip.
    /// </summary>
    public static string GetStringFormat( byte[] inClipID )
    {
      var clipString = new StringBuilder( FPMisc.STRING_BUFFER_SIZE );

      Native.Clip.GetStringFormat( inClipID, clipString );

      return clipString.ToString();
    }


    private FPTagCollection myTags;

    /// <summary>
    ///An ArrayList containing the Tag objects on a Clip. This should ONLY be used for reading of the Tags
    ///on a finalized clip - modification and creation are not supported.
    /// </summary>
    public FPTagCollection Tags => this.myTags ?? (this.myTags = new FPTagCollection( this ));

    private FPAttributeCollection myAttributes;

    /// <summary>
    ///An ArrayList containing the DescriptionAttribute objects on a Clip. This should ONLY be used for reading of the Tags
    ///on a finalized clip - modification and creation are not supported.
    /// </summary>
    public FPAttributeCollection Attributes
    {
      get
      {
        if( this.myAttributes == null ) this.myAttributes = new FPAttributeCollection( this );

        return this.myAttributes;
      }
    }

    public bool OnHold => Native.Clip.GetRetentionHold( this ) == FPBool.True;

    public void SetRetentionHold( bool holdState, string holdID ) =>
      Native.Clip.SetRetentionHold( this, holdState ? FPBool.True : FPBool.False, holdID );

    public TimeSpan EBRPeriod
    {
      get
      {
        var period = (int) Native.Clip.GetEBRPeriod( this );

        return period < 0 ? new TimeSpan( period ) : new TimeSpan( 0, 0, period );
      }

      set
      {
        if( value.Ticks > 0 )
        {
          Native.Clip.EnableEBRWithPeriod( this, (FPLong) value.TotalSeconds );
        }
        else
        {
          Native.Clip.EnableEBRWithPeriod( this, (FPLong) value.Ticks );
        }
      }
    }

    public string EBRClassName => Native.Clip.GetEBRClassName( this );

    public FPRetentionClass EBRClass
    {
      set => Native.Clip.EnableEBRWithClass( this, value );
    }

    public bool EBREnabled => Native.Clip.IsEBREnabled( this ) == FPBool.True;

    public void TriggerEBREvent() => Native.Clip.TriggerEBREvent( this );

    public TimeSpan TriggerEBRPeriod
    {
      set
      {
        if( value.Ticks != FPMisc.INFINITE_RETENTION_PERIOD )
        {
          Native.Clip.TriggerEBREventWithPeriod( this, (FPLong) value.TotalSeconds );
        }
        else
        {
          Native.Clip.TriggerEBREventWithPeriod( this, (FPLong) value.Ticks );
        }
      }
    }

    public FPRetentionClass TriggerEBRClass
    {
      set => Native.Clip.TriggerEBREventWithClass( this, value );
    }
  }  // end of class Clip
}
