namespace ScrappingManagement.Web.Models;

public class Customer
{
	public int Id { get; set; }
	public string? Name { get; set; }
	public string? Location { get; set; }
	public string? ContactNo { get; set; }
	public decimal? OpeningBalance { get; set; } = 0m;
}
