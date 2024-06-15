using C_Sharp_Web_Programming_Final_Project_MySQL_Connection.Models;
using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
//this works
namespace C_Sharp_Web_Programming_Final_Project_MySQL_Connection.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LocationController : ControllerBase
    {
        private readonly string _connectionString;

        public LocationController(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("MySqlConnection");
        }

        [HttpGet]
        public ActionResult<IEnumerable<Bookings>> GetAllLocations()
        {
            try
            {
                using (var connection = new MySqlConnection(_connectionString))
                {
                    connection.Open();
                    var query = "SELECT * FROM location";
                    using (var command = new MySqlCommand(query, connection))
                    using (var reader = command.ExecuteReader())
                    {
                        var location = new List<Location>();
                        while (reader.Read())
                        {
                            location.Add(new Location
                            {
                                Location_id = reader.GetInt32("Location_ID"),
                                Address = reader.GetString("Address"),
                                Zip_Code = reader.GetInt32("Zip_Code"),
                                Country = reader.GetString("Country")
                            }) ;
                        }
                        return Ok(location);
                    }
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("{Zip_Code}")]
        public ActionResult<List<Location>> GetLocationByZipCode(int Zip_Code)
        {
            try
            {
                var locations = new List<Location>();
                using (var connection = new MySqlConnection(_connectionString))
                {
                    connection.Open();
                    var query = "SELECT * FROM location WHERE Zip_Code = @zipCode";
                    using (var command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@zipCode", Zip_Code);
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                locations.Add(new Location
                                {
                                    Location_id = reader.GetInt32("Location_ID"),
                                    Address = reader.GetString("Address"),
                                    Zip_Code = reader.GetInt32("Zip_Code"),
                                    Country = reader.GetString("Country")
                                });
                            }
                        }
                    }
                }

                if (locations.Count == 0)
                {
                    return NotFound();
                }

                return Ok(locations);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("GetAllZipcodes")]
        public ActionResult<IEnumerable<string>> GetAllZipcodes()
        {
            try
            {
                using (var connection = new MySqlConnection(_connectionString))
                {
                    connection.Open();
                    var query = "SELECT DISTINCT Zip_Code FROM location";
                    using (var command = new MySqlCommand(query, connection))
                    {
                        using (var reader = command.ExecuteReader())
                        {
                            var zipcodes = new List<string>();
                            while (reader.Read())
                            {
                                zipcodes.Add(reader.GetString("Zip_Code"));
                            }

                            return Ok(zipcodes);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }


        [HttpPost("New Location")]
        public ActionResult NewLocation(
    [FromForm] string Address,
    [FromForm] string ZipCode,
    [FromForm] string Country)
        {
            try
            {
                using (var connection = new MySqlConnection(_connectionString))
                {
                    connection.Open();
                    var query = @"
                INSERT INTO location (Address, Zip_Code, Country)
                VALUES (@Address, @Zip_Code, @Country);
                SELECT LAST_INSERT_ID();";
                    using (var command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Address", Address);
                        command.Parameters.AddWithValue("@Zip_Code", ZipCode);
                        command.Parameters.AddWithValue("@Country", Country);

                        var locationId = Convert.ToInt32(command.ExecuteScalar());

                        return Ok(new { LocationId = locationId });
                    }
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("GetById/{Location_id}")]
        public ActionResult<Location> GetLocationById(int Location_id)
        {
            try
            {
                using (var connection = new MySqlConnection(_connectionString))
                {
                    connection.Open();
                    var query = "SELECT * FROM location WHERE Location_ID = @locationId";
                    using (var command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@locationId", Location_id);
                        using (var reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                var location = new Location
                                {
                                    Location_id = reader.GetInt32("Location_ID"),
                                    Address = reader.GetString("Address"),
                                    Zip_Code = reader.GetInt32("Zip_Code"),
                                    Country = reader.GetString("Country")
                                };

                                return Ok(location);
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

    }

}
