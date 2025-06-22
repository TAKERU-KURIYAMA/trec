using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Api.Models;
using Api.common;
using API.Controllers;

namespace Api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class SupplementController : BaseController
    {
        private readonly TrecPlansRDBContext _context;

        public SupplementController(TrecPlansRDBContext context, ILogger<SupplementController> logger)
            : base(logger)
        {
            _context = context;
        }

        #region Supplement Master

        [HttpGet("supplements")]
        public async Task<IActionResult> GetSupplements()
        {
            try
            {
                var userId = GetRequiredUserId();
                var supplements = await _context.SupplementMasters
                    .Where(s => s.UserId == userId && s.IsActive)
                    .OrderBy(s => s.SupplementName)
                    .Select(s => new
                    {
                        s.SupplementId,
                        s.SupplementName,
                        s.Unit,
                        s.Description
                    })
                    .ToListAsync();

                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Data = supplements
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting supplements");
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "サプリメント一覧の取得に失敗しました"
                });
            }
        }

        [HttpPost("supplements")]
        public async Task<IActionResult> CreateSupplement([FromBody] CreateSupplementRequest request)
        {
            try
            {
                var userId = GetRequiredUserId();
                var supplement = new SupplementMaster
                {
                    UserId = userId,
                    SupplementName = request.SupplementName,
                    Unit = request.Unit,
                    Description = request.Description,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _context.SupplementMasters.Add(supplement);
                await _context.SaveChangesAsync();

                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Data = new
                    {
                        supplement.SupplementId,
                        supplement.SupplementName,
                        supplement.Unit,
                        supplement.Description
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating supplement");
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "サプリメントの登録に失敗しました"
                });
            }
        }

        [HttpPut("supplements/{id}")]
        public async Task<IActionResult> UpdateSupplement(int id, [FromBody] UpdateSupplementRequest request)
        {
            try
            {
                var userId = GetRequiredUserId();
                var supplement = await _context.SupplementMasters
                    .FirstOrDefaultAsync(s => s.SupplementId == id && s.UserId == userId);

                if (supplement == null)
                {
                    return NotFound(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "サプリメントが見つかりません"
                    });
                }

                supplement.SupplementName = request.SupplementName;
                supplement.Unit = request.Unit;
                supplement.Description = request.Description;
                supplement.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Message = "サプリメントを更新しました"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating supplement");
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "サプリメントの更新に失敗しました"
                });
            }
        }

        [HttpDelete("supplements/{id}")]
        public async Task<IActionResult> DeleteSupplement(int id)
        {
            try
            {
                var userId = GetRequiredUserId();
                var supplement = await _context.SupplementMasters
                    .FirstOrDefaultAsync(s => s.SupplementId == id && s.UserId == userId);

                if (supplement == null)
                {
                    return NotFound(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "サプリメントが見つかりません"
                    });
                }

                supplement.IsActive = false;
                supplement.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Message = "サプリメントを削除しました"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting supplement");
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "サプリメントの削除に失敗しました"
                });
            }
        }

        #endregion

        #region Intake Records

        [HttpGet("intakes")]
        public async Task<IActionResult> GetIntakeRecords([FromQuery] DateTime? date = null)
        {
            try
            {
                var userId = GetRequiredUserId();
                var targetDate = date ?? DateTime.Today;

                var records = await _context.SupplementIntakeRecords
                    .Include(r => r.Supplement)
                    .Where(r => r.UserId == userId && r.IntakeDate == targetDate.Date)
                    .OrderBy(r => r.IntakeTime)
                    .Select(r => new
                    {
                        r.RecordId,
                        r.SupplementId,
                        SupplementName = r.Supplement.SupplementName,
                        Unit = r.Supplement.Unit,
                        r.IntakeDate,
                        r.IntakeTime,
                        r.Amount,
                        r.TimingType,
                        r.Memo
                    })
                    .ToListAsync();

                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Data = records
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting intake records");
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "摂取記録の取得に失敗しました"
                });
            }
        }

        [HttpPost("intakes")]
        public async Task<IActionResult> RecordIntake([FromBody] RecordIntakeRequest request)
        {
            try
            {
                var userId = GetRequiredUserId();
                
                var supplement = await _context.SupplementMasters
                    .FirstOrDefaultAsync(s => s.SupplementId == request.SupplementId && s.UserId == userId);

                if (supplement == null)
                {
                    return NotFound(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "サプリメントが見つかりません"
                    });
                }

                var record = new SupplementIntakeRecord
                {
                    UserId = userId,
                    SupplementId = request.SupplementId,
                    IntakeDate = request.IntakeDate.Date,
                    IntakeTime = request.IntakeTime,
                    Amount = request.Amount,
                    TimingType = request.TimingType,
                    Memo = request.Memo,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _context.SupplementIntakeRecords.Add(record);
                await _context.SaveChangesAsync();

                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Message = "摂取記録を保存しました",
                    Data = new
                    {
                        record.RecordId,
                        record.SupplementId,
                        SupplementName = supplement.SupplementName,
                        Unit = supplement.Unit,
                        record.IntakeDate,
                        record.IntakeTime,
                        record.Amount,
                        record.TimingType,
                        record.Memo
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error recording intake");
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "摂取記録の保存に失敗しました"
                });
            }
        }

        [HttpDelete("intakes/{id}")]
        public async Task<IActionResult> DeleteIntakeRecord(int id)
        {
            try
            {
                var userId = GetRequiredUserId();
                var record = await _context.SupplementIntakeRecords
                    .FirstOrDefaultAsync(r => r.RecordId == id && r.UserId == userId);

                if (record == null)
                {
                    return NotFound(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "摂取記録が見つかりません"
                    });
                }

                _context.SupplementIntakeRecords.Remove(record);
                await _context.SaveChangesAsync();

                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Message = "摂取記録を削除しました"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting intake record");
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "摂取記録の削除に失敗しました"
                });
            }
        }

        #endregion

        #region Schedules

        [HttpGet("schedules")]
        public async Task<IActionResult> GetSchedules()
        {
            try
            {
                var userId = GetRequiredUserId();
                var schedules = await _context.SupplementSchedules
                    .Include(s => s.Supplement)
                    .Where(s => s.UserId == userId && s.IsActive)
                    .OrderBy(s => s.ScheduleTime)
                    .Select(s => new
                    {
                        s.ScheduleId,
                        s.SupplementId,
                        SupplementName = s.Supplement.SupplementName,
                        Unit = s.Supplement.Unit,
                        s.ScheduleTime,
                        s.Amount,
                        s.TimingType,
                        s.DaysOfWeek,
                        s.Memo
                    })
                    .ToListAsync();

                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Data = schedules
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting schedules");
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "スケジュールの取得に失敗しました"
                });
            }
        }

        [HttpPost("schedules")]
        public async Task<IActionResult> CreateSchedule([FromBody] CreateScheduleRequest request)
        {
            try
            {
                var userId = GetRequiredUserId();
                
                var supplement = await _context.SupplementMasters
                    .FirstOrDefaultAsync(s => s.SupplementId == request.SupplementId && s.UserId == userId);

                if (supplement == null)
                {
                    return NotFound(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "サプリメントが見つかりません"
                    });
                }

                var schedule = new SupplementSchedule
                {
                    UserId = userId,
                    SupplementId = request.SupplementId,
                    ScheduleTime = request.ScheduleTime,
                    Amount = request.Amount,
                    TimingType = request.TimingType,
                    DaysOfWeek = request.DaysOfWeek ?? "ALL",
                    IsActive = true,
                    Memo = request.Memo,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _context.SupplementSchedules.Add(schedule);
                await _context.SaveChangesAsync();

                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Message = "スケジュールを作成しました",
                    Data = new
                    {
                        schedule.ScheduleId,
                        schedule.SupplementId,
                        SupplementName = supplement.SupplementName,
                        Unit = supplement.Unit,
                        schedule.ScheduleTime,
                        schedule.Amount,
                        schedule.TimingType,
                        schedule.DaysOfWeek,
                        schedule.Memo
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating schedule");
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "スケジュールの作成に失敗しました"
                });
            }
        }

        [HttpDelete("schedules/{id}")]
        public async Task<IActionResult> DeleteSchedule(int id)
        {
            try
            {
                var userId = GetRequiredUserId();
                var schedule = await _context.SupplementSchedules
                    .FirstOrDefaultAsync(s => s.ScheduleId == id && s.UserId == userId);

                if (schedule == null)
                {
                    return NotFound(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "スケジュールが見つかりません"
                    });
                }

                schedule.IsActive = false;
                schedule.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Message = "スケジュールを削除しました"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting schedule");
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "スケジュールの削除に失敗しました"
                });
            }
        }

        #endregion
    }

    #region Request Models

    public class CreateSupplementRequest
    {
        public string SupplementName { get; set; }
        public string Unit { get; set; }
        public string? Description { get; set; }
    }

    public class UpdateSupplementRequest
    {
        public string SupplementName { get; set; }
        public string Unit { get; set; }
        public string? Description { get; set; }
    }

    public class RecordIntakeRequest
    {
        public int SupplementId { get; set; }
        public DateTime IntakeDate { get; set; }
        public TimeSpan IntakeTime { get; set; }
        public decimal Amount { get; set; }
        public string? TimingType { get; set; }
        public string? Memo { get; set; }
    }

    public class CreateScheduleRequest
    {
        public int SupplementId { get; set; }
        public TimeSpan ScheduleTime { get; set; }
        public decimal Amount { get; set; }
        public string? TimingType { get; set; }
        public string? DaysOfWeek { get; set; }
        public string? Memo { get; set; }
    }

    #endregion
}