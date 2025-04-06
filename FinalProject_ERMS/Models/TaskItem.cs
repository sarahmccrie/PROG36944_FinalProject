using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FinalProject_ERMS.Models
{   
    public class TaskItem
    {
        [Key]
        public int TaskId { get; set; }

        [Required]
        public string Name { get; set; }

        public string Description { get; set; }

        [Required]
        [ForeignKey("Project")]
        public int ProjectId { get; set; }
        public Project Project { get; set; }

        [Required]
        [ForeignKey("AssignedEmployee")]
        public int AssignedEmployeeId { get; set; }
        public Employee AssignedEmployee { get; set; }

        [Required]
        public string Priority { get; set; }

        [Required]
        public string Status { get; set; }
    }
}
