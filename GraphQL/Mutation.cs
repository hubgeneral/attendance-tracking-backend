using attendance_tracking_backend.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace attendance_tracking_backend.GraphQL
{
    public class Mutation   //resolvers
    {
       
  //User Mutations ******************************

        //Login with username and password
        public async Task<string> Login(string username, string password, [Service] SignInManager<AppUser> signInManager,[Service] UserManager<AppUser> userManager, [Service] IConfiguration config)
        {
              var user  = await userManager.FindByNameAsync(username);
        
             if (user == null) throw new GraphQLException("Invalid username or password");

            var result = await signInManager.CheckPasswordSignInAsync(user,password, false);
            if (!result.Succeeded) throw new GraphQLException("Invalid username or password");

            var claims = new[]
                {
                    
                    new Claim("Id", user.Id.ToString() ?? ""),
                    new Claim("username", user.UserName ?? ""),
                    new Claim("role", user.Role ?? "User"),
                    new Claim("email", user.Email ?? "")
                   /*new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                    * new Claim(ClaimTypes.Name, user.UserName ?? ""),
                    new Claim(ClaimTypes.Role, user.Role ?? "User"),
                    new Claim(ClaimTypes.Email, user.Email ?? "Email")*/
                };

            //Read key from config
            var secret = config["Jwt:Key"];
            if (string.IsNullOrEmpty(secret)) throw new InvalidOperationException("JWT key is not configured in appsettings.json or environment variables.");

            //Create key and signing credentials
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            //  Create token
            var token = new JwtSecurityToken(issuer: config["Jwt:Issuer"], audience: config["Jwt:Audience"], claims: claims,   expires: DateTime.UtcNow.AddHours(1),signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
        
        //Creae User
        public async Task<AppUser> CreateUser(
            string employeeName,
            string email,
            string staffId,
            string password,
            string role,
            string status,
            [Service] DatabaseContext dbcontext,
            [Service] UserManager<AppUser> userManager
            )
        {         
                var user = new AppUser
                {
                    EmployeeName = employeeName,
                    Email = email,
                    StaffId = staffId,
                    Password = password, // Consider hashing the password
                    Role = role,
                    Status = status
                };

                dbcontext.Users.Add(user);
               await dbcontext.SaveChangesAsync();
                return user;
        }

        // Update User       
        public async  Task<AppUser?> UpdateUser(
            int id,
            string employeeName,
            string email,
            string staffId,
            string password,
            string role,
            string status,
            [Service] DatabaseContext dbcontext,
            [Service] UserManager<AppUser> userManager
            )
        {        
            var user =  dbcontext.Users.Find(id);
            
            if (user == null) return null; 

            user.EmployeeName = employeeName;
            user.Email = email;
            user.StaffId = staffId;
            user.Password = userManager.PasswordHasher.HashPassword(user,password); ;  // Consider hashing the password 
            user.Role = role;
            user.Status = status;

            await dbcontext.SaveChangesAsync();
            return user;
        }

        // Delete User
        public async Task<bool> DeleteUser(int id, [Service] DatabaseContext dbcontext)
        {
            var user = dbcontext.Users.Find(id);
            if (user == null) return false;

            dbcontext.Users.Remove(user);
            await dbcontext.SaveChangesAsync();
            return true;
        }

 // Attendance Mutations ****************************************************************/

        public async Task<Attendance> CreateAttendance(
                DateTime clockin,
                DateTime clockout,
                int totalhoursworked,
                string status,
                DateOnly currentdate,
                int appuserid,
                [Service] DatabaseContext dbcontext
            )
        {
            var attendance = new Attendance()
            {
                ClockIn = clockin,
                ClockOut = clockout,
                TotalHoursWorked = totalhoursworked ,
                Status =  status,
                CurrentDate = currentdate ,
                AppUserId = appuserid 
            };

            dbcontext.Attendances.Add(attendance);
            await dbcontext.SaveChangesAsync();
            return attendance;
        }

        public async Task<Attendance?> UpdateAttendance(
             DateTime clockin,
             DateTime clockout,
             int totalhoursworked,
             string status,
             DateOnly currentdate,
             int appuserid,
             [Service] DatabaseContext dbcontext
         )
        {
            var attendance = dbcontext.Attendances.FirstOrDefault(a=> a.AppUserId == appuserid && a.CurrentDate == currentdate);

            if (attendance == null) return null;

            attendance.ClockIn = clockin;
            attendance.ClockOut = clockout;
            attendance.TotalHoursWorked = totalhoursworked;
            attendance.Status = status;
            attendance.CurrentDate = currentdate;
            attendance.AppUserId = appuserid;

            await dbcontext.SaveChangesAsync();
            return attendance;
        }

        public async Task<bool> DeleteAttendance(int id , [Service] DatabaseContext dbcontext)
        {
            var attendance = dbcontext.Attendances.Find(id);
            if (attendance == null) return false;

            dbcontext.Attendances.Remove(attendance);
            await dbcontext.SaveChangesAsync();
            return true;
        }

//Clocking Mutations ******************************************************************/

       
    }
}

