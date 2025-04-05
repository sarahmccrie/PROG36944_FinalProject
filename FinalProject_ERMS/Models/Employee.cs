namespace FinalProject_ERMS.Models {
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public class Employee {
    [Key]
    public int EmployeeId { get; set; }

    [Required]
    public string Name { get; set; }

    [Required, EmailAddress]
    public string Email { get; set; }

    [Required]
    public string Role { get; set; }
    public ICollection<Project> ManagedProjects { get; set; }
    public ICollection<TaskItem> AssignedTasks { get; set; }
} }
