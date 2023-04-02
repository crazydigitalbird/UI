using Core.Models.Agencies.Cabinets;
using System.Linq;

namespace UI.Models
{
    public class Cabinet
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public ICollection<SheetView> Profiles { get; set; } = new List<SheetView>();

        public ICollection<ApplicationUser> Users { get; set; } = new List<ApplicationUser>();

        public static explicit operator Cabinet(AgencyCabinet cabinetAgency)
        {
            var cabinet = new Cabinet() { Id = cabinetAgency.Id, Name = cabinetAgency.Name };
            //var individualSheets = cabinetAgency.Sheets.Select(s => (SheetView)s.Sheet).ToList();
            //var cabinetSheets = cabinetAgency.Sheets.SelectMany(s => s.Cabinet?.Sheets)?.Select(s => (SheetView)s?.Sheet)?.ToList() ?? new List<SheetView>();
            //var allSheets = individualSheets.Concat(cabinetSheets).ToList(); // Union
            //cabinet.Profiles = allSheets;
            return cabinet;
        }
    }
}
