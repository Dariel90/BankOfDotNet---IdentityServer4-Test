using IdentityModel.Client;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;


namespace BankOfDotNet.ConsoleClient
{
    class Program
    {
        private static HttpClient _client;
        
        public static void Main(string[] args){
            if (_client == null)
            {
                _client = new HttpClient();
            }
            MainAsync().GetAwaiter().GetResult();
            
        }
        
        private static async Task MainAsync()
        {
            try
            {
                //Discover all endpoint using metadata og identity sever
                var client = new HttpClient();
                var apiUri = "http://localhost:5000";
                var disco = await _client.GetDiscoveryDocumentAsync(apiUri);
                if (disco.IsError)
                {
                    Console.WriteLine(disco.Error);
                    return;
                }

                //Grab a bearer token
                var tokenResponse = await client.RequestClientCredentialsTokenAsync(new ClientCredentialsTokenRequest
                {
                    Address = disco.TokenEndpoint,
                    ClientId = "client",
                    ClientSecret = "secret",
                    Scope = "bankOfDotNetApi.read",
                });

                if (tokenResponse.IsError)
                {
                    Console.WriteLine(tokenResponse.Error);
                    return;
                }

                Console.WriteLine(tokenResponse.Json);
                Console.WriteLine("\n\n");

                //Consume Customer API
                _client.SetBearerToken(tokenResponse.AccessToken);

                var customerInfo = new StringContent(
                    JsonConvert.SerializeObject(new
                    {
                        Id = 6,
                        FirstName = "Dariel",
                        LastName = "Amores"
                    }), Encoding.UTF8, "application/json");

                var createCustomerResponse = await _client.PostAsync("http://localhost:16181/api/customers", customerInfo);

                if (!createCustomerResponse.IsSuccessStatusCode)
                {
                    Console.WriteLine(createCustomerResponse.StatusCode);
                }

                var getCustomerResponse = await _client.GetAsync("http://localhost:16181/api/customers");               
                if (!getCustomerResponse.IsSuccessStatusCode)
                {
                    Console.WriteLine(getCustomerResponse.StatusCode);
                }
                else
                {
                    var content = await getCustomerResponse.Content.ReadAsStringAsync();
                    Console.WriteLine(JArray.Parse(content));
                }

                Console.Read();
            }
            catch (Exception e)
            {

                Console.WriteLine(e.Message);
            }
            
        }
        
    }
}
