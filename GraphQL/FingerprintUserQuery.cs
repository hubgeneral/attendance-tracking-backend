using attendance_tracking_backend.Data;
using attendance_tracking_backend.DTO;
using HotChocolate.Authorization;
using HotChocolate.Data;
using Microsoft.EntityFrameworkCore;
using System;


namespace attendance_tracking_backend.GraphQL
{
    //[Authorize(Roles = new[] {"User"})]
    [ExtendObjectType(OperationTypeNames.Query)]
    public class FingerprintUserQuery
    {

        [AllowAnonymous]
        [UseProjection, UseFiltering, UseSorting]
        public IQueryable<FingerprintUser> GetFingerprintUsers([Service] DatabaseContext dbcontext)
        {
            var query = from user in dbcontext.AppUsers


                        select new FingerprintUser
                        {
                            UserId = user.Id,
                            EmployeeName = user.EmployeeName!
                        };

            return query;
        }

    }

}

