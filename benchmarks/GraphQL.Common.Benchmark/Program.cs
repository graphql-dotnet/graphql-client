using BenchmarkDotNet.Running;
using GraphQL.Common.Benchmark.Request;

namespace GraphQL.Common.Benchmark {

	public class Program{

		public static void Main(string[] args){
			var graphQLRequestSummary = BenchmarkRunner.Run<GraphQLRequestBenchmark>();
		}

	}

}
