using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace API_SaveMe.HelperModels
{
    public class AdditionalExpenseRequest
    {

        [Required]
        public bool IsFromCategory { get; set; }
        public string FromCategory { get; set; }
        
        [Required]
        public string ToCategory { get; set; }
        
        [Required]
        public decimal AmountToAdd { get; set; }

        [Required]       
        public int UserDataID { get; set; }
    }
}