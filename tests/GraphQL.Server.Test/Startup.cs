using GraphQL.MicrosoftDI;
using GraphQL.Server.Test.GraphQL;
using GraphQL.Server.Ui.GraphiQL;

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
        app.UseGraphQLWebSockets<TestSchema>();
        app.UseGraphQL<TestSchema>();
        app.UseGraphQLGraphiQL(new GraphiQLOptions { });
    }

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddGraphQL(builder => builder
            .AddSchema<TestSchema>()
            .AddApolloTracing(enableMetrics: true)
            .AddErrorInfoProvider(opt => opt.ExposeExceptionStackTrace = true)
            .AddWebSockets()
        );
    }
}
