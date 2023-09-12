namespace Sample_DTR_API.Global_variables
{
    public class CurrentDate
    {
        private DateTime _date = Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM-dd"));
        public DateTime currentDate { get { return _date; } }
    }
}