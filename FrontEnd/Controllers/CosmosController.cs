using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Documents.Client;
using Models;

namespace FrontEnd.Controllers
{
    [Route("api/[controller]")]
    public class CosmosController : Controller
    {
        private const string EndpointUri = "https://codeslingerstour.documents.azure.com:443/";
        private const string PrimaryKey = "YoZqHb6KfBGcx3icWUyUMghSqd7gJpScL5N6QexlXEDNe4GIxhBfnbtZQl71r0WtJol7OUxiOK6MeudWwubRcg==";
        private DocumentClient docClient;

        public CosmosController()
        {
            docClient = new DocumentClient(new Uri(EndpointUri), PrimaryKey);
        }

        // GET api/values
        [HttpGet]
        public IEnumerable<User> Get()
        {
            FeedOptions queryOptions = new FeedOptions { MaxItemCount = 1000 };

            IQueryable<User> users = docClient.CreateDocumentQuery<User>(
                UriFactory.CreateDocumentCollectionUri("sfhack-perfmonitoring", "users"),
                "SELECT * FROM users",
                queryOptions);

            return users;
        }

        // GET api/values/5
        [HttpGet("{id}")]
        public User Get(int id)
        {
            FeedOptions queryOptions = new FeedOptions { MaxItemCount = 1 };
            
            User user = docClient.CreateDocumentQuery<User>(
                UriFactory.CreateDocumentCollectionUri("sfhack-perfmonitoring", "users"),
                $"SELECT * FROM users WHERE users.UserId = {id}")
                .ToList()
                .FirstOrDefault();

            return user;
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
