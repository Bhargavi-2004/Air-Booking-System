using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DTO;

namespace Repository.Interfaces
{
    public interface IGuestRepository
    {
        Task<bool> AddGuestdetails(GuestDetailsDTO guestDetails, int flightId, string seatType,string bookingId);
        Task<List<GuestDetailsDTO>> GetGuestsByBookingId(string bookingId);
    }
}
