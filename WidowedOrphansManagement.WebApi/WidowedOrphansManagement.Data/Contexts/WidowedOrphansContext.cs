using Microsoft.EntityFrameworkCore;
using WidowedOrphansManagement.Models;

namespace WidowedOrphansManagement.Data.Contexts
{
    public class WidowedOrphansContext : DbContext
    {
        public WidowedOrphansContext(DbContextOptions<WidowedOrphansContext> options)
            : base(options)
        {
        }

        public DbSet<Parent> Parents { get; set; }
        public DbSet<Orphan> Orphans { get; set; }
        public DbSet<Status> Statuses { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // קשר בין הורה לסטטוס אישי
            modelBuilder.Entity<Parent>()
                .HasOne(p => p.Status)
                .WithMany()
                .HasForeignKey(p => p.StatusId)
                .OnDelete(DeleteBehavior.Restrict);

            // קשר בין יתום לסטטוס אישי
            modelBuilder.Entity<Orphan>()
                .HasOne(o => o.Status)
                .WithMany()
                .HasForeignKey(o => o.StatusId)
                .OnDelete(DeleteBehavior.Restrict);

            // קשר בין יתום להורה - הקשר יהיה לפי תעודת הזהות של ההורה
            modelBuilder.Entity<Orphan>()
                .HasOne(o => o.Parent)  // קשר להורה
                .WithMany(p => p.Orphans)  // הורה יכול להיות קשור ליותר מיתום אחד
                .HasForeignKey(o => o.ParentIdentityNumber)  // הקשר יהיה לפי תעודת הזהות של ההורה
                .HasPrincipalKey(p => p.IdentityNumber)  // המפתח הראשי של ההורה הוא תעודת הזהות
                .OnDelete(DeleteBehavior.Cascade);  // כאשר הורה נמחק, נמחקים גם הילדים
        }
    }
}
