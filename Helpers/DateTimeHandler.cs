namespace attendance_tracking_backend.Helpers
{
   /* public class DateTimeHandler
    {
        public static DateTime startDateTime;
        public static  DateTime closingDateTime;



        public static DateTime GetStartDateTime() {

            string startTime = "08:00:00";
            TimeSpan time = TimeSpan.Parse(startTime);
            // Combine today's UTC date with that time
            DateTime startDateTime = DateTime.UtcNow.Date.Add(time);

            return startDateTime;
        }

        public static DateTime GetClosingDateTime() {

            string closingTime = "17:00:00";
            TimeSpan time = TimeSpan.Parse(closingTime);
            // Combine today's UTC date with that time
            DateTime closingDateTime = DateTime.UtcNow.Date.Add(time);
            return closingDateTime;
        }
         
    }*/

    public static class DateTimeHandler
    {
        // Returns the UTC DateTime for today at 08:00 (UTC)
        public static DateTime GetStartDateTimeUtc(DateTime? day = null)
        {
            var date = (day ?? DateTime.UtcNow).Date;
            return date.AddHours(8); // 08:00 UTC
        }

        // Returns the UTC DateTime for today at 17:00 (UTC)
        public static DateTime GetClosingDateTimeUtc(DateTime? day = null)
        {
            var date = (day ?? DateTime.UtcNow).Date;
            return date.AddHours(17); // 17:00 UTC
        }
    }
}
