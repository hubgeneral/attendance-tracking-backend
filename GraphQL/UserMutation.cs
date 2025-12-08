using attendance_tracking_backend.Data;
using attendance_tracking_backend.DTO;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Npgsql.PostgresTypes;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;


namespace attendance_tracking_backend.GraphQL
{
    [ExtendObjectType(OperationTypeNames.Mutation)]

    public class UserMutation   //resolvers
    {
        //User Mutations ******************
        //Login with username and password

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
                    new Claim("userRole", userRole!.Name!),
                    new Claim("IsPasswordReset",user.IsPasswordReset.ToString() )          
                };

            //Read key from config
            var secret = config["TokenSettings:Key"];
            if (string.IsNullOrEmpty(secret)) throw new InvalidOperationException("JWT key missing.");             //Console.WriteLine("Key bytes length: " + Convert.FromBase64String(secret).Length + " **********************************************************") ;

            //Create key and signing credentials
            //var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
            //var key = new SymmetricSecurityKey(Convert.FromBase64String(secret));
            //var key = new SymmetricSecurityKey(Base64UrlEncoder.DecodeBytes(secret));
            var key = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(secret));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            //Create token
            
            //var accessToken = new JwtSecurityToken(issuer: config["TokenSettings:Issuer"], audience: config["TokenSettings:Audience"], claims: claims, expires: DateTime.UtcNow.AddDays(7), signingCredentials: creds);
            var accessToken = new SecurityTokenDescriptor
            {
                SigningCredentials = creds,
                Subject = new ClaimsIdentity(claims),
                Issuer = config["TokenSettings:Issuer"],
                Audience = config["TokenSettings:Audience"],
                Expires = DateTime.UtcNow.AddDays(7)
            };
            //var accessToken = new JwtSecurityToken(issuer: config["TokenSettings:Issuer"], audience: config["TokenSettings:Audience"], claims: claims, signingCredentials: creds);

            //return new JwtSecurityTokenHandler().WriteToken(token);
            var handler = new JwtSecurityTokenHandler();
                
                var token  = handler.CreateToken(accessToken);
            var accessTokenString = handler.WriteToken(token);

            // --- Create Refresh Token ---
            var refreshToken = new RefreshToken
            {
                Token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64)),
                Expires = DateTime.UtcNow.AddDays(7),
                AppUserId = user.Id
            };

            dbcontext.RefreshTokens.Add(refreshToken);
            await dbcontext.SaveChangesAsync();

            var resetToken = "";
            var encodedToken = "";

            resetToken = await userManager.GeneratePasswordResetTokenAsync(user);
            encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(resetToken));

            return new UserLoginResponse
            {
                AccessToken = accessTokenString,
                RefreshToken = refreshToken.Token,
                Id = user.Id.ToString(),
                UserName = user.UserName,
                Role = userRole.Name,
                IsPasswordReset = user.IsPasswordReset,
                ResetToken = encodedToken
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
            var secret = config["TokenSettings:Key"];
            if (string.IsNullOrEmpty(secret)) throw new InvalidOperationException("JWT key is not configured in appsettings.json or environment variables.");
            //Create key and signing credentials
            //var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
            var key = new SymmetricSecurityKey(Convert.FromBase64String(secret));

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            //Create token
            //var accessToken = new JwtSecurityToken(issuer: config["TokenSettings:Issuer"], audience: config["TokenSettings:Audience"], claims: claims, expires: DateTime.UtcNow.AddHours(3), signingCredentials: creds);
            //var accessToken = new JwtSecurityToken(issuer: config["TokenSettings:Issuer"], audience: config["TokenSettings:Audience"], claims: claims, expires: DateTime.UtcNow.AddDays(7), signingCredentials: creds);
            var accessToken = new JwtSecurityToken(issuer: config["TokenSettings:Issuer"], audience: config["TokenSettings:Audience"], claims: claims, signingCredentials: creds);
            //return new JwtSecurityTokenHandler().WriteToken(token);
            var accessTokenString = new JwtSecurityTokenHandler().WriteToken(accessToken);

            // --- Create Refresh Token ---
            var refreshToken = new RefreshToken
            {
                Token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64)),
                Expires = DateTime.UtcNow.AddDays(7),
                AppUserId = user.Id
            };

           
   
           var resetToken = await userManager.GeneratePasswordResetTokenAsync(user);
           var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(resetToken));
          

            return new UserLoginResponse
            {
                AccessToken = accessTokenString,
                RefreshToken = refreshToken.Token,
                Id = user.Id.ToString(),
                UserName = user.UserName,
                Role = userRole.Name,
                IsPasswordReset = user.IsPasswordReset,
                ResetToken = encodedToken,
            };
        }
       
        public async Task<UserResetPasswordResponse> ResetPassword(string token,string username, string password, [Service] UserManager<AppUser> userManager, [Service] IConfiguration config)
        {
            //if (password != confirmpassword) throw new GraphQLException("Passwords do not match");

            var user = await userManager.FindByNameAsync(username);
            if (user == null) throw new GraphQLException("User does not exist");

            var decodedToken = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(token)); //decode encoded token
            var result = await userManager.ResetPasswordAsync(user!,decodedToken, password);

            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                throw new GraphQLException($"Failed to reset password: {errors}");
            }

            user.IsPasswordReset = true;
            await userManager.UpdateAsync(user);

            return new UserResetPasswordResponse
            {
                message = "The password has been successfully reset",
                IsPasswordReset = user.IsPasswordReset
            };
        }
        //Creae User
        public async Task<AppUser> CreateUser( string employeeName, string email,string staffId, string password, string role, string status, [Service] DatabaseContext dbcontext, [Service] UserManager<AppUser> userManager )
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
        public async  Task<AppUser?> UpdateUser( int id,string employeeName, string email,string staffId, string password, string role, string status, [Service] DatabaseContext dbcontext, [Service] UserManager<AppUser> userManager )
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

 
    }
}

