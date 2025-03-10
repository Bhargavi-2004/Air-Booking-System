using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace DTO
{
    public class FlightDTO
    {
        public int FlightId { get; set; }

        [Required(ErrorMessage = "Source is required")]
        [StringLength(50)]
        public string Source { get; set; }

        [Required(ErrorMessage = "Destination is required")]
        [StringLength(50)]
        public string Destination { get; set; }

        [Required(ErrorMessage = "Departure Date is required")]
        [JsonConverter(typeof(JsonDateConverter))] // Custom Converter to ensure yyyy-MM-dd format
        public DateTime DepartureDate { get; set; }

        //[Required(ErrorMessage = "Arrival Date is required")]
        //[JsonConverter(typeof(JsonDateConverter))]
        //public DateTime ArrivalDate { get; set; }

        [StringLength(255)]
        public string? FlightImage { get; set; }
    }
    public class JsonDateConverter : JsonConverter<DateTime>
    {
        private readonly string format = "yyyy-MM-dd";

        public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            return DateTime.ParseExact(reader.GetString(), format, CultureInfo.InvariantCulture);
        }

        public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString(format));
        }
    }
}
