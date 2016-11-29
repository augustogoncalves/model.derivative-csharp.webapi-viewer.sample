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

using Autodesk.Forge.OSS;
using Autodesk.Forge.OAuth;
using System.Collections.Generic;
using System.Web.Http;
using System.Threading.Tasks;

namespace WebAPISample.Controllers
{
  // ********
  // This controller is not yet used on this project, just to demonstrate some features.
  // ********


  public class BucketsController : ApiController
  {
    private async Task<OAuth> GetOAuth(Scope[] scope)
    {
      OAuth oauth = await OAuth2LeggedToken.AuthenticateAsync(
        Config.FORGE_CLIENT_ID, Config.FORGE_CLIENT_SECRET,
        (scope == null ? Config.FORGE_SCOPE_PUBLIC : scope));
      return oauth;
    }

    [HttpGet]
    [Route("api/forge/buckets")]
    public async Task<IEnumerable<Bucket>> GetBuckets([FromUri]int limit = 100, [FromUri]Region region = Region.US, [FromUri]string startAt = "")
    {
      OAuth oauth = await GetOAuth(new Scope[] { Scope.BucketRead });
      AppBuckets buckets = new AppBuckets(oauth);
      return await buckets.GetBucketsAsync(limit, region, startAt);
    }

    [HttpGet]
    [Route("api/forge/buckets/{bucketKey}/details")]
    public async Task<BucketDetails> GetBucket(string bucketKey)
    {
      OAuth oauth = await GetOAuth(new Scope[] { Scope.BucketRead });
      BucketDetails bucket = await BucketDetails.InitializeAsync(oauth, bucketKey);
      return bucket;
    }

    [HttpGet]
    [Route("api/forge/buckets/{bucketKey}/objects")]
    public async Task<IEnumerable<Autodesk.Forge.OSS.Object>> GetObjects(string bucketKey, [FromUri]int limit = 100, [FromUri]string startAt = "")
    {
      OAuth oauth = await GetOAuth(new Scope[] { Scope.DataRead });
      Bucket bucket = new Bucket(oauth, bucketKey);
      return await bucket.GetObjectsAsync(limit, startAt);
    }
  }
}