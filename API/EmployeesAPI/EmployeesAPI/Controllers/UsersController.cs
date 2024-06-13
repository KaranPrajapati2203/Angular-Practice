using EmployeesAPI.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Npgsql;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace EmployeesAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly string cs;
        private readonly JwtOption _options;
        public UsersController(IConfiguration configuration, IOptions<JwtOption> options)
        {
            cs = configuration.GetConnectionString("con");
            _options = options.Value;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] Users user)
        {
            try
            {
                using (var connection = new NpgsqlConnection(cs))
                {
                    await connection.OpenAsync();

                    // Check if email already exists
                    var checkUserSql = "SELECT COUNT(1) FROM Users WHERE Email = @Email";
                    using (var checkUserCmd = new NpgsqlCommand(checkUserSql, connection))
                    {
                        checkUserCmd.Parameters.AddWithValue("@Email", user.Email);
                        int userCount = Convert.ToInt32(checkUserCmd.ExecuteScalar());
                        if (userCount > 0)
                        {
                            return BadRequest("Email already exists.");
                        }
                    }

                    // Insert new user
                    var insertUserSql = "INSERT INTO users (Email, Password) VALUES (@Email, @Password)";
                    using (var cmd = new NpgsqlCommand(insertUserSql, connection))
                    {
                        cmd.Parameters.AddWithValue("@Email", user.Email);
                        cmd.Parameters.AddWithValue("@Password", user.Password);
                        cmd.ExecuteNonQuery();
                        //int newUserId = (int)await cmd.ExecuteScalarAsync();
                        //user.Id = newUserId;

                        return Ok("registration successful.");
                    }
                }
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Error: {ex.Message}");
            }
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] Users loginDetails)
        {
            try
            {
                var jwtKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(_options.Key));
                var credential = new SigningCredentials(jwtKey, SecurityAlgorithms.HmacSha256);

                List<Claim> claims = new List<Claim>()
                {
                    new Claim("Email",loginDetails.Email)
                };

                var sToken = new JwtSecurityToken(_options.Key, _options.Issuer, claims, expires: DateTime.Now.AddHours(1), signingCredentials: credential);
                var token = new JwtSecurityTokenHandler().WriteToken(sToken);

                using (var connection = new NpgsqlConnection(cs))
                {
                    await connection.OpenAsync();

                    // Get the user by email
                    var getUserSql = "SELECT Id, Email, Password FROM Users WHERE Email = @Email";
                    using (var cmd = new NpgsqlCommand(getUserSql, connection))
                    {
                        cmd.Parameters.AddWithValue("@Email", loginDetails.Email);

                        using (var reader = await cmd.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync())
                            {
                                var user = new Users
                                {
                                    Id = reader.GetInt32(reader.GetOrdinal("Id")),
                                    Email = reader.GetString(reader.GetOrdinal("Email")),
                                    Password = reader.GetString(reader.GetOrdinal("Password"))
                                };

                                // Verify the password
                                if (loginDetails.Password == user.Password)
                                {
                                    //return Ok("Login successful.");
                                    return Ok(new { token = token });
                                }
                                else
                                {
                                    return Unauthorized("Invalid email or password.");
                                }
                            }
                            else
                            {
                                return Unauthorized("Invalid email or password.");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Error: {ex.Message}");
            }
        }
    }
}