using Catalog.API.DTOs.Request;
using Catalog.API.Enums;
using Catalog.API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WalletPlatform.Shared.Models;

namespace Catalog.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CatalogController : ControllerBase
{
    private readonly ICatalogService _catalogService;

    public CatalogController(ICatalogService catalogService)
    {
        _catalogService = catalogService;
    }

    /// <summary>Browse catalog — public, no auth required</summary>
    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] string? category = null)
    {
        CatalogItemCategory? cat = null;
        if (!string.IsNullOrWhiteSpace(category) &&
            Enum.TryParse<CatalogItemCategory>(category, true, out var parsed))
            cat = parsed;

        var result = await _catalogService.GetAllAsync(cat);
        return Ok(ApiResponse<object>.Ok(result));
    }

    /// <summary>Get single item details</summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _catalogService.GetByIdAsync(id);
        return Ok(ApiResponse<object>.Ok(result));
    }

    /// <summary>Create catalog item — Admin only</summary>
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create([FromBody] CreateCatalogItemRequestDto dto)
    {
        var result = await _catalogService.CreateAsync(dto);
        return Ok(ApiResponse<object>.Ok(result, "Catalog item created."));
    }

    /// <summary>Deactivate catalog item — Admin only</summary>
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Deactivate(Guid id)
    {
        await _catalogService.DeactivateAsync(id);
        return Ok(ApiResponse<object>.Ok(null, "Catalog item deactivated."));
    }
}