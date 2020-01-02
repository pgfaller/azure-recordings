using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Recordings
{
    public static class RecordOneReading
    {
        [FunctionName("RecordOneReading")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req,
            [CosmosDB(
                databaseName: "Climate",
                collectionName: "Recordings",
                ConnectionStringSetting = "CosmosDBConnection")]
                IAsyncCollector<Document> documents,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var data = JsonConvert.DeserializeObject<Recording>(requestBody);
            DateTime timestamp = UnixTimeStampToDateTime(data.timestamp);
            var outputDocument = new Document
            {
                month = $"{timestamp.Year:D4}-{timestamp.Month:D2}",
                timestamp = data.timestamp,
                temperature = data.temperature,
                humidity = data.humidity
            };
            await documents.AddAsync(outputDocument);
            return data != null
                ? (ActionResult)new OkObjectResult($"Recorded data for {timestamp.ToUniversalTime()} (UTC)")
                : new BadRequestObjectResult("Invalid data in the request body");
        }

        public static DateTime UnixTimeStampToDateTime(uint unixTimeStamp)
        {
            // Unix timestamp is seconds past epoch
            System.DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
            dtDateTime = dtDateTime.AddSeconds(unixTimeStamp);
            return dtDateTime;
        }
    }

    public class Recording
    {
        public uint timestamp { get; set; }
        public double temperature { get; set; }
        public double humidity { get; set; }
    };
    public class Document : Recording
    {
        public string month { get; set; }
    };
}
