using System;
using HotChocolate.Data;
using attendance_tracking_backend.Data;
using Microsoft.EntityFrameworkCore;

namespace attendance_tracking_backend.GraphQL
{
    public class Query
    {
        //User Queries ******************************

        [UseProjection,UseFiltering,UseSorting]
        public IQueryable<AppUser> GetUsers([Service] DatabaseContext dbcontext)
        {
            return dbcontext.AppUsers;
        }

        public AppUser? GetUserById(int id, [Service] DatabaseContext dbcontext)
        {
            return dbcontext.AppUsers.Find(id);
        }


        [UseProjection, UseFiltering, UseSorting]
        public AppUser? GetUserLeaveById(int Id,[Service] DatabaseContext dbcontext)
        {                           // join User with Leaves
            return  dbcontext.AppUsers.Include(u => u.Leaves).FirstOrDefault(u => u.Id == Id);
        }


        //Leave Queries ********************************


        [UseProjection, UseFiltering, UseSorting]
        public IQueryable<Leave> GetLeaves([Service] DatabaseContext dbcontext)
            {
              return dbcontext.Leaves;
            }

        public Leave? GetLeaveById(int Id, [Service] DatabaseContext dbcontext)
        {
            return dbcontext.Leaves.Find(Id);
        }




        //Attendance Inputs ***************************************

    }

   
}
