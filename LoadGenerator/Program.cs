using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace LoadGenerator
{
    class Program
    {
        private const string COSMOS_SERVICE_ENDPOINT = "http://codeslingerstour.southcentralus.cloudapp.azure.com:8581/api/cosmos";
        private const string RELIABLE_COLLECTION_ENDPOINT = "http://codeslingerstour.southcentralus.cloudapp.azure.com:8581/api/reliablecollection";
        private HttpClient _httpClient;

        static void Main(string[] args)
        {
            Program p = new Program();
            List<Task> tasks = new List<Task>();
            tasks.Add(p.GenerateCosmosLoad());
            tasks.Add(p.GenerateReliableCollectionLoad());
            Console.ReadKey();
        }

        public Program()
        {
            _httpClient = new HttpClient();
        }

        private async Task GenerateCosmosLoad()
        {
            Random rand = new Random();
            while (true)
            {
                int value = rand.Next(1, 1000);
                var result = await _httpClient.GetStringAsync($"{COSMOS_SERVICE_ENDPOINT}/{value}");
                Console.WriteLine($"Pinging cosmos with id {value}");
                await Task.Delay(10);
            }
        }

        private async Task GenerateReliableCollectionLoad()
        {
            Random rand = new Random();
            while (true)
            {
                int value = rand.Next(1, 1000);
                var result = _httpClient.GetStringAsync($"{RELIABLE_COLLECTION_ENDPOINT}/{value}");
                Console.WriteLine($"Pinging collection with id {value}");
                await Task.Delay(10);
            }
        }
    }
}
