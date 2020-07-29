using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Linq;
using Microsoft.WindowsAzure.Storage.Table;

using Newtonsoft.Json;

namespace Recordings
{
    public static class RetrieveRecordings
    {
        [FunctionName("RetrieveRecordings")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            [CosmosDB( ConnectionStringSetting = "CosmosDBConnection")] DocumentClient client,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            // Validation and error checking omitted for demo purposes
                        
            string month = req.Query["month"]; // read storeId to get driver for from querystring
            if (month == null) {
                month = DateTime.Now.Year.ToString("D4") + "-" + DateTime.Now.Month.ToString("D2");
            }
        
            Uri readingCollectionUri = UriFactory.CreateDocumentCollectionUri(databaseId: "Climate", collectionId: "Recordings");
        
            var options = new FeedOptions { EnableCrossPartitionQuery = true }; // Enable cross partition query
        
            IDocumentQuery<Document> query = client.CreateDocumentQuery<Document>(readingCollectionUri, options)
                                                .Where(reading => reading.month == month)
                                                .OrderBy(reading => reading.timestamp)
                                                .AsDocumentQuery();
        
            var readingsForMonth = new List<Recording>();
        
            while (query.HasMoreResults)
            {
                foreach (Recording reading in await query.ExecuteNextAsync())
                {
                    readingsForMonth.Add(reading);
                }
            }                       
        
            return new OkObjectResult(readingsForMonth);
        }

        [FunctionName("RetrieveTableRecordings")]
        public static async Task<IActionResult> Retrieve(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            [Table("Recordings", Connection = "StorageConnection")] CloudTable cloudTable,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            // Validation and error checking omitted for demo purposes
                        
            string month = req.Query["month"]; // read storeId to get driver for from querystring
            if (month == null) {
                month = DateTime.Now.Year.ToString("D4") + "-" + DateTime.Now.Month.ToString("D2");
            }
            TableQuery<RecordingTableEntity> rangeQuery = new TableQuery<RecordingTableEntity>().Where(
                TableQuery.CombineFilters(
                    TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, 
                        "Climate"),
                    TableOperators.And,
                    TableQuery.GenerateFilterCondition("Month", QueryComparisons.Equal, 
                        month)));

            var readingsForMonth = new List<Recording>();

            // Execute the query and loop through the results
            foreach (RecordingTableEntity entity in 
                await cloudTable.ExecuteQuerySegmentedAsync(rangeQuery, null))
            {
                log.LogInformation(
                    $"{entity.PartitionKey}\t{entity.RowKey}\t{entity.Timestamp}\t{entity.Source}");
                readingsForMonth.Add(new Document {
                    source = entity.Source,
                    month = entity.Month,
                    timestamp = (uint) entity.Epoch,
                    temperature = entity.Temperature,
                    humidity = entity.Humidity
                });
            }                       
        
            return new OkObjectResult(readingsForMonth);
        }
    }
}
