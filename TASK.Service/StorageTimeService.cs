using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TASK.Data;

namespace TASK.Service
{
    public static class StorageTimeService
    {
        public static double StoreTime(string lastDateStr,string lastTimeStr, string firstDateStr, string firstTimeStr, string shc)
        {
            double storageDate = 0;
            string lastDateTimestr = lastDateStr + " " + lastTimeStr;
            string firstDateTimestr = firstDateStr + " " + firstTimeStr;
            List<HolidayConfig> listHoliday = HolidayConfig.GetData();
            DateTime lastDateTime = DateTime.ParseExact(lastDateTimestr, "dd-MM-yy HH:mm", System.Globalization.CultureInfo.InvariantCulture);
            DateTime firstDateTime = DateTime.ParseExact(firstDateTimestr, "dd-MM-yy HH:mm", System.Globalization.CultureInfo.InvariantCulture);
            if(shc == "GRC" || shc == "HEA" || string.IsNullOrEmpty(shc.Trim()))
            {
                DateTime starChargeDate = firstDateTime.Date.AddDays(3);
                DateTime finishChargeDate = lastDateTime.Date.AddDays(1);
                int weekendDays = 0;
                for (DateTime date = starChargeDate; date.Date <= finishChargeDate.Date; date = date.AddDays(1))
                {
                    if ((date.DayOfWeek == DayOfWeek.Saturday) || (date.DayOfWeek == DayOfWeek.Sunday))
                    {
                        weekendDays++;
                    }
                    if (listHoliday.Any(c => c.DateHoliday.Value.Date == date.Date))
                    {
                        if ((date.DayOfWeek != DayOfWeek.Saturday) || (date.DayOfWeek != DayOfWeek.Sunday))
                        {
                            weekendDays++;
                        }
                    }
                }
                storageDate = (finishChargeDate - starChargeDate).TotalDays - weekendDays;
                if (storageDate <= 0)
                    storageDate = 1;

            }
            return storageDate;
        }
    }
}
