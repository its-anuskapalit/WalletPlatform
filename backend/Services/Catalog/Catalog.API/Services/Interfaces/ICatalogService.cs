using Catalog.API.DTOs.Request;
using Catalog.API.DTOs.Response;
using Catalog.API.Enums;

namespace Catalog.API.Services.Interfaces;

public interface ICatalogService
{
    Task<List<CatalogItemResponseDto>> GetAllAsync(CatalogItemCategory? category);
    Task<CatalogItemResponseDto>       GetByIdAsync(Guid id);
    Task<CatalogItemResponseDto>       CreateAsync(CreateCatalogItemRequestDto dto);
    Task                               DeactivateAsync(Guid id);
}