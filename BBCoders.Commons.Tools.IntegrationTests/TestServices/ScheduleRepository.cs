//------------------------------------------------------------------------------
// <auto-generated>
//
// Manual changes to this file may cause unexpected behavior in your application.
// Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------
using System;
using System.Threading.Tasks;
using MySqlConnector;

namespace BBCoders.Example.DataServices
{
    public class ScheduleRepository
    {
        private readonly string _connectionString;
        public ScheduleRepository(string connectionString){ this._connectionString = connectionString; }
        public async Task<ScheduleModel> SelectSchedule(Int64 Id)
        {
            using(var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                string sql = @"SELECT * FROM `Schedules` AS `s` WHERE `s`.`Id` = @Id";
                var cmd = new MySqlCommand(sql, connection);
                cmd.Parameters.AddWithValue("@Id", Id);
                return await GetResult(cmd);
            }
        }
        private async Task<ScheduleModel> GetResult(MySqlCommand cmd, ScheduleModel result = null)
        {
            var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                if(result == null) result = new ScheduleModel();
                result.Id = (Int64)reader["Id"];
                result.ActionId = Convert.IsDBNull(reader["ActionId"]) ? null : (Int64?)reader["ActionId"];
                result.CreatedById = (Int64)reader["CreatedById"];
                result.CreatedDate = (DateTime)reader["CreatedDate"];
                result.FingerPrintId = Convert.IsDBNull(reader["FingerPrintId"]) ? null : (Int64?)reader["FingerPrintId"];
                result.LastUpdatedById = (Int64)reader["LastUpdatedById"];
                result.LastUpdatedDate = (DateTime)reader["LastUpdatedDate"];
                result.ScheduleDate = (DateTime)reader["ScheduleDate"];
                result.ScheduleId = (Byte[])reader["ScheduleId"];
                result.ScheduleSiteId = (Int64)reader["ScheduleSiteId"];
            }
            reader.Close();
            return result;
        }
        public async Task<ScheduleModel> InsertSchedule(ScheduleModel ScheduleModel)
        {
            using(var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                string sql = @"INSERT INTO `Schedules` (`ActionId`, `CreatedById`, `CreatedDate`, `FingerPrintId`, `LastUpdatedById`, `LastUpdatedDate`, `ScheduleDate`, `ScheduleId`, `ScheduleSiteId`) VALUES (@ActionId, @CreatedById, @CreatedDate, @FingerPrintId, @LastUpdatedById, @LastUpdatedDate, @ScheduleDate, @ScheduleId, @ScheduleSiteId);
                SELECT * FROM `Schedules` AS `s` WHERE `s`.`Id` = LAST_INSERT_ID()";
                var cmd = new MySqlCommand(sql, connection);
                cmd.Parameters.AddWithValue("@ActionId", ScheduleModel.ActionId);
                cmd.Parameters.AddWithValue("@CreatedById", ScheduleModel.CreatedById);
                cmd.Parameters.AddWithValue("@CreatedDate", ScheduleModel.CreatedDate);
                cmd.Parameters.AddWithValue("@FingerPrintId", ScheduleModel.FingerPrintId);
                cmd.Parameters.AddWithValue("@LastUpdatedById", ScheduleModel.LastUpdatedById);
                cmd.Parameters.AddWithValue("@LastUpdatedDate", ScheduleModel.LastUpdatedDate);
                cmd.Parameters.AddWithValue("@ScheduleDate", ScheduleModel.ScheduleDate);
                cmd.Parameters.AddWithValue("@ScheduleId", ScheduleModel.ScheduleId);
                cmd.Parameters.AddWithValue("@ScheduleSiteId", ScheduleModel.ScheduleSiteId);
                return await GetResult(cmd, ScheduleModel);
            }
        }
        public async Task<int> UpdateSchedule(ScheduleModel ScheduleModel)
        {
            using(var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                string sql = @"UPDATE `Schedules` AS `s` SET `s`.`ActionId` = @ActionId, `s`.`CreatedById` = @CreatedById, `s`.`CreatedDate` = @CreatedDate, `s`.`FingerPrintId` = @FingerPrintId, `s`.`LastUpdatedById` = @LastUpdatedById, `s`.`LastUpdatedDate` = @LastUpdatedDate, `s`.`ScheduleDate` = @ScheduleDate, `s`.`ScheduleId` = @ScheduleId, `s`.`ScheduleSiteId` = @ScheduleSiteId WHERE `s`.`Id` = @Id;";
                var cmd = new MySqlCommand(sql, connection);
                cmd.Parameters.AddWithValue("@Id", ScheduleModel.Id);
                cmd.Parameters.AddWithValue("@ActionId", ScheduleModel.ActionId);
                cmd.Parameters.AddWithValue("@CreatedById", ScheduleModel.CreatedById);
                cmd.Parameters.AddWithValue("@CreatedDate", ScheduleModel.CreatedDate);
                cmd.Parameters.AddWithValue("@FingerPrintId", ScheduleModel.FingerPrintId);
                cmd.Parameters.AddWithValue("@LastUpdatedById", ScheduleModel.LastUpdatedById);
                cmd.Parameters.AddWithValue("@LastUpdatedDate", ScheduleModel.LastUpdatedDate);
                cmd.Parameters.AddWithValue("@ScheduleDate", ScheduleModel.ScheduleDate);
                cmd.Parameters.AddWithValue("@ScheduleId", ScheduleModel.ScheduleId);
                cmd.Parameters.AddWithValue("@ScheduleSiteId", ScheduleModel.ScheduleSiteId);
                return await cmd.ExecuteNonQueryAsync();
            }
        }
        public async Task<int> DeleteSchedule(Int64 Id)
        {
            using(var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                string sql = @"DELETE FROM `Schedules` AS `s` WHERE `s`.`Id` = @Id";
                var cmd = new MySqlCommand(sql, connection);
                cmd.Parameters.AddWithValue("@Id", Id);
                return await cmd.ExecuteNonQueryAsync();
            }
        }
        public async Task<GetSheduleResponseModel> GetShedule(GetSheduleRequestModel GetSheduleRequestModel)
        {
            using(var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                string sql = @"SELECT `s`.`Id`, `s`.`ActionId`, `s`.`CreatedById`, `s`.`CreatedDate`, `s`.`FingerPrintId`, `s`.`LastUpdatedById`, `s`.`LastUpdatedDate`, `s`.`ScheduleDate`, `s`.`ScheduleId`, `s`.`ScheduleSiteId`, `s0`.`Id`, `s0`.`IsActive`, `s0`.`Name`, `s0`.`ScheduleSiteId`
				FROM `Schedules` AS `s`
				INNER JOIN `ScheduleSites` AS `s0` ON `s`.`ScheduleSiteId` = `s0`.`Id`
				WHERE `s`.`ScheduleId` = @__Value_0";
                var cmd = new MySqlCommand(sql, connection);
                cmd.Parameters.AddWithValue("@__Value_0", GetSheduleRequestModel.id);
                GetSheduleResponseModel result = null;
                var reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    result = new GetSheduleResponseModel();
                    result.ScheduleId = (Int64)reader[0];
                    result.ScheduleActionId = Convert.IsDBNull(reader[1]) ? null : (Int64?)reader[1];
                    result.ScheduleCreatedById = (Int64)reader[2];
                    result.ScheduleCreatedDate = (DateTime)reader[3];
                    result.ScheduleFingerPrintId = Convert.IsDBNull(reader[4]) ? null : (Int64?)reader[4];
                    result.ScheduleLastUpdatedById = (Int64)reader[5];
                    result.ScheduleLastUpdatedDate = (DateTime)reader[6];
                    result.ScheduleScheduleDate = (DateTime)reader[7];
                    result.ScheduleScheduleId = (Byte[])reader[8];
                    result.ScheduleScheduleSiteId = (Int64)reader[9];
                    result.ScheduleSiteId = (Int64)reader[10];
                    result.ScheduleSiteIsActive = (Boolean)reader[11];
                    result.ScheduleSiteName = (String)reader[12];
                    result.ScheduleSiteScheduleSiteId = (Byte[])reader[13];
                }
                reader.Close();
                return result;
            }
        }
        public async Task<GetScheduleActionAndLocationResponseModel> GetScheduleActionAndLocation(GetScheduleActionAndLocationRequestModel GetScheduleActionAndLocationRequestModel)
        {
            using(var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                string sql = @"SELECT `s`.`Id`, `s`.`ActionId`, `s`.`CreatedById`, `s`.`CreatedDate`, `s`.`FingerPrintId`, `s`.`LastUpdatedById`, `s`.`LastUpdatedDate`, `s`.`ScheduleDate`, `s`.`ScheduleId`, `s`.`ScheduleSiteId`, `a`.`Id`, `a`.`ActionId`, `a`.`Name`, `s0`.`Id`, `s0`.`IsActive`, `s0`.`Name`, `s0`.`ScheduleSiteId`
				FROM `Schedules` AS `s`
				INNER JOIN `Actions` AS `a` ON `s`.`ActionId` = `a`.`Id`
				INNER JOIN `ScheduleSites` AS `s0` ON `s`.`ScheduleSiteId` = `s0`.`Id`
				WHERE (`a`.`ActionId` = @__Value_0) AND (`s0`.`ScheduleSiteId` = @__Value_1)";
                var cmd = new MySqlCommand(sql, connection);
                cmd.Parameters.AddWithValue("@__Value_0", GetScheduleActionAndLocationRequestModel.ActionId);
                cmd.Parameters.AddWithValue("@__Value_1", GetScheduleActionAndLocationRequestModel.LocationId);
                GetScheduleActionAndLocationResponseModel result = null;
                var reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    result = new GetScheduleActionAndLocationResponseModel();
                    result.ScheduleId = (Int64)reader[0];
                    result.ScheduleActionId = Convert.IsDBNull(reader[1]) ? null : (Int64?)reader[1];
                    result.ScheduleCreatedById = (Int64)reader[2];
                    result.ScheduleCreatedDate = (DateTime)reader[3];
                    result.ScheduleFingerPrintId = Convert.IsDBNull(reader[4]) ? null : (Int64?)reader[4];
                    result.ScheduleLastUpdatedById = (Int64)reader[5];
                    result.ScheduleLastUpdatedDate = (DateTime)reader[6];
                    result.ScheduleScheduleDate = (DateTime)reader[7];
                    result.ScheduleScheduleId = (Byte[])reader[8];
                    result.ScheduleScheduleSiteId = (Int64)reader[9];
                    result.ActionId = (Int64)reader[10];
                    result.ActionActionId = (Byte[])reader[11];
                    result.ActionName = (String)reader[12];
                    result.ScheduleSiteId = (Int64)reader[13];
                    result.ScheduleSiteIsActive = (Boolean)reader[14];
                    result.ScheduleSiteName = (String)reader[15];
                    result.ScheduleSiteScheduleSiteId = (Byte[])reader[16];
                }
                reader.Close();
                return result;
            }
        }
    }
}
