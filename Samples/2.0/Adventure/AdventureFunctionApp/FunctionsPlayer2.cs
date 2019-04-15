using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AdventureGrainInterfaces;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Orleans;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using Orleans;
using Orleans.Configuration;

namespace AdventureFunctionApp
{
    public static class FunctionsPlayer2
    {
        public const string PlayerName = "Functions Player 2";
        public static readonly Guid PlayerGuid = Guid.Parse("E339D2B0-12C6-44A4-AEA0-8AD7FA689A12");
        public static readonly List<string> Directions = new List<string> { "north", "south", "east", "west" };

        private static Random random = new Random();           

        [FunctionName("FunctionsPlayer2")]
        public static async Task Run(
            [TimerTrigger("5/10 * * * * *")]TimerInfo myTimer,
            [Orleans(typeof(IPlayerGrain))] IClusterClient orleansClient,
            ILogger log)
        {            
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
    }
}
