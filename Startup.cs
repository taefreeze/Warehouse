using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.EntityFrameworkCore;
using Warehouse.Data;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Net.Http;
using System.Text.Json;
using System.Security.Claims;
using Warehouse.Models;

namespace Warehouse
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddHttpContextAccessor();
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseNpgsql(
                    Configuration.GetConnectionString("DefaultConnection")));
            services.AddIdentity<ApplicationUser, IdentityRole>(options => options.SignIn.RequireConfirmedAccount = true)
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();

            services.ConfigureApplicationCookie(options =>
            {
                options.AccessDeniedPath = "/Identity/Account/AccessDenied";
                options.Cookie.Name = "WareHouse";
                options.Cookie.HttpOnly = true;
                options.ExpireTimeSpan = TimeSpan.FromMinutes(60);
                options.LoginPath = "/Identity/Account/Login";
                // ReturnUrlParameter requires 
                //using Microsoft.AspNetCore.Authentication.Cookies;
                options.ReturnUrlParameter = CookieAuthenticationDefaults.ReturnUrlParameter;
                options.SlidingExpiration = true;
            });

            services.AddAuthentication().AddOAuth("LineNotify", "Line Notify", options =>
             {
                 options.ClientId = Configuration["Authentication:LineNotify:ClientID"];
                 options.ClientSecret = Configuration["Authentication:LineNotify:ClientSecret"];
                 options.AuthorizationEndpoint = LineNotify.AUTHORIZAION_ENDPOINT;
                 options.TokenEndpoint = LineNotify.TOKEN_ENDPOINT;
                 options.UserInformationEndpoint = LineNotify.USERINFO_ENDPOINT;
                 options.CallbackPath = new Microsoft.AspNetCore.Http.PathString("/notify");
                 options.Scope.Add("notify");
                 options.ClaimActions.MapJsonKey(LineNotify.TARGET, "target");
                 options.ClaimActions.MapJsonKey(LineNotify.TARGET_TYPE, "targetType");

                 options.Events = new Microsoft.AspNetCore.Authentication.OAuth.OAuthEvents
                 {
                     OnCreatingTicket = async context =>
                     {
                         var request = new HttpRequestMessage(HttpMethod.Get, context.Options.UserInformationEndpoint);
                         request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", context.AccessToken);
                         request.Headers.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

                         var response = await context.Backchannel.SendAsync(request, context.HttpContext.RequestAborted);
                         response.EnsureSuccessStatusCode();

                         var json = await response.Content.ReadAsStringAsync();
                         var user = JsonDocument.Parse(json).RootElement;
                         var userId = context.Properties.Items["XsrfId"];

                         context.RunClaimActions(user);
                         var identity = new ClaimsIdentity();
                         identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, userId));
                         identity.AddClaim(new Claim(LineNotify.ACCESS_TOKEN, context.AccessToken));
                         context.Principal.AddIdentity(identity);
                     },
                     OnRemoteFailure = context =>
                     {
                         context.Response.Redirect("/");
                         context.HandleResponse();
                         return Task.CompletedTask;
                     }
                 };
             });
            services.AddTransient<LineNotify>();
            services.AddControllersWithViews();
            services.AddRazorPages();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IServiceProvider service)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseDatabaseErrorPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            SeedData(service).Wait();
            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Products}/{action=Warning}/{id?}");
                endpoints.MapRazorPages();
            });
        }

        private async Task SeedData(IServiceProvider service)
		{
            var roleManager = service.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = service.GetRequiredService<UserManager<ApplicationUser>>();
            var dbContext = service.GetRequiredService<ApplicationDbContext>();
            if (!await roleManager.RoleExistsAsync("User"))
			{
                await roleManager.CreateAsync(new IdentityRole("User"));
			}
            if (!await roleManager.RoleExistsAsync("Staff"))
            {
                await roleManager.CreateAsync(new IdentityRole("Staff"));
            }
        }
    }
}
