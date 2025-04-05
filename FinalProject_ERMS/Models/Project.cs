namespace FinalProject_ERMS.Models
{
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    public class Project
    {
        [Key]
        public int ProjectId { get; set; }

        [Required]
        public string Name { get; set; }

        public string Description { get; set; }

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        
        [ForeignKey("Manager")]
        public int ManagerId { get; set; }

        public Employee Manager { get; set; }

        // Navigation property
        public ICollection<TaskItem> Tasks { get; set; }
    }
}
