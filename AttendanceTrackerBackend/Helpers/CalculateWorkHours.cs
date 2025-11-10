namespace attendance_tracking_backend.Helpers
{
    public class CalculateWorkHours
    {
        public static decimal CalculateWorkingHours(DateTime? clockIn, DateTime? clockOut)
        {
            if (clockIn == null || clockOut == null)
                return 0;

            var startWork = new TimeSpan(8, 0, 0);
            var endWork = new TimeSpan(17, 0, 0);

            var actualStart = clockIn.Value.TimeOfDay < startWork ? startWork : clockIn.Value.TimeOfDay;
            var actualEnd = clockOut.Value.TimeOfDay > endWork ? endWork : clockOut.Value.TimeOfDay;

            var total = actualEnd - actualStart;

            if (total.TotalHours < 0)
                return 0;

            return (decimal)total.TotalHours;
        }
    }
}
