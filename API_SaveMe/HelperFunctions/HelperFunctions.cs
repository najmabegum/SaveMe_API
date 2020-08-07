using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;

namespace API_SaveMe.HelperFunctions
{
    public static class HelperFunctions
    {
        public static int CurrentWeekNumber
        {
            get
            {
                CultureInfo myCI = new CultureInfo("en-US");
                Calendar myCal = myCI.Calendar;

                CalendarWeekRule myCWR = myCI.DateTimeFormat.CalendarWeekRule;
                DayOfWeek myFirstDOW = myCI.DateTimeFormat.FirstDayOfWeek;
                DateTime curr = DateTime.UtcNow;
                int currentWeekNumber = myCal.GetWeekOfYear(curr, myCWR, myFirstDOW);

                return currentWeekNumber;
            }
        }
    }
}