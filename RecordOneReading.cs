using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Microsoft.WindowsAzure.Storage.Table;

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
                source = (data.source == null) ? "(unknown)" : data.source,
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


        [FunctionName("TableOneReading")]
        public static async Task<IActionResult> TableOutput(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req,
            [Table("Recordings", Connection = "StorageConnection")] CloudTable cloudTable,
            ILogger log)
        {
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var data = JsonConvert.DeserializeObject<Recording>(requestBody);
            log.LogInformation($"C# HTTP trigger function processed a request: {JsonConvert.SerializeObject(data)}");
            DateTime timestamp = UnixTimeStampToDateTime(data.timestamp);
            RecordingTableEntity entity = new RecordingTableEntity {
                PartitionKey = "Climate",
                RowKey = $"{data.source}:{data.timestamp}",
                Source = data.source,
                Month = $"{timestamp.Year:D4}-{timestamp.Month:D2}",
                Epoch = data.timestamp,
                Temperature = data.temperature,
                Humidity = data.humidity
            };
            TableResult result = await cloudTable.ExecuteAsync(TableOperation.InsertOrReplace(entity));
            return result.HttpStatusCode / 100 == 2
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
}
