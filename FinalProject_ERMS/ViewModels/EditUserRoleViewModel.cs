namespace FinalProject_ERMS.ViewModels
{
    public class EditUserRoleViewModel
    {
        public string UserId { get; set; }
        public string Email { get; set; }
        public string SelectedRole { get; set; }
        public List<string> AvailableRoles { get; set; }
    }
}
