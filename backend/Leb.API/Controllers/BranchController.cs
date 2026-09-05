using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Leb.Core.Entities;
using Leb.Core.Interfaces;

namespace Leb.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BranchController : ControllerBase
    {
        private readonly IBranchRepository _branchRepository;
        private readonly ITimeSlotRepository _timeSlotRepository;
        private readonly IHolidayRepository _holidayRepository;
        private readonly IAuditLogRepository _auditLogRepository;

        public BranchController(
            IBranchRepository branchRepository,
            ITimeSlotRepository timeSlotRepository,
            IHolidayRepository holidayRepository,
            IAuditLogRepository auditLogRepository)
        {
            _branchRepository = branchRepository;
            _timeSlotRepository = timeSlotRepository;
            _holidayRepository = holidayRepository;
            _auditLogRepository = auditLogRepository;
        }

        [HttpGet]
        public async Task<IActionResult> GetBranches()
        {
            var list = await _branchRepository.GetAllBranchesAsync();
            return Ok(list);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetBranchById(int id)
        {
            var branch = await _branchRepository.GetBranchByIdAsync(id);
            if (branch == null) return NotFound(new { Message = "Branch not found." });
            return Ok(branch);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> CreateBranch([FromBody] Branch branch)
        {
            int id = await _branchRepository.CreateBranchAsync(branch);
            
            await _auditLogRepository.CreateAuditLogAsync(new AuditLog
            {
                Action = "CREATE",
                TableName = "Branches",
                RecordId = id,
                NewValues = $"Name: {branch.Name}, City: {branch.City}"
            });

            return Ok(new { Message = "Branch created successfully.", BranchId = id });
        }

        [Authorize(Roles = "Admin")]
        [HttpPut]
        public async Task<IActionResult> UpdateBranch([FromBody] Branch branch)
        {
            await _branchRepository.UpdateBranchAsync(branch);

            await _auditLogRepository.CreateAuditLogAsync(new AuditLog
            {
                Action = "UPDATE",
                TableName = "Branches",
                RecordId = branch.BranchId,
                NewValues = $"Name: {branch.Name}, IsActive: {branch.IsActive}"
            });

            return Ok(new { Message = "Branch updated successfully." });
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteBranch(int id)
        {
            await _branchRepository.DeleteBranchAsync(id);

            await _auditLogRepository.CreateAuditLogAsync(new AuditLog
            {
                Action = "DELETE",
                TableName = "Branches",
                RecordId = id
            });

            return Ok(new { Message = "Branch deleted successfully." });
        }

        // --- TIME SLOTS ---
        [HttpGet("{id}/slots")]
        public async Task<IActionResult> GetSlots(int id)
        {
            var list = await _timeSlotRepository.GetSlotsByBranchAsync(id);
            return Ok(list);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("slots")]
        public async Task<IActionResult> CreateSlot([FromBody] TimeSlot slot)
        {
            int id = await _timeSlotRepository.CreateTimeSlotAsync(slot);
            return Ok(new { Message = "Time slot created successfully.", SlotId = id });
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("slots")]
        public async Task<IActionResult> UpdateSlot([FromBody] TimeSlot slot)
        {
            await _timeSlotRepository.UpdateTimeSlotAsync(slot);
            return Ok(new { Message = "Time slot updated successfully." });
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("slots/{id}")]
        public async Task<IActionResult> DeleteSlot(int id)
        {
            await _timeSlotRepository.DeleteTimeSlotAsync(id);
            return Ok(new { Message = "Time slot deleted successfully." });
        }

        // --- HOLIDAYS ---
        [HttpGet("{id}/holidays")]
        public async Task<IActionResult> GetHolidays(int id)
        {
            var list = await _holidayRepository.GetHolidaysByBranchAsync(id);
            return Ok(list);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("holidays")]
        public async Task<IActionResult> AddHoliday([FromBody] Holiday holiday)
        {
            int id = await _holidayRepository.CreateHolidayAsync(holiday);
            return Ok(new { Message = "Holiday created successfully.", HolidayId = id });
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("holidays/{id}")]
        public async Task<IActionResult> DeleteHoliday(int id)
        {
            await _holidayRepository.DeleteHolidayAsync(id);
            return Ok(new { Message = "Holiday removed successfully." });
        }
    }
}
