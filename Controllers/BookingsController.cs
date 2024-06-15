using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using MySql.Data.MySqlClient;
using C_Sharp_Web_Programming_Final_Project_MySQL_Connection.Models;
using Microsoft.AspNetCore.Http.HttpResults;

namespace C_Sharp_Web_Programming_Final_Project_MySQL_Connection.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BookingsController : ControllerBase
    {
        private readonly string _connectionString;

        public BookingsController(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("MySqlConnection");
        }

        [HttpGet]
        public ActionResult<IEnumerable<Bookings>> GetAllBookings()
        {
            try
            {
                using (var connection = new MySqlConnection(_connectionString))
                {
                    connection.Open();
                    var query = "SELECT * FROM bookings";
                    using (var command = new MySqlCommand(query, connection))
                    using (var reader = command.ExecuteReader())
                    {
                        var bookings = new List<Bookings>();
                        while (reader.Read())
                        {
                            bookings.Add(new Bookings
                            {
                                Booking_id = reader.GetInt32("Booking_ID"),
                                User_id = reader.GetInt32("User_ID"),
                                Date_Start = new DateOnly(reader.GetDateTime("Date_Start").Year, reader.GetDateTime("Date_Start").Month, reader.GetDateTime("Date_Start").Day),
                                Date_End = new DateOnly(reader.GetDateTime("Date_End").Year, reader.GetDateTime("Date_End").Month, reader.GetDateTime("Date_End").Day),
                                Camping_id = reader.GetInt32("Camping_ID"),
                                Price = reader.GetDouble("Price")
                            });
                        }
                        return Ok(bookings);
                    }
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("{User_id}")]
        public ActionResult<User> GetBookingByUserId(int User_id)
        {
            try
            {
                using (var connection = new MySqlConnection(_connectionString))
                {
                    connection.Open();
                    var query = "SELECT * FROM bookings WHERE User_ID = @id";
                    using (var command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@id", User_id);
                        using (var reader = command.ExecuteReader())
                        {
                            var bookingsList = new List<Bookings>();
                            while (reader.Read())
                            {
                                bookingsList.Add(new Bookings
                                {
                                    User_id = reader.GetInt32("User_ID"),
                                    Date_Start = new DateOnly(reader.GetDateTime("Date_Start").Year, reader.GetDateTime("Date_Start").Month, reader.GetDateTime("Date_Start").Day),
                                    Date_End = new DateOnly(reader.GetDateTime("Date_End").Year, reader.GetDateTime("Date_End").Month, reader.GetDateTime("Date_End").Day),
                                    Camping_id = reader.GetInt32("Camping_ID"),
                                    Price = reader.GetDouble("Price")
                                });
                            }
                            if (bookingsList.Count > 0)
                            {
                                return Ok(bookingsList);
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

        [HttpGet("GetBookingsByOwner/{ownerId}")]
        public ActionResult<IEnumerable<Bookings>> GetBookingsByOwner(int ownerId)
        {
            try
            {
                using (var connection = new MySqlConnection(_connectionString))
                {
                    connection.Open();
                    var query = @"
                SELECT b.*
                FROM bookings b
                INNER JOIN camping c ON b.Camping_ID = c.Camping_ID
                WHERE c.User_ID = @ownerId";

                    using (var command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@ownerId", ownerId);
                        using (var reader = command.ExecuteReader())
                        {
                            var bookings = new List<Bookings>();
                            while (reader.Read())
                            {
                                bookings.Add(new Bookings
                                {
                                    Booking_id = reader.GetInt32("Booking_ID"),
                                    User_id = reader.GetInt32("User_ID"),
                                    Camping_id = reader.GetInt32("Camping_ID"),
                                    Date_Start = new DateOnly(reader.GetDateTime("Date_Start").Year, reader.GetDateTime("Date_Start").Month, reader.GetDateTime("Date_Start").Day),
                                    Date_End = new DateOnly(reader.GetDateTime("Date_End").Year, reader.GetDateTime("Date_End").Month, reader.GetDateTime("Date_End").Day),
                                    Price = reader.GetDouble("Price")
                                });
                            }
                            return Ok(bookings);
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
        public ActionResult NewBooking(
    [FromForm] int User_ID,
    [FromForm] DateOnly Date_Start,
    [FromForm] DateOnly Date_End,
    [FromForm] int Camping_ID,
    [FromForm] double Price)
        {
            try
            {
                using (var connection = new MySqlConnection(_connectionString))
                {
                    connection.Open();
                    var query = @"
                INSERT INTO bookings (User_ID, Date_Start, Date_End, Camping_ID, Price)
                VALUES (@User_ID, @Date_Start, @Date_End, @Camping_ID, @Price)";
                    using (var command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@User_ID", User_ID);
                        command.Parameters.AddWithValue("@Date_Start", Date_Start.ToString("yyyy-MM-dd"));
                        command.Parameters.AddWithValue("@Date_End", Date_End.ToString("yyyy-MM-dd"));
                        command.Parameters.AddWithValue("@Camping_ID", Camping_ID);
                        command.Parameters.AddWithValue("@Price", Price);
                        command.ExecuteNonQuery();
                    }
                }

                return Ok("Booking registered successfully.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
}
