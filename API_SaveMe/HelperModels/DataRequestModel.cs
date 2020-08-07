using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace API_SaveMe.HelperModels
{
    public class DataRequestModel
    {
        public string Type { get; set; }
        public string Period { get; set; }
        public int UserDataID { get; set; }
        public string CategoryName { get; set; }
    }
}