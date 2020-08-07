using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace API_SaveMe.HelperModels
{
    public class WeeklyLimit
    {
        public string CategoryName { get; set; }
        public decimal Amount { get; set; }
        public int UserDataID { get; set; }
    }
}