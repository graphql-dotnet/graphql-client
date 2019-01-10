using System;
using System.Text;
using BenchmarkDotNet.Attributes;
using GraphQL.Common.Request;

namespace GraphQL.Common.Benchmark.Request {

	public class GraphQLRequestBenchmark {

		private readonly string query;
		private readonly string operationName;
		private readonly dynamic variables;
		private readonly GraphQLRequest graphQLRequest1 = Generate();
		private readonly GraphQLRequest graphQLRequest2 = Generate();

		public GraphQLRequestBenchmark() {
			var random = new Random();
			var data = new byte[1000];
			random.NextBytes(data);
			this.query = Encoding.Default.GetString(data);
			random.NextBytes(data);
			this.operationName = Encoding.Default.GetString(data);
			this.variables = new {
				objectA = random.NextDouble(),
				objectB = random.Next()
			};
		}

		[Benchmark]
		public GraphQLRequest Constructor() => new GraphQLRequest(this.query) { OperationName = this.operationName, Variables = this.variables };

		[Benchmark]
		public bool Equality() => this.graphQLRequest1.Equals(this.graphQLRequest2);

		[Benchmark]
		public bool InEquality() => !this.graphQLRequest1.Equals(this.graphQLRequest2);

		private static GraphQLRequest Generate() {
			var random = new Random();
			var data = new byte[1000];
			random.NextBytes(data);
			var query = Encoding.Default.GetString(data);
			random.NextBytes(data);
			var operationName = Encoding.Default.GetString(data);
			var variables = new {
				objectA = random.NextDouble(),
				objectB = random.Next()
			};
			return new GraphQLRequest(query) {
				OperationName = operationName,
				Variables = variables
			};
		}

	}

}
