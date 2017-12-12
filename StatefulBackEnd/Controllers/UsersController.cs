using System;
using System.Collections.Generic;
using System.Fabric;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.ServiceFabric.Data;
using Microsoft.ServiceFabric.Data.Collections;
using Models;

namespace StatefulBackEnd.Controllers
{
    [Route("api/[controller]")]
    public class UsersController : Controller
    {
        private readonly IReliableStateManager stateManager;

        public UsersController(IReliableStateManager stateManager)
        {
            this.stateManager = stateManager;
        }

        //// GET api/users
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            try
            {
                List<User> result = new List<User>();

                ConditionalValue<IReliableDictionary<long, User>> tryGetResult =
                    await this.stateManager.TryGetAsync<IReliableDictionary<long, User>>(StatefulBackEnd.UsersDictionaryName);

                if (tryGetResult.HasValue)
                {
                    IReliableDictionary<long, User> dictionary = tryGetResult.Value;

                    using (ITransaction tx = this.stateManager.CreateTransaction())
                    {
                        IAsyncEnumerable<KeyValuePair<long, User>> enumerable = await dictionary.CreateEnumerableAsync(tx);
                        IAsyncEnumerator<KeyValuePair<long, User>> enumerator = enumerable.GetAsyncEnumerator();

                        while (await enumerator.MoveNextAsync(CancellationToken.None))
                        {
                            var user = enumerator.Current.Value;
                            result.Add(user);
                        }
                    }
                }
                return this.Ok(result);
            }
            catch (FabricException)
            {
                return new ContentResult { StatusCode = 503, Content = "The service was unable to process the request. Please try again." };
            }
        }

        // GET api/users/5
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(long id)
        {
            try
            {
                IReliableDictionary<long, User> usersDictionary =
                    await this.stateManager.GetOrAddAsync<IReliableDictionary<long, User>>(StatefulBackEnd.UsersDictionaryName);

                using (ITransaction tx = this.stateManager.CreateTransaction())
                {
                    ConditionalValue<User> result = await usersDictionary.TryGetValueAsync(tx, id);

                    if (result.HasValue)
                    {
                        return this.Ok(result.Value);
                    }

                    return this.NotFound();
                }
            }
            catch (FabricNotPrimaryException)
            {
                return new ContentResult { StatusCode = 410, Content = "The primary replica has moved.  Please re-resolve the service." };
            }
            catch (FabricException)
            {
                return new ContentResult { StatusCode = 503, Content = "The service was unable to process the request. Please try again." };
            }
        }
    }
}
