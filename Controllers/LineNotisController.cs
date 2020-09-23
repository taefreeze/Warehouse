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
using Warehouse.Models.LineNoti;

namespace Warehouse.Controllers
{
	public class LineNotisController : Controller
	{
		public IActionResult Index()
		{
			return View();
		}
		public IActionResult Authen()
		{
			return Authentication();
		}

		[HttpPost]
		public IActionResult Authentication()
		{
			string redirectedUrl = "https://notify-bot.line.me/oauth/authorize";
			redirectedUrl += "?response_mode=form_post&response_type=code&client_id=QVTkriWSZ7V6lyFAAwj5yQ&redirect_uri=https://localhost:44352/LineNotis/CallBack&scope=notify";
			redirectedUrl += "&state=111," + DateTime.Now.Ticks;

			return Redirect(redirectedUrl);
		}

		[HttpPost]
		public Task<IActionResult> Callback([FromForm] string state, [FromForm] string code)
		{
			string redirectedurl = "https://notify-bot.line.me/oauth/token";
			redirectedurl += "?response_mode=form_post&code=code&grant_type=authorization_code&redirect_uri=https://localhost:44352/LineNotis/CallBack";
			redirectedurl += "client_id=...&client_secret=...";

			return null;
		}

		[HttpPost]
		public async Task lineNotify(LineNotiViewModel text)
		{
			string token = "XGDiguKp1XyTeePNmALSKrrCurwQ0qXacCMMOyg47Ol";
			string msg = text.ToString();
			try
			{
				var client = new HttpClient();
				client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
				var data = new List<KeyValuePair<string, string>>();
				data.Add(new KeyValuePair<string, string>("message", text.msg));
				var httpcontent = new FormUrlEncodedContent(data);
				var response = await client.PostAsync("https://notify-api.line.me/api/notify", httpcontent);
				response.EnsureSuccessStatusCode();
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.ToString());
			}
		}
	}

}
