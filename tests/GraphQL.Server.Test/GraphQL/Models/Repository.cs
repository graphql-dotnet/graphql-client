using System;
using GraphQL.Types;

namespace GraphQL.Server.Test.GraphQL.Models
{

    public class Repository
    {
        public int DatabaseId { get; set; }
        public string Id { get; set; }
        public string Name { get; set; }
        public object Owner { get; set; }
        public Uri Url { get; set; }
    }

    public class RepositoryGraphType : ObjectGraphType<Repository>
    {

        public RepositoryGraphType()
        {
            this.Name = nameof(Repository);
            this.Field(expression => expression.DatabaseId);
            this.Field<NonNullGraphType<IdGraphType>>("id");
            this.Field(expression => expression.Name);
            //this.Field(expression => expression.Owner);
            this.Field<NonNullGraphType<UriGraphType>>("url");
        }

    }

}
