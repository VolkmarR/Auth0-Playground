using Auth0.AuthenticationApi;
using Auth0.AuthenticationApi.Models;
using Auth0.ManagementApi;
using Microsoft.Extensions.Configuration;
using Auth0.ManagementApi.Models;

namespace UserManagement;

internal class Program
{
    static async Task Main(string[] args)
    {
        IConfiguration Configuration = new ConfigurationBuilder()
          .SetBasePath(Directory.GetCurrentDirectory())
          .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
          .AddUserSecrets<Program>()
          .Build();

        Auth0Config auth0Config = new();

        Configuration.GetSection("Auth0").Bind(auth0Config);
        var token = await GetToken(auth0Config);

        foreach (var item in await ListUsers(token, auth0Config))
            Console.WriteLine($"User: {item.NickName} - Email: {item.Email} - Last Login: {item.LastLogin}");
    }

    static async Task<string> GetToken(Auth0Config config)
    {
        var authClient = new AuthenticationApiClient(config.Domain);
        var token = await authClient.GetTokenAsync(new ClientCredentialsTokenRequest { ClientId = config.Client_Id, ClientSecret = config.Client_Secret, Audience = config.Audience });
        return token.AccessToken;
    }

    static async Task<List<UserItem>> ListUsers(string token, Auth0Config config)
    {
        var managementClient = new ManagementApiClient(token, config.Domain);
        var result = await managementClient.Users.GetAllAsync(new GetUsersRequest { });
        return result.Select(q => new UserItem(q.Email, q.NickName, q.LastLogin)).ToList();
    }
}
