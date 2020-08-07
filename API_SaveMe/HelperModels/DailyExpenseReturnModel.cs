using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace API_SaveMe.HelperModels
{
    public class DailyExpenseReturnModel
    {
        public bool isDatabaseUpdated { get; set; }
        public bool hasThresholdReached { get; set; }
    }
}