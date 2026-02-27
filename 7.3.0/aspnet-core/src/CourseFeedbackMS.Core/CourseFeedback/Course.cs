using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CourseFeedbackMS.CourseFeedback
{
    public class Course : FullAuditedEntity, IMustHaveTenant
    {
        public int TenantId { get; set; }

        [Required]
        [MaxLength(200)]
        public string CourseName { get; set; }

        [Required]
        [MaxLength(200)]
        public string InstructorName { get; set; }

        public bool IsActive { get; set; } = true;

        // Navigation property
        public virtual ICollection<Feedback> Feedbacks { get; set; }
    }
}
