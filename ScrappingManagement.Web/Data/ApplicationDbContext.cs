using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace ScrappingManagement.Web.Data
{
	public class ApplicationDbContext : IdentityDbContext
	{
		private readonly IConfiguration _config;

		public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options, IConfiguration config)
		    : base(options)
		{
			_config = config;
		}


		protected override void OnModelCreating(ModelBuilder builder)
		{
			base.OnModelCreating(builder);

			if (_config.GetValue<string>("Database") == "POSTGRESQL")
				builder.HasDefaultSchema("scpe");
		}
	}
}
