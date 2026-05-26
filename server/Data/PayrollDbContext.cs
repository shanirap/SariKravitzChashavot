using Microsoft.EntityFrameworkCore;
using AccountingProject.Models;
using AccountingProject.Infrastructure;
using System.Text.Json;

namespace AccountingProject.Data
{
    public class PayrollDbContext : DbContext
    {
        private static readonly HashSet<string> SensitiveAuditProperties = new(StringComparer.OrdinalIgnoreCase)
        {
            "Password",
            "PasswordHash",
            "Token",
            "Jwt",
            "JwtToken",
            "BootstrapAdminPassword"
        };

        private readonly ICurrentUserService? _currentUserService;

        public PayrollDbContext(DbContextOptions<PayrollDbContext> options)
            : base(options)
        {
        }

        public PayrollDbContext(
            DbContextOptions<PayrollDbContext> options,
            ICurrentUserService currentUserService)
            : base(options)
        {
            _currentUserService = currentUserService;
        }

        public DbSet<Employer> Employers { get; set; }
        public DbSet<Employee> Employees { get; set; }
        public DbSet<EmploymentData> EmploymentData { get; set; }
        public DbSet<EmploymentDataSlot> EmploymentDataSlots { get; set; }
        public DbSet<EmployerInstitutionSymbol> EmployerInstitutionSymbols { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<PayrollMonthlyInputBatch> PayrollMonthlyInputBatches { get; set; }
        public DbSet<PayrollMonthlyInputRow> PayrollMonthlyInputRows { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<EmploymentData>()
                .HasOne(e => e.Employee)
                .WithMany(e => e.EmploymentData)
                .HasForeignKey(e => e.EmployeeId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<EmploymentData>()
                .HasOne(e => e.Employer)
                .WithMany(e => e.EmploymentData)
                .HasForeignKey(e => e.EmployerId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Employee>()
                .HasOne(e => e.Employer)
                .WithMany(e => e.Employees)
                .HasForeignKey(e => e.EmployerId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Employee>()
                .HasIndex(e => new { e.EmployerId, e.IdNumber })
                .IsUnique()
                .HasFilter("[IsDeleted] = 0");

            modelBuilder.Entity<EmployerInstitutionSymbol>()
                .HasOne(s => s.Employer)
                .WithMany()
                .HasForeignKey(s => s.EmployerId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<EmployerInstitutionSymbol>()
                .HasQueryFilter(s => !s.Employer!.IsDeleted);

            modelBuilder.Entity<EmployerInstitutionSymbol>()
                .Property(s => s.InstitutionType)
                .HasMaxLength(20)
                .HasDefaultValue("אחר");

            modelBuilder.Entity<EmploymentDataSlot>()
                .HasOne(s => s.EmploymentData)
                .WithMany(e => e.Slots)
                .HasForeignKey(s => s.EmploymentDataId)
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();

            modelBuilder.Entity<EmploymentDataSlot>()
                .HasQueryFilter(s => !s.EmploymentData!.IsDeleted);

            modelBuilder.Entity<Employer>()
                .HasQueryFilter(e => !e.IsDeleted);

            modelBuilder.Entity<Employee>()
                .HasQueryFilter(e => !e.IsDeleted);

            modelBuilder.Entity<EmploymentData>()
                .HasQueryFilter(e => !e.IsDeleted);

            modelBuilder.Entity<EmploymentData>()
                .HasIndex(e => new { e.EmployeeId, e.EmployerId, e.AcademicYear })
                .IsUnique()
                .HasFilter("[IsDeleted] = 0");

            modelBuilder.Entity<Employer>()
                .HasIndex(e => e.BusinessNumber)
                .HasFilter("[חפ] IS NOT NULL AND [IsDeleted] = 0")
                .IsUnique();

            modelBuilder.Entity<AuditLog>()
                .HasIndex(a => a.ChangedAtUtc);

            modelBuilder.Entity<User>()
                .HasIndex(u => u.Username)
                .IsUnique();

            modelBuilder.Entity<EmploymentData>().Property(e => e.Grade1Total).HasPrecision(18, 2);
            modelBuilder.Entity<EmploymentData>().Property(e => e.Grade1JobPercent).HasPrecision(18, 2);
            modelBuilder.Entity<EmploymentData>().Property(e => e.Grade1TrainingFundPercent).HasPrecision(18, 2);
            modelBuilder.Entity<EmploymentData>().Property(e => e.Grade1AgeHours).HasPrecision(18, 2);
            modelBuilder.Entity<EmploymentData>().Property(e => e.Grade1MotherBenefitPercent).HasPrecision(18, 2);
            modelBuilder.Entity<EmploymentData>().Property(e => e.Grade1TrainingBenefits).HasPrecision(18, 2);
            modelBuilder.Entity<EmploymentData>().Property(e => e.Grade1DoubleDegree).HasPrecision(18, 2);
            modelBuilder.Entity<EmploymentData>().Property(e => e.Grade2Total).HasPrecision(18, 2);
            modelBuilder.Entity<EmploymentData>().Property(e => e.Grade2JobPercent).HasPrecision(18, 2);
            modelBuilder.Entity<EmploymentData>().Property(e => e.Grade2TrainingFundPercent).HasPrecision(18, 2);
            modelBuilder.Entity<EmploymentData>().Property(e => e.Grade2AgeHours).HasPrecision(18, 2);
            modelBuilder.Entity<EmploymentData>().Property(e => e.Grade2MotherBenefitPercent).HasPrecision(18, 2);
            modelBuilder.Entity<EmploymentData>().Property(e => e.Grade2TrainingBenefits).HasPrecision(18, 2);
            modelBuilder.Entity<EmploymentData>().Property(e => e.Grade2DoubleDegree).HasPrecision(18, 2);
            modelBuilder.Entity<EmploymentDataSlot>().Property(s => s.WeeklyHours).HasPrecision(18, 2);
            modelBuilder.Entity<EmploymentDataSlot>().Property(s => s.JobBase).HasPrecision(18, 2);

            var payrollMonthlyInputBatch = modelBuilder.Entity<PayrollMonthlyInputBatch>();
            payrollMonthlyInputBatch.HasKey(b => b.Id);
            payrollMonthlyInputBatch.Property(b => b.EmployerId).IsRequired();
            payrollMonthlyInputBatch.Property(b => b.AcademicYear).IsRequired();
            payrollMonthlyInputBatch.Property(b => b.Month).IsRequired();
            payrollMonthlyInputBatch.Property(b => b.GregorianYear).IsRequired();
            payrollMonthlyInputBatch.Property(b => b.OriginalFileName).IsRequired();

            payrollMonthlyInputBatch
                .HasOne(b => b.Employer)
                .WithMany()
                .HasForeignKey(b => b.EmployerId)
                .OnDelete(DeleteBehavior.Restrict);

            payrollMonthlyInputBatch
                .HasMany(b => b.Rows)
                .WithOne(r => r.Batch)
                .HasForeignKey(r => r.BatchId)
                .OnDelete(DeleteBehavior.Cascade);

            payrollMonthlyInputBatch
                .HasIndex(b => new { b.EmployerId, b.AcademicYear, b.Month, b.GregorianYear })
                .IsUnique()
                .HasFilter("[פעיל] = 1 AND [נמחק] = 0");

            var payrollMonthlyInputRow = modelBuilder.Entity<PayrollMonthlyInputRow>();
            payrollMonthlyInputRow.HasKey(r => r.Id);
            payrollMonthlyInputRow.Property(r => r.BatchId).IsRequired();
            payrollMonthlyInputRow.Property(r => r.EmployerId).IsRequired();
            payrollMonthlyInputRow.Property(r => r.AcademicYear).IsRequired();
            payrollMonthlyInputRow.Property(r => r.Month).IsRequired();
            payrollMonthlyInputRow.Property(r => r.GregorianYear).IsRequired();

            payrollMonthlyInputRow.HasIndex(r => r.BatchId);
            payrollMonthlyInputRow.HasIndex(r => new { r.EmployerId, r.AcademicYear, r.Month, r.GregorianYear });
            payrollMonthlyInputRow.HasIndex(r => r.IdNumber);
            payrollMonthlyInputRow.HasIndex(r => r.OketzEmployeeNumber);
            payrollMonthlyInputRow.HasIndex(r => r.InstitutionSymbol);

            payrollMonthlyInputRow.Property(r => r.Seniority).HasPrecision(18, 2);
            payrollMonthlyInputRow.Property(r => r.WeeklyHours).HasPrecision(18, 2);
            payrollMonthlyInputRow.Property(r => r.JobBase).HasPrecision(18, 2);
            payrollMonthlyInputRow.Property(r => r.JobPercent).HasPrecision(18, 2);
            payrollMonthlyInputRow.Property(r => r.AgeHours).HasPrecision(18, 2);
            payrollMonthlyInputRow.Property(r => r.TrainingBenefits).HasPrecision(18, 2);
            payrollMonthlyInputRow.Property(r => r.DoubleDegree).HasPrecision(18, 2);
            payrollMonthlyInputRow.Property(r => r.TrainingFund).HasPrecision(18, 2);
            payrollMonthlyInputRow.Property(r => r.GeneralMultiplier).HasPrecision(18, 2);
        }

        public override int SaveChanges()
        {
            ApplyAuditMetadata();
            return base.SaveChanges();
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            ApplyAuditMetadata();
            return await base.SaveChangesAsync(cancellationToken);
        }

        private void ApplyAuditMetadata()
        {
            var now = DateTimeOffset.UtcNow;
            var changedBy = NormalizeChangedBy(_currentUserService?.GetAuditActor());
            var auditEntries = new List<AuditLog>();

            foreach (var entry in ChangeTracker.Entries().Where(e => e.Entity is not AuditLog))
            {
                if (entry.Entity is IAuditableEntity auditable)
                {
                    if (entry.State == EntityState.Added)
                    {
                        auditable.CreatedAtUtc = now;
                        auditable.UpdatedAtUtc = now;
                    }
                    else if (entry.State == EntityState.Modified)
                    {
                        auditable.UpdatedAtUtc = now;
                    }
                }

                if (entry.Entity is ISoftDeletable softDeletable && entry.State == EntityState.Deleted)
                {
                    entry.State = EntityState.Modified;
                    softDeletable.IsDeleted = true;
                    softDeletable.DeletedAtUtc = now;

                    if (entry.Entity is IAuditableEntity softAuditable)
                    {
                        softAuditable.UpdatedAtUtc = now;
                    }
                }

                if (entry.State is EntityState.Added or EntityState.Modified or EntityState.Deleted)
                {
                    var changes = entry.Properties
                        .Where(p => entry.State == EntityState.Added || p.IsModified || entry.State == EntityState.Deleted)
                        .ToDictionary(
                            p => p.Metadata.Name,
                            p => BuildAuditChange(p, entry.State));

                    auditEntries.Add(new AuditLog
                    {
                        EntityName = entry.Metadata.ClrType.Name,
                        Action = entry.State.ToString(),
                        EntityKey = TryGetPrimaryKey(entry),
                        ChangesJson = JsonSerializer.Serialize(changes),
                        ChangedBy = changedBy,
                        ChangedAtUtc = now
                    });
                }
            }

            if (auditEntries.Count > 0)
            {
                AuditLogs.AddRange(auditEntries);
            }
        }

        private static string? TryGetPrimaryKey(Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry entry)
        {
            var key = entry.Properties.FirstOrDefault(p => p.Metadata.IsPrimaryKey());
            return key?.CurrentValue?.ToString() ?? key?.OriginalValue?.ToString();
        }

        private static object BuildAuditChange(
            Microsoft.EntityFrameworkCore.ChangeTracking.PropertyEntry property,
            EntityState state)
        {
            if (IsSensitiveProperty(property.Metadata.Name))
            {
                return new
                {
                    Original = "REDACTED",
                    Current = "REDACTED"
                };
            }

            return new
            {
                Original = state == EntityState.Added ? null : property.OriginalValue,
                Current = state == EntityState.Deleted ? null : property.CurrentValue
            };
        }

        private static bool IsSensitiveProperty(string propertyName) =>
            SensitiveAuditProperties.Contains(propertyName)
            || propertyName.Contains("Password", StringComparison.OrdinalIgnoreCase)
            || propertyName.Contains("Token", StringComparison.OrdinalIgnoreCase);

        private static string NormalizeChangedBy(string? changedBy)
        {
            if (string.IsNullOrWhiteSpace(changedBy))
                return "system";

            var trimmed = changedBy.Trim();
            return trimmed.Length <= 100 ? trimmed : trimmed[..100];
        }
    }
}
