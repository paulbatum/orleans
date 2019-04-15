using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AdventureGrainInterfaces;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using Orleans;
using Orleans.Configuration;

namespace AdventureFunctionApp
{
    public static class FunctionsPlayer1
    {
        public const string PlayerName = "Functions Player 1";
        public static readonly Guid PlayerGuid = Guid.Parse("AE42D285-EFFC-45CA-8514-2FFEA8FF8BEF");
        public static readonly List<string> Directions = new List<string> { "north", "south", "east", "west" };

        private static Random random = new Random();
        private static IClusterClient orleansClient;        
        
        [FunctionName("FunctionsPlayer1")]
        public static async Task Run([TimerTrigger("0/10 * * * * *")]TimerInfo myTimer, ILogger log)
        {
            await InitializeOrleansConnection();
            var player = orleansClient.GetGrain<IPlayerGrain>(PlayerGuid);

            if(await player.Name() == "nobody")
            {
                await player.SetName(PlayerName);
                var room1 = orleansClient.GetGrain<IRoomGrain>(0);
                await player.SetRoomGrain(room1);
            }

            var observation = await player.Play("look");
            var possibleDirections = Directions.Where(d => observation.Contains(d)).ToList();

            var randomDirection = possibleDirections[random.Next(0, possibleDirections.Count)];
            log.LogInformation($"I'm {PlayerName} and I'm going: {randomDirection}.");

            var output = await player.Play(randomDirection);
            log.LogInformation(output);            
        }

        private static async Task InitializeOrleansConnection()
        {
            // ignore the race
            if (orleansClient == null)
            {
                var client = new ClientBuilder()                    
                    .UseLocalhostClustering()
                    .Configure<ClusterOptions>(options =>
                    {
                        options.ClusterId = "dev";
                        options.ServiceId = "AdventureApp";
                    })
                    .ConfigureApplicationParts(parts => parts.AddApplicationPart(typeof(IRoomGrain).Assembly).WithReferences())
                    .Build();

                await client.Connect();

                orleansClient = client;
            }
        }
    }
}
