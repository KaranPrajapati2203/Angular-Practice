using EmployeesAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Npgsql;

namespace EmployeesAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EmployeeController : ControllerBase
    {
        private readonly string cs;
        public EmployeeController(IConfiguration configuration)
        {
            cs = configuration.GetConnectionString("con");
        }

        [HttpGet]
        [Authorize]
        public async Task<ActionResult<List<Employee>>> GetEmployees()
        {
            try
            {
                // Establish a connection to the database
                using (var connection = new NpgsqlConnection(cs))
                {
                    await connection.OpenAsync();

                    var sql = "SELECT * FROM employees";
                    using (var cmd = new NpgsqlCommand(sql, connection))
                    {
                        var students = new List<Employee>();
                        using (var reader = await cmd.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                var student = new Employee
                                {
                                    Id = reader.GetInt32(0),
                                    Name = reader.GetString(1),
                                    Email = reader.GetString(2),
                                    Phone = reader.GetString(3),
                                    Age = reader.GetInt32(4),
                                    Salary = reader.GetInt32(5),
                                };
                                students.Add(student);
                            }
                        }
                        return Ok(students);
                    }
                }
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Error: {ex.Message}");
            }
        }

        [HttpPost]
        public async Task<ActionResult<Employee>> AddEmployee(Employee employee)
        {
            try
            {
                using (var connection = new NpgsqlConnection(cs))
                {
                    await connection.OpenAsync();
                    var sql = "INSERT INTO Employees (Name, Email, Phone, Age, Salary) " +
                              "VALUES (@Name, @Email, @Phone, @Age, @Salary) " +
                              "RETURNING Id";
                    using (var cmd = new NpgsqlCommand(sql, connection))
                    {
                        cmd.Parameters.AddWithValue("@Name", employee.Name);
                        cmd.Parameters.AddWithValue("@Email", employee.Email);
                        cmd.Parameters.AddWithValue("@Phone", employee.Phone);
                        cmd.Parameters.AddWithValue("@Age", employee.Age);
                        cmd.Parameters.AddWithValue("@Salary", employee.Salary);

                        int newEmployeeId = (int)await cmd.ExecuteScalarAsync();
                        employee.Id = newEmployeeId;

                        return CreatedAtAction(nameof(GetEmployee), new { id = newEmployeeId }, employee);
                    }
                }
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Error: {ex.Message}");
            }
        }

        // Assuming GetEmployee method exists:
        [HttpGet("{id}")]
        public async Task<ActionResult<Employee>> GetEmployee(int id)
        {
            try
            {
                using (var connection = new NpgsqlConnection(cs))
                {
                    await connection.OpenAsync();

                    var sql = "SELECT * FROM Employees WHERE Id = @Id";
                    using (var cmd = new NpgsqlCommand(sql, connection))
                    {
                        cmd.Parameters.AddWithValue("@Id", id);
                        using (var reader = await cmd.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync())
                            {
                                var employee = new Employee
                                {
                                    Id = reader.GetInt32(reader.GetOrdinal("Id")),
                                    Name = reader.GetString(reader.GetOrdinal("Name")),
                                    Email = reader.GetString(reader.GetOrdinal("Email")),
                                    Phone = reader.GetString(reader.GetOrdinal("Phone")),
                                    Age = reader.GetInt32(reader.GetOrdinal("Age")),
                                    Salary = reader.GetInt32(reader.GetOrdinal("Salary"))
                                };
                                return Ok(employee);
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
                return StatusCode(StatusCodes.Status500InternalServerError, $"Error: {ex.Message}");
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateEmployee(int id, Employee updatedEmployee)
        {
            try
            {
                using (var connection = new NpgsqlConnection(cs))
                {
                    await connection.OpenAsync();

                    // Retrieve old data related to the employee ID
                    var oldEmployee = await GetEmployeeByIdAsync(id, connection);

                    // Check if the old employee data exists
                    if (oldEmployee == null)
                    {
                        return NotFound();
                    }

                    var sql = "UPDATE Employees " +
                              "SET Name = @Name, Email = @Email, Phone = @Phone, Age = @Age, Salary = @Salary " +
                              "WHERE Id = @Id";
                    using (var cmd = new NpgsqlCommand(sql, connection))
                    {
                        cmd.Parameters.AddWithValue("@Name", updatedEmployee.Name);
                        cmd.Parameters.AddWithValue("@Email", updatedEmployee.Email);
                        cmd.Parameters.AddWithValue("@Phone", updatedEmployee.Phone);
                        cmd.Parameters.AddWithValue("@Age", updatedEmployee.Age);
                        cmd.Parameters.AddWithValue("@Salary", updatedEmployee.Salary);
                        cmd.Parameters.AddWithValue("@Id", id);

                        int rowsAffected = await cmd.ExecuteNonQueryAsync();

                        // Check if any rows were affected
                        if (rowsAffected == 0)
                        {
                            // If no rows were affected, return NotFound response
                            return NotFound();
                        }

                        // Return NoContent response indicating successful update
                        return NoContent();
                    }
                }
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Error: {ex.Message}");
            }
        }


        private async Task<Employee> GetEmployeeByIdAsync(int id, NpgsqlConnection connection)
        {
            var sql = "SELECT Id, Name, Email, Phone, Age, Salary FROM Employees WHERE Id = @Id";
            using (var cmd = new NpgsqlCommand(sql, connection))
            {
                cmd.Parameters.AddWithValue("@Id", id);

                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    if (await reader.ReadAsync())
                    {
                        // Map retrieved data to Employee object
                        var employee = new Employee
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("Id")),
                            Name = reader.GetString(reader.GetOrdinal("Name")),
                            Email = reader.GetString(reader.GetOrdinal("Email")),
                            Phone = reader.GetString(reader.GetOrdinal("Phone")),
                            Age = reader.GetInt32(reader.GetOrdinal("Age")),
                            Salary = reader.GetInt32(reader.GetOrdinal("Salary"))
                        };
                        return employee;
                    }
                    else
                    {
                        return null;
                    }
                }
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteEmployee(int id)
        {
            try
            {
                using (var connection = new NpgsqlConnection(cs))
                {
                    await connection.OpenAsync();
                    var sql = "DELETE FROM Employees WHERE Id = @Id";
                    using (var cmd = new NpgsqlCommand(sql, connection))
                    {
                        cmd.Parameters.AddWithValue("@Id", id);

                        int rowsAffected = await cmd.ExecuteNonQueryAsync();
                        // Check if any rows were affected
                        if (rowsAffected == 0)
                        {
                            // If no rows were affected, return NotFound response
                            return NotFound();
                        }
                        // Return NoContent response indicating successful deletion
                        return NoContent();
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
