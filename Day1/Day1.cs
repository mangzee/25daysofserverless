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
using System.Linq;

namespace Day1
{
    public static class Day1
    {
        public static readonly Random Rand = new Random();
        public static List<string> Characters { get; set; }

        [FunctionName("Day1")]
        [Singleton] //I'm a poor kid ;)
        public static IActionResult Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequest req,
            ILogger log, ExecutionContext context)
        {
            log.LogInformation("Oh ho ho, Day1 - Serverless Dreidel");
            SetUpCharacters(context);
            return new OkObjectResult(SpinRandom());
        }

        private static void SetUpCharacters(ExecutionContext context)
        {
            if (Characters == null || Characters.Count == 0)
            {
                var fileContent = File.ReadAllText($"{context.FunctionAppDirectory}/Hebrew.json", System.Text.Encoding.UTF8);
                Characters = JsonConvert.DeserializeObject<List<string>>(fileContent);
            }
        }

        private static string SpinRandom()
        {
            int randomIndex = Rand.Next(Characters.Count);
            return Characters.ElementAt(randomIndex);
        }
    }
}