using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cache_Server
{
    class CacheOperations
    {
        private static Dictionary<string, object> cache = new Dictionary<string, object>();

        public void Initialize()
        {
/*
            if (cache == null)
            {
                cache = new Dictionary<string, object>();

            }*/
            
        }

        public void Add(string key, object value)
        {
            lock (cache)
            {
                if (!cache.TryGetValue(key, out _))
                {
                    cache.Add(key, value);
                }
            }
        }

        public void Remove(string key)
        {
            lock (cache)
            {
                if (cache.TryGetValue(key, out _))
                {
                    cache.Remove(key);
                }
            }
        }
        public object Get(string key)
        {
            lock (cache)
            {
                if (cache.TryGetValue(key, out _))
                {
                    return cache[key];
                }
                return "Not Found";
            }
        }

        public void Clear()
        {
            lock (cache)
            {
                cache.Clear();
            }
        }

        public void Dispose()
        {
            lock (cache)
            {
                cache.Clear();
            }
        }

        public bool isKeyExists(string key)
        {
            lock (cache)
            {
                if (cache.ContainsKey(key))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }
    }
}