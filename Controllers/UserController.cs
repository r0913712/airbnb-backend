using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using MySql.Data.MySqlClient;
using C_Sharp_Web_Programming_Final_Project_MySQL_Connection.Models; // Ensure this namespace includes your User model

namespace C_Sharp_Web_Programming_Final_Project_MySQL_Connection.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly string _connectionString;

        public UsersController(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("MySqlConnection");
        }

        [HttpGet]
        public ActionResult<IEnumerable<User>> GetAllUsers()
        {
            try
            {
                using (var connection = new MySqlConnection(_connectionString))
                {
                    connection.Open();
                    var query = "SELECT * FROM user";
                    using (var command = new MySqlCommand(query, connection))
                    using (var reader = command.ExecuteReader())
                    {
                        var users = new List<User>();
                        while (reader.Read())
                        {
                            users.Add(new User
                            {
                                User_id = reader.GetInt32("User_ID"),
                                F_Name = reader.GetString("F_Name"),
                                L_Name = reader.GetString("L_Name"),
                                Phone_Number = reader.GetString("Phone_Number"),
                                Email = reader.GetString("Email"),
                                Password = reader.GetString("Password"),
                                //Image_Path = reader.IsDBNull(reader.GetOrdinal("Image_Path")) ? null : reader.GetString(reader.GetOrdinal("Image_Path")),
                                Is_Owner = reader.GetBoolean("Is_Owner")
                            });
                        }
                        return Ok(users);
                    }
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("{id}")]
        public ActionResult<User> GetUserById(int id)
        {
            try
            {
                using (var connection = new MySqlConnection(_connectionString))
                {
                    connection.Open();
                    var query = "SELECT * FROM user WHERE User_ID = @id";
                    using (var command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@id", id);
                        using (var reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                return Ok(new User
                                {
                                    User_id = reader.GetInt32("User_ID"),
                                    F_Name = reader.GetString("F_Name"),
                                    L_Name = reader.GetString("L_Name"),
                                    Phone_Number = reader.GetString("Phone_Number"),
                                    Email = reader.GetString("Email"),
                                    Password = reader.GetString("Password"),
                                    /*Image_Path = reader.IsDBNull(reader.GetOrdinal("Image_Path")) ? null : reader.GetString(reader.GetOrdinal("Image_Path")),*/
                                    Is_Owner = reader.GetBoolean("Is_Owner")
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

        [HttpGet("email/{email}")]
        public ActionResult GetUserByEmail(string email)
        {
            try
            {
                using (var connection = new MySqlConnection(_connectionString))
                {
                    connection.Open();
                    var query = "SELECT User_ID, F_Name, L_Name, Phone_Number, Email, Password, Is_Owner, Image FROM user WHERE Email = @Email";
                    using (var command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Email", email);

                        using (var reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                byte[] imageBytes = null;
                                if (!reader.IsDBNull(reader.GetOrdinal("Image")))
                                {
                                    long length = reader.GetBytes(reader.GetOrdinal("Image"), 0, null, 0, 0); // Get the length of the data
                                    imageBytes = new byte[length];
                                    reader.GetBytes(reader.GetOrdinal("Image"), 0, imageBytes, 0, (int)length); // Read the data into the buffer
                                }

                                var user = new User
                                {
                                    User_id = reader.GetInt32("User_ID"),
                                    F_Name = reader.GetString("F_Name"),
                                    L_Name = reader.GetString("L_Name"),
                                    Phone_Number = reader.GetString("Phone_Number"),
                                    Email = reader.GetString("Email"),
                                    Password = reader.GetString("Password"),
                                    Image = imageBytes,
                                    Is_Owner = reader.GetBoolean("Is_Owner")
                                };

                                return Ok(user);
                            }
                            else
                            {
                                return NotFound($"User with email {email} not found.");
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


        [HttpPost("newuser")]
        public async Task<ActionResult<User>> NewUser([FromForm] string F_Name, [FromForm] string L_Name, [FromForm] string Phone_Number, [FromForm] string Email, [FromForm] string Password,
    [FromForm] IFormFile? Image, [FromForm] bool Is_Owner)
        {
            try
            {
                byte[] imageBytes;
                if (Image != null)
                {
                    using (var memoryStream = new MemoryStream())
                    {
                        await Image.CopyToAsync(memoryStream);
                        imageBytes = memoryStream.ToArray();
                    }
                }
                else
                {
                    // Load default image as bytes
                    var defaultImagePath = Path.Combine(Directory.GetCurrentDirectory(), "assets", "img", "pfp", "default_pfp.jpg");
                    imageBytes = System.IO.File.ReadAllBytes(defaultImagePath);
                }

                using (var connection = new MySqlConnection(_connectionString))
                {
                    connection.Open();
                    var query = @"
                INSERT INTO user (F_Name, L_Name, Phone_Number, Email, Password, Image, Is_Owner)
                VALUES (@F_Name, @L_Name, @Phone_Number, @Email, @Password, @Image, @Is_Owner);
                SELECT LAST_INSERT_ID();"; // Get the last inserted ID
                    using (var command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@F_Name", F_Name);
                        command.Parameters.AddWithValue("@L_Name", L_Name);
                        command.Parameters.AddWithValue("@Phone_Number", Phone_Number);
                        command.Parameters.AddWithValue("@Email", Email);
                        command.Parameters.AddWithValue("@Password", Password);
                        command.Parameters.AddWithValue("@Image", imageBytes); // Save image blob
                        command.Parameters.AddWithValue("@Is_Owner", Is_Owner);

                        var userId = Convert.ToInt32(command.ExecuteScalar());

                        // Fetch the created user by ID
                        var fetchUserQuery = "SELECT * FROM user WHERE User_id = @User_id";
                        using (var fetchCommand = new MySqlCommand(fetchUserQuery, connection))
                        {
                            fetchCommand.Parameters.AddWithValue("@User_id", userId);
                            using (var reader = fetchCommand.ExecuteReader())
                            {
                                if (reader.Read())
                                {
                                    User newUser = new User
                                    {
                                        User_id = reader.GetInt32("User_id"),
                                        F_Name = reader.GetString("F_Name"),
                                        L_Name = reader.GetString("L_Name"),
                                        Phone_Number = reader.GetString("Phone_Number"),
                                        Email = reader.GetString("Email"),
                                        Password = reader.GetString("Password"),
                                        Image = reader["Image"] as byte[],
                                        Is_Owner = reader.GetBoolean("Is_Owner")
                                    };
                                    return Ok(newUser);
                                }
                            }
                        }
                    }
                }

                return StatusCode(500, "User could not be created.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPut("updateuser")]
        public async Task<ActionResult<User>> UpdateUser(
    [FromForm] int User_ID,
    [FromForm] string F_Name,
    [FromForm] string L_Name,
    [FromForm] string Phone_Number,
    [FromForm] string Password,
    [FromForm] IFormFile? Image)
        {
            try
            {
                using (var connection = new MySqlConnection(_connectionString))
                {
                    connection.Open();

                    byte[] imageBytes = null;
                    if (Image != null)
                    {
                        using (var memoryStream = new MemoryStream())
                        {
                            await Image.CopyToAsync(memoryStream);
                            imageBytes = memoryStream.ToArray();
                        }
                    }
                    else
                    {
                        // Load default image as bytes
                        var defaultImagePath = Path.Combine(Directory.GetCurrentDirectory(), "assets", "img", "pfp", "default_pfp.jpg");
                        imageBytes = System.IO.File.ReadAllBytes(defaultImagePath);
                    }

                    var updateQuery = "UPDATE user SET F_Name = @F_Name, L_Name = @L_Name, Phone_Number = @Phone_Number, Password = @Password, Image = @Image WHERE User_ID = @UserId";
                    using (var updateCommand = new MySqlCommand(updateQuery, connection))
                    {
                        updateCommand.Parameters.AddWithValue("@F_Name", F_Name);
                        updateCommand.Parameters.AddWithValue("@L_Name", L_Name);
                        updateCommand.Parameters.AddWithValue("@Phone_Number", Phone_Number);
                        updateCommand.Parameters.AddWithValue("@Password", Password);
                        updateCommand.Parameters.AddWithValue("@Image", imageBytes);
                        updateCommand.Parameters.AddWithValue("@UserId", User_ID);

                        var rowsAffected = updateCommand.ExecuteNonQuery();
                        if (rowsAffected > 0)
                        {
                            var fetchQuery = "SELECT * FROM user WHERE User_ID = @UserId";
                            using (var fetchCommand = new MySqlCommand(fetchQuery, connection))
                            {
                                fetchCommand.Parameters.AddWithValue("@UserId", User_ID);
                                using (var reader = fetchCommand.ExecuteReader())
                                {
                                    if (reader.Read())
                                    {
                                        var updatedUser = new User
                                        {
                                            User_id = reader.GetInt32("User_ID"),
                                            F_Name = reader.GetString("F_Name"),
                                            L_Name = reader.GetString("L_Name"),
                                            Phone_Number = reader.GetString("Phone_Number"),
                                            Email = reader.GetString("Email"),
                                            Password = reader.GetString("Password"),
                                            Image = reader.IsDBNull(reader.GetOrdinal("Image")) ? null : (byte[])reader["Image"],
                                            Is_Owner = reader.GetBoolean("Is_Owner")
                                        };
                                        return Ok(updatedUser);
                                    }
                                }
                            }
                        }
                        else
                        {
                            return NotFound($"User with ID {User_ID} not found.");
                        }
                    }
                }

                return StatusCode(500, "User could not be updated.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
}