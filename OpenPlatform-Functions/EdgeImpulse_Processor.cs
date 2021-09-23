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

            string deviceConnectionString = data.azDevice;

            dynamic payloadObj = data.payload;
            string payloadString = JsonConvert.SerializeObject(payloadObj);

            SendIoTData(deviceConnectionString, payloadString);

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

            dynamic payloadObj = data.payload;
            string payloadString = JsonConvert.SerializeObject(payloadObj);


            SendIoTData(deviceConnectionString, payloadString);

            log.LogInformation(requestBody);

            string responseMessage = "Hello Edge Impulse";

            return new OkObjectResult(responseMessage);
        }

        private static async void SendIoTData(string deviceConnectionString, string telemetryPayload)
        {
            // if PnP model
            var options = new ClientOptions
            {
                // ModelId = "dtmi:nordicsemi:eidataforward;2",
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
