using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using System.Fabric;
using System.Net.Http;
using Microsoft.Azure.Documents;
using System.Fabric.Query;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace FrontEnd.Controllers
{
    [Route("api/[controller]")]
    public class ReliableCollectionController : Controller
    {
        private FabricClient _fabricClient;
        private HttpClient _httpClient;

        public ReliableCollectionController(FabricClient fabricClient, HttpClient httpClient)
        {
            _fabricClient = fabricClient;
            _httpClient = httpClient;
        }
        // GET api/values
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            List<User> users = new List<User>();

            ServicePartitionList partitionList = await _fabricClient.QueryManager.GetPartitionListAsync(new Uri("fabric:/SFPerfMonitoring/StatefulBackEnd"));

            foreach(Partition partition in partitionList)
            {
                long partitionKey = ((Int64RangePartitionInformation)partition.PartitionInformation).LowKey;

                string proxyUrl = $"http://localhost:{19081}/SFPerfMonitoring/StatefulBackEnd/api/users?PartitionKind={partition.PartitionInformation.Kind}&PartitionKey={partitionKey}";

                HttpResponseMessage response = await _httpClient.GetAsync(proxyUrl);

                if (response.StatusCode != System.Net.HttpStatusCode.OK)
                {
                    // if one partition returns a failure, you can either fail the entire request or skip that partition.
                    return this.StatusCode((int)response.StatusCode);
                }

                string stringResult = await response.Content.ReadAsStringAsync();

                var result = JObject.Parse(stringResult).ToObject<List<User>>();
                //var result = JsonConvert.DeserializeObject<List<User>>(stringResult);

                users.AddRange(result);
            }

            return this.Json(users);
        }

        // GET api/values/5
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            ServicePartitionList partitionList = await _fabricClient.QueryManager.GetPartitionListAsync(new Uri("fabric:/SFPerfMonitoring/StatefulBackEnd"));

            foreach (Partition partition in partitionList)
            {
                long partitionKey = ((Int64RangePartitionInformation)partition.PartitionInformation).LowKey;

                string proxyUrl = $"http://localhost:{19081}/SFPerfMonitoring/StatefulBackEnd/api/users/{id}?PartitionKind={partition.PartitionInformation.Kind}&PartitionKey={partitionKey}";

                HttpResponseMessage response = await _httpClient.GetAsync(proxyUrl);

                if (response.StatusCode != System.Net.HttpStatusCode.OK)
                {
                    // if one partition returns a failure, you can either fail the entire request or skip that partition.
                    return this.StatusCode((int)response.StatusCode);
                }

                string stringResult = await response.Content.ReadAsStringAsync();

                var result = JsonConvert.DeserializeObject<User>(stringResult);

                return this.Json(result);
            }

            return null;
        }

        // POST api/values
        [HttpPost]
        public void Post([FromBody]string value)
        {
        }

        // PUT api/values/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/values/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
