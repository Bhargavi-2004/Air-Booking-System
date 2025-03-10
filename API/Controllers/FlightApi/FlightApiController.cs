using System.Diagnostics;
using System.Globalization;
using DTO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.VisualBasic;
using Models;

namespace API.Controllers.FlightApi
{
    [Route("Api/[controller]")]
    [ApiController]
    public class FlightApiController : ControllerBase
    {
        #region Private members
        private readonly IConfiguration _configuration;
        #endregion

        #region constructor
        public FlightApiController(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        #endregion

        /// <summary> Search
        /// </summary>
        /// <param name="fightDto"></param>
        /// <returns></returns>
        [HttpGet("search")]
        public IActionResult SearchFlight([FromQuery] string source, [FromQuery] string destination,
                          [FromQuery] string departureDate)
        {
            if (string.IsNullOrEmpty(source) || string.IsNullOrEmpty(destination) ||
                string.IsNullOrEmpty(departureDate) )
            {
                return BadRequest("Invalid flight data");
            }

            DateTime depDate;
            string[] formats = { "yyyy-MM-dd", "dd/MM/yyyy", "MM/dd/yyyy" }; // Allow both formats

            // Try parsing both formats
            bool depSuccess = DateTime.TryParseExact(departureDate, formats, CultureInfo.InvariantCulture, DateTimeStyles.None, out depDate);

            if (!depSuccess)
            {
                Debug.WriteLine($"Date Parsing Failed -> DepartureDate: {departureDate}");
                return BadRequest("Invalid date format. Use yyyy-MM-dd or dd/MM/yyyy.");
            }
            string connectionString = _configuration.GetConnectionString("DefaultConnection");
            List<FlightDTO> flights = new List<FlightDTO>();

            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    string query = @"SELECT FlightId, Source, Destination, DepartureDate, FlightImage  
                   FROM Flights 
                   WHERE Source = @Source 
                   AND Destination = @Destination 
                   AND CAST (DepartureDate AS DATE) = @DepartureDate"; 

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Source", source);
                        command.Parameters.AddWithValue("@Destination", destination);
                        command.Parameters.AddWithValue("@DepartureDate", depDate);

                        connection.Open();
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                flights.Add(new FlightDTO
                                {
                                    FlightId = reader.GetInt32(reader.GetOrdinal("FlightId")),
                                    Source = reader["Source"].ToString(),
                                    Destination = reader["Destination"].ToString(),
                                    DepartureDate = Convert.ToDateTime(reader["DepartureDate"]),
                                    FlightImage = reader["FlightImage"] != DBNull.Value ? reader["FlightImage"].ToString() : "~/images/default.png",
                                });
                            }
                        }
                    }
                }

                return Ok(flights);
            }
            catch (SqlException ex)
            {
                return StatusCode(500, "Database error: " + ex.Message);
            }
        }


        /// <summary>
        /// Fetch Flight Details
        /// </summary>
        /// <param name="flightId"></param>
        /// <returns></returns>
        [HttpGet("Details")]
        public IActionResult FlightDetails([FromQuery] int flightId, [FromQuery] string seatType)
        {
            if (flightId <= 0)
            {
                return BadRequest("Invalid Flight Id.");
            }

            string connectionString = _configuration.GetConnectionString("DefaultConnection");

            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    string query = @"
                            SELECT 
                                f.FlightId, f.FlightName, f.Source, f.Destination, f.DepartureDate, 
                                f.ArrivalDate, f.Price, f.FlightImage, f.FlightImageId,
                                sa.SeatId, st.SeatType, sa.AvailableSeats
                            FROM Flights f
                            LEFT JOIN Seat_Avail sa ON f.FlightId = sa.FlightId
                            LEFT JOIN Seats st ON sa.SeatId = st.SeatId
                            WHERE f.FlightId = @flightId AND st.SeatType = @seatType";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@flightId", flightId);
                        command.Parameters.AddWithValue("@seatType", seatType);

                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            if (!reader.Read()) // Directly check if data exists
                            {
                                return Ok(new
                                {
                                    message = "The selected flight does not have the requested seat type.",
                                    flightId = flightId,
                                    requestedSeatType = seatType
                                });

                            }

                            // Return a single object (not a list)
                            var flightDetails = new
                            {
                                FlightId = reader["FlightId"],
                                FlightName = reader["FlightName"],
                                Source = reader["Source"],
                                Destination = reader["Destination"],
                                DepartureDate = reader["DepartureDate"],
                                ArrivalDate = reader["ArrivalDate"],
                                Price = reader["Price"],
                                FlightImage = reader["FlightImage"],
                                SeatId = reader["SeatId"],
                                SeatType = reader["SeatType"],
                                AvailableSeats = reader["AvailableSeats"],
                                FlightImageId = reader.GetInt32(reader.GetOrdinal("FlightImageId"))
                            };

                            return Ok(flightDetails); // ✅ No list, just a single object
                        }
                    }
                }
            }
            catch (SqlException ex)
            {
                return StatusCode(500, "Database error: " + ex.Message);
            }
        }

        /// <summary>
        /// Get available seats for booking
        /// </summary>
        /// <param name="flightId"></param>
        /// <param name="seatTypeId"></param>
        /// <returns></returns>
        [HttpGet("GetAvailableSeats")]
        public int GetAvailableSeats([FromQuery] int flightId,[FromQuery] string seatType)
        {
            int availableSeats = 0;

            string query = @"
                        SELECT sa.AvailableSeats 
                        FROM Seat_Avail sa
                        JOIN Seats s ON sa.SeatId = s.SeatId
                        WHERE sa.FlightId = @FlightId AND s.SeatType = @seatType";

            string connectionString = _configuration.GetConnectionString("DefaultConnection");

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@FlightId", flightId);
                    cmd.Parameters.AddWithValue("@SeatType", seatType);

                    var result = cmd.ExecuteScalar();
                    if (result != null)
                    {
                        availableSeats = Convert.ToInt32(result);
                    }
                }
            }

            return availableSeats;
        }
    }
}
