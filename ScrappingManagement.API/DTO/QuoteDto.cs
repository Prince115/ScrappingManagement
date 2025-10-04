namespace ScrappingManagement.API.DTO;

public class QuoteDetailDto
{
	public int Id { get; set; }
	public DateTime Date { get; set; }
	public string Location { get; set; }
	public int SupplierId { get; set; }
	public double FinalTotal { get; set; }
	public List<QuoteProductDetailDto> QuoteProducts { get; set; }
}

public class QuoteProductDetailDto
{
	public int Id { get; set; }
	public double LoadedWeight { get; set; }
	public double UnloadWeight { get; set; }
	public double Gross { get; set; }
	public double NetWeight { get; set; }
	public double Rate { get; set; }
	public double TotalAmount { get; set; }
	public int ProductId { get; set; }
	public string Name { get; set; }
}
