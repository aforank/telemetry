namespace Booking.API
{
    public class BookingResponse
    {
        private BookingDetails bookingDetails;

        public BookingResponse(BookingDetails bookingDetails)
        {
            this.bookingDetails = bookingDetails;
        }
    }
}