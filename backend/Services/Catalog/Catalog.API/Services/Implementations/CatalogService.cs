using Catalog.API.DTOs.Request;
using Catalog.API.DTOs.Response;
using Catalog.API.Entities;
using Catalog.API.Enums;
using Catalog.API.Repositories.Interfaces;
using Catalog.API.Services.Interfaces;

namespace Catalog.API.Services.Implementations;

public class CatalogService : ICatalogService
{
    private readonly ICatalogRepository _catalogRepo;
    private readonly ILogger<CatalogService> _logger;

    public CatalogService(ICatalogRepository catalogRepo, ILogger<CatalogService> logger)
    {
        _catalogRepo = catalogRepo;
        _logger      = logger;
    }

    public async Task<List<CatalogItemResponseDto>> GetAllAsync(CatalogItemCategory? category)
    {
        var items = await _catalogRepo.GetAllActiveAsync(category);
        return items.Select(MapToResponse).ToList();
    }

    public async Task<CatalogItemResponseDto> GetByIdAsync(Guid id)
    {
        var item = await _catalogRepo.GetByIdAsync(id)
            ?? throw new KeyNotFoundException("Catalog item not found.");

        return MapToResponse(item);
    }

    public async Task<CatalogItemResponseDto> CreateAsync(CreateCatalogItemRequestDto dto)
    {
        if (!Enum.TryParse<CatalogItemCategory>(dto.Category, out var category))
            throw new InvalidOperationException($"Invalid category: {dto.Category}");

        if (dto.PointsCost <= 0)
            throw new InvalidOperationException("Points cost must be greater than zero.");

        var item = new CatalogItem
        {
            Name        = dto.Name.Trim(),
            Description = dto.Description.Trim(),
            Category    = category,
            PointsCost  = dto.PointsCost,
            ImageUrl    = dto.ImageUrl,
            Brand       = dto.Brand?.Trim(),
            StockCount  = dto.StockCount,
            ValidFrom   = dto.ValidFrom,
            ValidUntil  = dto.ValidUntil,
            IsActive    = true
        };

        var created = await _catalogRepo.CreateAsync(item);
        _logger.LogInformation("Catalog item created: {Name} | {Points} pts", item.Name, item.PointsCost);
        return MapToResponse(created);
    }

    public async Task DeactivateAsync(Guid id) =>
        await _catalogRepo.DeactivateAsync(id);

    private static CatalogItemResponseDto MapToResponse(CatalogItem c) => new()
    {
        Id          = c.Id,
        Name        = c.Name,
        Description = c.Description,
        Category    = c.Category.ToString(),
        PointsCost  = c.PointsCost,
        ImageUrl    = c.ImageUrl,
        Brand       = c.Brand,
        StockCount  = c.StockCount,
        IsActive    = c.IsActive,
        ValidFrom   = c.ValidFrom,
        ValidUntil  = c.ValidUntil
    };
}