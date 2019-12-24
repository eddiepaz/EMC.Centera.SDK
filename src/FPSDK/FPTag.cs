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
  ///An object representing an FPTagRef.
  ///@author Graham Stuart
  ///@version
  /// </summary>
  public class FPTag : FPObject, IFPTag
  {
    internal FPTagRef theTag;

    /// <summary>
    ///Implicit conversion of FPTag to FPTagRef
    ///
    ///@param t An FPTag object.
    ///@return The FPTagRef associated with the FPTag.
    /// </summary>
    public static implicit operator FPTagRef( FPTag t ) => t.theTag;

    /// <summary>
    ///Implicit conversion of FPTagRef to FPTag.
    ///
    ///@param tagRef  An FPTagRef.
    ///@return The FPTag object associated with the FPTagRef.
    /// </summary>
    public static implicit operator FPTag( FPTagRef tagRef )
    {
      // Find the relevant Tag object in the hashtable for this FPTagRef
      FPTag tagObject;

      if( FPObject.SDKObjects.Contains( tagRef ) )
      {
        tagObject = (FPTag) FPObject.SDKObjects[tagRef];
      }
      else
      {
        throw new FPLibraryException( "FPTagRef is not associated with an FPTag object", FPMisc.WRONG_REFERENCE_ERR );
      }


      return tagObject;
    }

    /// <summary>
    ///Create a new Tag. See API Guide: FPTag_Create
    ///
    ///@param inParent  The parent Tag for the new Tag.
    ///@param inName  The name of the new Tag.
    /// </summary>
    public FPTag( FPTag inParent, string inName )
    {
      this.theTag = Native.Tag.Create( inParent, inName );
      this.AddObject( this.theTag, this );
    }

    /// <summary>
    ///Create a new Tag using an existing FPTagRef. See API Guide: FPTag_Create
    ///
    ///@param t Existing FPTagRef.
    /// </summary>
    internal FPTag( FPTagRef t )
    {
      this.theTag = t;
      this.AddObject( this.theTag, this );
    }

    /// <summary>
    ///Explicitly Close the FPTagRef. See API Guide: FPTag_Close
    ///
    /// </summary>
    public override void Close()
    {
      if( this.theTag != 0 )
      {
        this.RemoveObject( this.theTag );
        try
        {
          Native.Tag.Close( this.theTag );
        }
        catch( FPLibraryException e )
        {
          FPLogger.ConsoleMessage( "\nProblem closing tag " + e );
        }
        catch( NullReferenceException )
        {
          FPLogger.ConsoleMessage( "\nNull reference" );
        }

        this.theTag = 0;
      }
    }

    /// <summary>
    ///Create a copy of this Tag with under a new parent Tag. See API Guide: FPTag_Copy
    ///
    ///@param inNewParent The parent tag to use for the new (copy) Tag.
    ///@param inOptions   Options for creating the copy Tag.
    ///@return The new Tag.
    /// </summary>
    public FPTag Copy( FPTag inNewParent, int inOptions ) => new FPTag( Native.Tag.Copy( this, inNewParent, (FPInt) inOptions ) );

    /// <summary>
    ///Get the Clip that this Tag is associated with. See API Guide: FPTag_GetClipRef
    ///
    /// </summary>
    public FPClip FPClip => Native.Tag.GetClipRef( this );

    /// <summary>
    ///The next sibling Tag that this Tag is associated with. The Clip must have been opened in TREE mode.
    ///See API Guide: FPTag_GetSibling
    ///
    /// </summary>
    public FPTag NextSibling
    {
      get
      {
        var tagRef = Native.Tag.GetSibling( this );

        return tagRef != 0 ? new FPTag( tagRef ) : null;
      }
    }

    /// <summary>
    ///The previous sibling Tag that this Tag is associated with. The Clip must have been opened in TREE mode.
    ///See API Guide: FPTag_GetPrevSibling
    /// 
    /// </summary>
    public FPTag PrevSibling
    {
      get
      {
        var tagRef = Native.Tag.GetPrevSibling( this );

        return tagRef != 0 ? new FPTag( tagRef ) : null;
      }
    }

    /// <summary>
    ///The first child Tag that this Tag is associated with. The Clip must have been opened in TREE mode.
    ///See API Guide: FPTag_GetFirstChild
    /// 
    /// </summary>
    public FPTag FirstChild
    {
      get
      {
        var tagRef = Native.Tag.GetFirstChild( this );

        return tagRef != 0 ? new FPTag( tagRef ) : null;
      }
    }

    /// <summary>
    ///The parent Tag of this Tag. The Clip must have been opened in TREE mode.
    ///See API Guide: FPTag_GetParent
    /// 
    /// </summary>
    public FPTag Parent
    {
      get
      {
        var tagRef = Native.Tag.GetParent( this );

        return tagRef != 0 ? (FPTag) Native.Tag.GetParent( this ) : null;
      }
    }

    /// <summary>
    ///Remove this Tag (and all its children) from the Clip it is associated with. All the Tags are Disposed.
    ///The Clip must have been opened in TREE mode. See API Guide: FPTag_Delete
    ///
    /// </summary>
    public void Delete()
    {
      Native.Tag.Delete( this );
      this.RemoveObject( this.theTag );
      this.theTag = 0;
      this.Dispose();
    }

    /// <summary>
    ///The name of the Tag. Default FPMisc.STRING_BUFFER_SIZE is used for the buffer.
    ///See API Guide: FPTag_GetTagName
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

          Native.Tag.GetTagName( this, ref outString, ref len );
        } while( len > bufSize );

        return Encoding.UTF8.GetString( outString, 0, (int) len - 1 );
      }
    }

    /// <summary>
    ///Get a string representation of this Tag - the Tag name.
    ///
    ///@return The string representation of this object.
    /// </summary>
    public override string ToString() => this.Name;

    /// <summary>
    ///Set a string attribute on this Tag. See API Guide: FPTag_SetStringAttribute
    ///
    ///@param inAttrName  The Attribute Name to be set,
    ///@param inAttrValue The Value to be set.
    /// </summary>
    public void SetAttribute( string inAttrName, string inAttrValue )
    {
      Native.Tag.SetStringAttribute( this, inAttrName, inAttrValue );
    }

    /// <summary>
    ///Set a Long attribute on this Tag. See API Guide: FPTag_SetLongAttribute
    ///
    ///@param inAttrName  The Attribute Name to be set,
    ///@param inAttrValue The Value to be set.
    /// </summary>
    public void SetAttribute( string inAttrName, long inAttrValue )
    {
      Native.Tag.SetLongAttribute( this, inAttrName, (FPLong) inAttrValue );
    }

    /// <summary>
    ///Set a Boolean attribute on this Tag. See API Guide: FPTag_SetBooleanAttribute
    ///
    ///@param inAttrName  The Attribute Name to be set,
    ///@param inAttrValue The Value to be set.
    /// </summary>
    public void SetAttribute( string inAttrName, bool inAttrValue )
    {
      Native.Tag.SetBoolAttribute( this, inAttrName,
                                   inAttrValue ? FPBool.True : FPBool.False );
    }

    /// <summary>
    ///Get the value of a string attribute from this Tag. See API Guide: FPTag_GetStringAttribute
    ///
    ///@param inAttrName  The Attribute Name to be retrieved,
    ///@return  The string value of the attribute.
    /// </summary>
    public string GetStringAttribute( string inAttrName )
    {
      byte[] outString;
      FPInt bufSize = 0;
      FPInt len;

      do
      {
        bufSize += FPMisc.STRING_BUFFER_SIZE;
        len = bufSize;
        outString = new byte[(int) bufSize];

        Native.Tag.GetStringAttribute( this, inAttrName, ref outString, ref len );
      } while( len > bufSize );

      return Encoding.UTF8.GetString( outString, 0, (int) len - 1 );
    }


    /// <summary>
    ///Get the value of a Long attribute from this Tag. See API Guide: FPTag_GetLongAttribute
    ///
    ///@param inAttrName  The Attribute Name to be retrieved,
    ///@return  The Long value of the attribute.
    /// </summary>
    public long GetLongAttribute( string inAttrName ) =>
      (long) Native.Tag.GetLongAttribute( this, inAttrName );

    /// <summary>
    ///Get the value of a Boolean attribute from this Tag. See API Guide: FPTag_GetBoolAttribute
    ///
    ///@param inAttrName  The Attribute Name to be retrieved,
    ///@return  The Boolean value of the attribute.
    /// </summary>
    public bool GetBoolAttribute( string inAttrName ) =>
      Native.Tag.GetBoolAttribute( this, inAttrName ) == FPBool.True;

    /// <summary>
    ///Remove an attribute from this Tag. See API Guide: FPTag_RemoveAttribute
    ///
    ///@param inAttrName  The Attribute Name to be retrieved,
    /// </summary>
    public void RemoveAttribute( string inAttrName )
    {
      Native.Tag.RemoveAttribute( this, inAttrName );
    }

    /// <summary>
    ///The number of attributes on this Tag. See API Guide: FPTag_GetNumAttributes
    ///
    /// </summary>
    public int NumAttributes => (int) Native.Tag.GetNumAttributes( this );

    /// <summary>
    ///Returns an attribute name and value from this Tag using the given index number.
    ///See API Guide: FPTag_GetIndexAttribute
    ///
    ///@param inIndex   The index of the attribute to retrieve.
    ///@return  An FPAttribute containing the name-value string pair.
    /// </summary>
    public FPAttribute GetAttributeByIndex( int inIndex )
    {
      byte[] nameString;
      byte[] valString;
      FPInt valLen;
      FPInt nameSize = 0, valSize = 0;
      FPInt nameLen;

      do
      {
        nameSize += FPMisc.STRING_BUFFER_SIZE;
        nameLen = nameSize;
        nameString = new byte[(int) nameSize];

        valSize += FPMisc.STRING_BUFFER_SIZE;
        valLen = valSize;
        valString = new byte[(int) valSize];

        Native.Tag.GetIndexAttribute( this, (FPInt) inIndex, ref nameString, ref nameLen, ref valString, ref valLen );
      } while( nameLen > nameSize || valLen > valSize );

      return new FPAttribute( Encoding.UTF8.GetString( nameString, 0, (int) nameLen - 1 ), Encoding.UTF8.GetString( valString, 0, (int) valLen - 1 ) );

    }

    /// <summary>
    ///The size of the blob on this Tag. See API Guide: FPTag_GetBlobSize
    ///
    /// </summary>
    public long BlobSize => (long) Native.Tag.GetBlobSize( this );

    /// <summary>
    ///Write the contents of a Stream to the Blob on this Tag using DEFAULT_OPTIONS.
    ///See API Guide: FPTag_BlobWrite
    ///
    ///@param inStream  The Stream from which to read the blob that is to be written to the Tag.
    /// </summary>
    public void BlobWrite( FPStream inStream )
    {
      this.BlobWrite( inStream, FPMisc.OPTION_DEFAULT_OPTIONS );
    }

    /// <summary>
    ///Write the contents of a Stream to the Blob on this Tag.
    ///See API Guide: FPTag_BlobWrite
    ///
    ///@param inStream  The Stream from which to read the blob that is to be written to the Tag.
    ///@param inOptions The options available on the write.
    /// </summary>
    public void BlobWrite( FPStream inStream, long inOptions )
    {
      Native.Tag.BlobWrite( this, inStream, (FPLong) inOptions );
    }

    /// <summary>
    ///Read a portion of the Blob on this Tag to a Stream using DEFAULT_OPTIONS.
    ///See API Guide: FPTag_BlobReadPartial
    ///
    ///@param inStream    The Stream to write the Blob to.
    ///@param inSequenceID  The sequenceID of this fragment of the blob.
    /// </summary>
    public void BlobWritePartial( FPStream inStream, long inSequenceID )
    {
      this.BlobWritePartial( inStream, inSequenceID, FPMisc.OPTION_DEFAULT_OPTIONS );
    }

    /// <summary>
    ///Read a portion of the Blob on this Tag to a Stream. See API Guide: FPTag_BlobReadPartial
    ///
    ///@param inStream    The Stream to write the Blob to.
    ///@param inSequenceID  The sequenceID of this fragment of the blob.
    ///@param inOptions   The options available on the read.
    /// </summary>
    public void BlobWritePartial( FPStream inStream, long inSequenceID, long inOptions )
    {
      Native.Tag.BlobWritePartial( this, inStream, (FPLong) inOptions, (FPLong) inSequenceID );
    }

    /// <summary>
    ///Read the Blob on this Tag to a Stream using DEFAULT_OPTIONS.
    ///See API Guide: FPTag_BlobRead
    ///
    ///@param inStream  The Stream to write the Blob to.
    /// </summary>
    public void BlobRead( FPStream inStream )
    {
      this.BlobRead( inStream, FPMisc.OPTION_DEFAULT_OPTIONS );
    }

    /// <summary>
    ///Read the Blob on this Tag to a Stream.
    ///See API Guide: FPTag_BlobRead
    ///
    ///@param inStream  The Stream to write the Blob to.
    ///@param inOptions The options available on the read.
    /// </summary>
    public void BlobRead( FPStream inStream, long inOptions )
    {
      Native.Tag.BlobRead( this, inStream, (FPLong) inOptions );
    }

    /// <summary>
    ///Read a portion of the Blob on this Tag to a Stream using DEFAULT_OPTIONS.
    ///See API Guide: FPTag_BlobReadPartial
    ///
    ///@param inStream    The Stream to write the Blob to.
    ///@param inOffset    The offset position in the blob from where the read is to start.
    ///@param inReadLength  The offset position in the blob from where the read is to start.
    /// </summary>
    public void BlobReadPartial( FPStream inStream, long inOffset, long inReadLength )
    {
      this.BlobReadPartial( inStream, inOffset, inReadLength, FPMisc.OPTION_DEFAULT_OPTIONS );
    }

    /// <summary>
    ///Read a portion of the Blob on this Tag to a Stream. See API Guide: FPTag_BlobReadPartial
    ///
    ///@param inStream    The Stream to write the Blob to.
    ///@param inOffset    The offset position in the blob from where the read is to start.
    ///@param inReadLength  The offset position in the blob from where the read is to start.
    ///@param inOptions   The options available on the read.
    /// </summary>
    public void BlobReadPartial( FPStream inStream, long inOffset, long inReadLength, long inOptions )
    {
      Native.Tag.BlobReadPartial( this, inStream, (FPLong) inOffset, (FPLong) inReadLength, (FPLong) inOptions );
    }

    /// <summary>
    ///The status of the Blob on this Tag: 1 for exists and is OK, 0 for exists but not available,
    ///-1 for no blob. See API Guide: FPTag_BlobExists
    ///
    /// </summary>
    public int BlobStatus => (int) Native.Tag.BlobExists( this );

    private FPAttributeCollection myAttributes;

    /// <summary>
    ///An ArrayList containing the Attributes on the Tag. This should ONLY be used for reading of the Attributes
    ///on a finalized Tag - modification and creation are not supported.
    ///
    /// </summary>
    public FPAttributeCollection Attributes =>
      this.myAttributes = this.myAttributes ?? new FPAttributeCollection( this );
  }
}