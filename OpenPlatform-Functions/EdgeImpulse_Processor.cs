using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Microsoft.Azure.Devices.Client;
using System.Text;
using Newtonsoft.Json.Linq;

namespace OpenPlatform_Functions
{
    public static class EdgeImpulse_Processor
    {
        [FunctionName("EiDataForwarder")]
        public static async Task<IActionResult> EiDataForwarder(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = "eidata")] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);

            // get payload and metadata
            string deviceConnectionString = data.azDevice;
            string modelId = data.azPnpModelId;
            JObject payloadObj = data.payload;

            // add extra to payload 
            try
            {
                dynamic payload = payloadObj["payload"];
                JArray items = (JArray)payload["values"];

                payloadObj.Add(new JProperty("extras",
                    new JObject
                    {
                        new JProperty("values_length", items.Count)
                    })
                );

            } catch(Exception e)
            {
                Console.WriteLine($"Cannot get length of payload values array. Error message: {e.Message}");
            }

            string payloadString = JsonConvert.SerializeObject(payloadObj);

            SendIoTData(deviceConnectionString, payloadString, modelId);

            log.LogInformation(requestBody);

            string responseMessage = "Hello Edge Impulse";

            return new OkObjectResult(responseMessage);
        }

        [FunctionName("EiDataRunForwarder")]
        public static async Task<IActionResult> EiDataRunForwarder(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = "eirundata")] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);

            string deviceConnectionString = data.azDevice;
            string modelId = data.azPnpModelId;

            dynamic payloadObj = data.payload;
            string payloadString = JsonConvert.SerializeObject(payloadObj);


            SendIoTData(deviceConnectionString, payloadString, modelId);

            log.LogInformation(requestBody);

            string responseMessage = "Hello Edge Impulse";

            return new OkObjectResult(responseMessage);
        }

        private static async void SendIoTData(string deviceConnectionString, string telemetryPayload, string modelId)
        {
            // if PnP model
            var options = new ClientOptions
            {
                ModelId = modelId
            };

            Console.WriteLine($"SendIoTData");
            DeviceClient deviceClient = DeviceClient.CreateFromConnectionString(deviceConnectionString, TransportType.Mqtt, options);

            using var message = new Message(Encoding.UTF8.GetBytes(telemetryPayload))
            {
                ContentEncoding = "utf-8",
                ContentType = "application/json",
            };

            await deviceClient.SendEventAsync(message);
            await deviceClient.CloseAsync();
        }
    }
}
