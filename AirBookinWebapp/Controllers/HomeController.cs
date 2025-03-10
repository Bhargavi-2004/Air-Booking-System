using Microsoft.AspNetCore.Mvc;
using DTO;
using Microsoft.Data.SqlClient;

namespace AirBookinWebapp.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IConfiguration _configuration;
        private readonly string _connectionString;

        public HomeController(ILogger<HomeController> logger, IConfiguration configuration)
        {
            _configuration = configuration;
            _logger = logger;
            _connectionString = _configuration.GetConnectionString("DefaultConnection"); //  Correct assignment
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Get(string from, string to, string date)
        {
            _logger.LogInformation("Search Request Received: From = {From}, To = {To}, Date = {Date}", from, to, date);

            return Json(new { message = "Search request logged successfully!" });
        }

        [HttpPost]
        public IActionResult Get(FlightDTO flightDto)
        {

            return View();
        }

    }
}
