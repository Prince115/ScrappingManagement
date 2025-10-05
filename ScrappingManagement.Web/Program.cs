using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ScrappingManagement.Web.Data;
using ScrappingManagement.Web.Services;

namespace ScrappingManagement.Web
{
	public class Program
	{
		public static void Main(string[] args)
		{
			var builder = WebApplication.CreateBuilder(args);

			// Add services to the container.
			builder.Services.AddControllersWithViews();
			builder.Services.AddControllers();
			builder.Services.AddEndpointsApiExplorer();
			builder.Services.AddSwaggerGen();

			builder.Services.AddTransient<WhatsAppInvoiceService>();

			// Configure AppDbContext
			var useInMemory = builder.Configuration.GetValue<bool>("UseInMemoryDatabase");

			if (useInMemory)
			{
				builder.Services.AddDbContext<AppDbContext>(options =>
					options.UseInMemoryDatabase("ScrappingManagementDb"));
				builder.Services.AddDbContext<ApplicationDbContext>(options =>
					options.UseInMemoryDatabase("IdentityDb"));
			}
			else
			{
				builder.Services.AddDbContext<AppDbContext>(options =>
				    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
				builder.Services.AddDbContext<ApplicationDbContext>(options =>
				    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
			}

			// Add ASP.NET Core Identity with roles
			builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
				.AddRoles<IdentityRole>() // Add this line to enable roles
				.AddEntityFrameworkStores<ApplicationDbContext>();

			var app = builder.Build();

			// Seed roles
			using (var scope = app.Services.CreateScope())
			{
				var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
				var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();

				Task.Run(async () =>
				{
					string[] roleNames = { "Admin", "User" };
					foreach (var roleName in roleNames)
					{
						if (!await roleManager.RoleExistsAsync(roleName))
						{
							await roleManager.CreateAsync(new IdentityRole(roleName));
						}
					}

					// Create a default admin user if one doesn't exist and assign role
					var adminUser = await userManager.FindByEmailAsync("admin@ambitinfoway.com");
					if (adminUser == null)
					{
						adminUser = new IdentityUser { UserName = "admin@ambitinfoway.com", Email = "admin@ambitinfoway.com", EmailConfirmed = true };
						await userManager.CreateAsync(adminUser, "Ambit@1234");  
						await userManager.AddToRoleAsync(adminUser, "Admin");
					}
				}).Wait(); 
			}

			if (true || app.Environment.IsDevelopment())
			{
				app.UseSwagger();
				app.UseSwaggerUI(options =>
				{
					options.SwaggerEndpoint("/openapi/v1.json", "v1");
				});
			}
			if (!app.Environment.IsDevelopment())
			{
				app.UseExceptionHandler("/Home/Error");
				app.UseHsts();
			}

			app.UseHttpsRedirection();
			app.UseStaticFiles();

			app.UseRouting();

			app.UseAuthentication();
			app.UseAuthorization();

			app.MapRazorPages();

			app.MapControllerRoute(
			    name: "default",
			    pattern: "{controller=Quotes}/{action=Index}/{id?}");

			app.Run();
		}
	}
}
