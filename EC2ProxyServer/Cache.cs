using System;
using System.Collections.Concurrent;

class Cache
{
    private static readonly ConcurrentDictionary<string, (byte[], DateTime)> cacheStore = new ConcurrentDictionary<string, (byte[], DateTime)>();
    private static readonly TimeSpan cacheDuration = TimeSpan.FromMinutes(5);
    public static void AddToCache(string key, byte[] data)
    {
        cacheStore[key] = (data, DateTime.Now);
    }

    public static byte[]? GetFromCache(string key)
    {
        if (cacheStore.TryGetValue(key, out var cachedData))
        {
            if (DateTime.Now - cachedData.Item2 < cacheDuration)
            {
                return cachedData.Item1;
            }
            else
            {
                cacheStore.TryRemove(key, out _); // Remove expired cache
            }
        }
        return null;
    }
}