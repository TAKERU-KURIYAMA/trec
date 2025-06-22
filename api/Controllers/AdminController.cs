using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Api.Models;
using Api.Common;
using System.Diagnostics;

namespace API.Controllers
{
    /// <summary>
    /// 管理者専用のAPIエンドポイントを提供するコントローラー
    /// トレーニングメニューとタグの管理機能を担当
    /// </summary>
    [Route("admin")]
    public class AdminController : BaseController
    {
        private readonly TrecPlansRDBContext _context;

        public AdminController(ILogger<AdminController> logger, TrecPlansRDBContext context)
            : base(logger)
        {
            _context = context;
        }

        /// <summary>
        /// 管理者権限チェック用エンドポイント
        /// </summary>
        [HttpGet("check")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public IActionResult CheckAdminAccess()
        {
            try
            {
                RequireAdmin();
                
                var responseData = new
                {
                    is_admin = true,
                    login_id = GetCurrentUserLoginId(),
                    display_name = GetCurrentUserDisplayName()
                };

                return CreateSuccessResponse(responseData);
            }
            catch (UnauthorizedAccessException ex)
            {
                return HandleException(ex, nameof(CheckAdminAccess));
            }
        }

        /// <summary>
        /// トレーニングメニューの一覧取得（管理者専用）
        /// </summary>
        [HttpGet("menus")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetMenusForAdmin()
        {
            var stopwatch = Stopwatch.StartNew();
            
            try
            {
                RequireAdmin();

                StructuredLogger.LogRequestStart(Request.Method, Request.Path.ToString(), nameof(GetMenusForAdmin));

                var menus = await _context.TrainingMenus
                    .Include(m => m.TrainingTags)
                    .ThenInclude(tt => tt.Tag)
                    .OrderBy(m => m.MenuId)
                    .ToListAsync();

                var menuData = menus.Select(m => new
                {
                    menu_id = m.MenuId,
                    menu_name = m.Jpname,
                    description = m.Description,
                    tags = m.TrainingTags?.Select(tt => new
                    {
                        tag_id = tt.Tag?.TagId,
                        tag_name = tt.Tag?.Jpname
                    }).ToArray() ?? new object[0],
                    created_at = m.CreatedAt
                }).ToList();

                stopwatch.Stop();

                var responseData = new
                {
                    menus = menuData,
                    meta = new
                    {
                        total_count = menuData.Count,
                        retrieved_at = DateTime.UtcNow
                    }
                };

                StructuredLogger.LogRequestEnd(Request.Method, Request.Path.ToString(), StatusCodes.Status200OK,
                    stopwatch.Elapsed, new { menu_count = menuData.Count });

                return CreateSuccessResponse(responseData);
            }
            catch (UnauthorizedAccessException ex)
            {
                stopwatch.Stop();
                return HandleException(ex, nameof(GetMenusForAdmin));
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                StructuredLogger.LogError($"Error in {nameof(GetMenusForAdmin)}", ex, 
                    new { elapsed_ms = stopwatch.ElapsedMilliseconds });
                return HandleException(ex, nameof(GetMenusForAdmin));
            }
        }

        /// <summary>
        /// タグの一覧取得（管理者専用）
        /// </summary>
        [HttpGet("tags")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetTagsForAdmin()
        {
            var stopwatch = Stopwatch.StartNew();
            
            try
            {
                RequireAdmin();

                StructuredLogger.LogRequestStart(Request.Method, Request.Path.ToString(), nameof(GetTagsForAdmin));

                var tags = await _context.TagMasters
                    .Include(t => t.TrainingTags)
                    .ThenInclude(tt => tt.Menu)
                    .OrderBy(t => t.TagId)
                    .ToListAsync();

                var tagData = tags.Select(t => new
                {
                    tag_id = t.TagId,
                    tag_name = t.Jpname,
                    menu_count = t.TrainingTags?.Count ?? 0,
                    menus = t.TrainingTags?.Select(tt => new
                    {
                        menu_id = tt.Menu?.MenuId,
                        menu_name = tt.Menu?.Jpname
                    }).ToArray() ?? new object[0],
                    created_at = t.CreatedAt
                }).ToList();

                stopwatch.Stop();

                var responseData = new
                {
                    tags = tagData,
                    meta = new
                    {
                        total_count = tagData.Count,
                        retrieved_at = DateTime.UtcNow
                    }
                };

                StructuredLogger.LogRequestEnd(Request.Method, Request.Path.ToString(), StatusCodes.Status200OK,
                    stopwatch.Elapsed, new { tag_count = tagData.Count });

                return CreateSuccessResponse(responseData);
            }
            catch (UnauthorizedAccessException ex)
            {
                stopwatch.Stop();
                return HandleException(ex, nameof(GetTagsForAdmin));
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                StructuredLogger.LogError($"Error in {nameof(GetTagsForAdmin)}", ex, 
                    new { elapsed_ms = stopwatch.ElapsedMilliseconds });
                return HandleException(ex, nameof(GetTagsForAdmin));
            }
        }

        /// <summary>
        /// トレーニングメニューの追加（管理者専用）
        /// </summary>
        [HttpPost("menus")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> CreateMenu([FromBody] CreateMenuRequest request)
        {
            var stopwatch = Stopwatch.StartNew();
            
            try
            {
                RequireAdmin();

                if (!ModelState.IsValid)
                {
                    return CreateValidationErrorResponse();
                }

                StructuredLogger.LogRequestStart(Request.Method, Request.Path.ToString(), nameof(CreateMenu));

                // メニューIDの重複チェック
                var existingMenu = await _context.TrainingMenus
                    .FirstOrDefaultAsync(m => m.MenuId == request.MenuId);
                
                if (existingMenu != null)
                {
                    return CreateErrorResponse(ApplicationConstants.ErrorCodes.DuplicateDataError, 
                        $"メニューID '{request.MenuId}' は既に存在します");
                }

                var newMenu = new TrainingMenu
                {
                    MenuId = request.MenuId,
                    Jpname = request.MenuName,
                    Enname = request.EnglishName ?? string.Empty,
                    Description = request.Description ?? string.Empty,
                    CreatedAt = DateTime.UtcNow
                };

                _context.TrainingMenus.Add(newMenu);
                await _context.SaveChangesAsync();

                stopwatch.Stop();

                var responseData = new
                {
                    menu_id = newMenu.MenuId,
                    menu_name = newMenu.Jpname,
                    description = newMenu.Description,
                    created_at = newMenu.CreatedAt
                };

                StructuredLogger.LogRequestEnd(Request.Method, Request.Path.ToString(), StatusCodes.Status201Created,
                    stopwatch.Elapsed, new { menu_id = newMenu.MenuId });

                StructuredLogger.LogBusinessEvent("TrainingMenuCreated", 
                    new { menu_id = newMenu.MenuId, admin_user = GetCurrentUserLoginId() });

                return StatusCode(StatusCodes.Status201Created, 
                    HttpResponseHelper.CreateSuccessResponse(responseData));
            }
            catch (UnauthorizedAccessException ex)
            {
                stopwatch.Stop();
                return HandleException(ex, nameof(CreateMenu));
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                StructuredLogger.LogError($"Error in {nameof(CreateMenu)}", ex, 
                    new { elapsed_ms = stopwatch.ElapsedMilliseconds });
                return HandleException(ex, nameof(CreateMenu));
            }
        }

        /// <summary>
        /// トレーニングメニューの更新（管理者専用）
        /// </summary>
        [HttpPut("menus/{menuId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> UpdateMenu(string menuId, [FromBody] UpdateMenuRequest request)
        {
            var stopwatch = Stopwatch.StartNew();
            
            try
            {
                RequireAdmin();

                if (!ModelState.IsValid)
                {
                    return CreateValidationErrorResponse();
                }

                StructuredLogger.LogRequestStart(Request.Method, Request.Path.ToString(), nameof(UpdateMenu));

                var existingMenu = await _context.TrainingMenus
                    .FirstOrDefaultAsync(m => m.MenuId == menuId);
                
                if (existingMenu == null)
                {
                    return CreateErrorResponse(ApplicationConstants.ErrorCodes.DataNotFound, 
                        $"メニューID '{menuId}' が見つかりません");
                }

                existingMenu.Jpname = request.MenuName;
                existingMenu.Enname = request.EnglishName ?? existingMenu.Enname;
                existingMenu.Description = request.Description ?? existingMenu.Description;

                await _context.SaveChangesAsync();

                stopwatch.Stop();

                var responseData = new
                {
                    menu_id = existingMenu.MenuId,
                    menu_name = existingMenu.Jpname,
                    description = existingMenu.Description,
                    updated_at = DateTime.UtcNow
                };

                StructuredLogger.LogRequestEnd(Request.Method, Request.Path.ToString(), StatusCodes.Status200OK,
                    stopwatch.Elapsed, new { menu_id = existingMenu.MenuId });

                StructuredLogger.LogBusinessEvent("TrainingMenuUpdated", 
                    new { menu_id = existingMenu.MenuId, admin_user = GetCurrentUserLoginId() });

                return CreateSuccessResponse(responseData);
            }
            catch (UnauthorizedAccessException ex)
            {
                stopwatch.Stop();
                return HandleException(ex, nameof(UpdateMenu));
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                StructuredLogger.LogError($"Error in {nameof(UpdateMenu)}", ex, 
                    new { elapsed_ms = stopwatch.ElapsedMilliseconds });
                return HandleException(ex, nameof(UpdateMenu));
            }
        }

        /// <summary>
        /// トレーニングメニューの削除（管理者専用）
        /// </summary>
        [HttpDelete("menus/{menuId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> DeleteMenu(string menuId)
        {
            var stopwatch = Stopwatch.StartNew();
            
            try
            {
                RequireAdmin();

                StructuredLogger.LogRequestStart(Request.Method, Request.Path.ToString(), nameof(DeleteMenu));

                var existingMenu = await _context.TrainingMenus
                    .Include(m => m.TrainingRecordSets)
                    .Include(m => m.DailyTrainingRecords)
                    .Include(m => m.TrainingTags)
                    .FirstOrDefaultAsync(m => m.MenuId == menuId);
                
                if (existingMenu == null)
                {
                    return CreateErrorResponse(ApplicationConstants.ErrorCodes.DataNotFound, 
                        $"メニューID '{menuId}' が見つかりません");
                }

                // 関連するトレーニング記録がある場合は削除を拒否
                if (existingMenu.TrainingRecordSets.Any() || existingMenu.DailyTrainingRecords.Any())
                {
                    return CreateErrorResponse(ApplicationConstants.ErrorCodes.DuplicateDataError, 
                        "このメニューには関連するトレーニング記録があるため削除できません");
                }

                // タグの関連付けを削除
                _context.TrainingTags.RemoveRange(existingMenu.TrainingTags);
                
                // メニューを削除
                _context.TrainingMenus.Remove(existingMenu);
                await _context.SaveChangesAsync();

                stopwatch.Stop();

                StructuredLogger.LogRequestEnd(Request.Method, Request.Path.ToString(), StatusCodes.Status200OK,
                    stopwatch.Elapsed, new { menu_id = menuId });

                StructuredLogger.LogBusinessEvent("TrainingMenuDeleted", 
                    new { menu_id = menuId, admin_user = GetCurrentUserLoginId() });

                return CreateSuccessResponse(new { message = "メニューが正常に削除されました", menu_id = menuId });
            }
            catch (UnauthorizedAccessException ex)
            {
                stopwatch.Stop();
                return HandleException(ex, nameof(DeleteMenu));
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                StructuredLogger.LogError($"Error in {nameof(DeleteMenu)}", ex, 
                    new { elapsed_ms = stopwatch.ElapsedMilliseconds });
                return HandleException(ex, nameof(DeleteMenu));
            }
        }

        /// <summary>
        /// タグの追加（管理者専用）
        /// </summary>
        [HttpPost("tags")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> CreateTag([FromBody] CreateTagRequest request)
        {
            var stopwatch = Stopwatch.StartNew();
            
            try
            {
                RequireAdmin();

                if (!ModelState.IsValid)
                {
                    return CreateValidationErrorResponse();
                }

                StructuredLogger.LogRequestStart(Request.Method, Request.Path.ToString(), nameof(CreateTag));

                // タグIDの重複チェック
                var existingTag = await _context.TagMasters
                    .FirstOrDefaultAsync(t => t.TagId == request.TagId);
                
                if (existingTag != null)
                {
                    return CreateErrorResponse(ApplicationConstants.ErrorCodes.DuplicateDataError, 
                        $"タグID '{request.TagId}' は既に存在します");
                }

                var newTag = new TagMaster
                {
                    TagId = request.TagId,
                    Jpname = request.TagName,
                    Enname = request.EnglishName ?? string.Empty,
                    CreatedAt = DateTime.UtcNow
                };

                _context.TagMasters.Add(newTag);
                await _context.SaveChangesAsync();

                stopwatch.Stop();

                var responseData = new
                {
                    tag_id = newTag.TagId,
                    tag_name = newTag.Jpname,
                    created_at = newTag.CreatedAt
                };

                StructuredLogger.LogRequestEnd(Request.Method, Request.Path.ToString(), StatusCodes.Status201Created,
                    stopwatch.Elapsed, new { tag_id = newTag.TagId });

                StructuredLogger.LogBusinessEvent("TagCreated", 
                    new { tag_id = newTag.TagId, admin_user = GetCurrentUserLoginId() });

                return StatusCode(StatusCodes.Status201Created, 
                    HttpResponseHelper.CreateSuccessResponse(responseData));
            }
            catch (UnauthorizedAccessException ex)
            {
                stopwatch.Stop();
                return HandleException(ex, nameof(CreateTag));
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                StructuredLogger.LogError($"Error in {nameof(CreateTag)}", ex, 
                    new { elapsed_ms = stopwatch.ElapsedMilliseconds });
                return HandleException(ex, nameof(CreateTag));
            }
        }

        /// <summary>
        /// メニューへのタグ割り当て（管理者専用）
        /// </summary>
        [HttpPost("menus/{menuId}/tags")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> AssignTagToMenu(string menuId, [FromBody] AssignTagToMenuRequest request)
        {
            var stopwatch = Stopwatch.StartNew();
            
            try
            {
                RequireAdmin();

                if (!ModelState.IsValid)
                {
                    return CreateValidationErrorResponse();
                }

                StructuredLogger.LogRequestStart(Request.Method, Request.Path.ToString(), nameof(AssignTagToMenu));

                // メニューとタグの存在確認
                var menu = await _context.TrainingMenus
                    .FirstOrDefaultAsync(m => m.MenuId == menuId);
                
                if (menu == null)
                {
                    return CreateErrorResponse(ApplicationConstants.ErrorCodes.DataNotFound, 
                        $"メニューID '{menuId}' が見つかりません");
                }

                var tag = await _context.TagMasters
                    .FirstOrDefaultAsync(t => t.TagId == request.TagId);
                
                if (tag == null)
                {
                    return CreateErrorResponse(ApplicationConstants.ErrorCodes.DataNotFound, 
                        $"タグID '{request.TagId}' が見つかりません");
                }

                // 既に割り当て済みかチェック
                var existingAssignment = await _context.TrainingTags
                    .FirstOrDefaultAsync(tt => tt.MenuId == menuId && tt.TagId == request.TagId);
                
                if (existingAssignment != null)
                {
                    return CreateErrorResponse(ApplicationConstants.ErrorCodes.DuplicateDataError, 
                        $"メニュー '{menuId}' にタグ '{request.TagId}' は既に割り当てられています");
                }

                // タグ割り当てレコードを作成
                var trainingTag = new TrainingTag
                {
                    MenuId = menuId,
                    TagId = request.TagId,
                    Jpname = tag.Jpname,
                    Enname = tag.Enname,
                    CreatedAt = DateTime.UtcNow
                };

                _context.TrainingTags.Add(trainingTag);
                await _context.SaveChangesAsync();

                stopwatch.Stop();

                var responseData = new
                {
                    menu_id = menuId,
                    tag_id = request.TagId,
                    menu_name = menu.Jpname,
                    tag_name = tag.Jpname,
                    assigned_at = trainingTag.CreatedAt
                };

                StructuredLogger.LogRequestEnd(Request.Method, Request.Path.ToString(), StatusCodes.Status201Created,
                    stopwatch.Elapsed, new { menu_id = menuId, tag_id = request.TagId });

                StructuredLogger.LogBusinessEvent("TagAssignedToMenu", 
                    new { menu_id = menuId, tag_id = request.TagId, admin_user = GetCurrentUserLoginId() });

                return StatusCode(StatusCodes.Status201Created, 
                    HttpResponseHelper.CreateSuccessResponse(responseData));
            }
            catch (UnauthorizedAccessException ex)
            {
                stopwatch.Stop();
                return HandleException(ex, nameof(AssignTagToMenu));
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                StructuredLogger.LogError($"Error in {nameof(AssignTagToMenu)}", ex, 
                    new { elapsed_ms = stopwatch.ElapsedMilliseconds });
                return HandleException(ex, nameof(AssignTagToMenu));
            }
        }

        /// <summary>
        /// メニューからタグを削除（管理者専用）
        /// </summary>
        [HttpDelete("menus/{menuId}/tags/{tagId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> RemoveTagFromMenu(string menuId, string tagId)
        {
            var stopwatch = Stopwatch.StartNew();
            
            try
            {
                RequireAdmin();

                StructuredLogger.LogRequestStart(Request.Method, Request.Path.ToString(), nameof(RemoveTagFromMenu));

                // タグ割り当てレコードの存在確認
                var trainingTag = await _context.TrainingTags
                    .Include(tt => tt.Menu)
                    .Include(tt => tt.Tag)
                    .FirstOrDefaultAsync(tt => tt.MenuId == menuId && tt.TagId == tagId);
                
                if (trainingTag == null)
                {
                    return CreateErrorResponse(ApplicationConstants.ErrorCodes.DataNotFound, 
                        $"メニュー '{menuId}' からタグ '{tagId}' の割り当てが見つかりません");
                }

                _context.TrainingTags.Remove(trainingTag);
                await _context.SaveChangesAsync();

                stopwatch.Stop();

                var responseData = new
                {
                    menu_id = menuId,
                    tag_id = tagId,
                    menu_name = trainingTag.Menu?.Jpname,
                    tag_name = trainingTag.Tag?.Jpname,
                    message = "タグの割り当てが正常に削除されました"
                };

                StructuredLogger.LogRequestEnd(Request.Method, Request.Path.ToString(), StatusCodes.Status200OK,
                    stopwatch.Elapsed, new { menu_id = menuId, tag_id = tagId });

                StructuredLogger.LogBusinessEvent("TagRemovedFromMenu", 
                    new { menu_id = menuId, tag_id = tagId, admin_user = GetCurrentUserLoginId() });

                return CreateSuccessResponse(responseData);
            }
            catch (UnauthorizedAccessException ex)
            {
                stopwatch.Stop();
                return HandleException(ex, nameof(RemoveTagFromMenu));
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                StructuredLogger.LogError($"Error in {nameof(RemoveTagFromMenu)}", ex, 
                    new { elapsed_ms = stopwatch.ElapsedMilliseconds });
                return HandleException(ex, nameof(RemoveTagFromMenu));
            }
        }

        /// <summary>
        /// メニューに一括でタグを割り当て（管理者専用）
        /// </summary>
        [HttpPut("menus/{menuId}/tags")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> UpdateMenuTags(string menuId, [FromBody] UpdateMenuTagsRequest request)
        {
            var stopwatch = Stopwatch.StartNew();
            
            try
            {
                RequireAdmin();

                if (!ModelState.IsValid)
                {
                    return CreateValidationErrorResponse();
                }

                StructuredLogger.LogRequestStart(Request.Method, Request.Path.ToString(), nameof(UpdateMenuTags));

                // メニューの存在確認
                var menu = await _context.TrainingMenus
                    .Include(m => m.TrainingTags)
                    .FirstOrDefaultAsync(m => m.MenuId == menuId);
                
                if (menu == null)
                {
                    return CreateErrorResponse(ApplicationConstants.ErrorCodes.DataNotFound, 
                        $"メニューID '{menuId}' が見つかりません");
                }

                // 指定されたタグが全て存在するかチェック
                var existingTags = await _context.TagMasters
                    .Where(t => request.TagIds.Contains(t.TagId))
                    .ToListAsync();

                var missingTags = request.TagIds.Except(existingTags.Select(t => t.TagId)).ToList();
                if (missingTags.Any())
                {
                    return CreateErrorResponse(ApplicationConstants.ErrorCodes.DataNotFound, 
                        $"存在しないタグID: {string.Join(", ", missingTags)}");
                }

                // 既存のタグ割り当てを削除
                _context.TrainingTags.RemoveRange(menu.TrainingTags);

                // 新しいタグ割り当てを作成
                var newTrainingTags = existingTags.Select(tag => new TrainingTag
                {
                    MenuId = menuId,
                    TagId = tag.TagId,
                    Jpname = tag.Jpname,
                    Enname = tag.Enname,
                    CreatedAt = DateTime.UtcNow
                }).ToList();

                _context.TrainingTags.AddRange(newTrainingTags);
                await _context.SaveChangesAsync();

                stopwatch.Stop();

                var responseData = new
                {
                    menu_id = menuId,
                    menu_name = menu.Jpname,
                    assigned_tags = existingTags.Select(t => new
                    {
                        tag_id = t.TagId,
                        tag_name = t.Jpname
                    }).ToArray(),
                    updated_at = DateTime.UtcNow
                };

                StructuredLogger.LogRequestEnd(Request.Method, Request.Path.ToString(), StatusCodes.Status200OK,
                    stopwatch.Elapsed, new { menu_id = menuId, tag_count = request.TagIds.Count });

                StructuredLogger.LogBusinessEvent("MenuTagsUpdated", 
                    new { menu_id = menuId, tag_ids = request.TagIds, admin_user = GetCurrentUserLoginId() });

                return CreateSuccessResponse(responseData);
            }
            catch (UnauthorizedAccessException ex)
            {
                stopwatch.Stop();
                return HandleException(ex, nameof(UpdateMenuTags));
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                StructuredLogger.LogError($"Error in {nameof(UpdateMenuTags)}", ex, 
                    new { elapsed_ms = stopwatch.ElapsedMilliseconds });
                return HandleException(ex, nameof(UpdateMenuTags));
            }
        }
    }
}