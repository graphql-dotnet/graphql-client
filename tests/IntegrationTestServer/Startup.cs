using GraphQL;
using GraphQL.Client.Tests.Common;
using GraphQL.Client.Tests.Common.Chat.Schema;
using GraphQL.Client.Tests.Common.StarWars;
using GraphQL.Server.Ui.Altair;
using GraphQL.Server.Ui.GraphiQL;
using Microsoft.AspNetCore.Server.Kestrel.Core;

namespace IntegrationTestServer;

public class Startup
{
    public Startup(IConfiguration configuration, IWebHostEnvironment environment)
    {
        Configuration = configuration;
        Environment = environment;
    }

    public IConfiguration Configuration { get; }

    public IWebHostEnvironment Environment { get; }

    // This method gets called by the runtime. Use this method to add services to the container.
    // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
    public void ConfigureServices(IServiceCollection services)
    {
        services.Configure<KestrelServerOptions>(options => options.AllowSynchronousIO = true);
        services.AddHttpContextAccessor();
        services.AddChatSchema();
        services.AddStarWarsSchema();
        services.AddGraphQL(builder => builder
            .UseApolloTracing(enableMetrics: true)
            .ConfigureExecutionOptions(opt => opt.UnhandledExceptionDelegate = ctx =>
            {
                var logger = ctx.Context.RequestServices.GetRequiredService<ILogger<Startup>>();
                logger.LogError("{Error} occurred", ctx.OriginalException.Message);
                return Task.CompletedTask;
            })
            .AddErrorInfoProvider(opt => opt.ExposeExceptionDetails = Environment.IsDevelopment())
            .AddSystemTextJson()
            .AddGraphTypes(typeof(ChatSchema).Assembly));
    }

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }
        app.UseWebSockets();

        app.UseGraphQL<ChatSchema>(Common.CHAT_ENDPOINT);
        app.UseGraphQL<StarWarsSchema>(Common.STAR_WARS_ENDPOINT);

        app.UseGraphQLGraphiQL(options: new GraphiQLOptions { GraphQLEndPoint = Common.STAR_WARS_ENDPOINT });
        app.UseGraphQLAltair(options: new AltairOptions { GraphQLEndPoint = Common.CHAT_ENDPOINT });
    }
}
