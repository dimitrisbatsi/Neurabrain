using Microsoft.EntityFrameworkCore;
using Neurabrain.Domain.Entities;

namespace Neurabrain.Infrastructure.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // --- DbSets (Οι πίνακες της Βάσης) ---
        public DbSet<SystemSetting> SystemSettings { get; set; }
        public DbSet<LearningCondition> LearningConditions { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Organization> Organizations { get; set; }
        public DbSet<UserOrganization> UserOrganizations { get; set; }
        public DbSet<StudentProfile> StudentProfiles { get; set; }
        public DbSet<SourceMaterial> SourceMaterials { get; set; }
        public DbSet<GeneratedExercise> GeneratedExercises { get; set; }
        public DbSet<Subscription> Subscriptions { get; set; }
        public DbSet<Classroom> Classrooms { get; set; }
        public DbSet<ClassroomStudent> ClassroomStudents { get; set; }
        public DbSet<Assignment> Assignments { get; set; }
        public DbSet<ExerciseResult> ExerciseResults { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // 1. Ρυθμίσεις για SystemSettings
            modelBuilder.Entity<SystemSetting>(entity =>
            {
                entity.HasKey(e => e.Key);
                entity.Property(e => e.Value).IsRequired();
            });

            // 2. Ρυθμίσεις για LearningConditions
            modelBuilder.Entity<LearningCondition>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Description).HasMaxLength(500);
                entity.Property(e => e.AiPromptInstruction).HasMaxLength(2000);
            });

            // 3. Ρυθμίσεις για User
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.FullName).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Email).IsRequired().HasMaxLength(256);
                entity.HasIndex(e => e.Email).IsUnique(); // Το Email πρέπει να είναι μοναδικό!
            });

            // 4. Ρυθμίσεις για Organization
            modelBuilder.Entity<Organization>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
                entity.Property(e => e.TaxId).HasMaxLength(50); // ΑΦΜ
            });

            // 5. Ρυθμίσεις για UserOrganization (Many-to-Many σχέση)
            modelBuilder.Entity<UserOrganization>(entity =>
            {
                // Σύνθετο Primary Key (Το UserId και το OrganizationId μαζί)
                entity.HasKey(uo => new { uo.UserId, uo.OrganizationId });

                entity.HasOne(uo => uo.User)
                      .WithMany(u => u.UserOrganizations)
                      .HasForeignKey(uo => uo.UserId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(uo => uo.Organization)
                      .WithMany(o => o.UserOrganizations)
                      .HasForeignKey(uo => uo.OrganizationId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // 6. Ρυθμίσεις για StudentProfile
            modelBuilder.Entity<StudentProfile>(entity =>
            {
                entity.HasKey(e => e.Id);

                // Σύνδεση με User (Restrict για ασφάλεια στη διαγραφή)
                entity.HasOne(sp => sp.User)
                      .WithMany(u => u.StudentProfiles)
                      .HasForeignKey(sp => sp.UserId)
                      .OnDelete(DeleteBehavior.Restrict);

                // Σύνδεση με Organization
                entity.HasOne(sp => sp.Organization)
                      .WithMany(o => o.StudentProfiles)
                      .HasForeignKey(sp => sp.OrganizationId)
                      .OnDelete(DeleteBehavior.Restrict);

                // Σύνδεση με LearningCondition
                entity.HasOne(sp => sp.LearningCondition)
                      .WithMany(lc => lc.StudentProfiles)
                      .HasForeignKey(sp => sp.LearningConditionId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // 7. Ρυθμίσεις για SourceMaterial (Το "Συννεφάκι" του καθηγητή)
            modelBuilder.Entity<SourceMaterial>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Title).IsRequired().HasMaxLength(300);

                entity.HasOne(sm => sm.OwnerUser)
                      .WithMany(u => u.OwnedMaterials)
                      .HasForeignKey(sm => sm.OwnerUserId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // 8. Ρυθμίσεις για GeneratedExercise
            modelBuilder.Entity<GeneratedExercise>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.HasOne(ge => ge.SourceMaterial)
                      .WithMany(sm => sm.GeneratedExercises)
                      .HasForeignKey(ge => ge.SourceMaterialId)
                      .OnDelete(DeleteBehavior.Cascade); // Αν διαγραφεί το PDF, σβήνονται και οι ασκήσεις του

                entity.HasOne(ge => ge.LearningCondition)
                      .WithMany(lc => lc.GeneratedExercises)
                      .HasForeignKey(ge => ge.LearningConditionId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // 9. Ρυθμίσεις για Subscriptions
            modelBuilder.Entity<Subscription>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.PlanName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Status).IsRequired().HasMaxLength(50);

                // Ένας Οργανισμός μπορεί να έχει πολλές συνδρομές (π.χ. ιστορικό), 
                // αλλά αν διαγραφεί ο οργανισμός, διαγράφονται και οι συνδρομές του (Cascade)
                entity.HasOne(s => s.Organization)
                      .WithMany() // Δεν βάλαμε ICollection στο Organization.cs για τις συνδρομές, οπότε το αφήνουμε κενό
                      .HasForeignKey(s => s.OrganizationId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // 10. Ρύθμιση για το ClassroomStudent (Many-to-Many)
            modelBuilder.Entity<ClassroomStudent>()
                .HasKey(cs => new { cs.ClassroomId, cs.StudentId });

            modelBuilder.Entity<ClassroomStudent>()
                .HasOne(cs => cs.Classroom)
                .WithMany(c => c.ClassroomStudents)
                .HasForeignKey(cs => cs.ClassroomId)
                .OnDelete(DeleteBehavior.Cascade); // Αν διαγραφεί η τάξη, διαγράφονται και οι συνδέσεις

            modelBuilder.Entity<ClassroomStudent>()
                .HasOne(cs => cs.Student)
                .WithMany() // Δεν χρειαζόμαστε navigation property στο User για να μην το βαρύνουμε
                .HasForeignKey(cs => cs.StudentId)
                .OnDelete(DeleteBehavior.Restrict); // Προστασία: Δεν σβήνουμε τον User αν σβηστεί η σύνδεση

            // 11. Ρυθμίσεις για τις Αναθέσεις (Assignments)
            modelBuilder.Entity<Assignment>()
                .HasOne(a => a.GeneratedExercise)
                .WithMany(ge => ge.Assignments)
                .HasForeignKey(a => a.GeneratedExerciseId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Assignment>()
                .HasOne(a => a.Classroom)
                .WithMany(c => c.Assignments)
                .HasForeignKey(a => a.ClassroomId)
                .OnDelete(DeleteBehavior.Cascade);

            // 12. Ρυθμίσεις για τα Αποτελέσματα Ασκήσεων (ExerciseResults)
            modelBuilder.Entity<ExerciseResult>()
                .HasOne(er => er.Assignment)
                .WithMany() // Αν θέλουμε στο μέλλον μπορούμε να προσθέσουμε ICollection<ExerciseResult> στο Assignment
                .HasForeignKey(er => er.AssignmentId)
                .OnDelete(DeleteBehavior.Cascade); // Αν διαγραφεί η Ανάθεση, διαγράφονται και τα αποτελέσματά της

            modelBuilder.Entity<ExerciseResult>()
                .HasOne(er => er.Student)
                .WithMany()
                .HasForeignKey(er => er.StudentId)
                .OnDelete(DeleteBehavior.Cascade); // Αν διαγραφεί ο Μαθητής, διαγράφονται και τα αποτελέσματά του
        }
    }
}