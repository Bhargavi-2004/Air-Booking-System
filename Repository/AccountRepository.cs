using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using DTO;
using Models;
using Repository.Interfaces;

namespace Repository
{
    public class AccountRepository : IAccountRepository
    {
        private readonly HttpClient _httpClient;

        public AccountRepository()
        {
            var handler = new HttpClientHandler { UseCookies = true };
            _httpClient = new HttpClient(handler);
        }

        public async Task<bool> Register(User user)
        {
            var response = await _httpClient.PostAsJsonAsync("https://localhost:7081/Api/AccountApi/Register", user);
            return response.IsSuccessStatusCode;
        }

        public async Task<string> Login(LoginDTO loginDto)
        {
            var response = await _httpClient.PostAsJsonAsync("https://localhost:7081/Api/AccountApi/Login", loginDto);

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<LoginResponseDTO>();
                return result?.Token ?? string.Empty;  // Return the token
            }

            return string.Empty;  // Return empty string if login fails
        }

    }
}
