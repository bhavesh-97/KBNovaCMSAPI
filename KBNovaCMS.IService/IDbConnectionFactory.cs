using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KBNovaCMS.IService
{
    public interface IDbConnectionFactory : IDisposable
    {
        bool ValidateConnection();
        Task<bool> ValidateConnectionAsync();

        T? ExecuteScalar<T>(string query, CommandType commandType, Dictionary<string, object>? parameters = null);
        Task<T?> ExecuteScalarAsync<T>(string query, CommandType commandType, Dictionary<string, object>? parameters = null);

        void ExecuteWithoutResult(string query, CommandType commandType, out string errorMessage, Dictionary<string, object>? parameters = null, IDbTransaction? transaction = null);
        Task ExecuteWithoutResultAsync(string query, CommandType commandType, Dictionary<string, object>? parameters = null, IDbTransaction? transaction = null);

        IEnumerable<T> ExecuteWithResult<T>(string query, CommandType commandType, out string errorMessage, Dictionary<string, object>? parameters = null, IDbTransaction? transaction = null);
        Task<IEnumerable<T>> ExecuteWithResultAsync<T>(string query, CommandType commandType, Dictionary<string, object>? parameters = null, IDbTransaction? transaction = null);

        List<T> ExecuteWithResultList<T>(string query, CommandType commandType, out string errorMessage, Dictionary<string, object>? parameters = null, IDbTransaction? transaction = null);
        Task<List<T>> ExecuteWithResultListAsync<T>(string query, CommandType commandType, Dictionary<string, object>? parameters = null, IDbTransaction? transaction = null);

        DataTable GetDataTable(string query, CommandType commandType, out string errorMessage, Dictionary<string, object>? parameters = null, IDbTransaction? transaction = null);
        T GetSingle<T>(string query, CommandType commandType, Dictionary<string, object>? parameters = null);
        Task<T> GetSingleAsync<T>(string query, CommandType commandType, Dictionary<string, object>? parameters = null);

        T Get_T_TypeData<T>(string query, CommandType commandType, Dictionary<string, object>? parameters = null, IDbTransaction? transaction = null);
        T GetJSONData<T>(string query, CommandType commandType, Dictionary<string, object>? parameters = null, IDbTransaction? transaction = null);
        Task<T> GetJSONDataAsync<T>(string query, CommandType commandType, Dictionary<string, object>? parameters = null, IDbTransaction? transaction = null);
    }
}
