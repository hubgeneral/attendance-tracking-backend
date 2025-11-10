using FluentScheduler;

namespace attendance_tracking_backend.Services
{
    public class TotalDailyHoursWorkedRegistry : Registry
    {
       public TotalDailyHoursWorkedRegistry() {
            Schedule<TotalDailyHoursWorkedJob>()
                .ToRunEvery(1)
                .Days()
                .At(17, 2); //5:02 PM UTC
        }
            
    }
}
