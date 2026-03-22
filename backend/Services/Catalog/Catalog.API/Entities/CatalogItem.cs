using Catalog.API.Enums;

namespace Catalog.API.Entities;

public class CatalogItem
{
    public Guid                Id           { get; set; } = Guid.NewGuid();
    public string              Name         { get; set; } = string.Empty;
    public string              Description  { get; set; } = string.Empty;
    public CatalogItemCategory Category     { get; set; }
    public int                 PointsCost   { get; set; }
    public string?             ImageUrl     { get; set; }
    public string?             Brand        { get; set; }
    public int                 StockCount   { get; set; } = -1;  // -1 = unlimited
    public bool                IsActive     { get; set; } = true;
    public DateTime            ValidFrom    { get; set; } = DateTime.UtcNow;
    public DateTime?           ValidUntil   { get; set; }
    public DateTime            CreatedAt    { get; set; } = DateTime.UtcNow;
    public DateTime?           UpdatedAt    { get; set; }

    // Navigation
    public ICollection<Redemption> Redemptions { get; set; } = new List<Redemption>();
}