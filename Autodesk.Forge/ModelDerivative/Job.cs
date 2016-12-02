using Autodesk.Forge.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Forge.OAuth;
using RestSharp;
using System.Net;

namespace Autodesk.Forge.ModelDerivative
{
  public enum TranslationOutput
  {
    svf,
    stl,
    step,
    iges,
    obj,
    fbx,
    ifc,
    dwg
  }

  public enum SVFOutput
  {
    Views2d,
    Views3d
  }

  public class Job : ApiObject
  {
    private Job() : base(null) { }

    public static async Task<HttpStatusCode> PostJob(Autodesk.Forge.OAuth.OAuth oauth, PostJobModel parameters)
    {
      Dictionary<string, string> headers = new Dictionary<string, string>();
      headers.AddHeader(PredefinedHeadersExtension.PredefinedHeaders.ContentTypeJson);
      IRestResponse res = await REST.MakeAuthorizedRequestAsync(oauth, END_POINTS.POST_JOB, RestSharp.Method.POST, headers, null, parameters);
      return res.StatusCode;
    }
  }

  public class PostJobModel
  {
    public PostJobModel(string urn)
    {
      input = new ModelDerivative.PostJobModel.Input();
      input.urn = urn;
    }

    public PostJobModel(string urn, string rootFilename)
    {
      input = new ModelDerivative.PostJobModel.InputCompressed();
      input.urn = urn;
      ((InputCompressed)input).rootFilename = rootFilename;
      
    }

    public Input input;

    public class Input
    {
      public string urn;
    }

    public class InputCompressed : Input
    {
      public bool compressedUrn = true;
      public string rootFilename;
    }
  }

  public class PostSVFJobModel : PostJobModel
  {
    public PostSVFJobModel(string urn, SVFOutput[] svfOutput): base(urn)
    {
      output = new Output();
      output.formats = new List<Output.Formats>() { new Output.Formats() { views = new List<string>()} };

      foreach (SVFOutput svf in svfOutput)
        output.formats[0].views.Add(Enum.GetName(typeof(SVFOutput), svf).Replace("Views", string.Empty).ToLower());
    }

    public Output output; 

    public class Output
    {
      public List<Formats> formats;

      public class Formats
      {
        public string type = "svf";
        public List<string> views;
      }
    }
  }
}
