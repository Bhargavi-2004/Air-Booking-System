using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Models;
using DTO;
using Repository.Interfaces;
using System.Diagnostics;
using Stripe.Checkout;
using Stripe.V2;

namespace AirBookingApplication.Controllers
{
    public class BookingController : Controller
    {
        #region Private members
        private readonly IFlightRepository _flightRepository;
        private readonly IGuestRepository _guestRepository;
        private readonly IBookingRepository _bookingRepository;
        private static List<GuestDetailsDTO> _guestList = new List<GuestDetailsDTO>();
        #endregion

        #region Constructor
        public BookingController(IFlightRepository flightRepository, IGuestRepository guestRepository, IBookingRepository bookingRepository)
        {
            _flightRepository = flightRepository;
            _guestRepository = guestRepository;
            _bookingRepository = bookingRepository;
        }

        #endregion
        [Authorize]  // This ensures only logged-in users can access this action
        public IActionResult Book(int flightId)
        {
            var token = Request.Cookies["AuthToken"]; // Read JWT from cookies

            if (string.IsNullOrEmpty(token))
            {
                // If no token, redirect to login and keep the returnUrl to redirect back after login
                return RedirectToAction("Login", "Account", new { returnUrl = Url.Action("Payment", "Booking", new { flightId }) });
            }

            // If token exists, proceed with booking
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GuestDetails(int flightId, string seatType, string bookingId)
        {
            ViewBag.FlightId = flightId;
            ViewBag.SeatType = seatType;
            ViewBag.BookingId = bookingId;

            try
            {
                bool success = await _bookingRepository.CreateBooking(bookingId, flightId, seatType);
                if (success)
                {
                    TempData["SuccessMesssage"] = "Booking is created Successfully.";
                }
                else
                {
                    TempData["ErrorMesssage"] = "Something went wrong, please try again.";
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
            return View();
        }

        [HttpPost]
        public IActionResult AddGuest(List<GuestDetailsDTO> guests)
        {
            guests.Add(new GuestDetailsDTO());
            return View("GuestDetails", guests);
        }

        [HttpPost]
        public IActionResult Payment(string bookingId, decimal TotalPrice)
        {
            Debug.WriteLine($"{bookingId} {TotalPrice}");
            var options = new SessionCreateOptions
            {
                PaymentMethodTypes = new List<string> { "card" },
                LineItems = new List<SessionLineItemOptions>
                {
                    new SessionLineItemOptions
                    {
                        PriceData = new SessionLineItemPriceDataOptions
                        {
                            Currency = "usd",
                            ProductData = new SessionLineItemPriceDataProductDataOptions
                            {
                                Name = "Flight Booking"
                            },
                            UnitAmount = (long)(TotalPrice * 100),
                        },
                        Quantity = 1,
                    },
                },
                Mode = "payment",
                SuccessUrl = "https://localhost:7289/Booking/Success",
                CancelUrl = "https://localhost:7289/Booking/Cancel",
            };

            var service = new SessionService();
            Session session = service.Create(options);

            return Redirect(session.Url);
        }

        public IActionResult Success()
        {
            return View();
        }

        public ActionResult Cancel()
        {
            return View();
        }
    }
}
