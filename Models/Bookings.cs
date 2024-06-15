namespace C_Sharp_Web_Programming_Final_Project_MySQL_Connection.Models
{
    public class Bookings
    {
        public int Booking_id { get; set; }
        public int User_id { get; set; }
        public DateOnly Date_Start { get; set; }
        public DateOnly Date_End { get; set; }
        public int Camping_id { get; set; }
        public double Price { get; set; }
    }
}
