using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IdentityServer4.Models;

namespace BankOfDotNet.IdentitySvr
{
    public class Config
    {
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
                new Client
                {
                    ClientId = "client",
                    AllowedGrantTypes = new List<string> {GrantType.ClientCredentials},
                    ClientSecrets =
                    {
                        new Secret("secret".Sha256())
                    },
                    AllowedScopes = { "bankOfDotNetApi.read" }

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

        public static IEnumerable<IdentityResource> GetIdentityResources()
        {
            return new[]
            {
                new IdentityResources.OpenId(),
                new IdentityResources.Profile(),
                new IdentityResources.Email(),
                new IdentityResource
                {
                    Name = "role",
                    UserClaims = new List<string> {"role"}
                }
            };
        }
    }
}
