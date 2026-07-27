namespace BusTracking.Common.Models
{
    public class BroadcastModel
    {
        [Required(ErrorMessage = "Please select a target role.")]
        [Display(Name = "Target Role")]
        public int? SelectedRoleId { get; set; }

        [Required(ErrorMessage = "Please select at least one recipient user.")]
        public List<int> SelectedUserIds { get; set; } = [];

        [Required(ErrorMessage = "Notification Title is required.")]
        [MaxLength(150, ErrorMessage = "Title cannot exceed 150 characters.")]
        [Display(Name = "Message Title")]
        public string Title { get; set; } = "";

        [Required(ErrorMessage = "Notification Body is required.")]
        [MaxLength(1000, ErrorMessage = "Message body cannot exceed 1000 characters.")]
        [Display(Name = "Message Content")]
        public string Body { get; set; } = "";

        [Display(Name = "Notification Category")]
        public string NotificationType { get; set; } = "Broadcast";

        public List<SelectListItem> Roles { get; set; } = [];
    }
}
