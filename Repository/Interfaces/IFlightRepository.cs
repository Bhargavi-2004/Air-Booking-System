using DTO;
using Models;

namespace Repository.Interfaces
{
    public interface IFlightRepository
    {
        Task <List<FlightDTO>> SearchFlight(FlightDTO flightDto);

        Task<(Flights? flight, string? message)> FlightDetails(int flightId, string seatType);

        Task<int> GetAvailableSeats(int flightId, string seatType);
    }
}
