using DTO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace API.Controllers.GuestApi
{
    [Route("Api/[controller]")]
    [ApiController]
    public class GuestApiController : ControllerBase
    {
        private readonly string _connectionString;

        public GuestApiController(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }
        [HttpPost("AddGuest")]
        public IActionResult AddGuest([FromBody] GuestDetailsDTO guest,[FromQuery] string bookingId, [FromQuery] string seatType)
        {
            if (string.IsNullOrEmpty(bookingId) || bookingId.Length != 8)
            {
                return BadRequest(new { message = "Invalid BookingId format" });
            }

            string query = @"
    INSERT INTO GuestDetails (Name, Email, PassengerType, Age, PhoneNumber, Gender, BirthDate, City, Region, PostalCode, PassportNumber, Nationality, BookingId) 
    VALUES (@Name, @Email, @PassengerType, @Age, @PhoneNumber, @Gender, @BirthDate, @City, @Region, @PostalCode, @PassportNumber, @Nationality, @bookingId)";

            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    connection.Open();  // Open connection

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        // Add parameters to prevent SQL injection
                        command.Parameters.AddWithValue("@Name", guest.Name);
                        command.Parameters.AddWithValue("@Email", guest.Email);
                        command.Parameters.AddWithValue("@PassengerType", guest.PassengerType);
                        command.Parameters.AddWithValue("@Age", (object?)guest.Age ?? DBNull.Value);
                        command.Parameters.AddWithValue("@PhoneNumber", guest.PhoneNumber);
                        command.Parameters.AddWithValue("@Gender", guest.Gender);
                        command.Parameters.AddWithValue("@BirthDate", (object?)guest.BirthDate ?? DBNull.Value);
                        command.Parameters.AddWithValue("@City", guest.City);
                        command.Parameters.AddWithValue("@Region", guest.Region);
                        command.Parameters.AddWithValue("@PostalCode", guest.PostalCode);
                        command.Parameters.AddWithValue("@PassportNumber", guest.PassportNumber);
                        command.Parameters.AddWithValue("@Nationality", guest.Nationality);
                        command.Parameters.AddWithValue("@BookingId", bookingId);

                        command.ExecuteNonQuery();  // Execute query
                    }
                }

                return Ok(new { message = "Guest added successfully" });
            }
            catch (SqlException sqlEx)
            {
                return StatusCode(500, new { message = "Database error occurred", error = sqlEx.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred", error = ex.Message });
            }
        }
        
        [HttpGet("GetGuests")]
        public IActionResult GetGuestsByBookingId(string bookingId)
        {
            var guests = new List<GuestDetailsDTO>();

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                string query = "SELECT * FROM GuestDetails WHERE BookingId = @BookingId";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@BookingId", bookingId);
                    connection.Open();

                    using (SqlDataReader reader = command.ExecuteReader()) // Removed Async
                    {
                        while (reader.Read())
                        {
                            guests.Add(new GuestDetailsDTO
                            {
                                Name = reader["Name"].ToString(),
                                Email = reader["Email"].ToString(),
                                PassengerType = reader["PassengerType"].ToString(),
                                Age = reader["Age"] != DBNull.Value ? Convert.ToInt32(reader["Age"]) : (int?)null,
                                PhoneNumber = reader["PhoneNumber"].ToString(),
                                Gender = reader["Gender"].ToString(),
                                BirthDate = reader["BirthDate"] != DBNull.Value ? Convert.ToDateTime(reader["BirthDate"]) : (DateTime?)null,
                                City = reader["City"].ToString(),
                                Region = reader["Region"].ToString(),
                                PostalCode = reader["PostalCode"].ToString(),
                                PassportNumber = reader["PassportNumber"].ToString(),
                                Nationality = reader["Nationality"].ToString(),
                            });
                        }
                    }
                }
            }

            return Ok(guests); // Return as HTTP response
        }




    }
}
