using C_Sharp_Web_Programming_Final_Project.Models;
using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;

namespace C_Sharp_Web_Programming_Final_Project_MySQL_Connection.Controllers
{
    public class ReviewController
    {
        [ApiController]
        [Route("api/[controller]")]
        public class ReviewsController : ControllerBase
        {
            private readonly string _connectionString;

            public ReviewsController(IConfiguration configuration)
            {
                _connectionString = configuration.GetConnectionString("DefaultConnection");
            }

            [HttpPost("add")]
            public async Task<ActionResult> AddReview([FromForm] int Booking_ID, [FromForm] int Rate, [FromForm] string Message)
            {
                try
                {
                    using (var connection = new MySqlConnection(_connectionString))
                    {
                        connection.Open();
                        var query = @"
                    INSERT INTO review (Rate, Message, Booking_ID)
                    VALUES (@Rate, @Message, @Booking_ID)";
                        using (var command = new MySqlCommand(query, connection))
                        {
                            command.Parameters.AddWithValue("@Rate", Rate);
                            command.Parameters.AddWithValue("@Message", Message ?? (object)DBNull.Value);
                            command.Parameters.AddWithValue("@Booking_ID", Booking_ID);
                            await command.ExecuteNonQueryAsync();
                        }
                    }

                    return Ok(new { Message = "Review added successfully." });
                }
                catch (Exception ex)
                {
                    return StatusCode(500, $"Internal server error: {ex.Message}");
                }
            }

            [HttpGet("booking/{bookingId}")]
            public ActionResult GetReviewByBookingId(int bookingId)
            {
                try
                {
                    using (var connection = new MySqlConnection(_connectionString))
                    {
                        connection.Open();
                        var query = "SELECT * FROM review WHERE Booking_ID = @Booking_ID";
                        using (var command = new MySqlCommand(query, connection))
                        {
                            command.Parameters.AddWithValue("@Booking_ID", bookingId);

                            using (var reader = command.ExecuteReader())
                            {
                                if (reader.Read())
                                {
                                    var review = new Review
                                    {
                                        Review_id = reader.GetInt32("Review_ID"),
                                        Rate = reader.GetInt32("Rate"),
                                        Message = reader.GetString("Message"),
                                        Booking_id = reader.GetInt32("Booking_ID")
                                    };

                                    return Ok(review);
                                }
                                else
                                {
                                    return Ok(null);
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
}
