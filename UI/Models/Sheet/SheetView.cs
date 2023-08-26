using Core.Models.Agencies.Cabinets;
using Core.Models.Agencies.Groups;
using Core.Models.Sheets;
using Newtonsoft.Json;

namespace UI.Models
{
    public class SheetView
    {
        public int Id { get; set; }

        public int SheetId { get; set; }

        public Status Status { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string Avatar { get; set; }

        public decimal Balance { get; set; }

        public int Photo { get; set; }

        public int PrivatePhoto { get; set; }

        public int Video { get; set; }

        public int Audio { get; set; }

        public int Operators { get; set; }

        public AgencyGroup Group { get; set; }

        public Shift Shift { get; set; }

        public AgencyCabinet Cabinet { get; set; }

        public DateTime Created { get; set; }

        public static explicit operator SheetView(Sheet sheet)
        {
            var sheetView = new SheetView();
            sheetView.Id = sheet.Id;
            var SheetInfo = JsonConvert.DeserializeObject<SheetInfo>(sheet.Info);
            sheetView.SheetId = SheetInfo.Id;
            sheetView.FirstName = SheetInfo.Personal.Name;
            sheetView.LastName = SheetInfo.Personal.LastName;
            sheetView.Avatar = SheetInfo.Personal.AvatarSmall;
            sheetView.Created = sheet.Created;
            return sheetView;
        }
    }
}
