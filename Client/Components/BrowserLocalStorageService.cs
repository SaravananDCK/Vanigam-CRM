using Microsoft.JSInterop;
using Vanigam.CRM.Objects.Redis;

namespace Vanigam.CRM.Client.Components
{
    public interface ILocalStorageService
    {
        Task SetItemAsync(string key, string value);
        Task<string> GetItemAsync(string key);
        Task RemoveItemAsync(string key);
    }

    public class BrowserLocalStorageService(IJSRuntime jsRuntime) : ILocalStorageService
    {
        public async Task SetItemAsync(string key, string value)
        {
            await jsRuntime.InvokeVoidAsync("localStorage.setItem", key, value);
        }

        public async Task<string> GetItemAsync(string key)
        {
            return await jsRuntime.InvokeAsync<string>("localStorage.getItem", key);
        }

        public async Task RemoveItemAsync(string key)
        {
            await jsRuntime.InvokeVoidAsync("localStorage.removeItem", key);
        }
    }
    public class RedisLocalStorageService(RedisService redisService) : ILocalStorageService
    {
        public async Task SetItemAsync(string key, string value)
        {
            await redisService.SetItemAsync(key, value);
        }

        public async Task<string> GetItemAsync(string key)
        {
            return await redisService.GetItemAsync(key);
        }

        public async Task RemoveItemAsync(string key)
        {
            await redisService.RemoveItemAsync(key);
        }
    }
}

