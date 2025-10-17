namespace attendance_tracking_backend.Helpers
{
    public static class TimeRangeHelper
    {
        // returns overlap duration in seconds between [aStart,aEnd] and [bStart,bEnd]
     /*   public static double OverlapSeconds(DateTime aStart, DateTime aEnd, DateTime bStart, DateTime bEnd)
        {
            var start = aStart > bStart ? aStart : bStart;
            var end = aEnd < bEnd ? aEnd : bEnd;
            if (end <= start) return 0;
            return (end - start).TotalSeconds;
        }*/

        // returns overlap in seconds between [start1,end1] and [start2,end2]
        public static double OverlapSeconds(DateTime start1, DateTime end1, DateTime start2, DateTime end2)
        {
            var s = start1 > start2 ? start1 : start2;
            var e = end1 < end2 ? end1 : end2;
            if (e <= s) return 0.0;
            return (e - s).TotalSeconds;
        }
    }

}
