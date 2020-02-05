using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GraphQL.StarWars;

namespace GraphQL.Integration.Tests.TestData {
	public class StarWarsHumans: IEnumerable<object[]> {
		public IEnumerator<object[]> GetEnumerator() {
			yield return new object[] { 1, "Luke" };
			yield return new object[] { 2, "Vader" };
		}

		IEnumerator IEnumerable.GetEnumerator() {
			return GetEnumerator();
		}
	}
}
