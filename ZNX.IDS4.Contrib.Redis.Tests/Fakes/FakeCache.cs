﻿using Duende.IdentityServer.Services;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Duende.IdentityServer.Contrib.RedisStore.Tests.Cache
{
    public class FakeCache<T> : ICache<T> where T : class
    {
        private readonly IMemoryCache cache;

        private readonly ILogger<FakeCache<T>> logger;

        public FakeCache(IMemoryCache memoryCache, FakeLogger<FakeCache<T>> logger)
        {
            cache = memoryCache;
            this.logger = logger;
        }

        public Task<T> GetAsync(string key)
        {
            var result = cache.Get(key);

            if (result == null)
                logger.LogDebug($"Cache miss for {key}");
            else
                logger.LogDebug($"Cache hit for {key}");

            return Task.FromResult((T)result);
        }

        public Task<T> GetOrAddAsync(string key, TimeSpan duration, Func<Task<T>> get)
        {
            var keyResult = GetAsync(key);
            if (keyResult != null)
                return keyResult;

            var va = get.Invoke();
            var t = SetAsync(key, va.Result, duration);
                
            return Task.FromResult(t as T);
        }

        public Task SetAsync(string key, T item, TimeSpan expiration)
        {
            cache.Set(key, item, expiration);
            return Task.CompletedTask;
        }

        public Task RemoveAsync(string key)
        {
            cache.Remove(key);
            return Task.CompletedTask;
        }
    }
}
