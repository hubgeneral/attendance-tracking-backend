using attendance_tracking_backend.Data;
using attendance_tracking_backend.DTO;
using HotChocolate.Data;
using Microsoft.EntityFrameworkCore;
using System;


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
        public  IEnumerable<UserWithRoleResponse> GetUsersWithRoles([Service] DatabaseContext context)
        {
            var query = from user in context.Users
                        join userRole in context.UserRoles on user.Id equals userRole.UserId
                        join role in context.Roles on userRole.RoleId equals role.Id

                        select new UserWithRoleResponse
                        {
                            UserId = user.Id,
                            StaffId = user.StaffId!,
                            UserName = user.UserName!,
                            EmployeeName = user.EmployeeName!,
                            EmployeeType = user.EmployeeType!,
                            Email = user.Email!,
                            RoleId = role.Id,
                            RoleName = role.Name!,
                            Status = user.Status!
                        };

            return query.ToList();
        }

     /*   [UseProjection, UseFiltering, UseSorting]
        public async Task<IEnumerable<UserWithRoleResponse>> GetUsersWithRoles([Service] DatabaseContext context)
        {
            var query = from user in context.Users
                        join userRole in context.UserRoles on user.Id equals userRole.UserId
                        join role in context.Roles on userRole.RoleId equals role.Id

                        select new UserWithRoleResponse
                        {
                            UserId = user.Id,
                            StaffId = user.StaffId!,
                            UserName = user.UserName!,
                            EmployeeName = user.EmployeeName!,
                            EmployeeType = user.EmployeeType!,
                            Email = user.Email!,
                            RoleId = role.Id,
                            RoleName = role.Name!,
                            Status = user.Status!
                        };

            return await query.ToListAsync();
        }*/

    }

}

