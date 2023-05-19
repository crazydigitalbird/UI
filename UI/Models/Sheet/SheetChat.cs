using Core.Models.Sheets;
using Newtonsoft.Json;

namespace UI.Models
{
    public class SheetChat : Sheet
    {
        private SheetInfo sheetInfo;
        public SheetInfo SheetInfo
        {
            get
            {
                if(sheetInfo == null)
                {
                    sheetInfo = JsonConvert.DeserializeObject<SheetInfo>(base.Info);
                    sheetInfo.SheetId = Id;
                }
                return sheetInfo;
            }
        }
    }
}
