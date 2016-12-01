using Autodesk.Forge.ModelDerivative;
using Autodesk.Forge.OAuth;
using Autodesk.Forge.OSS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;

namespace WebAPISample.Controllers
{
  public class ModelDerivativeController : ApiController
  {
    public class TranslateObjectModel
    {
      public string bucketKey { get; set; }
      public string objectKey { get; set; }
    }

    private async Task<OAuth> GetOAuth(Scope[] scope)
    {
      OAuth oauth = await OAuth2LeggedToken.AuthenticateAsync(
        Config.FORGE_CLIENT_ID, Config.FORGE_CLIENT_SECRET,
        (scope == null ? Config.FORGE_SCOPE_PUBLIC : scope));
      return oauth;
    }

    [HttpPost]
    [Route("api/forge/modelderivative/translateObject")]
    public async Task<HttpStatusCode> TranslateObject([FromBody]TranslateObjectModel objModel)
    {
      Autodesk.Forge.OSS.Object obj = new Autodesk.Forge.OSS.Object(
        await GetOAuth(new Scope[] { Scope.DataRead, Scope.DataWrite, Scope.DataCreate }),
        objModel.bucketKey, objModel.objectKey);

      return await obj.Translate(new SVFOutput[] { SVFOutput.Views2d, SVFOutput.Views3d });
    }
  }
}