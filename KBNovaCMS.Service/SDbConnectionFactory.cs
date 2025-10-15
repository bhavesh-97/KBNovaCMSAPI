using KBNovaCMS.IService;
using Npgsql;
using System.Data;
using System.Reflection;
using KBNovaCMS.Model;
using System.Data.Common;
using Microsoft.Extensions.Configuration;
using System.Text.Json;

namespace KBNovaCMS.Service
{
    public class SDbConnectionFactory : IDbConnectionFactory
    {

        private readonly MDbConnection _dbConfig;
        private readonly INLog _logger;
        private bool _disposed;

        public SDbConnectionFactory(INLog logger)
        {
            _dbConfig = new MDbConnection();
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        ~SDbConnectionFactory()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    // Dispose managed resources if any
                }
                _disposed = true;
            }
        }

        private NpgsqlConnection GetDBConnection()
        {
            return new NpgsqlConnection(MDbConnection.ConnectionString);
        }

        #region Validate Connection
        public bool ValidateConnection()
        {
            try
            {
                using var connection = GetDBConnection();
                connection.Open();
                return true;
            }
            catch (Exception ex)
            {
                _logger.Log(Common.Enums.NLogType.Error, "Error in ValidateConnection()", ex, Common.Enums.NLogErrorFileName.ErrorLog);
                return false;
            }
        }

        public async Task<bool> ValidateConnectionAsync()
        {
            try
            {
                using var connection = GetDBConnection();
                await connection.OpenAsync().ConfigureAwait(false);
                return true;
            }
            catch (Exception ex)
            {
                await _logger.LogAsync(Common.Enums.NLogType.Error, "Error in ValidateConnectionAsync()", ex, Common.Enums.NLogErrorFileName.ErrorLog);
                return false;
            }
        }
        #endregion

        #region Execute Scalar
        public T? ExecuteScalar<T>(string query, CommandType commandType, Dictionary<string, object>? parameters = null)
        {
            try
            {
                using var connection = GetDBConnection();
                using var command = CreateCommand(connection, query, commandType, parameters);
                connection.Open();
                var result = command.ExecuteScalar();
                return result == DBNull.Value ? default : (T)Convert.ChangeType(result, typeof(T));
            }
            catch (Exception ex)
            {
                _logger.Log(Common.Enums.NLogType.Error, $"Error in ExecuteScalar() for {query}", ex, Common.Enums.NLogErrorFileName.ErrorLog);
                throw;
            }
        }

        public async Task<T?> ExecuteScalarAsync<T>(string query, CommandType commandType, Dictionary<string, object>? parameters = null)
        {
            try
            {
                using var connection = GetDBConnection();
                using var command = CreateCommand(connection, query, commandType, parameters);
                await connection.OpenAsync().ConfigureAwait(false);
                var result = await command.ExecuteScalarAsync().ConfigureAwait(false);
                return result == DBNull.Value ? default : (T)Convert.ChangeType(result, typeof(T));
            }
            catch (Exception ex)
            {
                await _logger.LogAsync(Common.Enums.NLogType.Error, $"Error in ExecuteScalarAsync() for {query}", ex, Common.Enums.NLogErrorFileName.ErrorLog);
                throw;
            }
        }
        #endregion

        #region Execute Without Result
        public void ExecuteWithoutResult(string query, CommandType commandType, out string errorMessage, Dictionary<string, object>? parameters = null, IDbTransaction? transaction = null)
        {
            errorMessage = string.Empty;
            try
            {
                using var connection = GetDBConnection();
                using var command = CreateCommand(connection, query, commandType, parameters, transaction);
                if (connection.State != ConnectionState.Open)
                    connection.Open();

                if (transaction == null)
                {
                    using var localTransaction = connection.BeginTransaction();
                    try
                    {
                        command.ExecuteNonQuery();
                        localTransaction.Commit();
                    }
                    catch
                    {
                        localTransaction.Rollback();
                        throw;
                    }
                }
                else
                {
                    command.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
                _logger.Log(Common.Enums.NLogType.Error, $"Error in ExecuteWithoutResult() for {query}", ex, Common.Enums.NLogErrorFileName.ErrorLog);
                throw;
            }
        }

        public async Task ExecuteWithoutResultAsync(string query, CommandType commandType, Dictionary<string, object>? parameters = null, IDbTransaction? transaction = null)
        {
            try
            {
                using var connection = GetDBConnection();
                using var command = CreateCommand(connection, query, commandType, parameters, transaction);
                if (connection.State != ConnectionState.Open)
                    await connection.OpenAsync().ConfigureAwait(false);

                if (transaction == null)
                {
                    using var localTransaction = connection.BeginTransaction();
                    try
                    {
                        await command.ExecuteNonQueryAsync().ConfigureAwait(false);
                        await localTransaction.CommitAsync().ConfigureAwait(false);
                    }
                    catch
                    {
                        await localTransaction.RollbackAsync().ConfigureAwait(false);
                        throw;
                    }
                }
                else
                {
                    await command.ExecuteNonQueryAsync().ConfigureAwait(false);
                }
            }
            catch (Exception ex)
            {
                await _logger.LogAsync(Common.Enums.NLogType.Error, $"Error in ExecuteWithoutResultAsync() for {query}", ex, Common.Enums.NLogErrorFileName.ErrorLog);
                throw;
            }
        }
        #endregion

        #region Execute With Result
        public IEnumerable<T> ExecuteWithResult<T>(string query, CommandType commandType, out string errorMessage, Dictionary<string, object>? parameters = null, IDbTransaction? transaction = null)
        {
            errorMessage = string.Empty;
            var results = new List<T>();
            try
            {
                using var connection = GetDBConnection();
                using var command = CreateCommand(connection, query, commandType, parameters, transaction);
                if (connection.State != ConnectionState.Open)
                    connection.Open();

                using var reader = command.ExecuteReader();
                results.AddRange(MapReaderToEntities<T>(reader));

                if (transaction == null)
                {
                    using var localTransaction = connection.BeginTransaction();
                    localTransaction.Commit();
                }

                return results;
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
                _logger.Log(Common.Enums.NLogType.Error, $"Error in ExecuteWithResult() for {query}", ex, Common.Enums.NLogErrorFileName.ErrorLog);
                throw;
            }
        }

        public async Task<IEnumerable<T>> ExecuteWithResultAsync<T>(string query, CommandType commandType, Dictionary<string, object>? parameters = null, IDbTransaction? transaction = null)
        {
            var results = new List<T>();
            try
            {
                using var connection = GetDBConnection();
                using var command = CreateCommand(connection, query, commandType, parameters, transaction);
                if (connection.State != ConnectionState.Open)
                    await connection.OpenAsync().ConfigureAwait(false);

                using var reader = await command.ExecuteReaderAsync().ConfigureAwait(false);
                results.AddRange(await MapReaderToEntitiesAsync<T>(reader));

                if (transaction == null)
                {
                    using var localTransaction = connection.BeginTransaction();
                    await localTransaction.CommitAsync().ConfigureAwait(false);
                }

                return results;
            }
            catch (Exception ex)
            {
                await _logger.LogAsync(Common.Enums.NLogType.Error, $"Error in ExecuteWithResultAsync() for {query}", ex, Common.Enums.NLogErrorFileName.ErrorLog);
                throw;
            }
        }
        #endregion

        #region Execute With Result List
        public List<T> ExecuteWithResultList<T>(string query, CommandType commandType, out string errorMessage, Dictionary<string, object>? parameters = null, IDbTransaction? transaction = null)
        {
            errorMessage = string.Empty;
            var results = new List<T>();
            try
            {
                using var connection = GetDBConnection();
                using var command = CreateCommand(connection, query, commandType, parameters, transaction);
                if (connection.State != ConnectionState.Open)
                    connection.Open();

                using var reader = command.ExecuteReader();
                results.AddRange(MapReaderToEntities<T>(reader));

                if (transaction == null)
                {
                    using var localTransaction = connection.BeginTransaction();
                    localTransaction.Commit();
                }

                return results;
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
                _logger.Log(Common.Enums.NLogType.Error, $"Error in ExecuteWithResultList() for {query}", ex, Common.Enums.NLogErrorFileName.ErrorLog);
                throw;
            }
        }

        public async Task<List<T>> ExecuteWithResultListAsync<T>(string query, CommandType commandType, Dictionary<string, object>? parameters = null, IDbTransaction? transaction = null)
        {
            var results = new List<T>();
            try
            {
                using var connection = GetDBConnection();
                using var command = CreateCommand(connection, query, commandType, parameters, transaction);
                if (connection.State != ConnectionState.Open)
                    await connection.OpenAsync().ConfigureAwait(false);

                using var reader = await command.ExecuteReaderAsync().ConfigureAwait(false);
                results.AddRange(await MapReaderToEntitiesAsync<T>(reader));

                if (transaction == null)
                {
                    using var localTransaction = connection.BeginTransaction();
                    await localTransaction.CommitAsync().ConfigureAwait(false);
                }

                return results;
            }
            catch (Exception ex)
            {
                await _logger.LogAsync(Common.Enums.NLogType.Error, $"Error in ExecuteWithResultListAsync() for {query}", ex, Common.Enums.NLogErrorFileName.ErrorLog);
                throw;
            }
        }
        #endregion

        #region Get DataTable
        public DataTable GetDataTable(string query, CommandType commandType, out string errorMessage, Dictionary<string, object>? parameters = null, IDbTransaction? transaction = null)
        {
            errorMessage = string.Empty;
            var dataTable = new DataTable();
            try
            {
                using var connection = GetDBConnection();
                using var command = CreateCommand(connection, query, commandType, parameters, transaction);
                if (connection.State != ConnectionState.Open)
                    connection.Open();

                using var reader = command.ExecuteReader();
                dataTable.Load(reader);

                return dataTable;
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
                _logger.Log(Common.Enums.NLogType.Error, $"Error in GetDataTable() for {query}", ex, Common.Enums.NLogErrorFileName.ErrorLog);
                throw;
            }
        }
        #endregion

        #region Get Single
        public T GetSingle<T>(string query, CommandType commandType, Dictionary<string, object>? parameters = null)
        {
            try
            {
                using var connection = GetDBConnection();
                using var command = CreateCommand(connection, query, commandType, parameters);
                connection.Open();

                using var reader = command.ExecuteReader();
                if (reader.Read())
                {
                    return MapReaderToEntity<T>(reader);
                }
                throw new InvalidOperationException("No results returned from query.");
            }
            catch (Exception ex)
            {
                _logger.Log(Common.Enums.NLogType.Error, $"Error in GetSingle() for {query}", ex, Common.Enums.NLogErrorFileName.ErrorLog);
                throw;
            }
        }

        public async Task<T> GetSingleAsync<T>(string query, CommandType commandType, Dictionary<string, object>? parameters = null)
        {
            try
            {
                using var connection = GetDBConnection();
                using var command = CreateCommand(connection, query, commandType, parameters);
                await connection.OpenAsync().ConfigureAwait(false);

                using var reader = await command.ExecuteReaderAsync().ConfigureAwait(false);
                if (await reader.ReadAsync().ConfigureAwait(false))
                {
                    return MapReaderToEntity<T>(reader);
                }
                throw new InvalidOperationException("No results returned from query.");
            }
            catch (Exception ex)
            {
                await _logger.LogAsync(Common.Enums.NLogType.Error, $"Error in GetSingleAsync() for {query}", ex, Common.Enums.NLogErrorFileName.ErrorLog);
                throw;
            }
        }
        #endregion

        #region Get JSON and Type Data
        public T Get_T_TypeData<T>(string query, CommandType commandType, Dictionary<string, object>? parameters = null, IDbTransaction? transaction = null)
        {
            try
            {
                using var connection = GetDBConnection();
                using var command = CreateCommand(connection, query, commandType, parameters, transaction);
                if (connection.State != ConnectionState.Open)
                    connection.Open();

                using var reader = command.ExecuteReader();
                if (reader.Read())
                {
                    return MapReaderToEntity<T>(reader);
                }
                throw new InvalidOperationException("No results returned from query.");
            }
            catch (Exception ex)
            {
                _logger.Log(Common.Enums.NLogType.Error, $"Error in Get_T_TypeData() for {query}", ex, Common.Enums.NLogErrorFileName.ErrorLog);
                throw;
            }
        }

        public T GetJSONData<T>(string query, CommandType commandType, Dictionary<string, object>? parameters = null, IDbTransaction? transaction = null)
        {
            try
            {
                using var connection = GetDBConnection();
                using var command = CreateCommand(connection, query, commandType, parameters, transaction);
                if (connection.State != ConnectionState.Open)
                    connection.Open();

                using var reader = command.ExecuteReader();
                if (reader.Read())
                {
                    var jsonString = reader.GetString(0); // Assume JSON is in the first column
                    return JsonSerializer.Deserialize<T>(jsonString) ?? throw new InvalidOperationException("Failed to deserialize JSON data.");
                }
                throw new InvalidOperationException("No results returned from query.");
            }
            catch (Exception ex)
            {
                _logger.Log(Common.Enums.NLogType.Error, $"Error in GetJSONData() for {query}", ex, Common.Enums.NLogErrorFileName.ErrorLog);
                throw;
            }
        }

        public async Task<T> GetJSONDataAsync<T>(string query, CommandType commandType, Dictionary<string, object>? parameters = null, IDbTransaction? transaction = null)
        {
            try
            {
                using var connection = GetDBConnection();
                using var command = CreateCommand(connection, query, commandType, parameters, transaction);
                if (connection.State != ConnectionState.Open)
                    await connection.OpenAsync().ConfigureAwait(false);

                using var reader = await command.ExecuteReaderAsync().ConfigureAwait(false);
                if (await reader.ReadAsync().ConfigureAwait(false))
                {
                    var jsonString = reader.GetString(0); // Assume JSON is in the first column
                    return JsonSerializer.Deserialize<T>(jsonString) ?? throw new InvalidOperationException("Failed to deserialize JSON data.");
                }
                throw new InvalidOperationException("No results returned from query.");
            }
            catch (Exception ex)
            {
                await _logger.LogAsync(Common.Enums.NLogType.Error, $"Error in GetJSONDataAsync() for {query}", ex, Common.Enums.NLogErrorFileName.ErrorLog);
                throw;
            }
        }
        #endregion

        #region Helper Methods
        private NpgsqlCommand CreateCommand(NpgsqlConnection connection, string query, CommandType commandType, Dictionary<string, object>? parameters = null, IDbTransaction? transaction = null)
        {
            var command = new NpgsqlCommand(query, connection)
            {
                CommandType = commandType,
                CommandTimeout = 30
            };

            if (transaction != null)
            {
                command.Transaction = transaction as NpgsqlTransaction;
            }

            if (parameters != null)
            {
                foreach (var param in parameters)
                {
                    command.Parameters.AddWithValue(param.Key, param.Value ?? DBNull.Value);
                }
            }

            return command;
        }

        private T MapReaderToEntity<T>(IDataReader reader)
        {
            var entity = Activator.CreateInstance<T>();
            var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => p.CanWrite)
                .ToDictionary(p => p.Name, StringComparer.OrdinalIgnoreCase);

            for (int i = 0; i < reader.FieldCount; i++)
            {
                var columnName = reader.GetName(i);
                if (properties.TryGetValue(columnName, out var property) && !reader.IsDBNull(i))
                {
                    var value = reader.GetValue(i);
                    property.SetValue(entity, Convert.ChangeType(value, property.PropertyType));
                }
            }

            return entity;
        }

        private IEnumerable<T> MapReaderToEntities<T>(IDataReader reader)
        {
            var results = new List<T>();
            while (reader.Read())
            {
                results.Add(MapReaderToEntity<T>(reader));
            }
            return results;
        }

        private async Task<IEnumerable<T>> MapReaderToEntitiesAsync<T>(NpgsqlDataReader reader)
        {
            var results = new List<T>();
            while (await reader.ReadAsync().ConfigureAwait(false))
            {
                results.Add(MapReaderToEntity<T>(reader));
            }
            return results;
        }
        #endregion
    }
}
