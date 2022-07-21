namespace GraphQL;

public class ErrorPath : List<object>
{
    public ErrorPath()
    {
    }

    public ErrorPath(IEnumerable<object> collection) : base(collection)
    {
    }
}
