using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using O2Ban.EnterprisePilot.Models;
namespace O2Ban.EnterprisePilot.Controllers;
[Authorize(Roles="Admin")]
public class UsersController:Controller
{
 private readonly UserManager<ApplicationUser> _users; public UsersController(UserManager<ApplicationUser> users)=>_users=users;
 public async Task<IActionResult> Index(){var list=await _users.Users.OrderBy(x=>x.Email).ToListAsync();var rows=new List<UserRow>();foreach(var u in list)rows.Add(new UserRow(u.Email??"",u.DisplayName,(await _users.GetRolesAsync(u)).FirstOrDefault()??"بدون نقش"));return View(rows);}
 [HttpPost,ValidateAntiForgeryToken] public async Task<IActionResult> Create(string email,string displayName,string password,string role){email=(email??"").Trim();displayName=(displayName??"").Trim();if(string.IsNullOrWhiteSpace(email)||string.IsNullOrWhiteSpace(displayName)||string.IsNullOrWhiteSpace(password)||role is not("Admin" or "Procurement")||password.Length<12){TempData["UserError"]="اطلاعات نامعتبر است؛ گذرواژه حداقل ۱۲ نویسه باشد.";return RedirectToAction(nameof(Index));}var u=new ApplicationUser{UserName=email,Email=email,EmailConfirmed=true,DisplayName=displayName,Department="تأمین"};var result=await _users.CreateAsync(u,password);if(!result.Succeeded){TempData["UserError"]=string.Join("؛ ",result.Errors.Select(x=>x.Description));return RedirectToAction(nameof(Index));}await _users.AddToRoleAsync(u,role);TempData["UserSuccess"]="کاربر ایجاد شد؛ گذرواژه را فقط از مسیر امن به او تحویل دهید.";return RedirectToAction(nameof(Index));}
}
public record UserRow(string Email,string DisplayName,string Role);
