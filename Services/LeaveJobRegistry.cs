using FluentScheduler;

namespace attendance_tracking_backend.Services
{
    public class LeaveJobRegistry : Registry
    {
        public LeaveJobRegistry()
        {
            Schedule<LeaveJob>()
                .ToRunEvery(1)
                .Days()
                .At(5, 0); // Run daily at 5 AM UTC
        }
    }
}
