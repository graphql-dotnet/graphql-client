using GraphQL.Types;

namespace GraphQL.Server.Test.GraphQL;

public class TestSchema : Schema
{
    public TestSchema()
    {
        Query = new TestQuery();
        //this.Mutation = new TestMutation();
        //this.Subscription = new TestSubscription();
    }
}
