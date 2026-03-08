using Microsoft.Data.SqlClient;
using System.Data;
namespace ApiThiBangLaiXeOto.Data
{
    public class SqlHelper
    {
        public readonly string _connectionString;

        public SqlHelper(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")!;
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
        public async Task<int> ExecuteScalarAsync(
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
                        return 0;
                    }

                    return int.Parse(result.ToString()!);
                }
            }
        }

        public async Task<int> ExecuteScalarAsync(
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
                return (result == null || result == DBNull.Value) ? 0 : Convert.ToInt32(result);
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
    }
}
