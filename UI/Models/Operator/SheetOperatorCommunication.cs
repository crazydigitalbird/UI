using Core.Models.Agencies.Operators;
using Core.Models.Sheets;

namespace UI.Models
{
    public class SheetOperatorCommunication
    {
        public Sheet Sheet { get; set; }

        public AgencyOperator Operator { get; set; }

        public bool Free { get; set; }
    }
}
