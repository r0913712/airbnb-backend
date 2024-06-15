namespace C_Sharp_Web_Programming_Final_Project_MySQL_Connection.Models
{
    public class Availability
    {
        public int Availability_id { get; set; }
        public int Camping_ID { get; set; }
        public DateOnly Date { get; set; }
        public bool Available { get; set; }

    }
}
