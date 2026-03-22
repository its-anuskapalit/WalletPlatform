namespace Catalog.API.DTOs.Request;

public class CreateCatalogItemRequestDto
{
    public string   Name        { get; set; } = string.Empty;
    public string   Description { get; set; } = string.Empty;
    public string   Category    { get; set; } = string.Empty;
    public int      PointsCost  { get; set; }
    public string?  ImageUrl    { get; set; }
    public string?  Brand       { get; set; }
    public int      StockCount  { get; set; } = -1;
    public DateTime ValidFrom   { get; set; } = DateTime.UtcNow;
    public DateTime? ValidUntil { get; set; }
}