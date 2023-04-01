using Core.Models.Agencies.Groups;
using Core.Models.Sheets;
using Newtonsoft.Json;

namespace UI.Models
{
    public class SheetView
    {
        public int Id { get; set; }

        public Status Status { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public int Balance { get; set; }

        public int Photo { get; set; }

        public int Video { get; set; }

        public int Audio { get; set; }

        public int Operators { get; set; }

        public AgencyGroup Group { get; set; }

        public Shift Shift { get; set; }

        public Cabinet Cabinet { get; set; }

        public static explicit operator SheetView(Sheet sheet)
        {
            var sheetView = new SheetView();
            sheetView.Id = sheet.Id;
            var info = JsonConvert.DeserializeObject<dynamic>(sheet.Info);
            sheetView.FirstName = info["name"];
            sheetView.LastName = info["lastName"];
            return sheetView;
        }
    }
}
