namespace ProductApi.Models
{
    public class Category : BaseEntity
    {
       
        public string Description { get; set; } = string.Empty; 

        public int CategoryId { get; set; }

        public string CategoryName { get; set; } = string.Empty;
    }
}         
