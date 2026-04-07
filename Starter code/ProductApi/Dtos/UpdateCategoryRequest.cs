namespace ProductApi.Models.Dtos
{
    public class UpdateCategoryRequest
    {
        public string CategoryName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }
}

