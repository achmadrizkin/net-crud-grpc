using System.Text.Json;
using net_test_generator_svc.Models;
using StackExchange.Redis;
using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;

namespace net_test_generator_svc.Repositories.Cache
{
	public interface ISiswaCache	{
	    Task<bool> SetCache(Siswa o);
	    Task<bool> SetCaches(List<Siswa> o);
	    Task<Siswa> GetCache(string id);
	    Task<List<Siswa>> GetCaches();
	    Task<bool> Clear();
		Task<bool> ClearSiswa(int SiswaId);
	}

public class SiswaCache : ISiswaCache
{
	private readonly IDistributedCache _cache;
	public SiswaCache(IDistributedCache cache)
	{
	    _cache = cache ?? throw new ArgumentNullException(nameof(cache));
	}

	public async Task<bool> Clear()
	{
	    await _cache.RemoveAsync("l-Siswa-cache");
	    await _cache.RemoveAsync("Siswa.*");
	    return true;
	}

	public async Task<bool> ClearSiswa(int SiswaId)
	{
		await _cache.RemoveAsync($"Siswa.{SiswaId}");
	    return true;
	}

	public async Task<Siswa> GetCache(string id)
	{
	    try
	    {
	        var o = await _cache.GetStringAsync($"Siswa.{id}");
	        if (o != null)
	        {
	            return JsonConvert.DeserializeObject<Siswa>(o);
	        }
	        return null;
	    }
	    catch
	    {
	        throw;
	    }
	}

	public async Task<List<Siswa>> GetCaches()
	{
	    try
	    {
	        var o = await _cache.GetStringAsync($"l-Siswa-cache");
	        if (o != null)
	        {
	            var lCurr = JsonConvert.DeserializeObject<List<Siswa>>(o);
	            return lCurr;
	        }

	        return null;
	    }
	    catch
	    {
	        throw;
	    }
	}

	public async Task<bool> SetCache(Siswa o)
	{
	    await _cache.SetStringAsync($"Siswa.{o.Id}", JsonConvert.SerializeObject(o), new DistributedCacheEntryOptions
	    {
	        AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(24)
	    });
	    return true;
	}

	public async Task<bool> SetCaches(List<Siswa> o)
	{
	    await _cache.SetStringAsync($"l-Siswa-cache", JsonConvert.SerializeObject(o), new DistributedCacheEntryOptions
	    {
	        AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(24)
	    });
	    return true;
	}
}
}
