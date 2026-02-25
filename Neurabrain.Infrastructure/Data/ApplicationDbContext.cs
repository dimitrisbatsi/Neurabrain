using Microsoft.EntityFrameworkCore;
using Neurabrain.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Neurabrain.Infrastructure.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        // Ορισμός των Πινάκων (DbSets)
        public DbSet<User> Users { get; set; }
        public DbSet<StudentProfile> StudentProfiles { get; set; }
        public DbSet<SourceMaterial> SourceMaterials { get; set; }
        public DbSet<GeneratedExercise> GeneratedExercises { get; set; }

        // Ρύθμιση Σχέσεων και Κανόνων με το Fluent API
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // 1. Ρυθμίσεις για τον πίνακα User
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Email).IsRequired().HasMaxLength(150);
                entity.HasIndex(e => e.Email).IsUnique(); // Το Email πρέπει να είναι μοναδικό
                entity.Property(e => e.FullName).IsRequired().HasMaxLength(100);
            });

            // 2. Σχέση: User (1) - StudentProfile (1) [One-to-One]
            modelBuilder.Entity<StudentProfile>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.HasOne(sp => sp.User)
                      .WithOne(u => u.StudentProfile)
                      .HasForeignKey<StudentProfile>(sp => sp.UserId)
                      .OnDelete(DeleteBehavior.Cascade); // Αν διαγραφεί ο χρήστης, διαγράφεται και το προφίλ του
            });

            // 3. Σχέση: User (1) - SourceMaterial (Πολλά) [One-to-Many]
            modelBuilder.Entity<SourceMaterial>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Title).IsRequired().HasMaxLength(200);

                entity.HasOne(sm => sm.UploadedByUser)
                      .WithMany(u => u.UploadedMaterials)
                      .HasForeignKey(sm => sm.UploadedByUserId)
                      .OnDelete(DeleteBehavior.Restrict); // Αποφυγή κυκλικών διαγραφών
            });

            // 4. Σχέση: SourceMaterial (1) - GeneratedExercise (Πολλά) [One-to-Many]
            modelBuilder.Entity<GeneratedExercise>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.AIContentJson).IsRequired(); // Δεν επιτρέπουμε άδειο JSON

                entity.HasOne(ge => ge.SourceMaterial)
                      .WithMany(sm => sm.GeneratedExercises)
                      .HasForeignKey(ge => ge.SourceMaterialId)
                      .OnDelete(DeleteBehavior.Cascade); // Αν διαγραφεί το αρχικό κείμενο, σβήνονται και οι ασκήσεις του
            });
        }
    }
}
