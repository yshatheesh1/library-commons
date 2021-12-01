using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace BBCoders.Commons.Tools.IntegrationTests.Context
{
    [Table("Actions")]
    [Index(nameof(ActionId), IsUnique = true)]
    public class Action
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        public long Id { get; set; }

        [Required]
        public Guid ActionId { get; set; }

        [Required]
        [MaxLength(250)]
        public string Name { get; set; }
    }

    [Table("Fingerprint")]
    [Index(nameof(FingerprintId), IsUnique = true)]
    public class Fingerprint
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        public long Id { get; set; }

        [Required]
        public Guid FingerprintId { get; set; }

        [Required]
        public long NmlsId { get; set; }

        [Required]
        public long StateId { get; set; }

        [Required]
        [Column(TypeName = "datetime")]
        public DateTime CreatedDate { get; set; }

        [Required]
        [Column(TypeName = "datetime")]
        public DateTime UpdatedDate { get; set; }

        [Required]
        public long CreatedById { get; set; }

        [Required]
        public long LastUpdatedById { get; set; }

        [Column(TypeName = "datetime")]
        public DateTime? ExpirationDate { get; set; }

        [Column(TypeName = "datetime")]
        public DateTime? RenewalDate { get; set; }

        public bool IsActive { get; set; }

        [ForeignKey("StateId")]
        public State State { get; set; }
    }



    [Table("States")]
    [Index(nameof(StateId), IsUnique = true)]
    public class State
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }
        public Guid StateId { get; set; }
        public string Name { get; set; }
    }

    [Table("Schedules")]
    [Index(nameof(ScheduleId), IsUnique = true)]
    public class Schedule
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        [Required]
        public Guid ScheduleId { get; set; }

        public long? FingerPrintId { get; set; }

        [Required]
        [Column(TypeName = "datetime")]
        public DateTime ScheduleDate { get; set; }

        public long ScheduleSiteId { get; set; }

        public long? ActionId { get; set; }

        [Required]
        [Column(TypeName = "datetime")]
        public DateTime CreatedDate { get; set; }

        [Required]
        public long CreatedById { get; set; }

        [Required]
        [Column(TypeName = "datetime")]
        public DateTime LastUpdatedDate { get; set; }

        [Required]
        public long LastUpdatedById { get; set; }


        [ForeignKey("FingerPrintId")]
        public Fingerprint Fingerprint { get; set; }

        [ForeignKey("ScheduleSiteId")]
        public ScheduleSite scheduleSite { get; set; }

        [ForeignKey("ActionId")]
        public Action Action { get; set; }
    }

    [Table("ScheduleSites")]
    [Index(nameof(ScheduleSiteId), IsUnique = true)]
    public class ScheduleSite
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        [Required]
        public Guid ScheduleSiteId { get; set; }

        [Required]
        [MaxLength(200)]
        public string Name { get; set; }

        public bool IsActive { get; set; }

    }
}