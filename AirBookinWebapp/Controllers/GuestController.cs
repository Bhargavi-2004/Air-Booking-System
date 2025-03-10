using Microsoft.AspNetCore.Mvc;
using Models;
using DTO;
using Repository.Interfaces;

public class GuestController : Controller
{
    #region Private members
    private readonly IFlightRepository _flightRepository;
    private readonly IGuestRepository _guestRepository;
    private static List<GuestDetailsDTO> _guestList = new List<GuestDetailsDTO>();
    #endregion

    #region Constructor
    public GuestController(IFlightRepository flightRepository, IGuestRepository guestRepository)
    {
        _flightRepository = flightRepository;
        _guestRepository = guestRepository;
    }
    #endregion

    [HttpGet]
    public async Task<IActionResult> AddGuest(List<GuestDetailsDTO> ExistingGuests,GuestDetailsDTO NewGuest,  int flightId, string seatType,string bookingId)
   {
        ViewBag.FlightId = flightId;
        ViewBag.SeatType = seatType;
        ViewBag.BookingId = bookingId;

        int availableSeats = await _flightRepository.GetAvailableSeats(flightId, seatType);
        var guestList = new List<GuestDetailsDTO>();

        if (ExistingGuests != null)
        {
            guestList.AddRange(ExistingGuests);
        }

        if (guestList.Count >= availableSeats)
        {
            TempData["ErrorMessage"] = $"Only {availableSeats} seats are available. You cannot add more guests.";
            return View("~/Views/Booking/GuestDetails.cshtml", guestList);
        }

        if (NewGuest != null)
        {
            guestList.Add(NewGuest);
        }

        // Send each guest to the repository (which calls the API)
        foreach (var guest in guestList)
        {
            bool success = await _guestRepository.AddGuestdetails(guest, flightId, seatType,bookingId);

            if (!success)
            {
                TempData["ErrorMessage"] = "An error occurred while adding guest details.";
                return View("~/Views/Booking/GuestDetails.cshtml", guestList);
            }
        }

        return View("~/Views/Booking/GuestDetails.cshtml", guestList);
    }

    [HttpPost]
    public async Task<IActionResult> SubmitGuestDetails(string bookingId, int flightId, string seatType)
    {
        var token = Request.Cookies["AuthToken"]; // Extract JWT token from cookie

        if (string.IsNullOrEmpty(token))
        {
            return RedirectToAction("LoginGuest", new { returnUrl = Url.Action("SubmitGuestDetails", "Guest") });
        }

        (Flights? flight, string? message) = await _flightRepository.FlightDetails(flightId, seatType); // Ensure correct return type

        if (flight == null)
        {
            ViewBag.Message = message ?? "The requested seat type is not available for this flight.";
            return BadRequest("Flight details not found!");
        }

        // Fetch all guests with the given Booking ID
        var guests = await _guestRepository.GetGuestsByBookingId(bookingId);

        if (guests == null || !guests.Any())
        {
            return BadRequest("Guest list is empty!");
        }

        ViewBag.Flight = flight;
        ViewBag.Guests = guests;
        ViewBag.BookingId = bookingId;

        var bookingDto = new BookingDTO
        {
            BookingId = bookingId,
            FlightId = flight.FlightId,
            FlightName = flight.FlightName,
            Source = flight.Source,
            Destination = flight.Destination,
            DepartureTime = flight.DepartureDate,
            SeatType = seatType,
            NumberOfPassengers = guests.Count,
            TotalPrice = guests.Count * flight.Price, // Assuming price per seat is available in flight
            Guests = guests
        };

        return View(bookingDto); // Load the checkout page
    }

    [HttpGet]
    public IActionResult LoginGuest(string returnUrl)
    {
        ViewBag.ReturnUrl = returnUrl;
        return View();
    }
}
