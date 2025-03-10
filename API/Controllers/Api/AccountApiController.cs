using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Models;
using DTO;

namespace AirBookingApplication.Controllers.Api
{
    [Route("Api/[controller]")] 
    [ApiController] 
    public class AccountApiController : ControllerBase
    {
        #region Private members
        private readonly IConfiguration _configuration;
        #endregion

        #region Private Methods
        private string GenerateJwtToken(string userId, string email)
        {
            var jwtSettings = _configuration.GetSection("JwtSettings");
            var key = Encoding.UTF8.GetBytes(jwtSettings["SecretKey"]);

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, userId), // Subject (User ID)
                new Claim(JwtRegisteredClaimNames.Email, email), // Email claim
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()) // Unique Token ID
            };

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMinutes(60),
                Issuer = jwtSettings["Issuer"],
                Audience = jwtSettings["Audience"],
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
        #endregion

        #region Constructor
        public AccountApiController(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        #endregion

        /// <summary>
        /// Register
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        // Create/Sign Up User:
        [HttpPost("Register")]
        public IActionResult Register(User user)
        {
            if (user == null)
            {
                return BadRequest("Invalid user data.");
            }

            if (ModelState.IsValid)
            {
                string connectionString = _configuration.GetConnectionString("DefaultConnection");

                try
                {
                    using (SqlConnection connection = new SqlConnection(connectionString))
                    {
                        string query = @"INSERT INTO Users (Name, Email, Password, BirthDate, City, Region, PostalCode) 
                                 VALUES (@Name, @Email, @Password, @BirthDate, @City, @Region, @PostalCode);
                                 SELECT SCOPE_IDENTITY();";  // Gets the newly inserted UserId

                        using (SqlCommand command = new SqlCommand(query, connection))
                        {
                            // Add User parameters
                            command.Parameters.AddWithValue("@Name", user.Name);
                            command.Parameters.AddWithValue("@Email", user.Email);
                            command.Parameters.AddWithValue("@Password", user.Password);
                            command.Parameters.AddWithValue("@BirthDate", (object?)user.BirthDate ?? DBNull.Value);
                            command.Parameters.AddWithValue("@City", (object?)user.City ?? DBNull.Value);
                            command.Parameters.AddWithValue("@Region", (object?)user.Region ?? DBNull.Value);
                            command.Parameters.AddWithValue("@PostalCode", (object?)user.PostalCode ?? DBNull.Value);

                            connection.Open();
                            int newUserId = Convert.ToInt32(command.ExecuteScalar()); // Get the inserted UserId

                            return Ok(new { UserId = newUserId });
                        }
                    }
                }
                catch (SqlException ex)
                {
                    return StatusCode(500, "Database error: " + ex.Message);
                }
                catch (Exception ex)
                {
                    return StatusCode(500, "Unexpected error: " + ex.Message);
                }
            }

            return BadRequest(ModelState);
        }

        /// <summary>
        /// Login 
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        [HttpPost("Login")]
        public IActionResult Login([FromBody] LoginDTO loginDto)
        {
            if (loginDto == null)
            {
                return BadRequest("Invalid user data.");
            }

            if (ModelState.IsValid)
            {
                string connectionString = _configuration.GetConnectionString("DefaultConnection");
                try
                {
                    using (SqlConnection connection = new SqlConnection(connectionString))
                    {
                        string query = "SELECT UserId, Name, Email FROM Users WHERE Email = @Email AND Password = @Password";

                        using (SqlCommand command = new SqlCommand(query, connection))
                        {
                            command.Parameters.AddWithValue("@Email", loginDto.Email);
                            command.Parameters.AddWithValue("@Password", loginDto.Password);

                            connection.Open();
                            using (SqlDataReader reader = command.ExecuteReader())
                            {
                                if (reader.HasRows)
                                {
                                    reader.Read();

                                    var userId = reader["UserId"].ToString();
                                    var email = reader["Email"].ToString();

                                    // Generate JWT Token
                                    var token = GenerateJwtToken(userId, email);

                                    // Store JWT in HTTP-Only Cookie
                                    var cookieOptions = new CookieOptions
                                    {
                                        HttpOnly = true, // Prevents JavaScript access (more secure)
                                        Secure = true, 
                                        SameSite = SameSiteMode.Lax, 
                                        Expires = DateTime.UtcNow.AddMinutes(60) // Token Expiration
                                    };

                                    Response.Cookies.Append("AuthToken", token, cookieOptions);

                                    return Ok(new { Message = "Login successful", Token = token });
                                }
                                else
                                {
                                    return Unauthorized("Invalid email or password.");
                                }
                            }
                        }
                    }
                }
                catch (SqlException ex)
                {
                    return StatusCode(500, "Database error: " + ex.Message);
                }
                catch (Exception ex)
                {
                    return StatusCode(500, "Unexpected error: " + ex.Message);
                }
            }

            return BadRequest(ModelState);
        }
    }
}
