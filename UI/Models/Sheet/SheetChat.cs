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
                return sheetInfo ?? JsonConvert.DeserializeObject<SheetInfo>(base.Info);
            }
        }
    }
}
