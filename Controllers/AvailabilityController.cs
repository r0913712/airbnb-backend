using C_Sharp_Web_Programming_Final_Project_MySQL_Connection.Models;
using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;


namespace C_Sharp_Web_Programming_Final_Project_MySQL_Connection.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AvailabilityController : ControllerBase
    {
        private readonly string _connectionString;

        public AvailabilityController(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("MySqlConnection");
        }

        [HttpGet]
        public ActionResult<IEnumerable<Availability>> GetAllAvailabilities()
        {
            try
            {
                using (var connection = new MySqlConnection(_connectionString))
                {
                    connection.Open();
                    var query = "SELECT * FROM availability";
                    using (var command = new MySqlCommand(query, connection))
                    using (var reader = command.ExecuteReader())
                    {
                        var availability = new List<Availability>();
                        while (reader.Read())
                        {
                            availability.Add(new Availability
                            {
                                Availability_id = reader.GetInt32("Availability_ID"),
                                Camping_ID = reader.GetInt32("Camping_ID"),
                                Date = new DateOnly(reader.GetDateTime("Date").Year, reader.GetDateTime("Date").Month, reader.GetDateTime("Date").Day),
                                Available = reader.GetBoolean("Available")
                            });
                        }
                        return Ok(availability);
                    }
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("{camping_id}")]
        public ActionResult<IEnumerable<Availability>> GetAvailabilityByCampingId(int camping_id)
        {
            try
            {
                using (var connection = new MySqlConnection(_connectionString))
                {
                    connection.Open();
                    var query = "SELECT * FROM availability WHERE Camping_ID = @id";
                    using (var command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@id", camping_id);
                        using (var reader = command.ExecuteReader())
                        {
                            var availabilityList = new List<Availability>();
                            while (reader.Read())
                            {
                                var availability = new Availability
                                {
                                    Availability_id = reader.GetInt32("Availability_ID"),
                                    Camping_ID = reader.GetInt32("Camping_ID"),
                                    Date = DateOnly.FromDateTime(reader.GetDateTime("Date")),
                                    Available = reader.GetBoolean("Available")
                                };
                                availabilityList.Add(availability);
                                //Console.WriteLine(availabilityList);
                            }
                        
                            if (availabilityList.Count > 0)
                            {
                                return Ok(availabilityList);
                            }
                            else
                            {
                                return NotFound();
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }


        [HttpGet("GetAvailableSpots")]
        public ActionResult<IEnumerable<Camping>> GetAvailableCampingSpots([FromQuery] DateTime startDate, [FromQuery] DateTime endDate, [FromQuery] int zipcode)
        {
            try
            {
                using (var connection = new MySqlConnection(_connectionString))
                {
                    connection.Open();
                    var query = @"
                SELECT c.*
                FROM camping c
                JOIN location l ON c.Location_ID = l.Location_ID
                WHERE l.Zip_Code = @zipcode
                AND NOT EXISTS (
                    SELECT 1
                    FROM availability a
                    WHERE a.Camping_ID = c.Camping_ID
                    AND a.Date BETWEEN @startDate AND @endDate
                    AND a.Available = 0
                )";

                    using (var command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@startDate", startDate);
                        command.Parameters.AddWithValue("@endDate", endDate);
                        command.Parameters.AddWithValue("@zipcode", zipcode);

                        using (var reader = command.ExecuteReader())
                        {
                            var campingSpots = new List<Camping>();
                            while (reader.Read())
                            {
                                campingSpots.Add(new Camping
                                {
                                    Camping_id = reader.GetInt32("Camping_ID"),
                                    Name = reader.GetString("Name"),
                                    User_id = reader.GetInt32("User_ID"),
                                    Location_id = reader.GetInt32("Location_ID"),
                                    Description = reader.IsDBNull(reader.GetOrdinal("Description")) ? null : reader.GetString("Description"),
                                    Price = reader.GetDouble("Price")
                                });
                            }

                            return Ok(campingSpots);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }



        [HttpPost("register")]
        public ActionResult RegisterAvailability(
            [FromForm] int Camping_ID,
            [FromForm] DateOnly Date_Start,
            [FromForm] DateOnly Date_End)
        {
            try
            {
                var dates = new List<DateOnly>();
                for (var date = Date_Start; date <= Date_End; date = date.AddDays(1))
                {
                    dates.Add(date);
                }

                using (var connection = new MySqlConnection(_connectionString))
                {
                    connection.Open();

                    foreach (var date in dates)
                    {
                        var query = @"
                            INSERT INTO availability (Camping_ID, Date, Available)
                            VALUES (@Camping_ID, @Date, @Available)";
                        using (var command = new MySqlCommand(query, connection))
                        {
                            command.Parameters.AddWithValue("@Camping_ID", Camping_ID);
                            command.Parameters.AddWithValue("@Date", date.ToString("yyyy-MM-dd"));
                            command.Parameters.AddWithValue("@Available", false);
                            command.ExecuteNonQuery();
                        }
                    }
                }

                return Ok("Availability registered successfully.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
}
