#if !NET8_0_OR_GREATER
using System.Runtime.Serialization;
#endif

namespace GraphQL.Client.Http;

[Serializable]
public class GraphQLSubscriptionException : Exception
{
    public GraphQLSubscriptionException()
    {
    }

    public GraphQLSubscriptionException(object error) : base(error.ToString())
    {
    }

#if !NET8_0_OR_GREATER
    protected GraphQLSubscriptionException(
        SerializationInfo info,
        StreamingContext context) : base(info, context)
    {
    }
#endif
}
