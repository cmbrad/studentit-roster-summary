using System;
using System.Text;
using System.Threading.Tasks;

using Google.Apis.Json;
using Google.Apis.Util.Store;

namespace StudentIT.Roster.Summary
{
    public class EnvironmentDataStore : IDataStore
    {
        public string EnvironmentKey { get; }

        public EnvironmentDataStore(string environmentKey)
        {
            EnvironmentKey = environmentKey;
        }

        public string Base64Encode(string plainText)
        {
            if (plainText == null)
                return "";
            var plainTextBytes = Encoding.UTF8.GetBytes(plainText);
            return Convert.ToBase64String(plainTextBytes);
        }

        public string Base64Decode(string base64EncodedData)
        {
            var base64EncodedBytes = Convert.FromBase64String(base64EncodedData);
            return Encoding.UTF8.GetString(base64EncodedBytes);
        }

        public Task StoreAsync<T>(string key, T value)
        {
            if (string.IsNullOrEmpty(key))
            {
                throw new ArgumentException("Key MUST have a value");
            }

            var serialized = NewtonsoftJsonSerializer.Instance.Serialize(value);
            var encoded = Base64Encode(serialized);
            Environment.SetEnvironmentVariable(EnvironmentKey, encoded);

            return Task.Delay(0);
        }

        public Task DeleteAsync<T>(string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                throw new ArgumentException("Key MUST have a value");
            }

            Environment.SetEnvironmentVariable(EnvironmentKey, "");

            return Task.Delay(0);
        }

        public Task<T> GetAsync<T>(string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                throw new ArgumentException("Key MUST have a value");
            }

            TaskCompletionSource<T> tcs = new TaskCompletionSource<T>();
            var encoded = Environment.GetEnvironmentVariable(EnvironmentKey);

            if (encoded != null)
            {
                try
                {
                    var decoded = Base64Decode(encoded);
                    tcs.SetResult(NewtonsoftJsonSerializer.Instance.Deserialize<T>(decoded));
                }
                catch (Exception ex)
                {
                    tcs.SetException(ex);
                }
            }
            else
            {
                tcs.SetResult(default(T));
            }
            return tcs.Task;
        }

        public Task ClearAsync()
        {
            Environment.SetEnvironmentVariable(EnvironmentKey, "");

            return Task.Delay(0);
        }
    }
}