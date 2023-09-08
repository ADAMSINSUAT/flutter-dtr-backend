namespace Sample_DTR_API.DTO
{
    public class GetDtrDTO
    {
        public DateOnly? TimeInDate { get; set; }

        public String? TimeInAm { get; set; }

        public String? TimeOutAm { get; set; }

        public String? TimeInPm { get; set; }

        public String? TimeOutPm { get; set; }

        public double? TotalLoggedHours { get; set; }

        public int UserId { get; set; }
    }
}
