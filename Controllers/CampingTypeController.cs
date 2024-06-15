using C_Sharp_Web_Programming_Final_Project_MySQL_Connection.Models;
using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
//this works

namespace C_Sharp_Web_Programming_Final_Project_MySQL_Connection.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CampingTypeController : ControllerBase
    {
        private readonly string _connectionString;

        public CampingTypeController(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("MySqlConnection");
        }
        [HttpGet]
        public ActionResult<IEnumerable<Bookings>> GetAllCampingType()
        {
            try
            {
                using (var connection = new MySqlConnection(_connectionString))
                {
                    connection.Open();
                    var query = "SELECT * FROM campingtype";
                    using (var command = new MySqlCommand(query, connection))
                    using (var reader = command.ExecuteReader())
                    {
                        var campingtype = new List<CampingType>();
                        while (reader.Read())
                        {
                            campingtype.Add(new CampingType
                            {
                                Camping_Type_id = reader.GetInt32("Camping_Type_ID"),
                                Camping_Type = reader.GetString("Camping_Type")
                            });
                        }
                        return Ok(campingtype);
                    }
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("{Camping_Type_id}")]
        public ActionResult<User> GetCampingTypebyCampingTypeID(int Camping_Type_id)
        {
            try
            {
                using (var connection = new MySqlConnection(_connectionString))
                {
                    connection.Open();
                    var query = "SELECT * FROM campingtype WHERE Camping_Type_ID = @id";
                    using (var command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@id", Camping_Type_id);
                        using (var reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                return Ok(new CampingType
                                {
                                    Camping_Type_id = reader.GetInt32("Camping_Type_ID"),
                                    Camping_Type = reader.GetString("Camping_Type")
                                });
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
