using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.Interfaces
{
    public interface IBookingRepository
    {
        Task<bool> CreateBooking(string bookingId, int flightId, string seatType);
    }
}
