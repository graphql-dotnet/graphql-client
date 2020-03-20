using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using GraphQL.Client.Abstractions;
using GraphQL.Subscription;
using GraphQL.Types;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

namespace GraphQL.Client.LocalExecution
{
    public static class GraphQLLocalExecutionClient
    {
        public static GraphQLLocalExecutionClient<TSchema> New<TSchema>(TSchema schema) where TSchema : ISchema
            => new GraphQLLocalExecutionClient<TSchema>(schema);

        public static GraphQLLocalExecutionClient<TSchema> New<TSchema>(TSchema schema, IGraphQLJsonSerializer serializer) where TSchema : ISchema
            => new GraphQLLocalExecutionClient<TSchema>(schema, serializer);
    }


    public class GraphQLLocalExecutionClient<TSchema> : IGraphQLClient where TSchema : ISchema
    {

        private static readonly JsonSerializerSettings _variablesSerializerSettings = new JsonSerializerSettings
        {
            Formatting = Formatting.Indented,
            DateTimeZoneHandling = DateTimeZoneHandling.Local,
            ContractResolver = new CamelCasePropertyNamesContractResolver(),
            Converters = new List<JsonConverter>
            {
                new GraphQLEnumConverter()
            }
        };

        public TSchema Schema { get; }
        public IGraphQLJsonSerializer Serializer { get; }


        private readonly DocumentExecuter _documentExecuter;

        public GraphQLLocalExecutionClient(TSchema schema)
        {
            Serializer.EnsureAssigned();
            Schema = schema;
            if (!Schema.Initialized)
                Schema.Initialize();
            _documentExecuter = new DocumentExecuter();
        }

        public GraphQLLocalExecutionClient(TSchema schema, IGraphQLJsonSerializer serializer) : this(schema)
        {
            Serializer = serializer;
        }

        public void Dispose() { }

        public Task<GraphQLResponse<TResponse>> SendQueryAsync<TResponse>(GraphQLRequest request, CancellationToken cancellationToken = default)
            => ExecuteQueryAsync<TResponse>(request, cancellationToken);

        public Task<GraphQLResponse<TResponse>> SendMutationAsync<TResponse>(GraphQLRequest request, CancellationToken cancellationToken = default)
            => ExecuteQueryAsync<TResponse>(request, cancellationToken);

        public IObservable<GraphQLResponse<TResponse>> CreateSubscriptionStream<TResponse>(GraphQLRequest request) =>
            Observable.Defer(() => ExecuteSubscriptionAsync<TResponse>(request).ToObservable())
                .Concat()
                .Publish()
                .RefCount();

        public IObservable<GraphQLResponse<TResponse>> CreateSubscriptionStream<TResponse>(GraphQLRequest request,
            Action<Exception> exceptionHandler)
            => CreateSubscriptionStream<TResponse>(request);

        #region Private Methods

        private async Task<GraphQLResponse<TResponse>> ExecuteQueryAsync<TResponse>(GraphQLRequest request, CancellationToken cancellationToken)
        {
            var executionResult = await ExecuteAsync(request, cancellationToken);
            return await ExecutionResultToGraphQLResponse<TResponse>(executionResult, cancellationToken);
        }
        private async Task<IObservable<GraphQLResponse<TResponse>>> ExecuteSubscriptionAsync<TResponse>(GraphQLRequest request, CancellationToken cancellationToken = default)
        {
            var result = await ExecuteAsync(request, cancellationToken);
            return ((SubscriptionExecutionResult)result).Streams?.Values.SingleOrDefault()?
                .SelectMany(executionResult => Observable.FromAsync(token => ExecutionResultToGraphQLResponse<TResponse>(executionResult, token)));
        }

        private async Task<ExecutionResult> ExecuteAsync(GraphQLRequest request, CancellationToken cancellationToken = default)
        {
            var serializedRequest = Serializer.SerializeToString(request);

            var deserializedRequest = JsonConvert.DeserializeObject<GraphQLRequest>(serializedRequest);
            var inputs = deserializedRequest.Variables != null
                ? (JObject.FromObject(request.Variables, JsonSerializer.Create(_variablesSerializerSettings)) as JObject)
                .ToInputs()
                : null;

            var result = await _documentExecuter.ExecuteAsync(options =>
            {
                options.Schema = Schema;
                options.OperationName = request.OperationName;
                options.Query = request.Query;
                options.Inputs = inputs;
                options.CancellationToken = cancellationToken;
            });

            return result;
        }

        private Task<GraphQLResponse<TResponse>> ExecutionResultToGraphQLResponse<TResponse>(ExecutionResult executionResult, CancellationToken cancellationToken = default)
        {
            // serialize result into utf8 byte stream
            var resultStream = new MemoryStream(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(executionResult, _variablesSerializerSettings)));
            // deserialize using the provided serializer
            return Serializer.DeserializeFromUtf8StreamAsync<TResponse>(resultStream, cancellationToken);
        }

        #endregion
    }
}
