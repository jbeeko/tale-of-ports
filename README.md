# A Tale of Ports

Like all good technical tales it started with tweet.  

![Domain only tweet.](./images/domain-only.png "")

That got me thinking, what about taking a "out of the box" Visual Studio Code C# HTTP Triggered functions project and porting it to F#. The idea is to do a few ports, one a naïve line bye line translation of the C# into F#. This is almost always very straight forward and the resulting F# tends to look like the C# with less ceremony. Other ports will explore if more idiomatic versions are an improvement. 

## The C# Version

To create the C# version I a recent version Visual Studio 2019 with .net Core 3.x installed. I simply  used the menu items to create a new HTTP Triggered Azure Functions Project. See xxxx for the repository. Except for a few changes to the naming this is what I got:

```
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace TaleOfPorts
{
    public static class CSDemo
    {
        [FunctionName("CSDemo")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            string name = req.Query["name"];

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            name = name ?? data?.name;

            string responseMessage = string.IsNullOrEmpty(name)
                ?   "This HTTP triggered function executed successfully. Pass a name in the query " +
                    "string or in the request body for a personalized response."
                : $"Hello, {name}. This HTTP triggered function executed successfully.";

            return new OkObjectResult(responseMessage);
        }
    }
}

```

Fairly straight forward, 