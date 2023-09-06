using Core.Models.Sheets;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace UI.Models
{
    public class AdminAgencyView
    {
        public int AgencyId { get; set; }

        public List<SheetView> Sheets { get; set; }

        public List<SheetSite> Sites { get; set; }

        public IEnumerable<SelectListItem> Groups { get; set; }

        public IEnumerable<SelectListItem> Cabinets { get; set; }

    }
}
