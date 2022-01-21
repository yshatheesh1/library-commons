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
using System.Data;
using System.Data.Common;
using System.Text;
using BBCoders.Example.DataModels;

namespace BBCoders.Example.DataServices
{
    public static class StatusRepository
    {
        public static async Task<List<StatusModel>> SelectBatchStatuses(this DbConnection connection, List<StatusKey> StatusKey, DbTransaction transaction = null, int? timeout = null)
        {
            var Id1sJoined = string.Join(",", StatusKey.Select((_, idx) => "@Id1" + idx));
            var Id2sJoined = string.Join(",", StatusKey.Select((_, idx) => "@Id2" + idx));
            var sql = $"SELECT `s`.`Id1`,`s`.`Id2`,`s`.`Description`,`s`.`StatusId` FROM `Status` AS `s` WHERE `s`.`Id1` IN (" + Id1sJoined + ") AND `s`.`Id2` IN (" + Id2sJoined + ");";
            var command = connection.CreateCommand(sql, transaction, timeout);
            for (var i = 0; i< StatusKey.Count(); i++)
            {
                command.CreateParameter("@Id1" + i, StatusKey[i].Id1);
                command.CreateParameter("@Id2" + i, StatusKey[i].Id2);
            }
            if (connection.State == ConnectionState.Closed)
                await connection.OpenAsync();
            var results = new List<StatusModel>();
            var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                var result = new StatusModel();
                result.Id1 = (Int64)reader[0];
                result.Id2 = (Int64)reader[1];
                result.Description = Convert.IsDBNull(reader[2]) ? null : (String?)reader[2];
                result.StatusId = (Guid)reader[3];
                results.Add(result);
            }
            reader.Close();
            return results;
        }
        public static async Task<List<StatusModel>> InsertBatchStatuses(this DbConnection connection, List<StatusModel> StatusModel, DbTransaction transaction = null, int? timeout = null)
        {
            var sqlBuilder = new StringBuilder();
            for (var i = 0; i< StatusModel.Count(); i++)
            {
                sqlBuilder.AppendLine($"INSERT INTO `Status` (`Id1`, `Id2`, `Description`, `StatusId`) VALUES (@Id1{i}, @Id2{i}, @Description{i}, @StatusId{i});");
                sqlBuilder.AppendLine($"SELECT `s`.`Id1`,`s`.`Id2`,`s`.`Description`,`s`.`StatusId` FROM `Status` AS `s` WHERE `s`.`Id1` = @Id1{i} AND `s`.`Id2` = @Id2{i};");
            }
            var sql = sqlBuilder.ToString();
            var command = connection.CreateCommand(sql, transaction, timeout);
            for (var i = 0; i< StatusModel.Count(); i++)
            {
                command.CreateParameter("@Id1" + i, StatusModel[i].Id1);
                command.CreateParameter("@Id2" + i, StatusModel[i].Id2);
                command.CreateParameter("@Description" + i, StatusModel[i].Description);
                command.CreateParameter("@StatusId" + i, StatusModel[i].StatusId);
            }
            if (connection.State == ConnectionState.Closed)
                await connection.OpenAsync();
            var results = new List<StatusModel>();
            var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync() || (await reader.NextResultAsync() && await reader.ReadAsync()))
            {
                var result = new StatusModel();
                result.Id1 = (Int64)reader[0];
                result.Id2 = (Int64)reader[1];
                result.Description = Convert.IsDBNull(reader[2]) ? null : (String?)reader[2];
                result.StatusId = (Guid)reader[3];
                results.Add(result);
            }
            reader.Close();
            return results;
        }
        public static async Task<List<StatusModel>> UpdateBatchStatuses(this DbConnection connection, List<StatusModel> StatusModel, DbTransaction transaction = null, int? timeout = null)
        {
            var sqlBuilder = new StringBuilder();
            for (var i = 0; i< StatusModel.Count(); i++)
            {
                sqlBuilder.AppendLine($"UPDATE `Status` AS `s` SET `s`.`Description` = @Description{i}, `s`.`StatusId` = @StatusId{i} WHERE `s`.`Id1` = @Id1{i} AND `s`.`Id2` = @Id2{i};");
                sqlBuilder.AppendLine($"SELECT `s`.`Id1`,`s`.`Id2`,`s`.`Description`,`s`.`StatusId` FROM `Status` AS `s` WHERE `s`.`Id1` = @Id1{i} AND `s`.`Id2` = @Id2{i};");
            }
            var sql = sqlBuilder.ToString();
            var command = connection.CreateCommand(sql, transaction, timeout);
            for (var i = 0; i< StatusModel.Count(); i++)
            {
                command.CreateParameter("@Id1" + i, StatusModel[i].Id1);
                command.CreateParameter("@Id2" + i, StatusModel[i].Id2);
                command.CreateParameter("@Description" + i, StatusModel[i].Description);
                command.CreateParameter("@StatusId" + i, StatusModel[i].StatusId);
            }
            if (connection.State == ConnectionState.Closed)
                await connection.OpenAsync();
            var results = new List<StatusModel>();
            var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync() || (await reader.NextResultAsync() && await reader.ReadAsync()))
            {
                var result = new StatusModel();
                result.Id1 = (Int64)reader[0];
                result.Id2 = (Int64)reader[1];
                result.Description = Convert.IsDBNull(reader[2]) ? null : (String?)reader[2];
                result.StatusId = (Guid)reader[3];
                results.Add(result);
            }
            reader.Close();
            return results;
        }
        public static async Task<int> DeleteBatchStatuses(this DbConnection connection, List<StatusKey> StatusKey, DbTransaction transaction = null, int? timeout = null)
        {
            var Id1sJoined = string.Join(",", StatusKey.Select((_, idx) => "@Id1" + idx));
            var Id2sJoined = string.Join(",", StatusKey.Select((_, idx) => "@Id2" + idx));
            var sql = $"DELETE FROM `Status` AS `s` WHERE `s`.`Id1` IN (" + Id1sJoined + ") AND `s`.`Id2` IN (" + Id2sJoined + ");";
            var command = connection.CreateCommand(sql, transaction, timeout);
            for (var i = 0; i< StatusKey.Count(); i++)
            {
                command.CreateParameter("@Id1" + i, StatusKey[i].Id1);
                command.CreateParameter("@Id2" + i, StatusKey[i].Id2);
            }
            if (connection.State == ConnectionState.Closed)
                await connection.OpenAsync();
            return await command.ExecuteNonQueryAsync();
        }
        public static async Task<StatusModel> SelectStatuses(this DbConnection connection, StatusKey StatusKey, DbTransaction transaction = null, int? timeout = null)
        {
            var sql = $"SELECT `s`.`Id1`,`s`.`Id2`,`s`.`Description`,`s`.`StatusId` FROM `Status` AS `s` WHERE `s`.`Id1` = @Id1 AND `s`.`Id2` = @Id2;";
            var command = connection.CreateCommand(sql, transaction, timeout);
            command.CreateParameter("@Id1", StatusKey.Id1);
            command.CreateParameter("@Id2", StatusKey.Id2);
            if (connection.State == ConnectionState.Closed)
                await connection.OpenAsync();
            var results = new List<StatusModel>();
            var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                var result = new StatusModel();
                result.Id1 = (Int64)reader[0];
                result.Id2 = (Int64)reader[1];
                result.Description = Convert.IsDBNull(reader[2]) ? null : (String?)reader[2];
                result.StatusId = (Guid)reader[3];
                results.Add(result);
            }
            reader.Close();
            return results.FirstOrDefault();
        }
        public static async Task<StatusModel> InsertStatuses(this DbConnection connection, StatusModel StatusModel, DbTransaction transaction = null, int? timeout = null)
        {
            var sql = @"INSERT INTO `Status` (`Id1`, `Id2`, `Description`, `StatusId`) VALUES (@Id1, @Id2, @Description, @StatusId);SELECT `s`.`Id1`,`s`.`Id2`,`s`.`Description`,`s`.`StatusId` FROM `Status` AS `s` WHERE `s`.`Id1` = @Id1 AND `s`.`Id2` = @Id2;";
            var command = connection.CreateCommand(sql, transaction, timeout);
            command.CreateParameter("@Id1", StatusModel.Id1);
            command.CreateParameter("@Id2", StatusModel.Id2);
            command.CreateParameter("@Description", StatusModel.Description);
            command.CreateParameter("@StatusId", StatusModel.StatusId);
            if (connection.State == ConnectionState.Closed)
                await connection.OpenAsync();
            var results = new List<StatusModel>();
            var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                var result = new StatusModel();
                result.Id1 = (Int64)reader[0];
                result.Id2 = (Int64)reader[1];
                result.Description = Convert.IsDBNull(reader[2]) ? null : (String?)reader[2];
                result.StatusId = (Guid)reader[3];
                results.Add(result);
            }
            reader.Close();
            return results.FirstOrDefault();
        }
        public static async Task<StatusModel> UpdateStatuses(this DbConnection connection, StatusModel StatusModel, DbTransaction transaction = null, int? timeout = null)
        {
            var sql = @"UPDATE `Status` AS `s` SET `s`.`Description` = @Description, `s`.`StatusId` = @StatusId WHERE `s`.`Id1` = @Id1 AND `s`.`Id2` = @Id2;SELECT `s`.`Id1`,`s`.`Id2`,`s`.`Description`,`s`.`StatusId` FROM `Status` AS `s` WHERE `s`.`Id1` = @Id1 AND `s`.`Id2` = @Id2;";
            var command = connection.CreateCommand(sql, transaction, timeout);
            command.CreateParameter("@Id1", StatusModel.Id1);
            command.CreateParameter("@Id2", StatusModel.Id2);
            command.CreateParameter("@Description", StatusModel.Description);
            command.CreateParameter("@StatusId", StatusModel.StatusId);
            if (connection.State == ConnectionState.Closed)
                await connection.OpenAsync();
            var results = new List<StatusModel>();
            var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                var result = new StatusModel();
                result.Id1 = (Int64)reader[0];
                result.Id2 = (Int64)reader[1];
                result.Description = Convert.IsDBNull(reader[2]) ? null : (String?)reader[2];
                result.StatusId = (Guid)reader[3];
                results.Add(result);
            }
            reader.Close();
            return results.FirstOrDefault();
        }
        public static async Task<int> DeleteStatuses(this DbConnection connection, StatusKey StatusKey, DbTransaction transaction = null, int? timeout = null)
        {
            var sql = $"DELETE FROM `Status` AS `s` WHERE `s`.`Id1` = @Id1 AND `s`.`Id2` = @Id2;";
            var command = connection.CreateCommand(sql, transaction, timeout);
            command.CreateParameter("@Id1", StatusKey.Id1);
            command.CreateParameter("@Id2", StatusKey.Id2);
            if (connection.State == ConnectionState.Closed)
                await connection.OpenAsync();
            return await command.ExecuteNonQueryAsync();
        }
        private static DbCommand CreateCommand(this DbConnection connection, string sql, DbTransaction transaction = null, int? timeout = null)
        {
            var dbCommand = connection.CreateCommand();
            dbCommand.CommandText = sql;
            dbCommand.CommandType = CommandType.Text;
            dbCommand.Transaction = transaction;
            dbCommand.CommandTimeout = timeout.HasValue ? timeout.Value : dbCommand.CommandTimeout;
            return dbCommand;
        }
        private static DbParameter CreateParameter(this DbCommand command, string name, object value)
        {
            var parameter = command.CreateParameter();
            parameter.ParameterName = name;
            parameter.Value = value;
            command.Parameters.Add(parameter);
            return parameter;
        }
    }
}
