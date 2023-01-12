using System.ComponentModel.DataAnnotations;

namespace UI.Models
{
    public enum Interval
    {
        Today,
        Week,
        Month,
        [Display(Name = "Last Month")]
        LastMonth
    }
}
