namespace UI.Models
{
    public class AdminAgencyStatisticsViewModel
    {
        public string Downtime { get; internal set; }

        public string DowntimeIncrement { get; internal set; }

        public AgencyMetrik Metrik { get; internal set; }

        public BalanceStatisticAgency BalanceStatisticAgency { get; internal set; }        

        public List<ApplicationUser> AdminsAgency { get; internal set; }

        public AgencySheetsStatistic AgencySheetsStatistic { get; internal set; }
    }

    public class BalanceType
    {
        public int ChangePercent { get; internal set; }

        public string CSSClass { get; internal set; }

        public string Text { get; internal set; }
    }
}
