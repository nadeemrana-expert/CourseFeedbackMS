using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CourseFeedbackMS.CourseFeedback
{
    public class Feedback : FullAuditedEntity, IMustHaveTenant
    {
        public int TenantId { get; set; }

        [Required]
        [MaxLength(200)]
        public string StudentName { get; set; }

        [Required]
        public int CourseId { get; set; }

        [ForeignKey("CourseId")]
        public virtual Course Course { get; set; }

        [MaxLength(2000)]
        public string Comment { get; set; }

        [Range(1, 5)]
        public int Rating { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        // File upload
        [MaxLength(500)]
        public string AttachmentPath { get; set; }

        [MaxLength(200)]
        public string AttachmentFileName { get; set; }
    }
}
