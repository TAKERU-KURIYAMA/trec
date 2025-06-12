using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Azure.WebJobs;
using System.Dynamic;
using Microsoft.Azure.Functions.Worker;
using Microsoft.AspNetCore.Http;
using Common;
using Api.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;


namespace API.Training
{
    /// <summary>
    /// </summary>
    /// <param name="req"></param>
    /// <param name="log"></param>
    public static class GetMenu
    {
        [Function("GetTrainingMenu")]
        public static async Task<IActionResult> RunAsync(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "training/menu")] HttpRequest req, FunctionContext context)
        {
            var log = context.GetLogger("GetTrainingMenu");

            try
            {
                Logger.Entry(log, req);

                DbContextOptions<MessageRDBContext> options = new DbContextOptionsBuilder<MessageRDBContext>()
                .UseSqlServer(Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection"))
                .Options;
                using (MessageRDBContext DBcontext = new MessageRDBContext(options))
                {
                    List<ResponseMenu> responseMenus = await GetMenuList(DBcontext);
                    List<ResponseTag> responseTags =await GetTagList(DBcontext);

                    return Terminate(log, responseMenus, responseTags);
                }
            }
            catch (AppException aex)
            {
                Logger.Error(log, aex);
                return Response.CreateErrorResponse(aex.Cause);
            }
            catch (Exception ex)
            {
                Logger.Error(log, new AppException(FoundationCode.Errors.SERVER_ERROR, ex));
                return Response.CreateErrorResponse(FoundationCode.Errors.SERVER_ERROR);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="DBcontext"></param>
        /// <returns></returns>
        private static async Task<List<ResponseMenu>> GetMenuList(MessageRDBContext DBcontext)
        {
            return await DBcontext.TrainingMenus
             .Include(m => m.TrainingTags)
             .Select(m => new ResponseMenu
             {
                 MenuId = m.MenuId,
                 JPName = m.Jpname,
                 ENName = m.Enname,
                 Description = m.Description,
                 CreatedAt = m.CreatedAt,
                 Tags = m.TrainingTags.Select(t => new TagRef { TagId = t.TagId }).ToList()
             }).ToListAsync();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="DBcontext"></param>
        /// <returns></returns>
        private static async Task<List<ResponseTag>> GetTagList(MessageRDBContext DBcontext)
        {
            return await DBcontext.TrainingTags
            .Select(t => new ResponseTag
            {
                TagId = t.TagId,
                JPName = t.Jpname,
                ENName = t.Enname
            })
            .ToListAsync();
        }

        /// <summary>
        /// �I������
        /// </summary>
        /// <param name="log"></param>
        /// <returns></returns>
        private static ObjectResult Terminate(ILogger log, List<ResponseMenu> responseMenus, List<ResponseTag> responseTags)
        {
            dynamic resJson = new ExpandoObject();
            resJson.response_menus = responseMenus;
            resJson.response_tags = responseTags;

            Logger.Exit(log, resJson);
            return Response.CreateOkResponse(resJson);
        }

        /// <summary>
        /// 
        /// </summary>
        private sealed class ResponseMenu
        {
            public string MenuId { get; set; } = string.Empty;
            public string JPName { get; set; } = string.Empty;
            public string ENName { get; set; } = string.Empty;
            public string? Description { get; set; } = string.Empty;
            public DateTime? CreatedAt { get; set; } = DateTime.MinValue;
            public List<TagRef>? Tags { get; set; } = null;
        }

        /// <summary>
        /// 
        /// </summary>
        private sealed class TagRef
        {
            public string TagId { get; set; } = string.Empty;
        }

        ///
        private sealed class ResponseTag
        {
            public string TagId { get; set; } = string.Empty;
            public string JPName { get; set; } = string.Empty;
            public string ENName { get; set; } = string.Empty;

        }
    }
}


