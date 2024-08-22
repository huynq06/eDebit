using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StorageManagement
{
    public class DictionaryStorage<TKey, T>
    {
        #region singleton pattern

        private static object _lock = new object();
        private static DictionaryStorage<TKey, T> _instance;
        public static DictionaryStorage<TKey, T> Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_lock)
                    {
                        _instance = new DictionaryStorage<TKey, T>();
                    }
                }
                return _instance;
            }
        }

        #endregion singleton pattern

        public ConcurrentDictionary<TKey, T> _dictData = new ConcurrentDictionary<TKey, T>();

        // Since this class apply singleton pattern, make the default constructor private to prevent 
        // instances from being created with "new Storage()"
        public DictionaryStorage()
        {
            _dictData = new ConcurrentDictionary<TKey, T>();
        }
        public void SetValueKey(TKey keyData,T data)
        {
            _dictData[keyData] = data;
        }
        public T GetValue(TKey keyData)
        {
            return _dictData[keyData];
        }


        /// <summary>
        /// Tập các key của Storage
        /// </summary>
        public ICollection<TKey> GetKeys()
        {
            return _dictData.Keys;
        }

        /// <summary>
        /// Đẩy dữ liệu vào Storage theo key dữ liệu
        /// </summary>
        public void EnqueueElementData(TKey keyData, T data)
        {
            if (!_dictData.ContainsKey(keyData))
                _dictData.TryAdd(keyData, data);
        }

        /// <summary>
        /// Lấy dữ liệu từ Storage và xóa dữ liệu đã được lấy ra khỏi Storage
        /// </summary>
        public T DequeueElementData(TKey keyData)
        {
            if (!_dictData.ContainsKey(keyData))
                return default(T);
            T retValue = default(T);
            _dictData.TryRemove(keyData, out retValue);
            return retValue;
        }

        public int Count()
        {
            return _dictData.Count;
        }

        public KeyValuePair<TKey, T> Find(Func<KeyValuePair<TKey, T>, bool> predicate)
        {
            return _dictData.FirstOrDefault(predicate);
        }

        public bool Any(Func<KeyValuePair<TKey, T>, bool> predicate)
        {
            return _dictData.Any(predicate);
        }

        public List<T> DequeueElementData(Func<KeyValuePair<TKey, T>, bool> predicate)
        {
            List<T> retList = new List<T>();
            var keys = _dictData.Where(predicate).Select(p => p.Key).ToList();
            for (int i = 0; i < keys.Count; i++)
            {
                T retValue = default(T);
                _dictData.TryRemove(keys[i], out retValue);
                if (!retValue.Equals(default(T)))
                    retList.Add(retValue);

            }
            return retList;
        }

        public T Dequeue(Func<KeyValuePair<TKey, T>, bool> predicate)
        {
            var keypair = Find(predicate);
            if (keypair.Equals(default(KeyValuePair<TKey, T>)))
                return default(T);
            T value = default(T);
            _dictData.TryRemove(keypair.Key, out value);
            return value;
        }

        public bool Remove(TKey key)
        {
            T value = default(T);
            return _dictData.TryRemove(key, out value);
        }

        public bool Remove(Func<KeyValuePair<TKey, T>, bool> predicate)
        {
            var keypair = Find(predicate);
            if (keypair.Equals(default(KeyValuePair<TKey, T>)))
                return false;
            return Remove(keypair.Key);
        }

        public void WriteListToFile(string file)
        {
            try
            {
                if (_dictData != null && _dictData.Count > 0)
                {
                    string jsonData = Newtonsoft.Json.JsonConvert.SerializeObject(_dictData);
                    using (StreamWriter write = new StreamWriter(file, false))
                    {
                        write.Write(jsonData);
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
       
        public void LoadFromFile(string file, bool delete)
        {
            try
            {
                if (File.Exists(file))
                {
                    string value = string.Empty;
                    using (StreamReader reader = new StreamReader(file))
                    {
                        value = reader.ReadToEnd();
                    }
                    if (!string.IsNullOrEmpty(value))
                    {
                        var data = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<TKey, T>>(value);
                        if (data != null && data.Count > 0)
                        {
                            foreach (var item in data)
                            {
                                _dictData.TryAdd(item.Key, item.Value);
                            }
                            data.Clear();
                            data = null;
                        }
                    }
                    if (delete)
                    {
                        File.Delete(file);
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public bool IsEmpty()
        {
            return _dictData.Count == 0;
        }
    }
}
