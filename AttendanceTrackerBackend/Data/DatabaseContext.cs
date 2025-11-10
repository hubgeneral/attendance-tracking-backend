using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace attendance_tracking_backend.Data
{
    public class DatabaseContext : IdentityDbContext<AppUser, AppRole, int,IdentityUserClaim<int>, AppUserRole, IdentityUserLogin<int>, IdentityRoleClaim<int>, IdentityUserToken<int>>   //IdentityDbContext<AppUser,IdentityRole<int>, int> //DbContext
    {
        public DatabaseContext() { }
         public DatabaseContext(DbContextOptions<DatabaseContext> options)
            : base(options) { }

        public DbSet<AppUser> AppUsers { get; set; }
        public DbSet<AppRole> AppRoles { get; set; }
        public override DbSet<AppUserRole> UserRoles { get; set; }
        public DbSet<Leave> Leaves { get; set; }
        public DbSet<Attendance> Attendances { get; set; }
        public DbSet<RequestLog> RequestLogs { get; set; }
        public DbSet<EntryExitLog> EntryExitLogs { get; set; }        
       // public DbSet<ManualLog> ManualLogs { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Composite key for the join table
            modelBuilder.Entity<AppUserRole>()
                .HasKey(ur => new { ur.UserId, ur.RoleId });

            // Relationships
            modelBuilder.Entity<AppUser>()
                .HasMany(u => u.UserRoles)
                .WithOne(ur => ur.User)
                .HasForeignKey(ur => ur.UserId)
                .IsRequired();

            modelBuilder.Entity<AppRole>()
                .HasMany(r => r.UserRoles)
                .WithOne(ur => ur.Role)
                .HasForeignKey(ur => ur.RoleId)
                .IsRequired();

            // Seed default roles
            modelBuilder.Entity<AppRole>().HasData(
                new AppRole { Id = 1, Name = "Admin", NormalizedName = "ADMIN" },
                new AppRole { Id = 2, Name = "User", NormalizedName = "USER" }
            );

            // Entity relationships
            modelBuilder.Entity<Leave>()
                .HasOne(l => l.User)
                .WithMany(u => u.Leaves)
                .HasForeignKey(l => l.AppUserId);

            modelBuilder.Entity<Attendance>()
                .HasOne(a => a.User)
                .WithMany(u => u.Attendances)
                .HasForeignKey(a => a.AppUserId);

            modelBuilder.Entity<EntryExitLog>()
               .HasOne(e => e.User)
               .WithMany(u => u.EntryExitLogs)
               .HasForeignKey(e => e.AppUserId);

            modelBuilder.Entity<EntryExitLog>()
                .HasOne(e => e.Attendance)
                .WithMany(a => a.EntryExitLogs)
                .HasForeignKey(e => e.AttendanceId);

            modelBuilder.Entity<RequestLog>()
                .HasOne(r => r.User)
                .WithMany(u => u.RequestLogs)
                .HasForeignKey(r => r.AppUserId);

           
           /* modelBuilder.Entity<ManualLog>()
                .HasOne(a => a.User)
                .WithMany(u => u.ManualLogs)
                .HasForeignKey(a => a.AppUserId);*/

            modelBuilder.Entity<RefreshToken>()
              .HasOne(r => r.User)
              .WithMany(u => u.RefreshTokens)
              .HasForeignKey(a => a.AppUserId);


            // Prevent duplicate emails
            modelBuilder.Entity<AppUser>()
                .HasIndex(u => u.Email)
                .IsUnique();
               //.HasFilter("[Email] IS NOT NULL");

        }
    }
}
