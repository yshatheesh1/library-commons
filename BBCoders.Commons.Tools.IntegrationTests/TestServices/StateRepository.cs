//------------------------------------------------------------------------------
// <auto-generated>
//
// Manual changes to this file may cause unexpected behavior in your application.
// Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------
using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using MySqlConnector;

namespace BBCoders.Example.DataServices
{
    public class StateRepository
    {
        private readonly string _connectionString;
        public StateRepository(string connectionString){ this._connectionString = connectionString; }
        public async Task<StateModel> SelectState(Int64 Id)
        {
            using(var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                string sql = @"SELECT * FROM `States` AS `s` WHERE `s`.`Id` = @Id";
                var cmd = new MySqlCommand(sql, connection);
                cmd.Parameters.AddWithValue("@Id", Id);
                return await GetResult(cmd);
            }
        }
        private async Task<StateModel> GetResult(MySqlCommand cmd, StateModel result = null)
        {
            var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                if(result == null) result = new StateModel();
                result.Id = (Int64)reader["Id"];
                result.Name = Convert.IsDBNull(reader["Name"]) ? null : (String?)reader["Name"];
                result.StateId = (Byte[])reader["StateId"];
            }
            reader.Close();
            return result;
        }
        public async Task<StateModel> InsertState(StateModel StateModel)
        {
            using(var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                string sql = @"INSERT INTO `States` (`Name`, `StateId`) VALUES (@Name, @StateId);
                SELECT * FROM `States` AS `s` WHERE `s`.`Id` = LAST_INSERT_ID()";
                var cmd = new MySqlCommand(sql, connection);
                cmd.Parameters.AddWithValue("@Name", StateModel.Name);
                cmd.Parameters.AddWithValue("@StateId", StateModel.StateId);
                return await GetResult(cmd, StateModel);
            }
        }
        public async Task<int> UpdateState(StateModel StateModel)
        {
            using(var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                string sql = @"UPDATE `States` AS `s` SET `s`.`Name` = @Name, `s`.`StateId` = @StateId WHERE `s`.`Id` = @Id;";
                var cmd = new MySqlCommand(sql, connection);
                cmd.Parameters.AddWithValue("@Id", StateModel.Id);
                cmd.Parameters.AddWithValue("@Name", StateModel.Name);
                cmd.Parameters.AddWithValue("@StateId", StateModel.StateId);
                return await cmd.ExecuteNonQueryAsync();
            }
        }
        public async Task<int> DeleteState(Int64 Id)
        {
            using(var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                string sql = @"DELETE FROM `States` AS `s` WHERE `s`.`Id` = @Id";
                var cmd = new MySqlCommand(sql, connection);
                cmd.Parameters.AddWithValue("@Id", Id);
                return await cmd.ExecuteNonQueryAsync();
            }
        }
    }
}
