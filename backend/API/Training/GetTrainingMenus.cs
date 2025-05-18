using backend.Models;
using Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API;

public class GetTrainingMenus
{
    public async Task<IActionResult> Run(
        AuthRDBContext dbContext)
    {
        try
        {
            var menus = await dbContext.TrainingMenus
            .OrderBy(m => m.MenuId)
            .Select(m => new GetTrainingMenusResponse
            {
                MenuId = m.MenuId,
                MenuName = m.Name,
                Description = m.Description
            })
            .ToListAsync();

            return Utils.ResponseBuilder.Success(menus);
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

    public class GetTrainingMenusResponse
    {
        public string? MenuId { get; set; }
        public string MenuName { get; set; } = string.Empty;
        public string? Description { get; set; }
    }
}
