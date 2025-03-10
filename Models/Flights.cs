using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Models
{
    public class Flights
    {
        [Key] // Marks as Primary Key
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)] // Auto-Increment
        public int FlightId { get; set; }

        [Required]
        [StringLength(100)]
        public string FlightName { get; set; }

        [Required]
        [StringLength(50)]
        public string Source { get; set; }

        [Required]
        [StringLength(50)]
        public string Destination { get; set; }

        [Required]
        [DataType(DataType.DateTime)]
        public DateTime DepartureDate { get; set; }

        [Required]
        [DataType(DataType.DateTime)]
        public DateTime ArrivalDate { get; set; }

        [Required]
        [Column(TypeName = "decimal(10,2)")]
        public decimal Price { get; set; }

        [StringLength(255)]
        public string FlightImage { get; set; }

        public int FlightImageId { get; set; }

        [DataType(DataType.DateTime)]
        [DatabaseGenerated(DatabaseGeneratedOption.Computed)] // Default value (GETDATE)
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [DataType(DataType.DateTime)]
        [DatabaseGenerated(DatabaseGeneratedOption.Computed)] // Default value (GETDATE)
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
    }
}
