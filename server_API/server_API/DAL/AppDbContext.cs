using api_server.Model;
using api_server.Models;
using Microsoft.EntityFrameworkCore;
using server_API.Model;

namespace server_API.DAL
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options) { }

        public DbSet<Donor> Donors { get; set; }// תורמים
        public DbSet<Gift> gifts { get; set; } // מתנות
        public DbSet<Purchaser> Purchases { get; set; }  // רוכשים
        public DbSet<User> Users { get; set; }          // מנהלים
        public DbSet<Lotteries> Lotteries { get; set; } // הגרלות

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);// קריאה לבסיס

            // קשר 1:N בין Donor ל-Gift
            modelBuilder.Entity<Donor>()
                .HasMany(d => d.GiftList)
                .WithOne(g => g.Donor)
                .HasForeignKey(g => g.DonorId);

            // קשר 1:N בין Gift ל-Purchaser
            modelBuilder.Entity<Gift>()
                .HasMany(g => g.Purchases)     // כל מתנה יכולה להיות עם מספר רכישות
                .WithOne(p => p.Gift)
                .HasForeignKey(p => p.GiftId);

           modelBuilder.Entity<Lotteries>()
                .HasOne(l => l.User)
                .WithMany()                     // אין רשימה הפוכה
                .HasForeignKey(l => l.UserId)
                .OnDelete(DeleteBehavior.Restrict); // למנוע מחיקה של רכישה שהגרילה עליה כבר התבצעה

            modelBuilder.Entity<User>()
              .HasIndex(u => u.Email)
              .IsUnique();

            modelBuilder.Entity<User>()
              .HasIndex(u => u.UserName)
              .IsUnique();

            // אימייל ייחודי
            modelBuilder.Entity<Donor>()
                .HasIndex(d => d.Email)
                .IsUnique();

            // Precision ל-price
            modelBuilder.Entity<Gift>()
                 .Property(g => g.price)
                 .HasPrecision(18, 2);
        }
    }
}
