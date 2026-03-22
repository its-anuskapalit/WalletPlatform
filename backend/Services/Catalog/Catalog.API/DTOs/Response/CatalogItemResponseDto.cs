namespace Catalog.API.DTOs.Response;

public class CatalogItemResponseDto
{
    public Guid      Id          { get; set; }
    public string    Name        { get; set; } = string.Empty;
    public string    Description { get; set; } = string.Empty;
    public string    Category    { get; set; } = string.Empty;
    public int       PointsCost  { get; set; }
    public string?   ImageUrl    { get; set; }
    public string?   Brand       { get; set; }
    public int       StockCount  { get; set; }
    public bool      IsActive    { get; set; }
    public DateTime  ValidFrom   { get; set; }
    public DateTime? ValidUntil  { get; set; }
}