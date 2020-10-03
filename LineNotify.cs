using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Warehouse
{
	public class LineNotify
	{
		public const string TARGET = "line_notify_target";
		public const string TARGET_TYPE = "line_notify_target_type";
		public const string ACCESS_TOKEN = "line_notify_access_token";
		public const string AUTHORIZAION_ENDPOINT = "https://notify-bot.line.me/oauth/authorize";
		public const string TOKEN_ENDPOINT = "https://notify-bot.line.me/oauth/token";
		public const string USERINFO_ENDPOINT = "https://notify-api.line.me/api/status";
		public const string NOTIFY_ENDPOINT = "https://notify-api.line.me/api/notify";
		private readonly IHttpContextAccessor _httpContextAccessor;
		private readonly UserManager<IdentityUser> _userManager;

		public LineNotify(IHttpContextAccessor httpContextAccessor, UserManager<IdentityUser> userManager)
		{
			_httpContextAccessor = httpContextAccessor;
			_userManager = userManager;
		}

		public async Task<bool> SendNotify(string message)
		{
			try
			{
				var user = _httpContextAccessor.HttpContext.User;
				var appUser = await _userManager.GetUserAsync(user);
				var claims = await _userManager.GetClaimsAsync(appUser);
				var accessToken = claims.FirstOrDefault(c => c.Type == ACCESS_TOKEN).Value ?? throw new ArgumentException("No Token");
				var client = new HttpClient();
				client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
				var data = new List<KeyValuePair<string, string>>();
				data.Add(new KeyValuePair<string, string>("message", message));
				var response = await client.PostAsync(NOTIFY_ENDPOINT, new FormUrlEncodedContent(data));
				response.EnsureSuccessStatusCode();
				return true;
				 
			}
			catch (Exception)
			{
				return false;
			}
		}
	}
}
