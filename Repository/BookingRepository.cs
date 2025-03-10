using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Repository.Interfaces;

namespace Repository
{
    public class BookingRepository : IBookingRepository
    {
        #region Private members
        private readonly HttpClient _httpClient;
        #endregion

        #region Constructor
        public BookingRepository()
        {
            var handler = new HttpClientHandler { UseCookies = true };
            _httpClient = new HttpClient(handler);
        }
        #endregion
        public async Task<bool> CreateBooking(string bookingId, int flightId, string seatType)
        {
            string requestUrl = $"https://localhost:7081/Api/BookingApi/CreateBooking?flightId={flightId}&seatType={seatType}&bookingId={bookingId}";

            try
            {
                HttpResponseMessage response = await _httpClient.GetAsync(requestUrl);
                string responseBody = await response.Content.ReadAsStringAsync();
                if (response.IsSuccessStatusCode)
                {
                    return true;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
            return false;
        }
        
    }
}
