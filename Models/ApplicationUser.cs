using Microsoft.AspNetCore.Identity;

namespace O2Ban.EnterprisePilot.Models;

public class ApplicationUser : IdentityUser
{
    public string DisplayName { get; set; } = "";
    public string Department { get; set; } = "تأمین";
}
