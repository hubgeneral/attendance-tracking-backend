using attendance_tracking_backend.Data;
using attendance_tracking_backend.DTO;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Npgsql.PostgresTypes;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace attendance_tracking_backend.GraphQL
{

    public class UserMutation   //resolvers
    {
        //User Mutations ******************************

        //Login with username and password
        // public async Task<string> Login(string username, string password, [Service] SignInManager<AppUser> signInManager,[Service] UserManager<AppUser> userManager, [Service] IConfiguration config)
        public async Task<UserLoginResponse> Login(string username, string password, [Service] SignInManager<AppUser> signInManager, [Service] UserManager<AppUser> userManager, [Service] DatabaseContext dbcontext, [Service] IConfiguration config)
        {
            //Find
            var user  = await userManager.FindByNameAsync(username);
            var userRoleId = await dbcontext.UserRoles.Where(ur => ur.UserId == user!.Id).FirstOrDefaultAsync();
            var userRole = await dbcontext.Roles.FindAsync(userRoleId!.RoleId);

            //var arole = userManager.GetRolesAsync(user!);     //user with multiple roles
                   
            if (user == null) throw new GraphQLException("Invalid username or password");

            var result = await signInManager.CheckPasswordSignInAsync(user,password, false);
            if (!result.Succeeded) throw new GraphQLException("Invalid username or password");

            var claims = new[]
                {
                    
                    new Claim("Id", user.Id.ToString() ?? ""),
                    new Claim("userName", user.UserName ?? ""),
                    new Claim("userRole", userRole!.Name!)
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
            //return new JwtSecurityTokenHandler().WriteToken(token);
            var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

            return new UserLoginResponse
            {
                Token = tokenString,
                Id = user.Id.ToString(),
                UserName = user.UserName,
                Role =  userRole.Name
            };
        }
        
        public async Task<UserLoginResponse> LoginForForgottenPassword(string email,string staffid,string phoneno, [Service] UserManager<AppUser> userManager, [Service] IConfiguration config, [Service] DatabaseContext dbcontext)
        {                   
            var user = await userManager.Users.Where(u =>  u.Email == email && u.StaffId == staffid && u.PhoneNumber == phoneno).FirstOrDefaultAsync();
            if (user == null) throw new GraphQLException("Invalid email or staffid or phoneno");

            var userRoleId = await dbcontext.UserRoles.Where(ur => ur.UserId == user!.Id).FirstOrDefaultAsync();
            var userRole = await dbcontext.Roles.FindAsync(userRoleId!.RoleId);

            var claims = new[]
                  {
                    new Claim("Id", user.Id.ToString() ?? ""),
                    new Claim("username", user.UserName ?? ""),
                    new Claim("userRole", userRole!.Name!)
                };

            //Read key from config
            var secret = config["Jwt:Key"];
            if (string.IsNullOrEmpty(secret)) throw new InvalidOperationException("JWT key is not configured in appsettings.json or environment variables.");
            //Create key and signing credentials
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            //Create token
            var token = new JwtSecurityToken(issuer: config["Jwt:Issuer"], audience: config["Jwt:Audience"], claims: claims, expires: DateTime.UtcNow.AddHours(1), signingCredentials: creds);
            //return new JwtSecurityTokenHandler().WriteToken(token);
            var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

            return new UserLoginResponse
            {
                Token = tokenString,
                Id = user.Id.ToString(),
                UserName = user.UserName,
                Role =  userRole.Name
            };
        }
       
        public async Task<string> ResetPassword(string token,string username, string password, [Service] UserManager<AppUser> userManager, [Service] IConfiguration config)
        {
            //if (password != confirmpassword) throw new GraphQLException("Passwords do not match");

            var user = await userManager.FindByIdAsync(username);
            if (user == null) throw new GraphQLException("User does not exist");

            var result = await userManager.ResetPasswordAsync(user!,token, password);
            if (!result.Succeeded) throw new GraphQLException("Failed to reset password");


            return "The password has been successfully reset";
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
            var user = await dbcontext.Users.FindAsync(id);
            
            if (user == null) return null; 

            user.EmployeeName = employeeName;
            user.Email = email;
            user.StaffId = staffId;
            user.Password = userManager.PasswordHasher.HashPassword(user,password); ;  // Consider hashing the password 
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

