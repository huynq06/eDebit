using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utils
{
    public static class DateTimeExtensions
    {
        /// <summary>
        /// Lấy ngày đầu tiên của tháng
        /// </summary>
        public static DateTime GetFirstDayOfMonth(this DateTime value)
        {
            return new DateTime(value.Year, value.Month, 1, 0, 0, 0);
        }
        /// <summary>
        /// Lấy ngày cuối cùng của tháng
        /// </summary>
        public static DateTime GetLastDayOfMonth(this DateTime value)
        {
            return value.GetFirstDayOfMonth().AddMonths(1).AddSeconds(-1);
        }

        /// <summary>
        /// Cộng ngày tháng với quy ước 1 tháng có 30 ngày
        /// </summary>
        /// <param name="values">Số ngày cần cộng thêm</param>
        /// <returns></returns>
        public static DateTime AddDaysRound30(this DateTime d, int values)
        {
            int months = (values - 1) / 30;
            int days = (values - 1) % 30;
            DateTime retValue = d.AddMonths(months);
            if (retValue.Day + days <= new DateTime(retValue.Year, retValue.Month, 1).AddMonths(1).AddDays(-1).Day)
            {
                retValue = retValue.AddDays(days);
            }
            else
            {
                days = retValue.Day + days - 30;
                retValue = new DateTime(retValue.Year, retValue.Month + 1, days);
            }
            return retValue;
        }

        /// <summary>
        /// Trả về số ngày của tháng với quy ước một tháng có 30 ngày.
        /// </summary>
        public static int GetDayRound30(this DateTime d)
        {
            DateTime tmp = new DateTime(d.Year, d.Month, 1, 0, 0, 0);
            tmp = tmp.AddMonths(1).AddDays(-1);
            if (tmp.Day == d.Day) return 30;
            return d.Day;
        }
    }
}
