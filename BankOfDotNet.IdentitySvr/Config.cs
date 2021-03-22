using System.Collections.Generic;
using IdentityServer4.Models;
using IdentityServer4.Test;
using IdentityServer4;

namespace BankOfDotNet.IdentitySvr
{
    public class Config
    {

        public static IEnumerable<IdentityResource> GetIdentityResources()
        {
            return new List<IdentityResource>
            {
                new IdentityResources.OpenId(),
                new IdentityResources.Profile()
            };
        }

        public static List<TestUser> GetUsers()
        {
           return new List<TestUser>
           {
               new TestUser
               {
                   SubjectId = "1",
                   Username = "dariel",
                   Password = "dariel"
               },
               new TestUser
               {
                   SubjectId = "2",
                   Username = "daniela",
                   Password = "daniela"
               }
           };
        }

        public static IEnumerable<ApiResource> GetAllApiResources()
        {

            return new List<ApiResource>
            {
                new ApiResource
                {
                    Name = "bankOfDotNetApi",
                    DisplayName = "Customer Api For BankOfDotNet",
                    Description = "Allow the application to access API #1 on your behalf",
                    Scopes = new List<string> {"bankOfDotNetApi.read", "bankOfDotNetApi.write"},
                    ApiSecrets = new List<Secret> {new Secret("secret".Sha256())},
                    UserClaims = new List<string> {"role"}
                }
            };
        }

        public static IEnumerable<Client> GetClients()
        {
            return new List<Client>
            {
                //Client-Credential based grant type
                new Client
                {
                    ClientId = "client",
                    AllowedGrantTypes = new List<string> {GrantType.ClientCredentials},
                    ClientSecrets =
                    {
                        new Secret("secret".Sha256())
                    },
                    AllowedScopes = { "bankOfDotNetApi.read" }

                },

                //Resource Owner Grant Type
                new Client
                {
                    ClientId = "ro.client",
                    AllowedGrantTypes = GrantTypes.ResourceOwnerPassword,
                    ClientSecrets =
                    {
                        new Secret("secret".Sha256())
                    },
                    AllowedScopes = { "bankOfDotNetApi.read" }
                },

                //Implicit Flow Grant Type
                new Client
                {
                    ClientId = "mvc",
                    ClientName = "MVC Client",
                    AllowedGrantTypes = GrantTypes.Implicit,
                    RedirectUris = {"http://localhost:5003/signin-oidc"},
                    PostLogoutRedirectUris = { "http://localhost:5003/signout-callback-oidc"},
                    AllowedScopes = new List<string>
                    {
                        IdentityServerConstants.StandardScopes.OpenId,
                        IdentityServerConstants.StandardScopes.Profile,
                    },
                    //AllowAccessTokensViaBrowser = true
                }
            };
        }

        public static IEnumerable<ApiScope> GetApiScopes()
        {
            return new List<ApiScope>
            {
                new ApiScope(name: "bankOfDotNetApi.read",   displayName: "Read your data."),
                new ApiScope(name: "bankOfDotNetApi.write",  displayName: "Write your data."),
                new ApiScope(name: "bankOfDotNetApi.delete", displayName: "Delete your data."),
                new ApiScope(name: "bankOfDotNetApi", displayName: "manage bankOfDotNetApi api endpoints.")
            };
        }

        //public static IEnumerable<IdentityResource> GetIdentityResources()
        //{
        //    return new[]
        //    {
        //        new IdentityResources.OpenId(),
        //        new IdentityResources.Profile(),
        //        new IdentityResources.Email(),
        //        new IdentityResource
        //        {
        //            Name = "role",
        //            UserClaims = new List<string> {"role"}
        //        }
        //    };
        //}
    }
}
