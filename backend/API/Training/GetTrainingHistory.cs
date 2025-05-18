using backend.Models;
using Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Services;

namespace API;

public class GetTrainingHistory
{
    public async Task<IActionResult> Run(
        AuthRDBContext dbContext,
        IJwtService jwtService,
        [FromHeader(Name = "Authorization")] string authorizationHeader,
        [FromQuery] DateOnly? startDate,
        [FromQuery] DateOnly? endDate)
    {

        try
        {
            string? token = authorizationHeader?.Replace("Bearer ", "", StringComparison.OrdinalIgnoreCase);
            if (string.IsNullOrEmpty(token))
            {
                throw new AppException(FoundationCode.Errors.Unauthorized);
            }

            var (isValid, userId) = jwtService.ValidateToken(token);
            if (!isValid || userId == null)
            {
                throw new AppException(FoundationCode.Errors.Unauthorized);
            }

            List<TrainingRecordSet> targetList = new List<TrainingRecordSet>();

            if (startDate.HasValue)
            {
                if (endDate.HasValue)
                {
                    targetList = dbContext.TrainingRecordSets
                        .Where(r => r.UserCommonId == userId
                            && r.TrainingDate >= endDate
                            && r.TrainingDate <= startDate)
                        .ToList();
                }
                else
                {
                    targetList = dbContext.TrainingRecordSets
                        .Where(r => r.UserCommonId == userId
                            && r.TrainingDate <= startDate)
                        .ToList();
                }
            }
            else if (endDate.HasValue)
            {
                targetList = dbContext.TrainingRecordSets
                .Where(r => r.UserCommonId == userId
                            && r.TrainingDate >= endDate)
                    .ToList();
            }
            else
            {
                throw new AppException(FoundationCode.Errors.InvalidInput);
            }

            List<GetTrainingHistoryResponse> resobj = targetList
                .GroupBy(r => new { r.TrainingDate, r.MenuId })
                .Select(g => new GetTrainingHistoryResponse
                {
                    TrainingDate = g.Key.TrainingDate,
                    MenuId = g.Key.MenuId,
                    Sets = g
                        .OrderBy(x => x.SetNumber)
                        .Select(x => new GetTrainingHistoryResponse.Set
                        {
                            SetNumber = x.SetNumber,
                            Reps = x.Reps,
                            Weight = x.Weight
                        }).ToList()
                }).ToList();

            return Utils.ResponseBuilder.Success(resobj);
        }
        catch (AppException ex)
        {
            return Utils.ResponseBuilder.Fail(ex);
        }
        catch (Exception)
        {
            var response = new ApiResponse
            {
                Code = "9999",
                Message = "不明なエラーが発生しました。"
            };

            var result = new ObjectResult(response);
            result.StatusCode = 500;
            return result;
        }

    }

    public class GetTrainingHistoryResponse
    {
        public DateOnly TrainingDate { get; set; }
        public required string MenuId { get; set; }
        public List<Set> Sets { get; set; } = new();

        public class Set
        {
            public int SetNumber { get; set; }
            public int Reps { get; set; }
            public decimal? Weight { get; set; }
        }
    }
}
