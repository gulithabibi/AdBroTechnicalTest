using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Runtime;
using Microsoft.ServiceFabric.Actors.Client;
using UserProfileService.Interfaces;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;

namespace UserProfileService
{
    /// <remarks>
    /// This class represents an actor.
    /// Every ActorID maps to an instance of this class.
    /// The StatePersistence attribute determines persistence and replication of actor state:
    ///  - Persisted: State is written to disk and replicated.
    ///  - Volatile: State is kept in memory only and replicated.
    ///  - None: State is kept in memory only and not replicated.
    /// </remarks>
    [StatePersistence(StatePersistence.Persisted)]
    internal class UserProfileService : Actor, IUserProfileService
    {
        private static readonly string CosmosDbConnectionString = "https://7ebaa927-0ee0-4-231-b9ee.documents.azure.com:443/";
        private static readonly string CosmosDbConnectionStringConfigKey = "0hIMQLArkz41W2yGYq4YWHllBdu6mLZVxuYXt7EbX6W7XWWxGcfAT3ThihqRIA7ZmRkn6bxSBWmHACDbM9N7Tg==";
        private const string CosmosDbDatabaseName = "AdBro";
        private string CosmosDbCollectionName = "Users";

        private readonly Container _cosmosDbContainer;

        /// <summary>
        /// Initializes a new instance of UserProfileService
        /// </summary>
        /// <param name="actorService">The Microsoft.ServiceFabric.Actors.Runtime.ActorService that will host this actor instance.</param>
        /// <param name="actorId">The Microsoft.ServiceFabric.Actors.ActorId for this actor instance.</param>
        public UserProfileService(ActorService actorService, ActorId actorId) 
            : base(actorService, actorId)
        {
            CosmosClient cosmosClient = new(CosmosDbConnectionString, CosmosDbConnectionStringConfigKey);
            Database database = cosmosClient.GetDatabase(CosmosDbDatabaseName);
            _cosmosDbContainer = database.GetContainer(CosmosDbCollectionName);
        }

        public async Task AddInterestsAsync(string[] interests)
        {
            UserProfile userProfile = await StateManager.GetStateAsync<UserProfile>(nameof(UserProfile));
            userProfile.Interests = interests;
            await StateManager.SetStateAsync(nameof(UserProfile), userProfile);
        }

        public async Task<string[]> GetInterestsAsync()
        {
            UserProfile userProfile = await StateManager.GetStateAsync<UserProfile>(nameof(UserProfile));
            return userProfile.Interests;
        }

        protected override async Task OnActivateAsync()
        {
            UserProfile userProfile = await _cosmosDbContainer.ReadItemAsync<UserProfile>(ActorId.ToString(), new PartitionKey(ActorId.ToString()));
            await StateManager.SetStateAsync(nameof(UserProfile), userProfile);
            await base.OnActivateAsync();
        }

        protected override async Task OnDeactivateAsync()
        {
            UserProfile userProfile = await StateManager.GetStateAsync<UserProfile>(nameof(UserProfile));
            await _cosmosDbContainer.UpsertItemAsync(userProfile, new PartitionKey(ActorId.ToString()));
            await base.OnDeactivateAsync();
        }

        private async Task<Container> GetContainer()
        {
            var cosmosClient = new CosmosClient(
                CosmosDbConnectionString, CosmosDbConnectionStringConfigKey,
                new CosmosClientOptions() { ApplicationName = "UserProfileActorService" });

            var database = await cosmosClient.CreateDatabaseIfNotExistsAsync(CosmosDbDatabaseName);
            var container = await database.Database.CreateContainerIfNotExistsAsync(
                CosmosDbCollectionName,
                "/id",
                400);

            return container;
        }

        /// <summary>
        /// TODO: Replace with your own actor method.
        /// </summary>
        /// <returns></returns>
        Task<int> IUserProfileService.GetCountAsync(CancellationToken cancellationToken)
        {
            return this.StateManager.GetStateAsync<int>("count", cancellationToken);
        }

        /// <summary>
        /// TODO: Replace with your own actor method.
        /// </summary>
        /// <param name="count"></param>
        /// <returns></returns>
        Task IUserProfileService.SetCountAsync(int count, CancellationToken cancellationToken)
        {
            // Requests are not guaranteed to be processed in order nor at most once.
            // The update function here verifies that the incoming count is greater than the current count to preserve order.
            return this.StateManager.AddOrUpdateStateAsync("count", count, (key, value) => count > value ? count : value, cancellationToken);
        }
    }
}
