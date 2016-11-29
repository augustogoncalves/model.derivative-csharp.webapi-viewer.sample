/////////////////////////////////////////////////////////////////////
// Copyright (c) Autodesk, Inc. All rights reserved
// Written by Forge Partner Development
//
// Permission to use, copy, modify, and distribute this software in
// object code form for any purpose and without fee is hereby granted,
// provided that the above copyright notice appears in all copies and
// that both that copyright notice and the limited warranty and
// restricted rights notice below appear in all supporting
// documentation.
//
// AUTODESK PROVIDES THIS PROGRAM "AS IS" AND WITH ALL FAULTS.
// AUTODESK SPECIFICALLY DISCLAIMS ANY IMPLIED WARRANTY OF
// MERCHANTABILITY OR FITNESS FOR A PARTICULAR USE.  AUTODESK, INC.
// DOES NOT WARRANT THAT THE OPERATION OF THE PROGRAM WILL BE
// UNINTERRUPTED OR ERROR FREE.
/////////////////////////////////////////////////////////////////////

using Autodesk.Forge.Internal;
using Newtonsoft.Json;

namespace Autodesk.Forge.OSS
{
  public class Object : ApiObject
  {
    internal Object() : base(null) { }
    internal Object(OAuth.OAuth auth) : base(auth) { }

    /// <summary>
    /// Bucket Key where this object resides
    /// </summary>
    [JsonProperty("bucketKey")]
    public string BucketKey { get; internal set; }

    /// <summary>
    /// Object name
    /// </summary>
    [JsonProperty("objectKey")]
    public string ObjectKey { get; internal set; }

    /// <summary>
    /// Object URN
    /// </summary>
    [JsonProperty("objectId")]
    public string ObjectId { get; internal set; }

    /// <summary>
    /// Object URN Base 64 Encoded
    /// </summary>
    [JsonProperty("objectIdBase64")]
    public string ObjectIdBase64
    {
      get
      {
        return ObjectId.Base64Encode();
      }
    }

    /// <summary>
    /// Checksum SHA1 integrity verifier
    /// </summary>
    [JsonProperty("sha1")]
    public string SHA1 { get; internal set; }

    /// <summary>
    /// Object size (in Bytes)
    /// </summary>
    [JsonProperty("size")]
    public int Size { get; internal set; }

    /// <summary>
    /// URL to download the object. A data:read scope is required to download the file. This property does not download the file, just return the URL.
    /// </summary>
    [JsonProperty("localization")]
    public string Localization { get; internal set; }
  }
}
