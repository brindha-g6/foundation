using Foundation.Infrastructure.Cms.Users;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Foundation.Features.Api
{
    [Route("myaccount/user-role")]
    public class UserRoleController : Controller
    {
        private readonly UserManager<SiteUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public UserRoleController(UserManager<SiteUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        [HttpPost("approve")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Approve(string email)
        {
            return await ModifyRole(email, "ApprovedUser", add: true);
        }

        [HttpPost("reject")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> rejectUser(string email)
        {
            //await ModifyRole(email, "RejectedUser", add: true);
            return await ModifyRole(email, "RejectedUser", add: false);
        }

        private async Task<IActionResult> ModifyRole(string email, string role, bool add)
        {
            if (string.IsNullOrWhiteSpace(email))
                return BadRequest("Invalid email");

            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
                return NotFound("User not found");

            // Ensure role exists
            if (!await _roleManager.RoleExistsAsync(role))
            {
                var createResult = await _roleManager.CreateAsync(new IdentityRole(role));
                if (!createResult.Succeeded)
                    return StatusCode(500, "Failed to create role.");
            }

            IdentityResult result;
            if (add)
                result = await _userManager.AddToRoleAsync(user, role);
            else
                result = await _userManager.RemoveFromRoleAsync(user, role);

            if (!result.Succeeded)
            {
                var errorMessages = string.Join("; ", result.Errors.Select(e => e.Description));
                return StatusCode(500, $"Role update failed: {errorMessages}");
            }

            return Ok();
        }
    }
}
