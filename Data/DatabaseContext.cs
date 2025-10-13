using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace attendance_tracking_backend.Data
{
    public class DatabaseContext : IdentityDbContext<AppUser, AppRole, int,
                        IdentityUserClaim<int>, AppUserRole,
                        IdentityUserLogin<int>, IdentityRoleClaim<int>,
                        IdentityUserToken<int>>//IdentityDbContext<AppUser,IdentityRole<int>, int> //DbContext
    {
         public DatabaseContext(DbContextOptions<DatabaseContext> options)
            : base(options) { }

        public DbSet<AppUser> AppUsers { get; set; }
        public DbSet<AppRole> AppRoles { get; set; }   
        public override DbSet<AppUserRole> UserRoles { get; set; }

      
        public DbSet<Leave> Leaves { get; set; }
        public DbSet<Attendance> Attendances { get; set; }
        public DbSet<Request> Requests { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            //composite key
            modelBuilder.Entity<AppUserRole>()
          .HasKey(ur => new { ur.UserId, ur.RoleId });

            // User ↔ UserRoles (1:N)
            modelBuilder.Entity<AppUser>()
                .HasMany(u => u.UserRoles)
                .WithOne(ur => ur.User)
                .HasForeignKey(ur => ur.UserId)
                .IsRequired();

            // Role ↔ UserRoles (1:N)
            modelBuilder.Entity<AppRole>()
                .HasMany(r => r.UserRoles)
                .WithOne(ur => ur.Role)
                .HasForeignKey(ur => ur.RoleId)
                .IsRequired();


            modelBuilder.Entity<AppRole>().HasData(
                 new AppRole  {  Id = 1,  Name = "Admin", NormalizedName = "ADMIN"},
                 new AppRole { Id = 2, Name = "User",  NormalizedName = "USER" }
                );

            //Entity Relationships
            //************************
            modelBuilder.Entity<Leave>()
               .HasOne(l => l.User)
               .WithMany(u => u.Leaves)
               .HasForeignKey(l => l.AppUserId);

            modelBuilder.Entity<Attendance>()  
                .HasOne(a => a.User)
                .WithMany(u => u.Attendances)
                .HasForeignKey(a => a.AppUserId);

            //fetch  employee and leave data from api
            //****************************************
            //prevent duplicates 
            modelBuilder.Entity<AppUser>()
                .HasIndex(u => u.Email)
                .IsUnique().HasFilter("[Email] IS NOT NULL"); ; 

        }
    }
}
