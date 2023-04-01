using Core.Models.Agencies.Operators;

namespace UI.Models
{
    public class OperatorSessionsView
    {
        public AgencyOperator Operator { get; set; }

        public IEnumerable<AgencyOperatorSession> Sessions { get; set; }

        public int CountSheets()
        {
            var individualSheets = Sessions.SelectMany(s => s.Session.Sheets);
            var cabinetsSheets = Sessions.SelectMany(s => s.Session.Cabinets).SelectMany(c => c.Cabinet?.Sheets).Select(s => s.Sheet);
            var countSheets = individualSheets.Count() + cabinetsSheets.Count();
            return countSheets;
        }

        public bool IsBindedCabinet(int cabinetId)
        {
            bool isBinded = Sessions?.Any(aos => aos.Session.Cabinets?.Any(asc => asc.Cabinet.Id == cabinetId) ?? false) ?? false;
            return isBinded;
        }
    }
}
