namespace Leb.Core.Entities
{
    public class Test
    {
        public int TestId { get; set; }
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty; // Joined field
        public string Name { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public string SampleType { get; set; } = string.Empty;
        public string? Preparation { get; set; }
        public string? NormalRange { get; set; }
        public int DeliveryTimeHours { get; set; } = 24;
        public bool IsActive { get; set; } = true;
    }
}
