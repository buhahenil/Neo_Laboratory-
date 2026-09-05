using System.Collections.Generic;

namespace Leb.Core.Entities
{
    public class Package
    {
        public int PackageId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public bool IsActive { get; set; } = true;

        // Tests included in this package
        public List<Test> Tests { get; set; } = new List<Test>();
    }
}
