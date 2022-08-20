using System.Runtime.Serialization;

namespace GraphQL.Client.Http;

[Serializable]
public class GraphQLSubscriptionException : Exception
{
    //
    // For guidelines regarding the creation of new exception types, see
    //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/cpgenref/html/cpconerrorraisinghandlingguidelines.asp
    // and
    //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/dncscol/html/csharp07192001.asp
    //

    public GraphQLSubscriptionException()
    {
    }

    public GraphQLSubscriptionException(object error) : base(error.ToString())
    {
    }

    protected GraphQLSubscriptionException(
        SerializationInfo info,
        StreamingContext context) : base(info, context)
    {
    }
}
