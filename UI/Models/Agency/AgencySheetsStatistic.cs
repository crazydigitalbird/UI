namespace UI.Models
{
    public class AgencySheetsStatistic
    {
        private readonly List<SheetView> _sheets;

        public AgencySheetsStatistic(List<SheetView> sheets)
        {
            _sheets = sheets;
        }

        public int Count
        {
            get { return _sheets?.Count ?? 0; }
        }

        public string Increment()
        {
            var beginningCurrentMonth = GetBeginningCurrentMonth();
            var sheetsCountPreviousMonth = _sheets?.Where(s => s.Created < beginningCurrentMonth).Count() ?? 0;
            if (sheetsCountPreviousMonth != 0)
            {
                int increment = _sheets.Count * 100 / sheetsCountPreviousMonth - 100;
                if (increment > 0)
                {
                    return $"+{increment}";
                }
                else if (increment < 0)
                {
                    return $"-{increment}";
                }
            }
            return "0";
        }

        public int CountInWork
        {
            get { return _sheets?.Where(s => s.Status == Status.Online).Count() ?? 0; }
        }

        private DateTime GetBeginningCurrentMonth()
        {
            var currentDateTime = DateTime.Now;
            var beginningCurrentMonthDateTime = new DateTime(currentDateTime.Year, currentDateTime.Month, 1);
            return beginningCurrentMonthDateTime;
        }
    }
}
