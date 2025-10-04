namespace ScrappingManagement.Web.Dto;
public class QuoteDetailDto
{
	public int Id { get; set; }
	public DateTime Date { get; set; }
	public string Location { get; set; }
	public string BillNumber { get; set; }
	public string SupplierName { get; set; }
	public decimal FinalTotal { get; set; }
	public decimal Kato { get; set; }
	public decimal Total { get; set; }
	public List<QuoteProductDetailDto> Products { get; set; }
}

public class QuoteProductDetailDto
{
	public int Id { get; set; }
	public string ProductName { get; set; }
	public double LoadedWeight { get; set; }
	public double UnloadWeight { get; set; }
	public double Gross { get; set; }     
      public int? Nos { get; set; }  
	public int BoraCount { get; set; }
	public double BoraReport { get; set; }
	public double ProductReport { get; set; }
	public double NetWeight { get; set; }
	public double Rate { get; set; }
	public double TotalAmount { get; set; }
}