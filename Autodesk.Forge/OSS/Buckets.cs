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
using Newtonsoft.Json.Converters;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Autodesk.Forge.OSS
{
  public enum PolicyKey
  {
    Transient,
    Temporary,
    Persistent
  }

  public enum Region
  {
    US,
    EMEA
  }

  public class AppBuckets : ApiObject
  {
    public AppBuckets(OAuth.OAuth oauth) : base(oauth)
    {
      
    }

    private class BucketsResponse
    {
      [JsonProperty("items")]
      public IList<Bucket> Items { get; set; }
    }

    private List<Bucket> _listOfBuckets = new List<Bucket>();

    /// <summary>
    /// Returns an enumerable list of buckets for this Forge app (Client ID & Secret). 
    /// Requires bucket:read scope
    /// </summary>
    /// <param name="oauth">Authorization object with bucket:read scope</param>
    /// <param name="limit">Number of buckets to return. Underlying endpoint returns 100 at a time, if a higher limit is passed, this method will recursively call the endpoint until limit or number of buckets</param>
    /// <param name="region">Filter buckets by region</param>
    /// <param name="startAt">Start listing buckets at the specified bucketKey, in case of pagination</param>
    /// <returns></returns>
    public async Task<IEnumerable<Bucket>> GetBucketsAsync(int limit = 10, Region region = Region.US, string startAt = "")
    {
      while (limit > 0)
      {
        Dictionary<string, string> parameters = new Dictionary<string, string>();
        parameters.Add("limit", (limit>100 ? 100 : limit).ToString());
        parameters.Add("region", Enum.GetName(typeof(Region), region));
        if (!string.IsNullOrEmpty(startAt)) parameters.Add("startAt", startAt);
        
        IRestResponse res = await REST.MakeAuthorizedRequestAsync(Authorization, Internal.END_POINTS.GET_BUCKETS, RestSharp.Method.GET, null, parameters);
        if (res.StatusCode != System.Net.HttpStatusCode.OK)
          throw new Exception(string.Format("Endpoint {0} at Buckets.GetBucketsAsync returned {1}", Internal.END_POINTS.GET_BUCKETS, res.StatusCode));

        BucketsResponse response = JsonConvert.DeserializeObject<BucketsResponse>(res.Content);
        _listOfBuckets.AddRange(response.Items);

        startAt = (response.Items.Count > 0 ? response.Items.Last<Bucket>().BucketKey : string.Empty);
        limit -= (response.Items.Count < 100 ? limit : 100);
      }

      foreach (Bucket bucket in _listOfBuckets)
        bucket.Authorization = Authorization;

      return _listOfBuckets;
    }

    /// <summary>
    /// Check if the specified bucketKey exists. 
    /// This method may refresh all buckets from server
    /// Requires bucket:read scope
    /// </summary>
    /// <param name="bucketKey"></param>
    /// <returns></returns>
    public async Task<bool> ContainsAsync(string bucketKey)
    {
      if (_listOfBuckets.Count == 0) await GetBucketsAsync(int.MaxValue); // get all buckets
      foreach (Bucket b in _listOfBuckets)
        if (b.BucketKey.Equals(bucketKey))
          return true;
      return false;
    }

    /// <summary>
    /// Create a new bucket with the specified parameters. 
    /// Requires bucket:create scope
    /// </summary>
    /// <param name="bucketKey"></param>
    /// <param name="policyKey"></param>
    /// <param name="region"></param>
    /// <returns></returns>
    public async Task<BucketDetails> CreateBucketAsync(string bucketKey, PolicyKey policyKey, Region region = Region.US)
    {
      Dictionary<string, string> headers = new Dictionary<string, string>();
      headers.AddHeader(PredefinedHeadersExtension.PredefinedHeaders.ContentTypeJson);
      headers.Add("x-ads-region", Enum.GetName(typeof(Region), region));

      Dictionary<string, string> body = new Dictionary<string, string>();
      body.Add("bucketKey", bucketKey);
      body.Add("policyKey", Enum.GetName(typeof(PolicyKey), policyKey));

      IRestResponse res = await REST.MakeAuthorizedRequestAsync(Authorization, END_POINTS.POST_BUCKETS, Method.POST, headers, null, body);

      if (res.StatusCode != System.Net.HttpStatusCode.OK || res.StatusCode!= System.Net.HttpStatusCode.Conflict)
        throw new Exception(string.Format("Endpoint {0} at Buckets.CreateBucket returned {1}", Internal.END_POINTS.GET_BUCKETS, res.StatusCode));

      BucketDetails newBucket = new BucketDetails(Authorization);
      JsonConvert.PopulateObject(res.Content, newBucket);

      return newBucket;
    }
  }

  public class Bucket : Internal.ApiObject
  {
    /// <summary>
    /// Check if the provided bucketKey is valid (lower case letters, numbers and dash)
    /// </summary>
    /// <param name="bucketKey"></param>
    /// <returns></returns>
    public static bool IsValidBucketKey(string bucketKey)
    {
      Regex r = new Regex(@"^[a-z0-9\-]+$", RegexOptions.IgnorePatternWhitespace);
      return r.IsMatch(bucketKey);
    }

    internal Bucket() : base(null) { }
    protected Bucket(OAuth.OAuth oauth) : base(oauth) { }

    /// <summary>
    /// Instantiate a skeleton object, consider using BucketDetails.InitializeAsync before accessing any property.
    /// </summary>
    /// <param name="oauth"></param>
    /// <param name="bucketKey"></param>
    public Bucket(OAuth.OAuth oauth, string bucketKey) : base(oauth)
    {
      BucketKey = bucketKey;
    }

    [JsonProperty("bucketKey")]
    public string BucketKey { get; internal set; }

    /// <summary>
    /// Timestamp in epoch time
    /// </summary>
    [JsonProperty("createdDate")]
    public long CreatedTimestamp { get; internal set; }

    [JsonIgnore]
    public DateTime CreatedDate
    {
      get
      {
        DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
        return dtDateTime.AddMilliseconds(CreatedTimestamp);
      }
    }

    /// <summary>
    /// Policy values: transient, temporary, persistent
    /// </summary>
    [JsonProperty("policyKey")]
    [JsonConverter(typeof(StringEnumConverter))]
    public PolicyKey PolicyKey { get; internal set; }

    private class ObjectsResponse
    {
      [JsonProperty("items")]
      public IList<Object> Items { get; set; }

      [JsonProperty("next")]
      public string Next { get; internal set; }
    }

    private List<Object> _listOfObjects = new List<Object>();

    /// <summary>
    /// Return a enumerable list of objects on this bucket. Requires data:read scope
    /// </summary>
    /// <param name="limit">Number of buckets to return. Underlying endpoint returns 100 at a time, if a higher limit is passed, this method will recursively call the endpoint until limit or number of buckets</param>
    /// <param name="startAt"></param>
    /// <returns></returns>
    public async Task<IEnumerable<Object>> GetObjectsAsync(int limit = 10, string startAt = "")
    {
      while (limit > 0)
      {
        Dictionary<string, string> parameters = new Dictionary<string, string>();
        parameters.Add("limit", (limit > 100 ? 100 : limit).ToString());
        if (!string.IsNullOrEmpty(startAt)) parameters.Add("startAt", startAt);

        IRestResponse res = await REST.MakeAuthorizedRequestAsync(Authorization, string.Format(END_POINTS.GET_BUCKET_OBJECTS, BucketKey), RestSharp.Method.GET, null, parameters);
        if (res.StatusCode != System.Net.HttpStatusCode.OK)
          throw new Exception(string.Format("Endpoint {0} at Buckets.GetObjectsAsync returned {1}", Internal.END_POINTS.GET_BUCKET_OBJECTS, res.StatusCode));

        ObjectsResponse response = JsonConvert.DeserializeObject<ObjectsResponse>(res.Content);
        _listOfObjects.AddRange(response.Items);

        startAt = (response.Items.Count > 0 ? response.Items.Last<Object>().ObjectKey : string.Empty);
        limit -= (response.Items.Count < 100 ? limit : 100);
      }

      foreach (Object obj in _listOfObjects)
        obj.Authorization = Authorization;

      return _listOfObjects;
    }
  }

  public class BucketDetails : Bucket
  {
    internal BucketDetails(OAuth.OAuth oauth) : base(oauth) { }

    [JsonProperty("bucketOwner")]
    public string BucketOwner { get; internal set; }

    public class BucketPermissions
    {
      [JsonProperty("authId")]
      public string AuthId { get; internal set; }

      [JsonProperty("acess")]
      public string Access { get; internal set; }
    }

    [JsonProperty("permissions")]
    public IList<BucketPermissions> Permissions { get; set; }

    /// <summary>
    /// Requires bucket:read scope
    /// </summary>
    /// <param name="oauth"></param>
    /// <param name="bucketKey"></param>
    /// <returns></returns>
    public async static Task<BucketDetails> InitializeAsync(OAuth.OAuth oauth, string bucketKey)
    {
      if (!IsValidBucketKey(bucketKey))
        throw new Exception("BucketKey not valid: Possible values: -_.a-z0-9 (between 3-128 characters in length)");

      IRestResponse res = await REST.MakeAuthorizedRequestAsync(oauth, string.Format(END_POINTS.GET_BUCKET_DETAILS, bucketKey), RestSharp.Method.GET);
      if (res.StatusCode != System.Net.HttpStatusCode.OK)
        throw new Exception(string.Format("Endpoint {0} at BucketDetails.InitializeAsync returned {1}", Internal.END_POINTS.GET_BUCKET_DETAILS, res.StatusCode));

      BucketDetails bucket = new OSS.BucketDetails(oauth);
      JsonConvert.PopulateObject(res.Content, bucket);
      return bucket;
    }
  }
}
