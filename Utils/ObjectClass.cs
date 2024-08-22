using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.Common;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.Reflection;

namespace Utils
{
    public sealed class ObjectClass
    {
        // Since this class provides only static methods, make the default constructor private to prevent 
        // instances from being created with "new ObjectClass()"
        private ObjectClass() { }

        /// <summary>
        /// Gán giá trị của 1 SqlDataReader cho 1 class tương ứng
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="record"></param>
        /// <exception cref="Các trường trong record không tương ứng với các Property trong class hoặc record đã bị đóng"></exception>
        public static T CreateInstance<T>(DbDataReader reader) where T : class, new()
        {
            if (reader == null || !reader.HasRows) return null;
            if (reader.Read())
            {
                T retValue = new T();
                for (int i = 0; i < reader.FieldCount; i++)
                {
                    string fieldName = reader.GetName(i);
                    typeof(T).GetProperty(fieldName).SetValue(retValue, (reader[fieldName].Equals(DBNull.Value) ? null : reader[fieldName]), null);
                }
                reader.Dispose();
                return retValue;
            }
            reader.Dispose();
            return null;
        }

        /// <summary>
        /// Tạo 1 List từ 1 Reader
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="reader"></param>
        /// <returns></returns>
        public static List<T> CreateList<T>(DbDataReader reader) where T : class, new()
        {
            List<T> retValue = new List<T>();
            var propeties = GetProperties<T>();
            while (reader.Read())
            {
                T instance = new T();
                foreach (var propety in propeties)
                {
                    propety.SetValue(instance, (reader[propety.Name].Equals(DBNull.Value) ? null : reader[propety.Name]), null);
                }
                retValue.Add(instance);
            }
            reader.Dispose();
            return retValue;
        }

        /// <summary>
        /// Tạo DataTable từ 1 IProducerConsumerCollection
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <returns></returns>
        public static DataTable CreateTable<T>(IProducerConsumerCollection<T> source)
        {
            try
            {
                DataTable dtRet = new DataTable();
                var propertiesInfo = GetProperties<T>();
                foreach (var info in propertiesInfo)
                {
                    var type = info.PropertyType;
                    var underlyingType = Nullable.GetUnderlyingType(type);
                    dtRet.Columns.Add(info.Name, underlyingType ?? type);
                }
                while (source.Count > 0)
                {
                    T enqueueData;
                    if (source.TryTake(out enqueueData))
                    {
                        DataRow newRow = dtRet.NewRow();
                        foreach (var info in propertiesInfo)
                        {
                            newRow[info.Name] = info.GetValue(enqueueData, null) ?? DBNull.Value;
                        }
                        dtRet.Rows.Add(newRow);
                    }
                }
                return dtRet;
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        /// <summary>
        /// Tạo DataTable từ 1 IProducerConsumerCollection
        /// </summary>
        /// <param name="source">IProducerConsumerCollection to create table</param>
        /// <param name="batchSize">max length size of out put table</param>
        public static DataTable CreateTable<T>(IProducerConsumerCollection<T> source, int batchSize)
        {
            try
            {
                DataTable dtRet = new DataTable();
                var propertiesInfo = GetProperties<T>();
                foreach (var info in propertiesInfo)
                {
                    var type = info.PropertyType;
                    var underlyingType = Nullable.GetUnderlyingType(type);
                    dtRet.Columns.Add(info.Name, underlyingType ?? type);
                }
                while (source.Count > 0 && (batchSize--) > 0)
                {
                    T enqueueData;
                    if (source.TryTake(out enqueueData))
                    {
                        DataRow newRow = dtRet.NewRow();
                        foreach (var info in propertiesInfo)
                        {
                            newRow[info.Name] = info.GetValue(enqueueData, null) ?? DBNull.Value;
                        }
                        dtRet.Rows.Add(newRow);
                    }
                }
                return dtRet;
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        /// <summary>
        /// Tạo DataTable từ 1 Queue
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <returns></returns>
        public static DataTable CreateTable<T>(Queue<T> source)
        {
            try
            {
                DataTable dtRet = new DataTable();
                var propertiesInfo = GetProperties<T>();
                foreach (var info in propertiesInfo)
                {
                    var type = info.PropertyType;
                    var underlyingType = Nullable.GetUnderlyingType(type);
                    dtRet.Columns.Add(info.Name, underlyingType ?? type);
                }
                while (source.Count > 0)
                {
                    DataRow newRow = dtRet.NewRow();
                    T enqueueData = source.Dequeue();
                    foreach (var info in propertiesInfo)
                    {
                        newRow[info.Name] = info.GetValue(enqueueData, null) ?? DBNull.Value;
                    }
                    dtRet.Rows.Add(newRow);
                }
                return dtRet;
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        /// <summary>
        /// Tạo 1 DataTable từ 1 IEnumerable
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <returns></returns>
        public static DataTable CreateTable<T>(IEnumerable<T> source)
        {
            try
            {
                DataTable dtRet = CreateTableSchema<T>();
                if (source == null)
                    return dtRet;
                foreach (T item in source)
                {
                    DataRow newRow = dtRet.NewRow();
                    foreach (var info in GetProperties<T>())
                    {
                        newRow[info.Name] = info.GetValue(item, null) ?? DBNull.Value;
                    }
                    dtRet.Rows.Add(newRow);
                }
                return dtRet;
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public static DataTable CreateTableSchema<T>()
        {
            DataTable dtRet = new DataTable();
            foreach (var info in GetProperties<T>())
            {
                var type = info.PropertyType;
                var underlyingType = Nullable.GetUnderlyingType(type);
                dtRet.Columns.Add(info.Name, underlyingType ?? type);
            }
            return dtRet;
        }

        public static DataTable CreateTableSchema<T>(string tableName)
        {
            DataTable dtRet = CreateTableSchema<T>();
            dtRet.TableName = tableName;
            return dtRet;
        }

        public static DataTable CreateTableSchema(Type type)
        {
            DataTable dtRet = new DataTable();
            foreach (var info in GetProperties(type))
            {
                if (info.CanRead)
                {
                    var ptype = info.PropertyType;
                    var underlyingType = Nullable.GetUnderlyingType(ptype);
                    dtRet.Columns.Add(info.Name, underlyingType ?? ptype);
                }
            }
            return dtRet;
        }

        public static DataTable CreateTableSchema(Type type, string tableName)
        {
            DataTable dtRet = CreateTableSchema(type);
            dtRet.TableName = tableName;
            return dtRet;
        }

        public static void AddDataRow<T>(DataTable dt, T entity)
        {
            DataRow newRow = dt.NewRow();
            foreach (var info in GetProperties<T>())
            {
                newRow[info.Name] = info.GetValue(entity, null) ?? DBNull.Value;
            }
            dt.Rows.Add(newRow);
        }

        public static void AssignValue<T>(ref T obj, T value)
        {
            foreach (var propety in GetProperties<T>())
            {
                propety.SetValue(obj, propety.GetValue(value, null), null);
            }
        }

        public static IOrderedEnumerable<System.Reflection.PropertyInfo> GetProperties<T>()
        {
            return typeof(T).GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance)
                .OrderBy(x => x.MetadataToken);
        }

        public static IOrderedEnumerable<System.Reflection.PropertyInfo> GetProperties(Type type)
        {
            return GetProperties(type, System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
        }

        public static IOrderedEnumerable<System.Reflection.PropertyInfo> GetProperties(Type type, System.Reflection.BindingFlags bindingFlag)
        {
            return type.GetProperties(bindingFlag).OrderBy(x => x.MetadataToken);
        }

        public static byte[] ConvertToByteArray(object obj)
        {
            if (obj == null) return null;
            BinaryFormatter bf = new BinaryFormatter();
            MemoryStream ms = new MemoryStream();
            bf.Serialize(ms, obj);
            return ms.ToArray();
        }

        public static void AssignProperties(object sourceObj, object desObj)
        {
            PropertyInfo[] infos = sourceObj.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public).OrderBy(p => p.Name).ToArray();
            if (infos != null && infos.Length > 0)
            {
                foreach (var pi in infos)
                {
                    if (pi.CanWrite)
                    {
                        pi.SetValue(desObj, pi.GetValue(sourceObj, null), null);
                    }
                }
            }
        }
    }
}
