using ApiThiBangLaiXeOto.DTOs;
using Microsoft.Data.SqlClient;

namespace ApiThiBangLaiXeOto.Mapper
{
    public static class LicenceMapper
    {
        public static RawLicenceListDto ToRawLicenceListDto(SqlDataReader reader)
        {
            return new RawLicenceListDto
            {
                Id = reader.GetInt32(reader.GetOrdinal("Id")), // Theo SQL của bạn là LicenceId
                LicenceCode = reader.GetString(reader.GetOrdinal("LicenceCode")),
                TotalQuestion = reader.GetInt32(reader.GetOrdinal("TotalQuestion")),
                Duration = reader.GetInt32(reader.GetOrdinal("Duration")),
                PassScore = reader.GetInt32(reader.GetOrdinal("PassScore")),

                // Xử lý QuestionCount (Số lượng câu của từng danh mục) - Có thể NULL
                QuestionCount = reader.IsDBNull(reader.GetOrdinal("QuestionCount"))
                                ? 0
                                : reader.GetInt32(reader.GetOrdinal("QuestionCount")),

                // Xử lý CategoryName (Tên danh mục) - Chắc chắn có NULL ở các bằng B, C...
                CategoryName = reader.IsDBNull(reader.GetOrdinal("CategoryName"))
                               ? string.Empty
                               : reader.GetString(reader.GetOrdinal("CategoryName")),
                CategoryId = reader.IsDBNull(reader.GetOrdinal("CategoryId"))
                                ? 0
                                : reader.GetInt32(reader.GetOrdinal("CategoryId")),
            };
        }
    }
}