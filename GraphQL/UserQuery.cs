using System;
using HotChocolate.Data;
using attendance_tracking_backend.Data;
using Microsoft.EntityFrameworkCore;
using attendance_tracking_backend.DTO;


namespace attendance_tracking_backend.GraphQL
{
    public class UserQuery
    {
        //User Queries ******************************

        [UseProjection, UseFiltering, UseSorting]
        public IQueryable<AppUser> GetUsers([Service] DatabaseContext dbcontext)
        {
            return dbcontext.AppUsers;
        }

        public AppUser? GetUserById(int id, [Service] DatabaseContext dbcontext)
        {
            return dbcontext.AppUsers.Find(id);
        }

        [UseProjection, UseFiltering, UseSorting]

        public async Task<IEnumerable<UserWithRoleResponse>> GetUsersWithRoles([Service] DatabaseContext dbcontext)
        {

            var query =  from user in dbcontext.AppUsers
                         join userRole in dbcontext.UserRoles on user.Id equals userRole.UserId
                         join role in dbcontext.Roles on userRole.RoleId equals role.Id
                         select new UserWithRoleResponse
                         {
                             UserId = user.Id,
                             UserName = user.UserName,
                             StaffId = user.StaffId,
                             EmployeeName = user.EmployeeName,
                             EmployeeType = user.EmployeeType,
                             Email = user.Email,
                             RoleId = role.Id,
                             RoleName = role.Name
                         };

            return await query.ToListAsync();
        }
    }

}

/*  [UseProjection, UseFiltering, UseSorting]
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
*/