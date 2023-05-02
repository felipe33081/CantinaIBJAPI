using CantinaIBJ.Framework.Requests;

namespace CantinaIBJ.Framework.Interfaces
{
    /// <summary>
    /// Inteface básica de um recurso de API
    /// </summary>
    public interface IApiResources : IDisposable
    {
        string BaseURI { get; set; }

        #region GET
        Task<T> GetAsync<T>();
        Task<T> GetAsync<T>(string id);
        Task<T> GetAsync<T>(string id, List<KeyValueType> apiUserToken);
        Task<T> GetAsync<T>(string id, string partOfUrl, List<KeyValueType> apiUserToken);
        #endregion

        #region POST
        Task<T> PostAsync<T>(object data);
        Task<T> PostAsync<T>(object data, string partOfUrl);
        Task<T> PostAsync<T>(object data, string partOfUrl, string query, List<KeyValueType> apiUserToken, List<KeyValueType> form);
        #endregion

        #region PUT
        Task<T> PutAsync<T>(string id, object data);
        #endregion

        #region DELETE
        Task<T> DeleteAsync<T>(string id);
        #endregion

        Task<T> ProcessResponse<T>(HttpResponseMessage response);
    }
}
