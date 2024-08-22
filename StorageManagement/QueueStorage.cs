using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StorageManagement
{
    public class QueueStorage<T> where T : class
    {
        private static ConcurrentQueue<T> _queueData = new ConcurrentQueue<T>();

        /// <summary>
        /// Đẩy dữ liệu vào Storage theo key dữ liệu
        /// </summary>
        public static void EnqueueElementData(T data)
        {
            _queueData.Enqueue(data);
        }

        /// <summary>
        /// Đẩy 1 tập dữ liệu vào Storage theo key dữ liệu
        /// </summary>
        public static void EnqueueElementData(IEnumerable<T> data)
        {

            foreach (var item in data)
                _queueData.Enqueue(item);
        }

        /// <summary>
        /// Lấy dữ liệu từ Storage và xóa dữ liệu đã được lấy ra khỏi Storage
        /// </summary>
        public static T DequeueElementData()
        {
            T retValue = null;
            _queueData.TryDequeue(out retValue);
            return retValue;
        }

        /// <summary>
        /// Lấy 1 tập dữ liệu từ Storage và xóa dữ liệu đã được lấy ra khỏi Storage
        /// </summary>
        /// <returns></returns>
        public static List<T> DequeueElementData(int countElement)
        {
            List<T> retList = new List<T>();
            while (retList.Count < countElement && !_queueData.IsEmpty)
            {
                T dequeuElement = null;
                _queueData.TryDequeue(out dequeuElement);
                if (dequeuElement != null)
                    retList.Add(dequeuElement);
            }
            return retList;
        }

        /// <summary>
        /// Lấy toàn bô dữ liệu từ Storage và xóa dữ liệu đã được lấy ra khỏi Storage
        /// </summary>
        /// <returns></returns>
        public static List<T> DequeueAllElementData()
        {
            List<T> retList = new List<T>();
            while (!_queueData.IsEmpty)
            {
                T dequeuElement = null;
                _queueData.TryDequeue(out dequeuElement);
                if (dequeuElement != null)
                    retList.Add(dequeuElement);
            }
            return retList;
        }

        /// <summary>
        /// Tìm kiếm Items theo điều kiện 
        /// </summary>
        public static IEnumerable<T> FindItems(Func<T, bool> predicate)
        {
            return _queueData.Where(predicate);
        }

        /// <summary>
        /// Tìm kiếm Item theo điều kiện 
        /// </summary>
        public static T FindItem(Func<T, bool> predicate)
        {
            return _queueData.FirstOrDefault(predicate);
        }

        public static int Count()
        {
            return _queueData.Count;
        }
    }
}
