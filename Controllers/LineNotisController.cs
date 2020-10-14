using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Warehouse.Models;
namespace Warehouse.Controllers
{
    [Authorize]
    public class LineNotisController : Controller
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly LineNotify _lineNotify;
        public LineNotisController(SignInManager<ApplicationUser> signInManager, UserManager<ApplicationUser> userManager, LineNotify lineNotify)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _lineNotify = lineNotify;
        }
        public IActionResult Index()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Index(string message)
        {
            await _lineNotify.SendNotify(message);
            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult LineNotify(string provider)
        {
            var redirectUrl = Url.Action("LineNotifyCallback", "Linenotis");
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var properties = _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl, userId);
            return Challenge(properties, provider);
        }
        [HttpGet]
        public async Task<IActionResult> LineNotifyCallback(string remoteError = null)
        {
            if (!string.IsNullOrEmpty(remoteError))
            {
                ModelState.AddModelError(string.Empty, "Error");
                return View("Index");
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var info = await _signInManager.GetExternalLoginInfoAsync(userId);
            if (info == null)
            {
                return RedirectToAction("Index");
            }

            var user = await _userManager.GetUserAsync(User);
            await _userManager.RemoveClaimsAsync(user, info.Principal.Claims);
            foreach (var claim in info.Principal.Claims)
            {
                await _userManager.AddClaimAsync(user, claim);
            }

            return RedirectToAction("Index");
        }
    }

}
