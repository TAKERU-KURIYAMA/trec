using backend.Models;
using Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Services;

namespace API;

public class PostTrainingRecord
{
    public async Task<IActionResult> Run(
        AuthRDBContext dbContext,
        IJwtService jwtService,
        [FromHeader(Name = "Authorization")] string authorizationHeader,
        [FromBody] PostTrainingRecordRequest request)
    {
        try
        {
            string? token = authorizationHeader?.Replace("Bearer ", "", StringComparison.OrdinalIgnoreCase);
            if (string.IsNullOrEmpty(token))
            {
                throw new AppException(FoundationCode.Errors.Unauthorized, "トークンが設定されていない");
            }

            var (isValid, userId) = jwtService.ValidateToken(token);
            if (!isValid || userId == null)
            {
                throw new AppException(FoundationCode.Errors.Unauthorized, "トークンが無効");
            }

            //メニュー存在確認
            if (!await dbContext.TrainingMenus.AnyAsync(tm => tm.MenuId == request.MenuId))
            {
                throw new AppException(FoundationCode.Errors.InvalidInput, "存在しないメニュー");
            }


            var records = request.Sets.Select(set => new TrainingRecordSet
            {
                UserCommonId = userId,
                MenuId = request.MenuId,
                TrainingDate = request.TrainingDate,
                SetNumber = set.SetNumber,
                Reps = set.Reps,
                Weight = set.Weight
            }).ToList();

            dbContext.TrainingRecordSets.AddRange(records);
            await dbContext.SaveChangesAsync();

            return Utils.ResponseBuilder.Success();
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
    public class PostTrainingRecordRequest
    {
        public required string MenuId { get; set; }
        public required DateOnly TrainingDate { get; set; }
        public required List<Set> Sets { get; set; }

        public class Set
        {
            public required int SetNumber { get; set; }
            public required int Reps { get; set; }
            public decimal? Weight { get; set; }
        }
    }


}
