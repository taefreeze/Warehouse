using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Warehouse.Models;
using Warehouse.ViewModels;

namespace Warehouse.Controllers
{
	public class AdminController : Controller
	{
		private readonly RoleManager<IdentityRole> _roleManager;
		private readonly UserManager<ApplicationUser> _userManager;

		public AdminController(RoleManager<IdentityRole> roleManager, UserManager<ApplicationUser> userManager)
		{
			_roleManager = roleManager;
			_userManager = userManager;
		}
		public IActionResult Index()
		{
			return View();
		}

		[HttpGet]
		public IActionResult ListUsers()
		{
			var users = _userManager.Users;
			return View(users);
		}
		[HttpPost]
		public async Task<IActionResult> Give(string Id)
		{
			var user = await _userManager.FindByIdAsync(Id);
			await _userManager.AddToRoleAsync(user, "Staff");
			return RedirectToAction("ListUsers");
		}

		[HttpGet]
		public IActionResult ListRoles()
		{
			var roles = _roleManager.Roles;
			return View(roles);
		}
		[HttpGet]
		public async Task<IActionResult> EditRole(string roleId)
		{
			ViewBag.roleId = roleId;

			var role = await _roleManager.FindByIdAsync(roleId);

			if (role == null)
			{
				ViewBag.ErrorMessage = $"Role with Id = {roleId} cannot be found";
				return View("NotFound");
			}

			var model = new List<UserRolesViewModels>();

			foreach (var user in _userManager.Users)
			{
				var userRoleViewModel = new UserRolesViewModels
				{
					UserId = user.Id,
					UserName = user.UserName
				};

				if (await _userManager.IsInRoleAsync(user, role.Name))
				{
					userRoleViewModel.IsSelected = true;
				}
				else
				{
					userRoleViewModel.IsSelected = false;
				}

				model.Add(userRoleViewModel);
			}

			return View(model);
		}
		[HttpPost]
		public async Task<IActionResult> EditRole(List<UserRolesViewModels> model, string roleId)
		{
			var role = await _roleManager.FindByIdAsync(roleId);

			if (role == null)
			{
				ViewBag.ErrorMessage = $"Role with Id = {roleId} cannot be found";
				return View("NotFound");
			}

			for (int i = 0; i < model.Count; i++)
			{
				var user = await _userManager.FindByIdAsync(model[i].UserId);

				IdentityResult result = null;

				if (model[i].IsSelected && !(await _userManager.IsInRoleAsync(user, role.Name)))
				{
					result = await _userManager.AddToRoleAsync(user, role.Name);
				}
				else if (!model[i].IsSelected && await _userManager.IsInRoleAsync(user, role.Name))
				{
					result = await _userManager.RemoveFromRoleAsync(user, role.Name);
				}
				else
				{
					continue;
				}

				if (result.Succeeded)
				{
					if (i < (model.Count - 1))
						continue;
					else
						return RedirectToAction("EditRole", new { Id = roleId });
				}
			}

			return RedirectToAction("EditRole", new { Id = roleId });
		}
	}
}
