using System;
using HotChocolate.Data;
using attendance_tracking_backend.Data;
using Microsoft.EntityFrameworkCore;


namespace attendance_tracking_backend.GraphQL
{
    [ExtendObjectType(OperationTypeNames.Query)]
    public class AttendanceQuery
    {
        [UseProjection, UseFiltering,UseSorting]
        public IQueryable<Attendance> GetAttendances([Service] DatabaseContext dbcontext)
        {
            return dbcontext.Attendances;
        }

        public IQueryable<Attendance> GetAttendanceByUserId(string username, [Service] DatabaseContext dbcontext)
        {
            var user = dbcontext.Users.Where(u=> u.UserName == username).FirstOrDefault();
      
            return dbcontext.Attendances.Where(a => a.AppUserId == user!.Id);
        }

        
    }
}
