namespace DTO
{
    public class GuestDetailsDTO
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string PassengerType { get; set; } // "Adult" or "Child"
        public int? Age { get; set; }
        public string PhoneNumber { get; set; }
        public string Gender { get; set; }
        public DateTime? BirthDate { get; set; }
        public string City { get; set; }
        public string Region { get; set; }
        public string PostalCode { get; set; }
        public string PassportNumber { get; set; }
        public string Nationality { get; set; }
    }

}