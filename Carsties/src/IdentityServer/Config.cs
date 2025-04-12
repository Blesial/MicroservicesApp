using Duende.IdentityServer.Models;

namespace IdentityServer;

public static class Config
{
    public static IEnumerable<IdentityResource> IdentityResources =>
        new IdentityResource[]
        {
            // Will receive two tokens
            // 1. Access token for API
            // 2. Identity token for authentication
            // The access token is used to access the API
            // The identity token is used to authenticate the user and get user information
            new IdentityResources.OpenId(),
            new IdentityResources.Profile(),
        };

    public static IEnumerable<ApiScope> ApiScopes =>
        new ApiScope[]
        {
            new ApiScope("auctionApp", "Auction App full access"),
        };

    public static IEnumerable<Client> Clients =>
        new Client[]
        {
            new Client 
            {
                ClientId = "postman",
                ClientName = "Postman",
                // We allowed those scopes because we want to get the user information
                // We want to get the access token for the API
                // We want to get the identity token for authentication
                AllowedScopes = { "openid", "profile", "auctionApp" },
                RedirectUris = {"https://www.getpostman.com/oauth2/callback"},
                ClientSecrets = new[] {new Secret("NotASecret".Sha256())},
                // Postman will need to send that to the token endpoint
                AllowedGrantTypes = GrantTypes.ResourceOwnerPassword,
            }
        };
}
