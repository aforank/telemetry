using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace Booking.API
{
    public class EstimateRide
    {
        private static readonly Meter BookingMeter = new Meter("BookingMeter", "1.0");
        private static readonly Counter<double> RideAmount = BookingMeter.CreateCounter<double>("RideAmount");
        public static ActivitySource Source = new ActivitySource("EstimateRide.Component");

        public static double EstimatePrice()
        {
            using (var activity = Source.StartActivity("Estimating Ride Price"))
            {
                var price = Random.Shared.NextDouble() * 100;
                activity.SetTag("RidePrice", price);
                RideAmount.Add(price, new KeyValuePair<string, object?>("destination", "Taj Mahal"));
                return price;
            }
        }
    }
}
