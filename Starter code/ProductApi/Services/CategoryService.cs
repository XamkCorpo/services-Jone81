using Microsoft.EntityFrameworkCore;
using ProductApi.Common;
using ProductApi.Data;
using ProductApi.Mappings;
using ProductApi.Models;
using ProductApi.Models.Dtos;

namespace ProductApi.Services;

public class CategoryService : ICategoryService
{
    private readonly AppDbContext _context;
    private readonly ILogger<CategoryService> _logger;

    public CategoryService(AppDbContext context, ILogger<CategoryService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Result<List<CategoryResponse>>> GetAllAsync()
    {
        try
        {
            List<Category> categories = await _context.Categories.ToListAsync();
            List<CategoryResponse> response = categories.Select(c => c.ToResponse()).ToList();
            return Result.Success(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Virhe kategorioiden haussa");
            return Result.Failure<List<CategoryResponse>>("Kategorioiden haku epäonnistui");
        }
    }

    public async Task<Result<CategoryResponse>> GetByIdAsync(int id)
    {
        try
        {
            Category? category = await _context.Categories.FindAsync(id);

            if (category == null)
                return Result.Failure<CategoryResponse>($"Kategoriaa {id} ei löytynyt");

            return Result.Success(category.ToResponse());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Virhe kategorian haussa: {CategoryId}", id);
            return Result.Failure<CategoryResponse>("Kategorian haku epäonnistui");
        }
    }

    public async Task<Result<CategoryResponse>> CreateAsync(CategoryRequest request)
    {
        try
        {
            Category category = request.ToEntity();
            _context.Categories.Add(category);
            await _context.SaveChangesAsync();
            return Result.Success(category.ToResponse());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Virhe kategorian luomisessa: {CategoryName}", request.CategoryName);
            return Result.Failure<CategoryResponse>("Kategorian luominen epäonnistui");
        }
    }

    public async Task<Result<CategoryResponse>> UpdateAsync(int id, UpdateCategoryRequest request)
    {
        try
        {
            Category? existing = await _context.Categories.FindAsync(id);

            if (existing == null)
                return Result.Failure<CategoryResponse>($"Kategoriaa {id} ei löytynyt");

            existing.UpdateEntity(request);
            await _context.SaveChangesAsync();
            return Result.Success(existing.ToResponse());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Virhe kategorian päivittämisessä: {CategoryId}", id);
            return Result.Failure<CategoryResponse>("Kategorian päivittäminen epäonnistui");
        }
    }

    public async Task<Result> DeleteAsync(int id)
    {
        try
        {
            Category? category = await _context.Categories.FindAsync(id);

            if (category == null)
                return Result.Failure($"Kategoriaa {id} ei löytynyt");

            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Virhe kategorian poistamisessa: {CategoryId}", id);
            return Result.Failure("Kategorian poistaminen epäonnistui");
        }
    }
}
