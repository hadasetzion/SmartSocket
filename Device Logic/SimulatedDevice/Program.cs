// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

// This application uses the Azure IoT Hub device SDK for .NET
// For samples see: https://github.com/Azure/azure-iot-sdk-csharp/tree/main/iothub/device/samples

using System;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.Devices.Client;

namespace SimulatedDevice
{
    /// <summary>
    /// This sample illustrates the very basics of a device app sending telemetry and receiving a command.
    /// For a more comprehensive device app sample, please see
    /// <see href="https://github.com/Azure-Samples/azure-iot-samples-csharp/tree/main/iot-hub/Samples/device/DeviceReconnectionSample"/>.
    /// </summary>
    internal class Program
    {
        private static DeviceClient s_deviceClient;
        private static readonly TransportType s_transportType = TransportType.Mqtt;

        // The device connection string to authenticate the device with your IoT hub.
        // Using the Azure CLI:
        // az iot hub device-identity show-connection-string --hub-name {YourIoTHubName} --device-id MyDotnetDevice --output table
        private static string s_connectionString = "HostName=chargerHub.azure-devices.net;DeviceId=device2;SharedAccessKey=OQKjK+VhIWhb63r50nQNUgf9l2K5tKZ8BuBLJgRBJLE=";

        private static TimeSpan s_telemetryInterval = TimeSpan.FromSeconds(600); // Seconds
      
        private static Boolean LED_STATUS = false;
        private static long LED_ON_TIME = 3600000;
        private static long LAST_ON = 0;

        private static async Task Main(string[] args)
        {   
            // This sample accepts the device connection string as a parameter, if present
            ValidateConnectionString(args);

            // Connect to the IoT hub using the MQTT protocol
            s_deviceClient = DeviceClient.CreateFromConnectionString(s_connectionString, s_transportType);

            // Create a handler for the direct method call
            await s_deviceClient.SetMethodHandlerAsync("SetTelemetryInterval", SetTelemetryInterval, null);

            // Receive and handle messages from the IOT hub
            await s_deviceClient.SetReceiveMessageHandlerAsync(OnC2dMessageReceivedAsync, s_deviceClient);

            // Set up a condition to quit the sample
            Console.WriteLine("Press control-C to exit.");
            using var cts = new CancellationTokenSource();
            Console.CancelKeyPress += (sender, eventArgs) =>
            {
                eventArgs.Cancel = true;
                cts.Cancel();
                Console.WriteLine("Exiting...");
            };

            // Run the telemetry loop
            await SendDeviceToCloudMessagesAsync(cts.Token);

            // SendDeviceToCloudMessagesAsync is designed to run until cancellation has been explicitly requested by Console.CancelKeyPress.
            // As a result, by the time the control reaches the call to close the device client, the cancellation token source would
            // have already had cancellation requested.
            // Hence, if you want to pass a cancellation token to any subsequent calls, a new token needs to be generated.
            // For device client APIs, you can also call them without a cancellation token, which will set a default
            // cancellation timeout of 4 minutes: https://github.com/Azure/azure-iot-sdk-csharp/blob/64f6e9f24371bc40ab3ec7a8b8accbfb537f0fe1/iothub/device/src/InternalClient.cs#L1922
            await s_deviceClient.CloseAsync();

            s_deviceClient.Dispose();
            Console.WriteLine("Device simulator finished.");
        }

        private static void ValidateConnectionString(string[] args)
        {
            if (args.Any())
            {
                try
                {
                    var cs = IotHubConnectionStringBuilder.Create(args[0]);
                    s_connectionString = cs.ToString();
                }
                catch (Exception)
                {
                    Console.WriteLine($"Error: Unrecognizable parameter '{args[0]}' as connection string.");
                    Environment.Exit(1);
                }
            }
            else
            {
                try
                {
                    _ = IotHubConnectionStringBuilder.Create(s_connectionString);
                }
                catch (Exception)
                {
                    Console.WriteLine("This sample needs a device connection string to run. Program.cs can be edited to specify it, or it can be included on the command-line as the only parameter.");
                    Environment.Exit(1);
                }
            }
        }

        // Handle the direct method call.
        private static Task<MethodResponse> SetTelemetryInterval(MethodRequest methodRequest, object userContext)
        {
            var data = Encoding.UTF8.GetString(methodRequest.Data);

            // Check the payload is a single integer value.
            if (int.TryParse(data, out int telemetryIntervalInSeconds))
            {
                s_telemetryInterval = TimeSpan.FromSeconds(telemetryIntervalInSeconds);

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"Telemetry interval set to {s_telemetryInterval}");
                Console.ResetColor();

                // Acknowlege the direct method call with a 200 success message.
                string result = $"{{\"result\":\"Executed direct method: {methodRequest.Name}\"}}";
                return Task.FromResult(new MethodResponse(Encoding.UTF8.GetBytes(result), 200));
            }
            else
            {
                // Acknowlege the direct method call with a 400 error message.
                string result = "{\"result\":\"Invalid parameter\"}";
                return Task.FromResult(new MethodResponse(Encoding.UTF8.GetBytes(result), 400));
            }
        }

        // Async method to send simulated telemetry.
        private static async Task SendDeviceToCloudMessagesAsync(CancellationToken ct)
        {
            try
            {
                long next_telemetry_time = 0;

                while (!ct.IsCancellationRequested)
                {
                    long now = DateTimeOffset.Now.ToUnixTimeMilliseconds();
                    if (now > next_telemetry_time)
                    {
                        SendTelemetry();
                        next_telemetry_time = now + (long) s_telemetryInterval.TotalMilliseconds;
                    }
                    
                    if ((LAST_ON > 0) && (now - LED_ON_TIME > LAST_ON))
                    {
                        Console.WriteLine("LED time expired");
                        LED_STATUS = false;
                        LAST_ON = 0;
                        SendTelemetry();
                    }

                    await Task.Delay(TimeSpan.FromSeconds(1), ct);
                }
            }
            catch (TaskCanceledException) { } // User canceled
        }

        private static async void SendTelemetry()
        {
            string status = (LED_STATUS ? "on" : "off");
            Dictionary<string, string> connStringParts = s_connectionString.Split(';').Select(t => t.Split(new char[] { '=' }, 2)).ToDictionary(t => t[0].Trim(), t => t[1].Trim(), StringComparer.InvariantCultureIgnoreCase);
            string device_id = connStringParts["DeviceId"];
           
            // Create JSON message.
            string messageBody = JsonSerializer.Serialize(
                new
                {
                    device_id = device_id,
                    status = status,
                    time_left = LED_ON_TIME - (DateTimeOffset.Now.ToUnixTimeMilliseconds() - LAST_ON)
                });
            using var message = new Message(Encoding.ASCII.GetBytes(messageBody))
            {
                ContentType = "application/json",
                ContentEncoding = "utf-8",
            };

            // Add a custom application property to the message.
            // An IoT hub can filter on these properties without access to the message body.

            // Send the telemetry message.
            await s_deviceClient.SendEventAsync(message);
            Console.WriteLine($"{DateTime.Now} > Sending message: {messageBody}");
        }

        private static async Task OnC2dMessageReceivedAsync(Message receivedMessage, object _)
        {
            string status = (LED_STATUS ? "on" : "off");

            Console.WriteLine($"{DateTime.Now}> C2D message callback - message received with Id={receivedMessage.MessageId}.");

            String message = Encoding.ASCII.GetString(receivedMessage.GetBytes());
            if (message.Equals("on"))
            {
                LED_STATUS = true;
                LAST_ON = DateTimeOffset.Now.ToUnixTimeMilliseconds();
                Console.WriteLine("Light is on");
            }
            else if (message.Equals("off"))
            {
                LED_STATUS = false;
                LAST_ON = 0;
                Console.WriteLine("Light is off");
            }
            else if (message.Equals("status"))
            {
                Console.WriteLine($"LED status: {status}");
                Console.WriteLine($"LED time left: {LED_ON_TIME - (DateTimeOffset.Now.ToUnixTimeMilliseconds() - LAST_ON)}");
            }
            else
            {
                Console.WriteLine("Do Nothing");
            }

            SendTelemetry();

            await s_deviceClient.CompleteAsync(receivedMessage);
            Console.WriteLine($"{DateTime.Now}> Completed C2D message with Id={receivedMessage.MessageId}.");

            receivedMessage.Dispose();
        }

        
    }
}