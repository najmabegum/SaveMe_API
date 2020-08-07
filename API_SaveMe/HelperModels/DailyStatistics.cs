using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace API_SaveMe.HelperModels
{
    public class DailyStatistics
    {
        public decimal CurrentWeekTotalExpenditure { get; set; }

        public decimal CurrentWeekSavings { get; set; }

        public decimal CurrentWeekExpenses { get; set; }

        public bool IsWeekPlanSet { get; set; }
    }
}