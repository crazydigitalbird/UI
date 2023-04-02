using Core.Models.Agencies.Operators;

namespace UI.Models
{
    public class Operator
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string Email { get; set; }

        public string Time { get; set; }

        public string Team { get; set; }

        public List<SheetView> Sheets { get; set; }

        public static explicit operator Operator(AgencyOperator agencyOperator)
        {
            var oper = new Operator { Id = agencyOperator.Id, Name = agencyOperator.Member.User.Login, Email = agencyOperator.Member.User.Email };
            return oper;
        }
    }
}
