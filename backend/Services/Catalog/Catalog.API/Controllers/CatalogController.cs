using Catalog.API.DTOs.Request;
using Catalog.API.Enums;
using Catalog.API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WalletPlatform.Shared.Models;

namespace Catalog.API.Controllers;

// Marks this class as an API controller (auto model validation, binding, etc.)
[ApiController]

// Base route: api/catalog
[Route("api/[controller]")]
public class CatalogController : ControllerBase
{
    // Service layer dependency for catalog operations
    private readonly ICatalogService _catalogService;

    // Constructor for dependency injection
    public CatalogController(ICatalogService catalogService)
    {
        _catalogService = catalogService;
    }

    /// <summary>
    /// Retrieves all catalog items (public endpoint, no authentication required)
    /// Optional filtering by category
    /// </summary>
    /// <param name="category">Category filter (string → enum conversion)</param>
    /// <returns>List of catalog items</returns>
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] string? category = null)
    {
        // Convert string category to enum safely (case-insensitive)
        CatalogItemCategory? cat = null;
        if (!string.IsNullOrWhiteSpace(category) &&
            Enum.TryParse<CatalogItemCategory>(category, true, out var parsed))
            cat = parsed;

        // Fetch catalog items (filtered or all)
        var result = await _catalogService.GetAllAsync(cat);

        // Return standardized response
        return Ok(ApiResponse<object>.Ok(result));
    }

    /// <summary>
    /// Retrieves details of a single catalog item by ID
    /// </summary>
    /// <param name="id">Unique identifier of the catalog item</param>
    /// <returns>Catalog item details</returns>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        // Fetch item from service layer
        var result = await _catalogService.GetByIdAsync(id);

        // Return response
        return Ok(ApiResponse<object>.Ok(result));
    }

    /// <summary>
    /// Creates a new catalog item (restricted to Admin users only)
    /// </summary>
    /// <param name="dto">Catalog item creation data</param>
    /// <returns>Created catalog item</returns>
    [HttpPost]
    [Authorize(Roles = "Admin")] // Role-based authorization
    public async Task<IActionResult> Create([FromBody] CreateCatalogItemRequestDto dto)
    {
        // Call service to create new catalog item
        var result = await _catalogService.CreateAsync(dto);

        // Return success response
        return Ok(ApiResponse<object>.Ok(result, "Catalog item created."));
    }

    /// <summary>
    /// Deactivates a catalog item (soft delete) — Admin only
    /// </summary>
    /// <param name="id">Catalog item ID</param>
    /// <returns>Confirmation message</returns>
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")] // Only admins can deactivate items
    public async Task<IActionResult> Deactivate(Guid id)
    {
        // Calls service to deactivate item instead of permanent deletion
        await _catalogService.DeactivateAsync(id);

        // Return success message
        return Ok(ApiResponse<object>.Ok(null, "Catalog item deactivated."));
    }
}