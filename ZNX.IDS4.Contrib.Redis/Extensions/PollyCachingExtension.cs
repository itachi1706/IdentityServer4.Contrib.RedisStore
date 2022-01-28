using System;
using System.Threading.Tasks;
using Polly;
using Polly.Wrap;
using StackExchange.Redis;

namespace IdentityServer4.Contrib.RedisStore.Extensions
{
    public static class PollyCachingExtension
    {

        public static async Task PollyStringSetAsync(this IDatabase cache, RedisKey key, RedisValue value, TimeSpan? expiry = null, When when= When.Always, CommandFlags flags = CommandFlags.None)
        {
            await Policy.ExecuteAsync(() => cache.StringSetAsync(key, value, expiry, when, flags));
        }

        public static async Task<RedisValue> PollyStringGetAsync(this IDatabase cache, RedisKey cacheKey, CommandFlags flags = CommandFlags.None)
        {
            return await Policy.ExecuteAsync(() => cache.StringGetAsync(cacheKey, flags));
        }

        public static async Task<RedisValue[]> PollyStringGetAsync(this IDatabase cache, RedisKey[] keys, CommandFlags flags = CommandFlags.None)
        {
            return await Policy.ExecuteAsync(() => cache.StringGetAsync(keys, flags));
        }

        public static async Task<TimeSpan?> PollyKeyTimeToLiveAsync(this IDatabase cache, RedisKey key, CommandFlags flags = CommandFlags.None)
        {
            return await Policy.ExecuteAsync(() => cache.KeyTimeToLiveAsync(key, flags));
        }

        public static async Task<RedisValue[]> PollySetMembersAsync(this IDatabase cache, RedisKey key, CommandFlags flags = CommandFlags.None)
        {
            return await Policy.ExecuteAsync(() => cache.SetMembersAsync(key, flags));
        }

        public static async Task<bool> PollyExecuteAsync(this ITransaction transaction, CommandFlags flags = CommandFlags.None)
        {
            return await Policy.ExecuteAsync(() => transaction.ExecuteAsync(flags));
        }

        public static async Task<bool> PollyKeyDeleteAsync(this IDatabase cache, RedisKey key, CommandFlags flags = CommandFlags.None)
        {
            return await Policy.ExecuteAsync(() => cache.KeyDeleteAsync(key, flags));
        }

        public static AsyncPolicyWrap Policy { get; } =
            Polly.Policy.BulkheadAsync(70, 210)
            .WrapAsync(Polly.Policy.Handle<RedisTimeoutException>()
            .WaitAndRetryAsync(new[] {
                TimeSpan.FromSeconds(1),
                TimeSpan.FromSeconds(2),
                TimeSpan.FromSeconds(4),
                TimeSpan.FromSeconds(8),
                TimeSpan.FromSeconds(15),
                TimeSpan.FromSeconds(30)
            }
            )
        );
    }
}

