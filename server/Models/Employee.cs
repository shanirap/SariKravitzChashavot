using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace AccountingProject.Models
{
    [Table("עובדים")]
    [Index(nameof(EmployeeNumber))]
    public class Employee : IAuditableEntity, ISoftDeletable
    {
        [Key][Column("מזהה_עובד")] public int Id { get; set; }
        [Column("מזהה_מעסיק")]     public int EmployerId { get; set; }
        [Column("מספר_עובד")]      public int? EmployeeNumber { get; set; }
        [Required][Column("תז")]   public string IdNumber { get; set; } = string.Empty;
        [Column("שם_פרטי")]        public string? FirstName { get; set; }
        [Column("שם_משפחה")]       public string? LastName { get; set; }
        [Column("תאריך_לידה")]     public DateOnly? BirthDate { get; set; }
        [Column("מין")]            public string? Gender { get; set; }
        [Column("טל")]             public string? Phone { get; set; }

        [Column("תאריך_לידה_ילד_1")]  public DateOnly? ChildBirthDate1  { get; set; }
        [Column("תאריך_לידה_ילד_2")]  public DateOnly? ChildBirthDate2  { get; set; }
        [Column("תאריך_לידה_ילד_3")]  public DateOnly? ChildBirthDate3  { get; set; }
        [Column("תאריך_לידה_ילד_4")]  public DateOnly? ChildBirthDate4  { get; set; }
        [Column("תאריך_לידה_ילד_5")]  public DateOnly? ChildBirthDate5  { get; set; }
        [Column("תאריך_לידה_ילד_6")]  public DateOnly? ChildBirthDate6  { get; set; }
        [Column("תאריך_לידה_ילד_7")]  public DateOnly? ChildBirthDate7  { get; set; }
        [Column("תאריך_לידה_ילד_8")]  public DateOnly? ChildBirthDate8  { get; set; }
        [Column("תאריך_לידה_ילד_9")]  public DateOnly? ChildBirthDate9  { get; set; }
        [Column("תאריך_לידה_ילד_10")] public DateOnly? ChildBirthDate10 { get; set; }
        [Column("סטטוס_פעילות_ידני")] public bool? ManualActiveStatus { get; set; }

        public DateTimeOffset CreatedAtUtc { get; set; }
        public DateTimeOffset UpdatedAtUtc { get; set; }
        public bool IsDeleted { get; set; }
        public DateTimeOffset? DeletedAtUtc { get; set; }

        [ForeignKey("EmployerId")] public Employer? Employer { get; set; }
        public ICollection<EmploymentData> EmploymentData { get; set; } = new List<EmploymentData>();

        [NotMapped] public string FullName => $"{FirstName} {LastName}".Trim();
    }
}
