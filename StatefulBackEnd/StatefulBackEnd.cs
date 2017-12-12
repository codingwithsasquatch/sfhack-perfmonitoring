using System;
using System.Collections.Generic;
using System.Fabric;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.ServiceFabric.Services.Communication.AspNetCore;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;
using Microsoft.ServiceFabric.Data;
using Microsoft.ServiceFabric.Data.Collections;
using Models;
using Newtonsoft.Json;

namespace StatefulBackEnd
{
    /// <summary>
    /// The FabricRuntime creates an instance of this class for each service type instance. 
    /// </summary>
    internal sealed class StatefulBackEnd : StatefulService
    {
        private static readonly Uri MockDataUri = new Uri("https://raw.githubusercontent.com/codingwithsasquatch/sfhack-perfmonitoring/master/MOCK_DATA.json");
        public static readonly Uri UsersDictionaryName = new Uri("store:/users");

        public StatefulBackEnd(StatefulServiceContext context)
            : base(context)
        { }

        /// <summary>
        /// Optional override to create listeners (like tcp, http) for this service instance.
        /// </summary>
        /// <returns>The collection of listeners.</returns>
        protected override IEnumerable<ServiceReplicaListener> CreateServiceReplicaListeners()
        {
            return new ServiceReplicaListener[]
            {
                new ServiceReplicaListener(serviceContext =>
                    new KestrelCommunicationListener(serviceContext, (url, listener) =>
                    {
                        ServiceEventSource.Current.ServiceMessage(serviceContext, $"Starting Kestrel on {url}");

                        return new WebHostBuilder()
                                    .UseKestrel()
                                    .ConfigureServices(
                                        services => services
                                            .AddSingleton<StatefulServiceContext>(serviceContext)
                                            .AddSingleton<IReliableStateManager>(this.StateManager))
                                    .UseContentRoot(Directory.GetCurrentDirectory())
                                    .UseStartup<Startup>()
                                    .UseServiceFabricIntegration(listener, ServiceFabricIntegrationOptions.UseUniqueServiceUrl)
                                    .UseUrls(url)
                                    .Build();
                    }))
            };
        }

        protected override async Task RunAsync(CancellationToken cancellationToken)
        {
            // Load User Data
            string jsonString;
            using (var webClient = new System.Net.WebClient())
            {
                jsonString = webClient.DownloadString(MockDataUri);
            }
            var userArray = JsonConvert.DeserializeObject<User[]>(jsonString);

            var userDictionary = await this.StateManager.GetOrAddAsync<IReliableDictionary<long, User>>(UsersDictionaryName);

            using (var tx = this.StateManager.CreateTransaction())
            {
                foreach (var user in userArray)
                {
                    await userDictionary.AddOrUpdateAsync(tx, user.UserId, user, (key, value) => value);
                }

                await tx.CommitAsync();
            }
        }
    }
}
