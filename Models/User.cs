namespace C_Sharp_Web_Programming_Final_Project_MySQL_Connection.Models
{
    public class User
    {
        public int User_id { get; set; }
        public string F_Name { get; set; }
        public string L_Name { get; set; }
        public string Phone_Number { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public byte[] Image { get; set; }
        public bool Is_Owner { get; set; }
    }
}
