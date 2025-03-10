using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace API.Controllers.BookingApi
{
    [Route("Api/[controller]")]
    [ApiController]
    public class BookingApiController : ControllerBase
    {
        private readonly string _connectionString;

        public BookingApiController(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }



        [HttpGet("CreateBooking")]
        public IActionResult CreateBooking([FromQuery]string bookingId, [FromQuery] int flightId, [FromQuery] string seatType)
        {

            string query = @"
                            INSERT INTO Bookings (BookingId, FlightId, SeatType) 
                            VALUES (@bookingId, @flightId, @seatType)";

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@bookingId", bookingId);
                    command.Parameters.AddWithValue("@flightId", flightId);
                    command.Parameters.AddWithValue("@seatType", seatType);
                    command.ExecuteNonQuery();
                }
            }

            return Ok(new { BookingId = bookingId });
        }

    }
}
