//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace API_SaveMe.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class DailyExpense
    {
        public int DailyExpenseID { get; set; }
        public int UserDataID { get; set; }
        public int CategoryID { get; set; }
        public decimal Amount { get; set; }
        public Nullable<System.DateTime> DateCreated { get; set; }
        public Nullable<System.DateTime> DateModiﬁed { get; set; }
        public Nullable<int> CurrentWeek { get; set; }
        public Nullable<int> CurrentMonth { get; set; }
        public Nullable<int> CurrentYear { get; set; }
    
        public virtual Category Category { get; set; }
        public virtual UserData UserData { get; set; }
    }
}