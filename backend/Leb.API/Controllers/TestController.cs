using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Leb.Core.Entities;
using Leb.Core.Interfaces;

namespace Leb.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TestController : ControllerBase
    {
        private readonly ITestRepository _testRepository;
        private readonly ITestCategoryRepository _categoryRepository;
        private readonly IPackageRepository _packageRepository;
        private readonly IAuditLogRepository _auditLogRepository;

        public TestController(
            ITestRepository testRepository,
            ITestCategoryRepository categoryRepository,
            IPackageRepository packageRepository,
            IAuditLogRepository auditLogRepository)
        {
            _testRepository = testRepository;
            _categoryRepository = categoryRepository;
            _packageRepository = packageRepository;
            _auditLogRepository = auditLogRepository;
        }

        // --- TESTS CRUD ---
        [HttpGet]
        public async Task<IActionResult> GetTests()
        {
            var list = await _testRepository.GetAllTestsAsync();
            return Ok(list);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetTestById(int id)
        {
            var test = await _testRepository.GetTestByIdAsync(id);
            if (test == null) return NotFound(new { Message = "Test not found." });
            return Ok(test);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> CreateTest([FromBody] Test test)
        {
            int id = await _testRepository.CreateTestAsync(test);

            await _auditLogRepository.CreateAuditLogAsync(new AuditLog
            {
                Action = "CREATE",
                TableName = "Tests",
                RecordId = id,
                NewValues = $"Name: {test.Name}, Price: {test.Price}"
            });

            return Ok(new { Message = "Test created successfully.", TestId = id });
        }

        [Authorize(Roles = "Admin")]
        [HttpPut]
        public async Task<IActionResult> UpdateTest([FromBody] Test test)
        {
            await _testRepository.UpdateTestAsync(test);

            await _auditLogRepository.CreateAuditLogAsync(new AuditLog
            {
                Action = "UPDATE",
                TableName = "Tests",
                RecordId = test.TestId,
                NewValues = $"Name: {test.Name}, Price: {test.Price}, IsActive: {test.IsActive}"
            });

            return Ok(new { Message = "Test updated successfully." });
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTest(int id)
        {
            await _testRepository.DeleteTestAsync(id);

            await _auditLogRepository.CreateAuditLogAsync(new AuditLog
            {
                Action = "DELETE",
                TableName = "Tests",
                RecordId = id
            });

            return Ok(new { Message = "Test deleted successfully." });
        }

        // --- CATEGORIES CRUD ---
        [HttpGet("categories")]
        public async Task<IActionResult> GetCategories()
        {
            var list = await _categoryRepository.GetAllCategoriesAsync();
            return Ok(list);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("categories")]
        public async Task<IActionResult> CreateCategory([FromBody] TestCategory category)
        {
            int id = await _categoryRepository.CreateCategoryAsync(category);
            return Ok(new { Message = "Category created successfully.", CategoryId = id });
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("categories")]
        public async Task<IActionResult> UpdateCategory([FromBody] TestCategory category)
        {
            await _categoryRepository.UpdateCategoryAsync(category);
            return Ok(new { Message = "Category updated successfully." });
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("categories/{id}")]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            await _categoryRepository.DeleteCategoryAsync(id);
            return Ok(new { Message = "Category deleted successfully." });
        }

        // --- PACKAGES CRUD ---
        [HttpGet("packages")]
        public async Task<IActionResult> GetPackages()
        {
            var list = await _packageRepository.GetAllPackagesAsync();
            // Fetch tests for each package to populate details
            foreach (var pkg in list)
            {
                var tests = await _packageRepository.GetPackageTestsAsync(pkg.PackageId);
                pkg.Tests.AddRange(tests);
            }
            return Ok(list);
        }

        [HttpGet("packages/{id}")]
        public async Task<IActionResult> GetPackageById(int id)
        {
            var pkg = await _packageRepository.GetPackageByIdAsync(id);
            if (pkg == null) return NotFound(new { Message = "Package not found." });
            
            var tests = await _packageRepository.GetPackageTestsAsync(id);
            pkg.Tests.AddRange(tests);
            
            return Ok(pkg);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("packages")]
        public async Task<IActionResult> CreatePackage([FromBody] Package package)
        {
            int id = await _packageRepository.CreatePackageAsync(package);
            return Ok(new { Message = "Package created successfully.", PackageId = id });
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("packages")]
        public async Task<IActionResult> UpdatePackage([FromBody] Package package)
        {
            await _packageRepository.UpdatePackageAsync(package);
            return Ok(new { Message = "Package updated successfully." });
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("packages/{id}")]
        public async Task<IActionResult> DeletePackage(int id)
        {
            await _packageRepository.DeletePackageAsync(id);
            return Ok(new { Message = "Package deleted successfully." });
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("packages/add-test")]
        public async Task<IActionResult> AddTestToPackage([FromQuery] int packageId, [FromQuery] int testId)
        {
            await _packageRepository.AddTestToPackageAsync(packageId, testId);
            return Ok(new { Message = "Test added to package successfully." });
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("packages/remove-test")]
        public async Task<IActionResult> RemoveTestFromPackage([FromQuery] int packageId, [FromQuery] int testId)
        {
            await _packageRepository.RemoveTestFromPackageAsync(packageId, testId);
            return Ok(new { Message = "Test removed from package successfully." });
        }
    }
}
