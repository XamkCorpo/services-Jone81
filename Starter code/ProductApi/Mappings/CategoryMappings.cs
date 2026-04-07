using ProductApi.Models;
using ProductApi.Models.Dtos;

namespace ProductApi.Mappings
{
    public static class CategoryMappings
    {
        // Muuttaa tietokanta-entiteetin palautettavaksi DTO:ksi
        public static CategoryResponse ToResponse(this Category category)
        {
            return new CategoryResponse
            {
                CategoryId = category.CategoryId,
                CategoryName = category.CategoryName,
                Description = category.Description
            };
        }

        // Muuttaa luontipyynnön tietokanta-entiteetiksi
        public static Category ToEntity(this CategoryRequest request)
        {
            return new Category
            {
                CategoryName = request.CategoryName,
                Description = request.Description
            };
        }

        // Päivittää olemassa olevaa entiteettiä pyynnön tiedoilla
        public static void UpdateEntity(this Category category, UpdateCategoryRequest request)
        {
            category.CategoryName = request.CategoryName;
            category.Description = request.Description;
        }
    }
}
