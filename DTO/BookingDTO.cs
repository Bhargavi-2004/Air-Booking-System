namespace DTO
{
    public class BookingDTO
    {
        public string BookingId { get; set; }
        public int FlightId { get; set; }
        public string FlightName { get; set; }
        public string Source { get; set; }
        public string Destination { get; set; }
        public DateTime DepartureTime { get; set; }
        public string SeatType { get; set; }
        public int NumberOfPassengers { get; set; }
        public decimal TotalPrice { get; set; }
        public List<GuestDetailsDTO> Guests { get; set; } = new List<GuestDetailsDTO>();
    }

}
