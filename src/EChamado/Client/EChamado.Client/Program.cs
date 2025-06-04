using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using Microsoft.AspNetCore.Components.Authorization;
using EChamado.Client.Services;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace EChamado.Client
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);
            builder.RootComponents.Add<App>("#app");

            // Configura HttpClient normal
            builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

            // Configuração OIDC (Authorization Code + PKCE)
            builder.Services.AddOidcAuthentication(options =>
            {
                // Carrega as configurações de appsettings.json
                builder.Configuration.Bind("oidc", options.ProviderOptions);

                // Garante os escopos necessários
                options.ProviderOptions.DefaultScopes.Clear();
                foreach (var scope in builder.Configuration.GetSection("oidc:DefaultScopes").Get<string[]>())
                {
                    options.ProviderOptions.DefaultScopes.Add(scope);
                }
            });

            // HttpClient que usa tokens para chamadas autenticadas
            builder.Services.AddHttpClient<ChamadoService>(client =>
            {
                client.BaseAddress = new Uri(builder.Configuration["BackendUrl"]);
            })
            .AddHttpMessageHandler<BaseAddressAuthorizationMessageHandler>();

            builder.Services.AddScoped<AuthenticationStateProvider,
                RemoteAuthenticationService<RemoteAuthenticationState, RemoteUserAccount, OidcProviderOptions>>();

            await builder.Build().RunAsync();
        }
    }
}
