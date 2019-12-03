using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Net.Http;

namespace Day3
{
    public static class WebHook
    {
        private static HttpClient httpClient = new HttpClient();

        [FunctionName("WebHook")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var data = JsonConvert.DeserializeObject<PayLoad>(requestBody);

            var uploader = data.pusher.email;
            var repository = data.repository.url;
            foreach (var commit in data.commits)
            {
                foreach (var addedfile in commit.added)
                {
                    try
                    {
                        if (addedfile.IndexOf(".png", StringComparison.InvariantCultureIgnoreCase) > -1)
                        {
                            var imageUrl = $"{repository}/raw/master/{addedfile}";
                            var formContent = new FormUrlEncodedContent(new[]
                            {
                            new KeyValuePair<string, string>("image", imageUrl),
                            new KeyValuePair<string, string>("album", Environment.GetEnvironmentVariable(Constants.ImgurAlbumDeleteHash)),
                            new KeyValuePair<string, string>("type", "url"),
                            new KeyValuePair<string, string>("name", $"{addedfile}"),
                            new KeyValuePair<string, string>("description", $"Uploaded by {uploader} from github"),
                        });
                            var message = new HttpRequestMessage(HttpMethod.Post, "https://api.imgur.com/3/upload")
                            {
                                Content = formContent
                            };
                            message.Headers.Add("Authorization", $"Client-ID { Environment.GetEnvironmentVariable(Constants.ImgurClientId)}");
                            var response = await httpClient.SendAsync(message);
                        }
                    }
                    catch (Exception ex)
                    {
                        log.LogError(ex, "Error during upload");
                    }
                }
            }

            return new OkObjectResult($"Success uploaded to Imgur");
        }
    }
}