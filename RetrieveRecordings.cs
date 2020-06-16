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
        
            Uri readingCollectionUri = UriFactory.CreateDocumentCollectionUri(databaseId: "Climate", collectionId: "Recordings");
        
            var options = new FeedOptions { EnableCrossPartitionQuery = true }; // Enable cross partition query
        
            IDocumentQuery<Document> query = client.CreateDocumentQuery<Document>(readingCollectionUri, options)
                                                .Where(reading => reading.month == month)
                                                .OrderBy(reading => reading.timestamp)
                                                .AsDocumentQuery();
        
            var readingsForMonth = new List<Document>();
        
            while (query.HasMoreResults)
            {
                foreach (Document reading in await query.ExecuteNextAsync())
                {
                    readingsForMonth.Add(reading);
                }
            }                       
        
            return new OkObjectResult(readingsForMonth);
        }
    }
}
