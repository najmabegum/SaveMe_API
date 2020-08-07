using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace API_SaveMe.HelperModels
{
    public class AddExpense
    {
        public int UserDataID { get; set; }
        public string Category { get; set; }
        public decimal Amount { get; set; }
    }
}