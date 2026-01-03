namespace QL_HethongDiennuoc.Services.ApiClients;

public interface IApiClient
{
    Task<T?> GetAsync<T>(string endpoint);
    Task<TResponse?> PostAsync<TResponse>(string endpoint, object data);
    Task<TResponse?> PutAsync<TResponse>(string endpoint, object data);
    Task<bool> DeleteAsync(string endpoint);
    Task<byte[]?> GetBytesAsync(string endpoint);
}
