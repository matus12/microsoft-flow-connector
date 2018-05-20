using KenticoCloud.Delivery;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web.Http;
using Contracts.Models;
using Contracts.Services;

namespace Flow.Controllers
{
    public class FlowController : ApiController
    {
        private readonly IFlowInfoService _flowInfoService;

        public FlowController(IFlowInfoService flowInfoService)
        {
            _flowInfoService = flowInfoService;
        }

        [Route("api/v1/sample/{projectId}/{codename}")]
        public async Task<HttpResponseMessage> Get(string projectId, string codename)
        {
            var content = await GetDataForPublish(projectId, codename);
            content = content.Replace("\n", "");
            content = content.Replace("&nbsp", " ");
            var response = Request.CreateResponse(HttpStatusCode.OK);
            response.Content = new StringContent(
                content,
                Encoding.UTF8,
                "application/json"
            );

            return response;
        }

        [HttpPost]
        [Route("api/v1/webhook")]
        public async Task<IHttpActionResult> Post([FromBody] Webhook webhook)
        {
            var projectId = webhook.Message.ProjectId.ToString();
            var codename = webhook.Data.Items[0].Codename;
            var targetUrl = _flowInfoService.GetUrl(
                projectId,
                codename
            );

            if (webhook.Message.Operation != "publish" 
                && (webhook.Message.Type != "content_item_variant" || webhook.Message.Type != "content_item") 
                || targetUrl == null)
            {
                return await Task.FromResult(StatusCode(HttpStatusCode.NoContent));
            }

            var str = await GetDataForPublish(projectId, codename);

            var httpClient = new HttpClient();

            var content = new StringContent(
                str,
                Encoding.UTF8,
                "application/json"
            );

            await httpClient.PostAsync(
                targetUrl,
                content);

            return await Task.FromResult(StatusCode(HttpStatusCode.OK));
        }

        [Route("api/v1/flow")]
        public async Task<IHttpActionResult> Post(FlowData flowData)
        {
            _flowInfoService.AddFlowInfo(flowData);

            return await Task.FromResult(StatusCode(HttpStatusCode.Accepted));
        }

        private static async Task<string> GetDataForPublish(string projectId, string codename)
        {
            var deliveryClient = new DeliveryClient(projectId);
            var response = await deliveryClient.GetItemAsync(codename);
            var elements = response.Item.Elements;

            var children = elements.Children();
            var str = new StringBuilder("{" + '\n');
            foreach (var result in children)
            {
                var type = result.First.type.ToString();
                var value = result.First.value.ToString();
                if (type.Equals("multiple_choice"))
                {
                    continue;
                }

                if (type.Equals("rich_text"))
                {
                    var strippedText = StripHtml(value);
                    str.Append('"' + result.Name + '"' + ":" + '"' + strippedText + '"');
                }

                else
                {
                    if (!value[0].ToString().Equals("["))
                    {
                        str.Append('"' + result.Name + '"' + ":" + '"' + value + '"');
                    }
                    else
                    {
                        str.Append('"' + result.Name + '"' + ":" + value);
                    }
                }

                str.Append(", \n");
            }

            str.Append("}");

            return str.ToString().Remove(str.Length-4, 2);
        }

        private static string StripHtml(string input)
        {
            return Regex.Replace(input, "<.*?>", string.Empty);
        }
    }
}