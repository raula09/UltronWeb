namespace ProductWeb.Models.DTO
{
    public class ProductUpdateDto
    {
        public string? Name { get; set; }
        public string? Description { get; set; }
        public string? Size { get; set; }
        public string? Color { get; set; }
        public string? Category { get; set; }
        public string? Material { get; set; }
        public decimal? Price { get; set; }
        public int? Quantity { get; set; }
    }
}
