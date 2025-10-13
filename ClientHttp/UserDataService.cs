using attendance_tracking_backend.DTO;
using attendance_tracking_backend.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace attendance_tracking_backend.ClientHttp
{
    public class UserDataService
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly RoleManager<IdentityRole<int>> _roleManager;

        public UserDataService(UserManager<AppUser> userManager, RoleManager<IdentityRole<int>> roleManager) 
        {
            this._userManager = userManager;
            this._roleManager = roleManager;
        }

        public async Task StoreDataAsync(List<EmployeeData> data)
        {
            var users = data;


            // Ensure roles exist
            if (!await _roleManager.RoleExistsAsync("Admin"))
                await _roleManager.CreateAsync(new IdentityRole<int>("Admin"));

            if (!await _roleManager.RoleExistsAsync("User"))
                await _roleManager.CreateAsync(new IdentityRole<int>("User"));
                

            if (users != null)
            {
                foreach (var dto in users)
                {
                    // Check if StaffId already exists to avoid duplicates
                    var existingUser = await _userManager.FindByNameAsync(dto.StaffId!);

                    var existingEmail = await _userManager.FindByEmailAsync(dto.Email!);

                    if (existingUser != null) continue;

                    if (existingEmail != null) continue;
                    
                    var user = new AppUser
                    {
                        EmployeeName = dto.EmployeeName,
                        Email = dto.Email,
                        StaffId = dto.StaffId,
                        UserName = dto.StaffId,
                       // NormalizedUserName = _userManager.NormalizeName(dto.StaffId),
                       // Role = "Employee"
                    };

                  var result =   await _userManager.CreateAsync(user, "password@123");

                    if(result.Succeeded)
                    { await _userManager.AddToRoleAsync(user, "User"); }
                    
                    if (!result.Succeeded)
                    {  Console.WriteLine($"Failed to create user {dto.StaffId}: {string.Join(", ", result.Errors.Select(e => e.Description))}"); }
                }

            }
       
        }
    }
}


/*if (users != null)
          {
              foreach (var dto in users)
              {
                  // Check if StaffId already exists to avoid duplicates
                  var existingUser = await _dbcontext.AppUsers.FirstOrDefaultAsync(u => u.StaffId == dto.StaffId);
                  var exisingUser = await _userManager.FindByNameAsync(dto.StaffId!);

                  if (existingUser == null)
                  {
                      var user = new AppUser
                      {
                          EmployeeName = dto.EmployeeName,
                          Email = dto.Email,
                          StaffId = dto.StaffId, 
                          UserName = dto.StaffId,
                      };

                     _dbcontext.AppUsers.Add(user);
                  }
              }
              await _dbcontext.SaveChangesAsync();


          }*/