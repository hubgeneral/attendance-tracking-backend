using attendance_tracking_backend.DTO;
using attendance_tracking_backend.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace attendance_tracking_backend.ClientHttp
{
    public class UserDataService
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly RoleManager<AppRole> _roleManager;

        public UserDataService(UserManager<AppUser> userManager, RoleManager<AppRole> roleManager) 
        {
            this._userManager = userManager;
            this._roleManager = roleManager;
        }

        public async Task StoreDataAsync(List<EmployeeData> data)
        {
            var users = data;

            // Ensure roles exist
            if (!await _roleManager.RoleExistsAsync("Admin"))
                await _roleManager.CreateAsync(new AppRole("Admin"));

            if (!await _roleManager.RoleExistsAsync("User"))
                await _roleManager.CreateAsync(new AppRole("User"));
                
            if (users != null)
            {
                foreach (var dto in users)
                {
                    // Check if StaffId already exists to avoid duplicates
                    var existingUser = await _userManager.FindByNameAsync(dto.StaffId!);

                    var existingEmail = await _userManager.FindByEmailAsync(dto.Email!);

                    if (existingUser != null || existingEmail != null || dto.Status == "Retired") continue;  
                    
                    var user = new AppUser
                    {
                        EmployeeName = dto.EmployeeName,
                        Email = dto.Email,
                        StaffId = dto.StaffId,
                        UserName = dto.StaffId,
                        EmployeeType = dto.EmploymentType,
                        IsPasswordReset = false
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

