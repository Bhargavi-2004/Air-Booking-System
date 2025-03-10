using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using DTO;
using Repository.Interfaces;
using System.Text.Json;
using System.Diagnostics;
using System.Web;
using System.Net.Http;
using System.Net.Http.Headers;
using Models;

namespace Repository
{
    public class FlightRepository : IFlightRepository
    {

        #region Private members
        private readonly HttpClient _httpClient;
        #endregion

        #region Constructor
        public FlightRepository()
        {
            var handler = new HttpClientHandler { UseCookies = true };
            _httpClient = new HttpClient(handler);
        }
        #endregion

        /// <summary>
        /// Search Flight
        /// </summary>
        /// <param name="flightDto"></param>
        /// <returns>List of flights</returns>
        public async Task<List<FlightDTO>> SearchFlight(FlightDTO flightDto)
        {
            if (flightDto.DepartureDate != null)
            {
            Debug.WriteLine($"Repository Received: {flightDto.DepartureDate}");
            }
            try
            {

                var queryParams = $"source={Uri.EscapeDataString(flightDto.Source)}" +
                  $"&destination={Uri.EscapeDataString(flightDto.Destination)}" +
                  $"&departureDate={Uri.EscapeDataString(flightDto.DepartureDate.ToString("MM/dd/yyyy"))}";
                  //$"&arrivalDate={Uri.EscapeDataString(flightDto.ArrivalDate.ToString("MM/dd/yyyy"))}";

                string requestUrl = $"https://localhost:7081/Api/FlightApi/search?{queryParams}";

                Debug.WriteLine($"Requesting URL: {requestUrl}"); // Debugging Output

                HttpResponseMessage response = await _httpClient.GetAsync(requestUrl);
                string responseBody = await response.Content.ReadAsStringAsync();
                Debug.WriteLine($"Raw API Response: {responseBody}");

                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<List<FlightDTO>>();
                }
                else
                {
                    Console.WriteLine($"Error: {response.StatusCode} - {await response.Content.ReadAsStringAsync()}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
            }
            return new List<FlightDTO>(); // Return empty list if error occurs
        }

        /// <summary>
        /// Flight Details
        /// </summary>
        /// <param name="flight"></param>
        /// <returns>Detail of specific flight</returns>
        public async Task<(Flights? flight, string? message)> FlightDetails(int flightId, string seatType)
        {
            try
            {
                string requestUrl = $"https://localhost:7081/Api/FlightApi/Details?flightId={flightId}&seatType={seatType}";
                Debug.WriteLine($"Requesting Flight Details: {requestUrl}");

                HttpResponseMessage response = await _httpClient.GetAsync(requestUrl);
                string responseBody = await response.Content.ReadAsStringAsync();
                Debug.WriteLine($"Raw API Response: {responseBody}");

                if (response.IsSuccessStatusCode)
                {
                    var messageResponse = await response.Content.ReadFromJsonAsync<Dictionary<string, object>>();
                    if (messageResponse != null && messageResponse.ContainsKey("message"))
                    {
                        return (null, messageResponse["message"].ToString()); // Return the message if present
                    }
                    else
                    {
                        var flight = JsonSerializer.Deserialize<Flights>(responseBody);
                        if (flight != null)
                        {
                            return (flight, null); // Flight found, no error message
                        }
                    }
                }
                else
                {
                    Console.WriteLine($"Error: {response.StatusCode} - {responseBody}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
            }

            return (null, "An error occurred while fetching flight details.");
        }

        public async Task<int> GetAvailableSeats(int flightId, string seatType)
        {
            int availableSeats = 0;

            Debug.WriteLine($"flightId: {flightId}, seatType: {seatType}");

            string requestUrl = $"https://localhost:7081/Api/FlightApi/GetAvailableSeats?flightId={flightId}&seatType={seatType}";
            Debug.WriteLine($"Generated URL: {requestUrl}");

            try
            {

                HttpResponseMessage response = await _httpClient.GetAsync(requestUrl);
                string responseBody = await response.Content.ReadAsStringAsync();
                if (response.IsSuccessStatusCode)
                {
                    if (int.TryParse(responseBody, out int result))
                    {
                        availableSeats = result;
                    }
                    else
                    {
                        Console.WriteLine("Error: Unable to parse available seats.");
                    }
                } 
                else
                {
                    Console.WriteLine($"Error: {response.StatusCode} - {responseBody}");
                }
            } 
            catch(Exception e)
            {
                Console.WriteLine(e.Message);
            }

            return availableSeats;
        }

    }
}
