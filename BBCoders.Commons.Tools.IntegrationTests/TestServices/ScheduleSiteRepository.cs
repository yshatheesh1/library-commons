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
    public class ScheduleSiteRepository
    {
        private readonly string _connectionString;
        public ScheduleSiteRepository(string connectionString){ this._connectionString = connectionString; }
        public async Task<ScheduleSiteModel> SelectScheduleSite(Int64 Id)
        {
            using(var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                string sql = @"SELECT * FROM `ScheduleSites` AS `s` WHERE `s`.`Id` = @Id";
                var cmd = new MySqlCommand(sql, connection);
                cmd.Parameters.AddWithValue("@Id", Id);
                return await GetResult(cmd);
            }
        }
        private async Task<ScheduleSiteModel> GetResult(MySqlCommand cmd, ScheduleSiteModel result = null)
        {
            var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                if(result == null) result = new ScheduleSiteModel();
                result.Id = (Int64)reader["Id"];
                result.IsActive = (Boolean)reader["IsActive"];
                result.Name = (String)reader["Name"];
                result.ScheduleSiteId = (Byte[])reader["ScheduleSiteId"];
            }
            reader.Close();
            return result;
        }
        public async Task<ScheduleSiteModel> InsertScheduleSite(ScheduleSiteModel ScheduleSiteModel)
        {
            using(var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                string sql = @"INSERT INTO `ScheduleSites` (`IsActive`, `Name`, `ScheduleSiteId`) VALUES (@IsActive, @Name, @ScheduleSiteId);
                SELECT * FROM `ScheduleSites` AS `s` WHERE `s`.`Id` = LAST_INSERT_ID()";
                var cmd = new MySqlCommand(sql, connection);
                cmd.Parameters.AddWithValue("@IsActive", ScheduleSiteModel.IsActive);
                cmd.Parameters.AddWithValue("@Name", ScheduleSiteModel.Name);
                cmd.Parameters.AddWithValue("@ScheduleSiteId", ScheduleSiteModel.ScheduleSiteId);
                return await GetResult(cmd, ScheduleSiteModel);
            }
        }
        public async Task<int> UpdateScheduleSite(ScheduleSiteModel ScheduleSiteModel)
        {
            using(var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                string sql = @"UPDATE `ScheduleSites` AS `s` SET `s`.`IsActive` = @IsActive, `s`.`Name` = @Name, `s`.`ScheduleSiteId` = @ScheduleSiteId WHERE `s`.`Id` = @Id;";
                var cmd = new MySqlCommand(sql, connection);
                cmd.Parameters.AddWithValue("@Id", ScheduleSiteModel.Id);
                cmd.Parameters.AddWithValue("@IsActive", ScheduleSiteModel.IsActive);
                cmd.Parameters.AddWithValue("@Name", ScheduleSiteModel.Name);
                cmd.Parameters.AddWithValue("@ScheduleSiteId", ScheduleSiteModel.ScheduleSiteId);
                return await cmd.ExecuteNonQueryAsync();
            }
        }
        public async Task<int> DeleteScheduleSite(Int64 Id)
        {
            using(var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                string sql = @"DELETE FROM `ScheduleSites` AS `s` WHERE `s`.`Id` = @Id";
                var cmd = new MySqlCommand(sql, connection);
                cmd.Parameters.AddWithValue("@Id", Id);
                return await cmd.ExecuteNonQueryAsync();
            }
        }
        public async Task<System.Collections.Generic.List<GetScheduleSitesByLocationResponseModel>> GetScheduleSitesByLocation(GetScheduleSitesByLocationRequestModel GetScheduleSitesByLocationRequestModel)
        {
            using(var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                string sql = @"SELECT `s`.`Id`, `s`.`IsActive`, `s`.`Name`, `s`.`ScheduleSiteId`
				FROM `ScheduleSites` AS `s`
				WHERE `s`.`Name` LIKE @__Format_1";
                var cmd = new MySqlCommand(sql, connection);
                cmd.Parameters.AddWithValue("@__Format_1", GetScheduleSitesByLocationRequestModel.location);
                System.Collections.Generic.List<GetScheduleSitesByLocationResponseModel> results = new System.Collections.Generic.List<GetScheduleSitesByLocationResponseModel>();
                var reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    GetScheduleSitesByLocationResponseModel result = new GetScheduleSitesByLocationResponseModel();
                    result.ScheduleSiteId = (Int64)reader[0];
                    result.ScheduleSiteIsActive = (Boolean)reader[1];
                    result.ScheduleSiteName = (String)reader[2];
                    result.ScheduleSiteScheduleSiteId = (Byte[])reader[3];
                    results.Add(result);
                }
                reader.Close();
                return results;
            }
        }
        public async Task<System.Collections.Generic.List<GetSheduleSiteStatusResponseModel>> GetSheduleSiteStatus(GetSheduleSiteStatusRequestModel GetSheduleSiteStatusRequestModel)
        {
            using(var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                string sql = @"SELECT `s`.`Id`, `s`.`IsActive`, `s`.`Name`, `s`.`ScheduleSiteId`
				FROM `ScheduleSites` AS `s`
				WHERE `s`.`ScheduleSiteId` = @__Value_0";
                var cmd = new MySqlCommand(sql, connection);
                cmd.Parameters.AddWithValue("@__Value_0", GetSheduleSiteStatusRequestModel.id);
                System.Collections.Generic.List<GetSheduleSiteStatusResponseModel> results = new System.Collections.Generic.List<GetSheduleSiteStatusResponseModel>();
                var reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    GetSheduleSiteStatusResponseModel result = new GetSheduleSiteStatusResponseModel();
                    result.ScheduleSiteId = (Int64)reader[0];
                    result.ScheduleSiteIsActive = (Boolean)reader[1];
                    result.ScheduleSiteName = (String)reader[2];
                    result.ScheduleSiteScheduleSiteId = (Byte[])reader[3];
                    results.Add(result);
                }
                reader.Close();
                return results;
            }
        }
    }
}
