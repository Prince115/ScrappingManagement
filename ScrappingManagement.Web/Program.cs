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

			// Load config
			var useInMemory = builder.Configuration.GetValue<bool>("UseInMemoryDatabase");

			if (useInMemory)
			{
				builder.Services.AddDbContext<AppDbContext>(options =>
					options.UseInMemoryDatabase("ScrappingManagementDb"));
			}
			else
			{
				builder.Services.AddDbContext<AppDbContext>(options =>
				    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
			}
			var app = builder.Build();

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

			app.UseAuthorization();

			app.MapControllerRoute(
			    name: "default",
			    pattern: "{controller=Quotes}/{action=Index}/{id?}");

			app.Run();
		}
	}
}
