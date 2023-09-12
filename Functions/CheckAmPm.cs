namespace Sample_DTR_API.Functions
{
    public static class CheckAMPM
    {
        //Method for check if the time is now AM or PM
        public static string check()
        {
            var ampm = DateTime.Now;
            Console.WriteLine(ampm.Hour);
            Console.WriteLine(ampm.Minute);

            if (ampm.Hour >= 13)
            {
                return "PM";
            }
            else
            {
                return "AM";
            }
        }
    }
}
