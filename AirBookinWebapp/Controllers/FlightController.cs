using System.Diagnostics;
using DTO;
using Microsoft.AspNetCore.Mvc;
using Models;
using Newtonsoft.Json;
using Repository.Interfaces;

namespace AirBookingApplication.Controllers
{
    public class FlightController : Controller
    {
        #region Private members
        private readonly IFlightRepository _flightRepository;
        #endregion

        #region Constructor
        public FlightController(IFlightRepository flightRepository)
        {
            _flightRepository = flightRepository;
        }
        #endregion

        #region Methods
        /// <summary>
        /// Search Flight
        /// </summary>
        /// <param name="flightDto"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> SearchFlight(FlightDTO flightDto)
        {
            if (flightDto.Source == null || flightDto.Destination == null || flightDto.DepartureDate == null)
            {
                ModelState.AddModelError(string.Empty, "All fields are required.");
                return View("Index", new List<FlightDTO>());
            }


            Debug.WriteLine($"Received: {flightDto.DepartureDate}");

            // Store search criteria in Session
            HttpContext.Session.SetString("Source", flightDto.Source);
            HttpContext.Session.SetString("Destination", flightDto.Destination);
            HttpContext.Session.SetString("DepartureDate", flightDto.DepartureDate.ToString());

            List<FlightDTO> flights = await _flightRepository.SearchFlight(flightDto);
            try
            {
                if (flights.Count > 0)
                {
                    foreach (var flight in flights)
                    {
                        Debug.WriteLine(flight.FlightId);
                    }

                    // Store flight results in Session (serialize the list)
                    HttpContext.Session.SetString("FlightResults", JsonConvert.SerializeObject(flights));

                    return View("SearchFlight", flights);
                }
                else
                {
                    ViewBag.ErrorMessage = "No flights found for the selected route.";
                }
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = "Error fetching flight details: " + ex.Message;
            }
            return View(flights);
        }

        /// <summary>
        /// Index page
        /// </summary>
        /// <returns></returns>
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet("Flight/Details")]
        /// <summary>
        /// Flight Detail
        /// </summary>
        /// <returns></returns>
        public async Task<IActionResult> Details(int id,string seatType)
        {
            (Flights? flight, string? message) = await _flightRepository.FlightDetails(id, seatType); // Ensure correct return type

            if (!string.IsNullOrEmpty(message))
            {
                ViewBag.Message = message;
                return View("NoSeatAvailable"); // Show custom view if message is present
            }

            if (flight == null)
            {
                ViewBag.Message = message ?? "The requested seat type is not available for this flight.";
                return View("NoSeatAvailable"); // Show custom view
            }

            ViewBag.Message = message ?? "The requested seat type is not available for this flight.";
            ViewBag.FlightId = id;
            ViewBag.SeatType = seatType;
            Random random = new Random();
            string bookingId = random.Next(0x10000000, int.MaxValue).ToString("X8"); // 8-char hex
            ViewBag.BookingId = bookingId;

            //ViewBag.BookingId = Guid.NewGuid().ToString("N").Substring(0, 10).ToUpper();

            return View(flight);
        }

        public IActionResult FlightSearchResults()
        {
            // Retrieve search criteria from Session
            var source = HttpContext.Session.GetString("Source");
            var destination = HttpContext.Session.GetString("Destination");
            var departureDate = HttpContext.Session.GetString("DepartureDate");
            var arrivalDate = HttpContext.Session.GetString("ArrivalDate");

            // Retrieve flight results from Session
            var flightResultsJson = HttpContext.Session.GetString("FlightResults");
            List<FlightDTO> flightResults = string.IsNullOrEmpty(flightResultsJson)
                ? new List<FlightDTO>()
                : JsonConvert.DeserializeObject<List<FlightDTO>>(flightResultsJson);

            // Pass data to the view
            ViewBag.Source = source;
            ViewBag.Destination = destination;
            ViewBag.DepartureDate = departureDate;
            ViewBag.ArrivalDate = arrivalDate;

            return View("SearchFlight", flightResults);
        }

        #endregion
    }
}
