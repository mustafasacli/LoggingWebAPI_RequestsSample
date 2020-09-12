using LoggingWebAPI_RequestsSample.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http.Routing;

namespace LoggingWebAPI_RequestsSample.Handlers
{
    public class ApiLogHandler : DelegatingHandler
    {
        private readonly ApiLogDbContext db = new ApiLogDbContext();

        //http://arcware.net/logging-web-api-requests/
        /*
         * 
         * https://stackoverflow.com/questions/23825505/actionattributefilter-vs-delegatinghandler-advantages-disadvantages
         * 
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
{
  var log = new Log { Url = request.RequestUri };     
  var response = await base.SendAsync(request, cancellationToken);
  log.ContentLength = response.ContentLength;
  this.LogAsync(log);
  return response;
} 
        */
        /*
         https://stackoverflow.com/questions/23825505/actionattributefilter-vs-delegatinghandler-advantages-disadvantages
         https://stackoverflow.com/questions/11123015/when-to-use-httpmessagehandler-vs-actionfilter
         https://blogs.msdn.microsoft.com/kiranchalla/2012/05/05/asp-net-mvc4-web-api-stack-diagram/
             */
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var apiLogEntry = CreateApiLogEntryWithRequestData(request);
            if (request.Content != null)
            {
                await request.Content.ReadAsStringAsync()
                    .ContinueWith(task =>
                    {
                        apiLogEntry.RequestContentBody = task.Result;
                    }, cancellationToken);
            }

            var response = base.SendAsync(request, cancellationToken);

            await response.ContinueWith(task =>
            {
                var resp = response.Result;
                // Update the API log entry with response info
                apiLogEntry.ResponseStatusCode = (int)resp.StatusCode;
                apiLogEntry.ResponseTimestamp = DateTime.Now;

                if (resp.Content != null)
                {
                    apiLogEntry.ResponseContentBody = resp.Content.ReadAsStringAsync().Result;
                    apiLogEntry.ResponseContentType = resp.Content.Headers.ContentType.MediaType;
                    apiLogEntry.ResponseHeaders = SerializeHeaders(resp.Content.Headers);
                }

                // TODO: Save the API log entry to the database

                db.ApiLogEntry.Add(apiLogEntry);
                //Task<int> i = db.SaveChangesAsync();
                int i = db.SaveChanges();

            });

            return response.Result;

            /*
            return await base.SendAsync(request, cancellationToken)
                .ContinueWith(task =>
                {
                    var response = task.Result;

                    // Update the API log entry with response info
                    apiLogEntry.ResponseStatusCode = (int)response.StatusCode;
                    apiLogEntry.ResponseTimestamp = DateTime.Now;

                    if (response.Content != null)
                    {
                        apiLogEntry.ResponseContentBody = response.Content.ReadAsStringAsync().Result;
                        apiLogEntry.ResponseContentType = response.Content.Headers.ContentType.MediaType;
                        apiLogEntry.ResponseHeaders = SerializeHeaders(response.Content.Headers);
                    }

                    // TODO: Save the API log entry to the database

                    db.ApiLogEntry.Add(apiLogEntry);
                    //Task<int> i = db.SaveChangesAsync();
                    int i = db.SaveChanges();

                    return response;
                }, cancellationToken);
            */
        }

        private ApiLogEntry CreateApiLogEntryWithRequestData(HttpRequestMessage request)
        {
            var context = ((HttpContextBase)request.Properties["MS_HttpContext"]);
            var routeData = request.GetRouteData();

            return new ApiLogEntry
            {
                Application = "[insert-calling-app-here]",
                User = context.User.Identity.Name,
                Machine = Environment.MachineName,
                RequestContentType = context.Request.ContentType,
                RequestRouteTemplate = routeData.Route.RouteTemplate,
                RequestRouteData = SerializeRouteData(routeData),
                RequestIpAddress = context.Request.UserHostAddress,
                RequestMethod = request.Method.Method,
                RequestHeaders = SerializeHeaders(request.Headers),
                RequestTimestamp = DateTime.Now,
                RequestUri = request.RequestUri.ToString()
            };
        }

        private string SerializeRouteData(IHttpRouteData routeData)
        {
            return JsonConvert.SerializeObject(routeData, Formatting.Indented);
        }

        private string SerializeHeaders(HttpHeaders headers)
        {
            var dict = new Dictionary<string, string>();

            foreach (var item in headers.ToList())
            {
                if (item.Value != null)
                {
                    var header = String.Empty;
                    foreach (var value in item.Value)
                    {
                        header += value + " ";
                    }

                    // Trim the trailing space and add item to the dictionary
                    header = header.TrimEnd(" ".ToCharArray());
                    dict.Add(item.Key, header);
                }
            }

            return JsonConvert.SerializeObject(dict, Formatting.Indented);
        }
    }
}