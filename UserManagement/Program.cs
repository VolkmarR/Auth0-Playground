using Auth0.AuthenticationApi;
using Auth0.AuthenticationApi.Models;
using Auth0.ManagementApi;
using Microsoft.Extensions.Configuration;
using Auth0.ManagementApi.Models;
using System.CommandLine;
using Auth0.Core.Exceptions;

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


        var rootCommand = new RootCommand("User Management App");
        var listCommand = new Command("list", "Lists all the users");
        var addCommand = new Command("add", "Adds the users");

        var emailArgument = new Argument<string>("email");
        var passwordArgument = new Argument<string>("password");
        var tenantArgument = new Argument<string>("tenant");
        addCommand.AddArgument(emailArgument);
        addCommand.AddArgument(passwordArgument);
        addCommand.AddArgument(tenantArgument);

        rootCommand.Add(listCommand);
        rootCommand.Add(addCommand);

        listCommand.SetHandler(async () =>
        {
            Console.WriteLine("List Users");

            foreach (var item in await ListUsers(auth0Config))
                Console.WriteLine($"User: {item.NickName} - Email: {item.Email} - Last Login: {item.LastLogin}");
        });

        addCommand.SetHandler(async (string email, string password, string tenant) =>
        {
            var res = await AddUser(auth0Config, email, password, tenant);
            if (!string.IsNullOrEmpty(res))
                Console.WriteLine(res);
            else
                Console.WriteLine("User added sucessfully");

        }, emailArgument, passwordArgument, tenantArgument);


        await rootCommand.InvokeAsync(args);
    }

    static async Task<string> GetToken(Auth0Config config)
    {
        var authClient = new AuthenticationApiClient(config.Domain);
        var token = await authClient.GetTokenAsync(new ClientCredentialsTokenRequest { ClientId = config.Client_Id, ClientSecret = config.Client_Secret, Audience = config.Audience });
        return token.AccessToken;
    }

    static async Task<List<UserItem>> ListUsers(Auth0Config config)
    {
        var token = await GetToken(config);

        var managementClient = new ManagementApiClient(token, config.Domain);
        var result = await managementClient.Users.GetAllAsync(new GetUsersRequest { });
        return result.Select(q => new UserItem(q.Email, q.NickName, q.LastLogin)).ToList();
    }

    static async Task<string> AddUser(Auth0Config config, string email, string password, string tenant)
    {
        var token = await GetToken(config);

        var managementClient = new ManagementApiClient(token, config.Domain);
        try
        {
            var result = await managementClient.Users.CreateAsync(new UserCreateRequest
            {
                Connection = "Username-Password-Authentication",
                Email = email,
                Password = password,
                AppMetadata = new
                {
                    Tenant = tenant
                }
            });
            return "";
        }
        catch (ErrorApiException ex)
        {
            return ex.ApiError.Message;
        }
    }

}
