using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using MongoDB.Driver;
using System.Collections.Generic;
using System.Linq;

namespace Day4
{
    public static class Day4
    {
        private static readonly MongoClient client = new MongoClient(Environment.GetEnvironmentVariable("ENV_MONGODB_CONN_STRING"));

        [FunctionName("PotLuck")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", "delete", Route = null)] HttpRequest req,
            ILogger log)
        {
            try
            {
                var database = client.GetDatabase(Environment.GetEnvironmentVariable("ENV_MONGODB_DBNAME"));
                var users = database.GetCollection<User>("Users");
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                switch (req.Method.ToLower())
                {
                    case "delete":
                        var deleteData = JsonConvert.DeserializeObject<User>(requestBody);
                        var deleteUserFood = await users.Find(c => c.Name == deleteData.Name).FirstOrDefaultAsync(); ;
                        if (deleteUserFood != null && deleteUserFood.Foods?.Count > 0)
                        {
                            deleteUserFood.Foods = deleteUserFood.Foods.Select(c => c.ToLower()).Except(deleteData.Foods.Select(c => c.ToLower()), StringComparer.OrdinalIgnoreCase).ToList();
                            await users.ReplaceOneAsync(c => c.Id == deleteUserFood.Id, deleteUserFood);
                        }
                        break;

                    case "post":
                        var postData = JsonConvert.DeserializeObject<User>(requestBody);
                        var updateUserFood = await users.Find(c => c.Name == postData.Name).FirstOrDefaultAsync();
                        if (updateUserFood == null)
                        {
                            postData.Foods = postData.Foods.Select(c => c.ToLower()).Distinct().ToList();
                            await users.InsertOneAsync(postData);
                        }
                        else
                        {
                            if (updateUserFood.Foods?.Count > 0)
                                postData.Foods.AddRange(updateUserFood.Foods);
                            postData.Foods = postData.Foods.Select(c => c.ToLower()).Distinct().ToList();
                            postData.Id = updateUserFood.Id;
                            await users.ReplaceOneAsync(c => c.Id == updateUserFood.Id, postData);
                        }
                        break;
                }
                return new OkObjectResult(users.Find(c => true).ToList());
            }
            catch (Exception ex)
            {
                log.LogError(ex, $"Error during {req.Method}");
                return new BadRequestObjectResult("Uh oh something went wrong ,please try again");
            }
        }
    }
}