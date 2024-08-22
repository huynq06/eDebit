using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace PluggableModulesInterface
{
    public static class PluggableManage
    {
        private static object _lock = new object();
        private static Dictionary<string, IPluggableModule> _ListOfModule = new Dictionary<string, IPluggableModule>();
        private static Dictionary<string, Stream> _ListOfStream = new Dictionary<string, Stream>();

        /// <summary>
        /// Lấy ra danh sách các IPlugable module (*.dll)
        /// </summary>
        /// <returns></returns>
        public static Dictionary<string, IPluggableModule> GetListOfModule()
        {
            return _ListOfModule;
        }

        /// <summary>
        /// Lấy ra các Key của các IPlugable module tương ứng đang được start
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<int> GetRunningKeysOfModule()
        {
            return _ListOfModule.Values.Where(p => p.Running).Select(p => p.Key);
        }

        public static void RemoveModuleInstance(string key)
        {
            if (_ListOfModule.ContainsKey(key))
            {
                ((IPluggableModule)_ListOfModule[key]).Stop();
                ((IPluggableModule)_ListOfModule[key]).Dispose();
                _ListOfModule.Remove(key);
            }
        }

        public static void RemoveStream(string key)
        {
            if (_ListOfStream.ContainsKey(key))
            {
                ((Stream)_ListOfStream[key]).Close();
                _ListOfStream.Remove(key);
            }
        }

        //public static bool UnloadAssembly(string filePath)
        //{
        //    try
        //    {
        //        string streamKey = Path.GetFileName(filePath);
        //        foreach (string key in _ListOfModule.Keys)
        //        {
        //            string[] s = key.Split('|');
        //            if (s.Length >= 2)
        //            {
        //                if (s[0] == streamKey)
        //                {
        //                    PluggableManage.RemoveModuleInstance(key);
        //                    PluggableManage.RemoveStream(streamKey);
        //                    return true;
        //                }
        //            }
        //        }
        //        return true;
        //    }
        //    catch (Exception)
        //    {
        //        return false;
        //    }
        //}

        public static bool UnloadAssemblyOfStoppedModule(string filePath)
        {
            try
            {
                string streamKey = Path.GetFileName(filePath);
                foreach (string key in _ListOfModule.Keys)
                {
                    string[] s = key.Split('|');
                    if (s.Length >= 2)
                    {
                        if (s[0] == streamKey)
                        {
                            if (!_ListOfModule[key].Running)
                            {
                                PluggableManage.RemoveModuleInstance(key);
                                PluggableManage.RemoveStream(streamKey);
                                return true;
                            }
                            else
                            {
                                return false;
                            }
                        }
                    }
                }
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static bool UnloadAssemblyOfAllStoppedModule(string folderPath)
        {
            bool rs = true;
            try
            {
                if (!string.IsNullOrWhiteSpace(folderPath))
                {
                    string[] filePaths = Directory.GetFiles(folderPath, "*.dll");
                    foreach (string filePath in filePaths)
                    {
                        rs = UnloadAssemblyOfStoppedModule(filePath);
                        if (!rs) return rs;
                    }
                    string[] subFolderPaths = Directory.GetDirectories(folderPath);
                    foreach (string subFolderPath in subFolderPaths)
                    {
                        UnloadAssemblyOfAllStoppedModule(subFolderPath);
                    }
                }
            }
            catch (Exception)
            {
                rs = false;
            }
            return rs;
        }

        public static void Start()
        {
            lock (_lock)
            {
                foreach (var item in _ListOfModule)
                {
                    if (item.Value != null)
                    {
                        item.Value.Stop();
                        item.Value.Start();
                    }
                }
            }
        }

        public static void Stop()
        {
            lock (_lock)
            {
                foreach (var item in _ListOfModule)
                {
                    if (item.Value != null)
                        item.Value.Stop();
                }
            }
        }

        public static void WakeUp()
        {
            lock (_lock)
            {
                foreach (var item in _ListOfModule)
                {
                    if (item.Value != null)
                        item.Value.WakeUp();
                }
            }
        }

        public static IPluggableModule CreateModuleInstance(Assembly assembly, out string typeFullName)
        {
            Type type = assembly.GetTypes().FirstOrDefault(p => p.GetInterface("PluggableModules.IPluggableModule", false) != null);
            if (type != null)
            {
                if (!_ListOfModule.ContainsKey(type.FullName))
                {
                    var moduleInstance = Activator.CreateInstance(type);
                    typeFullName = type.FullName;
                    return (IPluggableModule)moduleInstance;
                }
            }
            typeFullName = "";
            return null;
        }

        public static KeyValuePair<string, IPluggableModule> LoadAssembly(string filePath)
        {
            try
            {
                // Use stream to lock dll file, not allow other program modifies it
                Stream stream = new FileStream(filePath, FileMode.Open);
                string streamKey = Path.GetFileName(filePath);
                if (!_ListOfStream.ContainsKey(streamKey)) _ListOfStream.Add(streamKey, stream);
                // Load assembly from byte array
                FileInfo fileInfo = new FileInfo(filePath);
                byte[] arrbyte = new byte[fileInfo.Length];
                stream.Read(arrbyte, 0, arrbyte.Length);
                Assembly assembly = Assembly.Load(arrbyte);
                // Create instance
                string typeFullName = "";
                IPluggableModule moduleInstance = CreateModuleInstance(assembly, out typeFullName);
                if (moduleInstance != null && !_ListOfModule.ContainsKey(typeFullName))
                {
                    moduleInstance.AssemblyLocation = filePath;
                    string key = streamKey + "|" + typeFullName;
                    _ListOfModule.Add(key, moduleInstance);
                    return new KeyValuePair<string, IPluggableModule>(key, moduleInstance);
                }
                return new KeyValuePair<string, IPluggableModule>();
            }
            catch (Exception)
            {
                throw;
            }
        }

        public static void LoadExistingModules(string folderPath)
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(folderPath))
                {
                    string[] filePaths = Directory.GetFiles(folderPath, "*.dll");
                    foreach (string filePath in filePaths)
                    {
                        if (UnloadAssemblyOfStoppedModule(filePath))
                        {
                            LoadAssembly(filePath);
                        }
                    }
                    string[] subFolderPaths = Directory.GetDirectories(folderPath);
                    foreach (string subFolderPath in subFolderPaths)
                    {
                        LoadExistingModules(subFolderPath);
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
