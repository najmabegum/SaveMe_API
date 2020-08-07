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
    
    public partial class WeeklyExpenseDetail
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public WeeklyExpenseDetail()
        {
            this.RemoteWeeklyExpenseDetails = new HashSet<RemoteWeeklyExpenseDetail>();
        }
    
        public int WeeklyExpenseDetailsID { get; set; }
        public int UserDataID { get; set; }
        public int CategoryID { get; set; }
        public decimal Amount { get; set; }
        public int CurrentWeek { get; set; }
        public int CurrentMonth { get; set; }
        public int CurrentYear { get; set; }
        public Nullable<System.DateTime> DateCreated { get; set; }
        public Nullable<System.DateTime> DateModiﬁed { get; set; }
    
        public virtual Category Category { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<RemoteWeeklyExpenseDetail> RemoteWeeklyExpenseDetails { get; set; }
        public virtual UserData UserData { get; set; }
    }
}
