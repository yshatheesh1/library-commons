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
    public class StateRepository
    {
        private readonly string _connectionString;
        public StateRepository(string connectionString){ this._connectionString = connectionString; }
        public async Task<StateSelectModel> SelectState(Int64 Id)
        {
            using(var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                string sql = @"SELECT `s`.`Id`,`s`.`Name`,`s`.`StateId` 
                FROM `States` AS `s`
                WHERE `s`.`Id` = @Id";
                var cmd = new MySqlCommand(sql, connection);
                cmd.Parameters.AddWithValue("@Id", Id);
                StateSelectModel result = null;
                var reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    result = new StateSelectModel();
                    result.Id = (Int64)reader["Id"];
                    result.Name = (String?)reader["Name"];
                    result.StateId = (Byte[])reader["StateId"];
                }
                reader.Close();
                return result;
            }
        }
        public async Task<Int64> InsertState(StateInsertModel StateInsertModel)
        {
            using(var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                string sql = @"INSERT INTO `States` (`Name`, `StateId`) VALUES (@Name, @StateId);
                SELECT LAST_INSERT_ID()";
                var cmd = new MySqlCommand(sql, connection);
                cmd.Parameters.AddWithValue("@Name", StateInsertModel.Name);
                cmd.Parameters.AddWithValue("@StateId", StateInsertModel.StateId);
                return Convert.ToInt64(await cmd.ExecuteScalarAsync());
            }
        }
        public async Task<int> UpdateState(StateUpdateModel StateUpdateModel)
        {
            using(var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                string sql = @"UPDATE `States` AS `s`
                SET `s`.`Name` = @Name, `s`.`StateId` = @StateId
                WHERE `s`.`Id` = @Id";
                var cmd = new MySqlCommand(sql, connection);
                cmd.Parameters.AddWithValue("@Id", StateUpdateModel.Id);
                cmd.Parameters.AddWithValue("@Name", StateUpdateModel.Name);
                cmd.Parameters.AddWithValue("@StateId", StateUpdateModel.StateId);
                return await cmd.ExecuteNonQueryAsync();
            }
        }
        public async Task<int> DeleteState(Int64 Id)
        {
            using(var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                string sql = @"DELETE FROM `States` AS `s`
                WHERE `s`.`Id` = @Id";
                var cmd = new MySqlCommand(sql, connection);
                cmd.Parameters.AddWithValue("@Id", Id);
                return await cmd.ExecuteNonQueryAsync();
            }
        }
    }
}
