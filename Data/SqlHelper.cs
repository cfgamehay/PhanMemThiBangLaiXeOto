using ApiThiBangLaiXeOto.DTOs;
using ApiThiBangLaiXeOto.Helper;
using Microsoft.Data.SqlClient;
using System.Data;
namespace ApiThiBangLaiXeOto.Data
{
    public class SqlHelper
    {
        public readonly string _connectionString;

        public SqlHelper(IConfiguration configuration)
        {
            // Prefer the built-in helper which looks in the ConnectionStrings section
            _connectionString = configuration.GetConnectionString("DefaultConnection")
                        ?? configuration["ConnectionStrings:DefaultConnection"]
                        ?? configuration.GetSection("ConnectionStrings")["DefaultConnection"]
                        ?? "";

            // Fail fast to help detect configuration problems during startup
            if (string.IsNullOrEmpty(_connectionString))
            {
                throw new InvalidOperationException("Connection string 'DefaultConnection' not found. Ensure appsettings.json is loaded and the key 'ConnectionStrings:DefaultConnection' exists.");
            }
        }

        //Dùng để thực thi các truy vấn trả về nhiều bản ghi bằng query hoặc stored procedure -> List<T> -> 200 OK
        //Mapper: hàm ánh xạ từ SqlDataReader sang kiểu T
        public async Task<List<T>> ExecuteQueryAsync<T>(
            string sqlText,
            Func<SqlDataReader, T> mapper,
            SqlParameter[]? parameters = null,
            CommandType commandType = CommandType.Text)
        {
            var results = new List<T>();
            using (var connection = new SqlConnection(_connectionString))
            {
                using (var command = new SqlCommand(sqlText, connection))
                {
                    command.CommandType = commandType;
                    if (parameters != null) command.Parameters.AddRange(parameters);

                    await connection.OpenAsync();
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            results.Add(mapper(reader));
                        }
                    }
                }
            }
            return results;
        }

        //Dùng để thực thi các truy vấn trả về một bản ghi bằng query hoặc stored procedure -> T hoặc null -> 200 OK hoặc 404 Not Found
        //Mapper: hàm ánh xạ từ SqlDataReader sang kiểu T
        public async Task<T?> ExecuteQuerySingleAsync<T>(
            string sqlText,
            Func<SqlDataReader, T> mapper,
            SqlParameter[]? parameters = null,
            CommandType commandType = CommandType.Text)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                using (var command = new SqlCommand(sqlText, connection))
                {
                    command.CommandType = commandType;
                    if (parameters != null) command.Parameters.AddRange(parameters);
                    await connection.OpenAsync();
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            return mapper(reader);
                        }
                    }
                }
            }
            return default;
        }
        //Dùng để thực thi các truy vấn không trả về bản ghi như Update, Delete trả về số dòng bị ảnh hưởng -> 204 No Content
        //Mapper: hàm ánh xạ từ SqlDataReader sang kiểu T
        public async Task<int> ExecuteNonQueryAsync(
            string sqlText,
            SqlParameter[]? parameters = null,
            CommandType commandType = CommandType.Text)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                using (var command = new SqlCommand(sqlText, connection))
                {
                    command.CommandType = commandType;
                    if (parameters != null) command.Parameters.AddRange(parameters);
                    await connection.OpenAsync();
                    return await command.ExecuteNonQueryAsync();
                }
            }
        }
        //Dùng để thực thi các truy vấn trả về một giá trị đơn Id dùng để create -> trả về id của bản ghi vừa tạo -> 201 Created
        public async Task<T?> ExecuteScalarAsync<T>(
            string sqlText,
            SqlParameter[]? parameters = null,
            CommandType commandType = CommandType.Text)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                using (var command = new SqlCommand(sqlText, connection))
                {
                    command.CommandType = commandType;
                    if (parameters != null) command.Parameters.AddRange(parameters);
                    await connection.OpenAsync();
                    var result = await command.ExecuteScalarAsync();

                    if (result == null || result == DBNull.Value)
                    {
                        return default;
                    }

                    return (T)Convert.ChangeType(result, typeof(T));
                }
            }
        }


        public async Task<T?> ExecuteScalarAsync<T>(
           string sqlText,
           SqlConnection connection,
           SqlTransaction transaction,
           SqlParameter[]? parameters = null,
           CommandType commandType = CommandType.Text)
        {
            using (var command = new SqlCommand(sqlText, connection, transaction))
            {
                command.CommandType = commandType;
                if (parameters != null) command.Parameters.AddRange(parameters);
                var result = await command.ExecuteScalarAsync();
                if (result == null || result == DBNull.Value)
                {
                    return default;
                }
                return (T)Convert.ChangeType(result, typeof(T));
            }
        }

        public async Task<int> ExecuteNonQueryAsync(
            string sqlText,
            SqlConnection connection,
            SqlTransaction transaction,
            SqlParameter[]? parameters = null,
            CommandType commandType = CommandType.Text)
        {
            using (var command = new SqlCommand(sqlText, connection, transaction))
            {
                command.CommandType = commandType;
                if (parameters != null) command.Parameters.AddRange(parameters);
                return await command.ExecuteNonQueryAsync();
            }
        }
        #region Hoàng Duy
        //User
        public async Task<string> GetHashPasswordAsync(string Username)
        {
            string password = "";
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                using (var command = new SqlCommand("sp_GetHashPassword", connection))
                {

                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@Username", SqlDbType.NVarChar) { Value = Username });

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            password = reader["password"]?.ToString() ?? "";
                        }
                    }
                }
            }
            return password;
        }
        public async Task<bool> CheckUserExistsAsync(string username)
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            using var command = new SqlCommand("sp_CheckUserExists", connection);
            command.CommandType = CommandType.StoredProcedure;
            command.Parameters.Add("@Username", SqlDbType.NVarChar, 100).Value = username;

            var result = await command.ExecuteScalarAsync();

            return result != null && Convert.ToBoolean(result);
        }

        public async Task<UserDTO?> GetUserAsync(int? userId = null, string? UserName = null)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                using (var cmd = new SqlCommand("sp_GetUser", connection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    if (userId != null)
                        cmd.Parameters.Add(new SqlParameter("@UserID", SqlDbType.Int)).Value = userId;
                    if (UserName != null)
                        cmd.Parameters.Add(new SqlParameter("@UserName", SqlDbType.NVarChar)).Value = UserName;
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            return new UserDTO
                            {
                                Id = Convert.ToInt32(reader["id"]),
                                UserName = reader["username"]?.ToString() ?? "",
                                CreatedAt = Convert.ToDateTime(reader["create_at"]),
                                Role = Convert.ToInt32(reader["role"]),
                                Status = Convert.ToBoolean(reader["status"])
                            };
                        }
                    }
                    ;
                }
                return null;
            }
        }
        public async Task InsertUserAsync(string username, string password)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                using (var cmd = new SqlCommand("sp_InsertUser", connection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add(new SqlParameter("@Username", SqlDbType.NVarChar, 100) { Value = username });
                    cmd.Parameters.Add(new SqlParameter("@PasswordHash", SqlDbType.NVarChar, 255) { Value = password });

                    await cmd.ExecuteNonQueryAsync();
                }
            }
        }
        public async Task UpdatePassword(int UserID, string Password)
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    var newPassword = PasswordHasher.HashPassword(Password);
                    using (var cmd = new SqlCommand("sp_UpdatePassword", connection))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add(new SqlParameter("@UserID", SqlDbType.Int) { Value = UserID });
                        cmd.Parameters.Add(new SqlParameter("@NewPassword", SqlDbType.NVarChar) { Value = newPassword });

                        await cmd.ExecuteNonQueryAsync();
                    }
                }
            }
            catch
            {
            }
        }
        #endregion
    }
}
