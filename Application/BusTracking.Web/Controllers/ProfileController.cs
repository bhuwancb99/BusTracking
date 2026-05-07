using BusTracking.Common.DTOs.User;
using BusTracking.Common.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BusTracking.Web.Controllers
{
    [Authorize]
    public class ProfileController : BaseController
    {
        private readonly IUserService _user;
        public ProfileController(IUserService user) => _user = user;

        public async Task<IActionResult> Index()
        {
            var r = await _user.GetProfileAsync(CurrentUserId);
            if (!r.Success) return NotFound();
            return View(r.Data);
        }

        [HttpGet]
        public async Task<IActionResult> Edit()
        {
            var r = await _user.GetProfileAsync(CurrentUserId);
            if (!r.Success) return NotFound();
            return View(new UpdateProfileDto
            {
                FullName = r.Data!.FullName,
                PhoneNumber = r.Data.PhoneNumber
            });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(UpdateProfileDto model)
        {
            if (!ModelState.IsValid) return View(model);
            var r = await _user.UpdateProfileAsync(CurrentUserId, model);
            if (!r.Success) { ModelState.AddModelError("", r.Message); return View(model); }
            TempData["SuccessMessage"] = r.Message;
            return RedirectToAction(nameof(Index));
        }
    }
}
