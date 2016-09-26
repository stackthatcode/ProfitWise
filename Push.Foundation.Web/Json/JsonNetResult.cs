using System;
using System.Text;
using System.Web.Mvc;
using Newtonsoft.Json;

namespace Push.Foundation.Web.Json
{
    public class JsonNetResult : ActionResult
    {
        public Encoding ContentEncoding { get; set; }
        public string ContentType { get; set; }
        public object Data { get; set; }


        public JsonNetResult(object data) : base()
        {
            this.Data = data;
        }

        public static JsonNetResult Success()
        {
            return new JsonNetResult(new { Success = true });
        }


        public override void ExecuteResult(ControllerContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }

            var response = context.HttpContext.Response;
            response.ContentType = !string.IsNullOrEmpty(ContentType) ? ContentType : "application/json";

            if (ContentEncoding != null)
            {
                response.ContentEncoding = ContentEncoding;
            }

            Data?.SerializeToJson(response.Output);
        }
    }
}