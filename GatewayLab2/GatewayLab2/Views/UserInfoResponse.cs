using System.Text.Json.Serialization;

namespace GatewayLab2.Views
{
    public class UserInfoResponse<T>
    {
        [JsonPropertyName("reservations")]
        public IEnumerable<ReservationResponse> Reservations { get; set; }

        [JsonPropertyName("loyalty")]
        public T Loyalty { get; set; }
    }
}
