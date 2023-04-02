using GraphQL.Server.Test.GraphQL;

namespace GraphQL.Server.Test;

public class Startup
{
    public void Configure(IApplicationBuilder app)
    {
        var webHostEnvironment = app.ApplicationServices.GetRequiredService<IWebHostEnvironment>();
        if (webHostEnvironment.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }
        app.UseHttpsRedirection();

        app.UseWebSockets();
        app.UseGraphQL<TestSchema>();
        app.UseGraphQLGraphiQL();
    }

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddGraphQL(builder => builder
            .AddSchema<TestSchema>()
            .UseApolloTracing(enableMetrics: true)
            .AddErrorInfoProvider(opt => opt.ExposeExceptionDetails = true)
        );
    }
}
