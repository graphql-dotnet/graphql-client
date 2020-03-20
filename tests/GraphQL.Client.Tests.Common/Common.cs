using GraphQL.Client.Tests.Common.Chat.Schema;
using GraphQL.StarWars;
using GraphQL.StarWars.Types;
using Microsoft.Extensions.DependencyInjection;

namespace GraphQL.Client.Tests.Common
{
    public static class Common
    {
        public const string StarWarsEndpoint = "/graphql/starwars";
        public const string ChatEndpoint = "/graphql/chat";

        public static StarWarsSchema GetStarWarsSchema()
        {
            var services = new ServiceCollection();
            services.AddTransient<IDependencyResolver>(provider => new FuncDependencyResolver(provider.GetService));
            services.AddStarWarsSchema();
            return services.BuildServiceProvider().GetRequiredService<StarWarsSchema>();
        }
        public static ChatSchema GetChatSchema()
        {
            var services = new ServiceCollection();
            services.AddTransient<IDependencyResolver>(provider => new FuncDependencyResolver(provider.GetService));
            services.AddChatSchema();
            return services.BuildServiceProvider().GetRequiredService<ChatSchema>();
        }

        public static void AddStarWarsSchema(this IServiceCollection services)
        {
            services.AddSingleton<StarWarsData>();
            services.AddSingleton<StarWarsQuery>();
            services.AddSingleton<StarWarsMutation>();
            services.AddSingleton<StarWarsSchema>();
            services.AddTransient<CharacterInterface>();
            services.AddTransient<DroidType>();
            services.AddTransient<EpisodeEnum>();
            services.AddTransient<HumanType>();
            services.AddTransient<HumanInputType>();
        }

        public static void AddChatSchema(this IServiceCollection services)
        {
            var chat = new Chat.Schema.Chat();
            services.AddSingleton(chat);
            services.AddSingleton<IChat>(chat);
            services.AddSingleton<ChatSchema>();
            services.AddSingleton<ChatQuery>();
            services.AddSingleton<ChatMutation>();
            services.AddSingleton<ChatSubscriptions>();
            services.AddSingleton<MessageType>();
            services.AddSingleton<MessageInputType>();
            services.AddSingleton<MessageFromType>();
        }
    }
}
