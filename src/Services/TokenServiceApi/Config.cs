using System.Collections.Generic;
using System.Security.Claims;
using IdentityServer4;
using IdentityServer4.Models;
using IdentityServer4.Test;
using Microsoft.Extensions.Configuration;

namespace TokenServiceApi
{
    public class Config
    {
        public static List<TestUser> GetUsers()
        {
            return new List<TestUser>
            {
                new TestUser
                {
                    SubjectId = "1",
                    Username = "admin",
                    Password = "password",

                    Claims = new []
                    {
                        new Claim("name", "Admin")
                    }
                }
            };
        }

        public static Dictionary<string, string> GetUrls(IConfiguration configuration)
        {
            var urls = new Dictionary<string,string>();
            urls.Add("Mvc",configuration.GetValue<string>("MvcClient"));
            urls.Add("BasketApi", configuration.GetValue<string>("BasketApiClient"));
            return urls;
        }

        public static IEnumerable<ApiResource> GetApiResources()
        {
            return new List<ApiResource>()
            {
                new ApiResource("basket","Shopping Cart Api"),
                new ApiResource("orders","Ordering Api")
            };
        }


        public static IEnumerable<IdentityResource> GetIdentityResources()
        {
            return new List<IdentityResource>()
            {
                new IdentityResources.OpenId(),
                new IdentityResources.Profile()
            };
        }

        public static IEnumerable<Client> GetClients(Dictionary<string, string> clientUrls)
        {
            return new List<Client>()
            {
                new Client()
                {
                    ClientId = "mvc",
                    ClientName = "MVC Client",
                    ClientSecrets = new []{new Secret("secret".Sha256()) },
                    AllowedGrantTypes = GrantTypes.Hybrid,
                    RedirectUris = {$"{clientUrls["Mvc"]}/signin-oidc" },
                    PostLogoutRedirectUris = {$"{clientUrls["Mvc"]}/signout-callback-oidc" },
                    AllowedScopes = new List<string>()
                    {
                        IdentityServerConstants.StandardScopes.OpenId,
                        IdentityServerConstants.StandardScopes.Profile,
                        IdentityServerConstants.StandardScopes.OfflineAccess,
                        "orders",
                        "basket"
                    },
                    AllowOfflineAccess = true
                },
                new Client()
                {
                    ClientId = "basketswaggerui",
                    ClientName = "Basket Swagger UI",
                    
                    AllowedGrantTypes = GrantTypes.Implicit,
                    AllowAccessTokensViaBrowser = true,
                    
                    RedirectUris = {$"{clientUrls["BasketApi"]}/swagger/oauth2-redirect.html" }, // o2c.html   oauth2-redirect.html
                    //PostLogoutRedirectUris = {$"{clientUrls["BasketApi"]}/swagger/" },
                    
                    AllowedScopes = new List<string>()
                    {
                        "basket"
                    }
                }
            };
        }

    }
}