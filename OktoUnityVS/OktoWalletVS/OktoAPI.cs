using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace OktoWallet.OktoSDK
{
    public class OktoSDK
    {
        private static HttpClient httpClient;
        private readonly string apiKey;
        private AuthDetails authDetails;
        private readonly string baseUrl;

        public OktoSDK(string apiKey, string buildType)
        {
            this.apiKey = apiKey;
            baseUrl = GetBaseUrl(buildType);
            httpClient = new HttpClient();
            httpClient.BaseAddress = new Uri(baseUrl);
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            httpClient.DefaultRequestHeaders.Add("x-api-key", apiKey);
        }

        private string GetBaseUrl(string buildType)
        {
            if (buildType == "Production")
            {
                return "https://apigw.okto.tech";
            }
            else if (buildType == "Staging")
            {
                return "https://3p-bff.oktostage.com";
            }
            else
            {
                return "https://sandbox-api.okto.tech";
            }
        }

        private void SetAuthorizationHeader(string authToken)
        {
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authToken);
        }

        public async Task UpdateAuthDetails(AuthDetails newAuthDetails)
        {
            authDetails = newAuthDetails;
            SetAuthorizationHeader(authDetails.AuthToken);
            await SaveAuthDetailsToLocalStorage(authDetails);
        }

        private async Task<AuthDetails> LoadAuthDetailsFromLocalStorage()
        {
            string storedAuthDetails = ""; 
            return JsonConvert.DeserializeObject<AuthDetails>(storedAuthDetails);
        }

        private async Task SaveAuthDetailsToLocalStorage(AuthDetails details)
        {
            string authDetailsJson = JsonConvert.SerializeObject(details);
        }


        public async Task<(object result, Exception error)> AuthenticateAsync(string idToken)
        {
            if (httpClient == null)
            {
                return (null, new Exception("SDK is not initialized"));
            }

            try
            {
                var requestBody = new
                {
                    id_token = idToken
                };
                var jsonContent = new StringContent(JsonConvert.SerializeObject(requestBody), Encoding.UTF8, "application/json");

                httpClient.DefaultRequestHeaders.Clear();
                httpClient.DefaultRequestHeaders.Add("Accept", "*/*");
                httpClient.DefaultRequestHeaders.Add("x-api-key", apiKey);

                var response = await httpClient.PostAsync($"{baseUrl}/api/v2/authenticate", jsonContent);

                if (response.IsSuccessStatusCode)
                {
                    var responseData = JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync());
                    if (responseData?.status == "success")
                    {
                        if (responseData?.data?.auth_token != null)
                        {
                            var authDetailsNew = new AuthDetails
                            {
                                AuthToken = responseData.data.auth_token,
                                RefreshToken = responseData.data.refresh_auth_token,
                                DeviceToken = responseData.data.device_token
                            };
                            UpdateAuthDetails(authDetailsNew);
                            return (authDetailsNew, null);
                        }
                        
                    }
                    return (null, new Exception("Server responded with an error"));
                }
                return (null, new Exception("Server responded with an error"));
            }
            catch (Exception ex)
            {
                return (null, ex);
            }
        }

        public async Task<(object result, Exception error)> AuthenticateWithUserIdAsync(string userId, string jwtToken)
        {
            if (httpClient == null)
            {
                return (null, new Exception("SDK is not initialized"));
            }

            try
            {
                var requestBody = new
                {
                    user_id = userId,
                    auth_token = jwtToken
                };
                var jsonContent = new StringContent(JsonConvert.SerializeObject(requestBody), Encoding.UTF8, "application/json");

                httpClient.DefaultRequestHeaders.Clear();
                httpClient.DefaultRequestHeaders.Add("Accept", "*/*");
                httpClient.DefaultRequestHeaders.Add("x-api-key", apiKey);

                var response = await httpClient.PostAsync($"{baseUrl}/api/v1/jwt-authenticate", jsonContent);

                if (response.IsSuccessStatusCode)
                {
                    var responseData = JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync());
                    if (responseData?.status == "success")
                    {
                        var authDetailsNew = new AuthDetails
                        {
                            AuthToken = responseData.data.auth_token,
                            RefreshToken = responseData.data.refresh_auth_token,
                            DeviceToken = responseData.data.device_token
                        };
                        UpdateAuthDetails(authDetailsNew);
                        return (authDetailsNew, null);
                    }
                    return (null, new Exception("Server responded with an error"));
                }
                return (null, new Exception("Server responded with an error"));
            }
            catch (Exception ex)
            {
                return (null, ex);
            }
        }

        public async Task<AuthDetails> RefreshToken()
        {
            if (authDetails != null)
            {
                try
                {
                    var request = new HttpRequestMessage(HttpMethod.Post, "/api/v1/refresh_token");
                    request.Headers.Add("x-refresh-authorization", $"Bearer {authDetails.RefreshToken}");
                    request.Headers.Add("x-device-token", authDetails.DeviceToken);

                    var response = await httpClient.SendAsync(request);
                    response.EnsureSuccessStatusCode();

                    var content = await response.Content.ReadAsStringAsync();
                    var authResponse = JsonConvert.DeserializeObject<AuthResponse>(content);

                    var newAuthDetails = new AuthDetails
                    {
                        AuthToken = authResponse.Data.AuthToken,
                        RefreshToken = authResponse.Data.RefreshToken,
                        DeviceToken = authResponse.Data.DeviceToken
                    };

                    await UpdateAuthDetails(newAuthDetails);
                    return newAuthDetails;
                }
                catch (Exception ex)
                {
                    throw new Exception("Failed to refresh token: " + ex.Message);
                }
            }
            return null;
        }

        public async Task<ApiResponse<T>> MakeGetRequest<T>(string endpoint, string queryUrl = null)
        {
            var url = queryUrl != null ? $"{endpoint}?{queryUrl}" : endpoint;

            try
            {
                var response = await httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();
                var content = await response.Content.ReadAsStringAsync();
                var apiResponse = JsonConvert.DeserializeObject<ApiResponse<T>>(content);

                if (apiResponse.Status == "SUCCESS")
                {
                    return apiResponse;
                }

                throw new Exception("Server responded with an error.");
            }
            catch (Exception ex)
            {
                throw new Exception("Request failed: " + ex.Message);
            }
        }

        public async Task<ApiResponse<T>> MakePostRequest<T>(string endpoint, object data)
        {
            try
            {
                var jsonContent = JsonConvert.SerializeObject(data);
                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
                var response = await httpClient.PostAsync(endpoint, content);

                response.EnsureSuccessStatusCode();

                var responseContent = await response.Content.ReadAsStringAsync();
                var apiResponse = JsonConvert.DeserializeObject<ApiResponse<T>>(responseContent);

                if (apiResponse.Status == "SUCCESS")
                {
                    return apiResponse;
                }

                throw new Exception("Server responded with an error.");
            }
            catch (Exception ex)
            {
                throw new Exception("Request failed: " + ex.Message);
            }
        }

        public async Task<PortfolioData> GetPortfolio()
        {
            ApiResponse<PortfolioData> response = await MakeGetRequest<PortfolioData>("/v1/portfolio");
            return response.Data;
        }

        public async Task<TokensData> GetSupportedTokens()
        {
            ApiResponse<TokensData> response = await MakeGetRequest<TokensData>("/v1/supported/tokens");
            return response.Data;
        }

        public async Task<User> GetUserDetails()
        {
            ApiResponse<User> response = await MakeGetRequest<User>("/v1/user_from_token");
            return response.Data;
        }

        public async Task<Order> TransferTokens(TransferTokensData data)
        {
            ApiResponse<Order> response = await MakePostRequest<Order>("/v1/transfer/tokens/execute", data);
            return response.Data;
        }

        public async Task<NftOrderDetails> TransferNft(TransferNftData data)
        {
            ApiResponse<NftOrderDetails> response = await MakePostRequest<NftOrderDetails>("/v1/nft/transfer", data);
            return response.Data;
        }
        public class Order
        {
            public string OrderId { get; set; }          
            public string NetworkName { get; set; }       
            public string OrderType { get; set; }         
            public string Status { get; set; }           
            public string TransactionHash { get; set; }  
        }


        public interface ApiResponse<T>
        {
            string Status { get; set; }
            T Data { get; set; }
        }

        public class AuthDetails
        {
            public string AuthToken { get; set; }     
            public string RefreshToken { get; set; }  
            public string DeviceToken { get; set; }    
        }


        public class AuthResponse
        {
            public AuthResponseData Data { get; set; }
        }

        public class AuthResponseData
        {
            public string AuthToken { get; set; }
            public string RefreshToken { get; set; }
            public string DeviceToken { get; set; }
        }

        public class PortfolioData
        {
            public Portfolio[] Tokens { get; set; }
            public decimal Total { get; set; }
        }

        public class Portfolio
        {
            public string TokenName { get; set; }
            public string TokenImage { get; set; }
            public string TokenAddress { get; set; }
            public string NetworkName { get; set; }
            public string Quantity { get; set; }
            public string AmountInINR { get; set; }
        }

        public class TokensData
        {
            public Token[] Tokens { get; set; }
        }

        public class Token
        {
            public string TokenName { get; set; }
            public string TokenAddress { get; set; }
            public string NetworkName { get; set; }
        }

        public class User
        {
            public string Email { get; set; }
            public string UserId { get; set; }
            public string CreatedAt { get; set; }
            public string Freezed { get; set; }
            public string FreezeReason { get; set; }
        }

        public class TransferTokensData
        {
            public string NetworkName { get; set; }
            public string TokenAddress { get; set; }
            public string Quantity { get; set; }
            public string RecipientAddress { get; set; }
        }

        public class NftOrderDetails
        {
            public string ExplorerSmartContractUrl { get; set; }
            public string Description { get; set; }
            public string Type { get; set; }
            public string CollectionId { get; set; }
            public string CollectionName { get; set; }
            public string NftTokenId { get; set; }
            public string TokenUri { get; set; }
            public string Id { get; set; }
            public string Image { get; set; }
            public string CollectionAddress { get; set; }
            public string CollectionImage { get; set; }
            public string NetworkName { get; set; }
            public string NetworkId { get; set; }
            public string NftName { get; set; }
        }

        public class TransferNftData
        {
            public string OperationType { get; set; }
            public string NetworkName { get; set; }
            public string CollectionAddress { get; set; }
            public string CollectionName { get; set; }
            public string Quantity { get; set; }
            public string RecipientAddress { get; set; }
            public string NftAddress { get; set; }
        }
    }
}
