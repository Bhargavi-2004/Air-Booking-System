using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using DTO;
using Models;
using Repository.Interfaces;
using System.Text.Json;

namespace Repository
{
    public class GuestRepository : IGuestRepository
    {
        private readonly HttpClient _httpClient;

        public GuestRepository(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<bool> AddGuestdetails(GuestDetailsDTO guest, int flightId, string seatType,string bookingId)
        {
            string requestUrl = $"https://localhost:7081/Api/GuestApi/AddGuest?flightId={flightId}&seatType={seatType}&bookingId={bookingId}";

            var requestBody = new
            {
                guest.Name,
                guest.Email,
                guest.PassengerType,
                guest.Age,
                guest.PhoneNumber,
                guest.Gender,
                guest.BirthDate,
                guest.City,
                guest.Region,
                guest.PostalCode,
                guest.PassportNumber,
                guest.Nationality,
                FlightId = flightId,
                SeatType = seatType
            };

            HttpResponseMessage response = await _httpClient.PostAsJsonAsync(requestUrl, requestBody);

            return response.IsSuccessStatusCode;
        }

        public async Task<List<GuestDetailsDTO>> GetGuestsByBookingId(string bookingId)
        {
            string requestUrl = $"https://localhost:7081/Api/GuestApi/GetGuests?bookingId={bookingId}";

            HttpResponseMessage response = await _httpClient.GetAsync(requestUrl);

            if (!response.IsSuccessStatusCode)
            {
                return new List<GuestDetailsDTO>(); // Return empty list if API fails
            }

            string jsonString = await response.Content.ReadAsStringAsync();

            return JsonSerializer.Deserialize<List<GuestDetailsDTO>>(jsonString, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true // Ensures case-insensitive mapping
            }) ?? new List<GuestDetailsDTO>();

        }

    }

}
