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
    public static class ActionRepository
    {
        public static async Task<List<ActionModel>> SelectBatchAction(this DbConnection connection, List<ActionKey> ActionKey, DbTransaction transaction = null, int? timeout = null)
        {
            var IdsJoined = string.Join(",", ActionKey.Select((_, idx) => "@Id" + idx));
            var sql = $"SELECT `a`.`Id`,`a`.`ActionId`,`a`.`Name` FROM `Actions` AS `a` WHERE `a`.`Id` IN (" + IdsJoined + ");";
            var command = connection.CreateCommand(sql, transaction, timeout);
            for (var i = 0; i< ActionKey.Count(); i++)
            {
                command.CreateParameter("@Id" + i, ActionKey[i].Id);
            }
            if (connection.State == ConnectionState.Closed)
                await connection.OpenAsync();
            var results = new List<ActionModel>();
            var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                var result = new ActionModel();
                result.Id = (Int64)reader[0];
                result.ActionId = (Guid)reader[1];
                result.Name = (String)reader[2];
                results.Add(result);
            }
            reader.Close();
            return results;
        }
        public static async Task<List<ActionModel>> InsertBatchAction(this DbConnection connection, List<ActionModel> ActionModel, DbTransaction transaction = null, int? timeout = null)
        {
            var sqlBuilder = new StringBuilder();
            for (var i = 0; i< ActionModel.Count(); i++)
            {
                sqlBuilder.AppendLine($"INSERT INTO `Actions` (`ActionId`, `Name`) VALUES (@ActionId{i}, @Name{i});");
                sqlBuilder.AppendLine($"SELECT `a`.`Id`,`a`.`ActionId`,`a`.`Name` FROM `Actions` AS `a` WHERE `a`.`Id` = LAST_INSERT_ID() AND ROW_COUNT() = 1;");
            }
            var sql = sqlBuilder.ToString();
            var command = connection.CreateCommand(sql, transaction, timeout);
            for (var i = 0; i< ActionModel.Count(); i++)
            {
                command.CreateParameter("@Id" + i, ActionModel[i].Id);
                command.CreateParameter("@ActionId" + i, ActionModel[i].ActionId);
                command.CreateParameter("@Name" + i, ActionModel[i].Name);
            }
            if (connection.State == ConnectionState.Closed)
                await connection.OpenAsync();
            var results = new List<ActionModel>();
            var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync() || (await reader.NextResultAsync() && await reader.ReadAsync()))
            {
                var result = new ActionModel();
                result.Id = (Int64)reader[0];
                result.ActionId = (Guid)reader[1];
                result.Name = (String)reader[2];
                results.Add(result);
            }
            reader.Close();
            return results;
        }
        public static async Task<List<ActionModel>> UpdateBatchAction(this DbConnection connection, List<ActionModel> ActionModel, DbTransaction transaction = null, int? timeout = null)
        {
            var sqlBuilder = new StringBuilder();
            for (var i = 0; i< ActionModel.Count(); i++)
            {
                sqlBuilder.AppendLine($"UPDATE `Actions` AS `a` SET `a`.`ActionId` = @ActionId{i}, `a`.`Name` = @Name{i} WHERE `a`.`Id` = @Id{i};");
                sqlBuilder.AppendLine($"SELECT `a`.`Id`,`a`.`ActionId`,`a`.`Name` FROM `Actions` AS `a` WHERE `a`.`Id` = @Id{i};");
            }
            var sql = sqlBuilder.ToString();
            var command = connection.CreateCommand(sql, transaction, timeout);
            for (var i = 0; i< ActionModel.Count(); i++)
            {
                command.CreateParameter("@Id" + i, ActionModel[i].Id);
                command.CreateParameter("@ActionId" + i, ActionModel[i].ActionId);
                command.CreateParameter("@Name" + i, ActionModel[i].Name);
            }
            if (connection.State == ConnectionState.Closed)
                await connection.OpenAsync();
            var results = new List<ActionModel>();
            var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync() || (await reader.NextResultAsync() && await reader.ReadAsync()))
            {
                var result = new ActionModel();
                result.Id = (Int64)reader[0];
                result.ActionId = (Guid)reader[1];
                result.Name = (String)reader[2];
                results.Add(result);
            }
            reader.Close();
            return results;
        }
        public static async Task<int> DeleteBatchAction(this DbConnection connection, List<ActionKey> ActionKey, DbTransaction transaction = null, int? timeout = null)
        {
            var IdsJoined = string.Join(",", ActionKey.Select((_, idx) => "@Id" + idx));
            var sql = $"DELETE FROM `Actions` AS `a` WHERE `a`.`Id` IN (" + IdsJoined + ");";
            var command = connection.CreateCommand(sql, transaction, timeout);
            for (var i = 0; i< ActionKey.Count(); i++)
            {
                command.CreateParameter("@Id" + i, ActionKey[i].Id);
            }
            if (connection.State == ConnectionState.Closed)
                await connection.OpenAsync();
            return await command.ExecuteNonQueryAsync();
        }
        public static async Task<ActionModel> SelectAction(this DbConnection connection, ActionKey ActionKey, DbTransaction transaction = null, int? timeout = null)
        {
            var sql = $"SELECT `a`.`Id`,`a`.`ActionId`,`a`.`Name` FROM `Actions` AS `a` WHERE `a`.`Id` = @Id;";
            var command = connection.CreateCommand(sql, transaction, timeout);
            command.CreateParameter("@Id", ActionKey.Id);
            if (connection.State == ConnectionState.Closed)
                await connection.OpenAsync();
            var results = new List<ActionModel>();
            var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                var result = new ActionModel();
                result.Id = (Int64)reader[0];
                result.ActionId = (Guid)reader[1];
                result.Name = (String)reader[2];
                results.Add(result);
            }
            reader.Close();
            return results.FirstOrDefault();
        }
        public static async Task<ActionModel> InsertAction(this DbConnection connection, ActionModel ActionModel, DbTransaction transaction = null, int? timeout = null)
        {
            var sql = @"INSERT INTO `Actions` (`ActionId`, `Name`) VALUES (@ActionId, @Name);SELECT `a`.`Id`,`a`.`ActionId`,`a`.`Name` FROM `Actions` AS `a` WHERE `a`.`Id` = LAST_INSERT_ID() AND ROW_COUNT() = 1;";
            var command = connection.CreateCommand(sql, transaction, timeout);
            command.CreateParameter("@ActionId", ActionModel.ActionId);
            command.CreateParameter("@Name", ActionModel.Name);
            if (connection.State == ConnectionState.Closed)
                await connection.OpenAsync();
            var results = new List<ActionModel>();
            var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                var result = new ActionModel();
                result.Id = (Int64)reader[0];
                result.ActionId = (Guid)reader[1];
                result.Name = (String)reader[2];
                results.Add(result);
            }
            reader.Close();
            return results.FirstOrDefault();
        }
        public static async Task<ActionModel> UpdateAction(this DbConnection connection, ActionModel ActionModel, DbTransaction transaction = null, int? timeout = null)
        {
            var sql = @"UPDATE `Actions` AS `a` SET `a`.`ActionId` = @ActionId, `a`.`Name` = @Name WHERE `a`.`Id` = @Id;SELECT `a`.`Id`,`a`.`ActionId`,`a`.`Name` FROM `Actions` AS `a` WHERE `a`.`Id` = LAST_INSERT_ID() AND ROW_COUNT() = 1;";
            var command = connection.CreateCommand(sql, transaction, timeout);
            command.CreateParameter("@Id", ActionModel.Id);
            command.CreateParameter("@ActionId", ActionModel.ActionId);
            command.CreateParameter("@Name", ActionModel.Name);
            if (connection.State == ConnectionState.Closed)
                await connection.OpenAsync();
            var results = new List<ActionModel>();
            var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                var result = new ActionModel();
                result.Id = (Int64)reader[0];
                result.ActionId = (Guid)reader[1];
                result.Name = (String)reader[2];
                results.Add(result);
            }
            reader.Close();
            return results.FirstOrDefault();
        }
        public static async Task<int> DeleteAction(this DbConnection connection, ActionKey ActionKey, DbTransaction transaction = null, int? timeout = null)
        {
            var sql = $"DELETE FROM `Actions` AS `a` WHERE `a`.`Id` = @Id;";
            var command = connection.CreateCommand(sql, transaction, timeout);
            command.CreateParameter("@Id", ActionKey.Id);
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
