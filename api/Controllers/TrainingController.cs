using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Api.Models;
using Common.Shared.Constants;
using Common.Shared.Exceptions;
using Common.Shared.Utilities;
using System.Diagnostics;

namespace API.Controllers
{
    /// <summary>
    /// トレーニング関連のAPIエンドポイントを提供するコントローラー
    /// トレーニングメニューの取得、記録の管理などを担当
    /// </summary>
    [ApiController]
    [Route("api/training")]
    public class TrainingController : ControllerBase
    {
        private readonly ILogger<TrainingController> _logger;
        private readonly MessageRDBContext _context;

        /// <summary>
        /// TrainingControllerのコンストラクタ
        /// </summary>
        /// <param name="logger">ロガー</param>
        /// <param name="context">データベースコンテキスト</param>
        public TrainingController(ILogger<TrainingController> logger, MessageRDBContext context)
        {
            _logger = logger;
            _context = context;
        }

        /// <summary>
        /// トレーニングメニューとタグの一覧を取得
        /// クライアントアプリでメニュー選択画面を表示する際に使用
        /// </summary>
        /// <returns>メニューリストとタグリストを含むレスポンス</returns>
        [HttpGet("menu")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetMenu()
        {
            var stopwatch = Stopwatch.StartNew();
            
            try
            {
                // 構造化ログでリクエスト開始を記録
                StructuredLogger.LogRequestStart(_logger, Request, nameof(GetMenu));

                // データベース接続確認（ヘルスチェックの意味も含む）
                var isConnected = await _context.Database.CanConnectAsync();
                if (!isConnected)
                {
                    throw new AppException(ApplicationConstants.ErrorCodes.ServerError, "データベースに接続できません");
                }

                _logger.LogDebug("データベース接続確認完了");

                // メニューとタグを並行取得してパフォーマンスを向上
                var menuTask = GetTrainingMenuListAsync();
                var tagTask = GetTrainingTagListAsync();

                await Task.WhenAll(menuTask, tagTask);

                var menus = await menuTask;
                var tags = await tagTask;

                // レスポンスデータを構築
                var responseData = new
                {
                    response_menus = menus,
                    response_tags = tags,
                    // 追加のメタデータ（必要に応じて）
                    meta = new
                    {
                        menu_count = menus.Count,
                        tag_count = tags.Count,
                        retrieved_at = DateTime.UtcNow
                    }
                };

                stopwatch.Stop();

                // 構造化ログでリクエスト完了を記録
                StructuredLogger.LogRequestEnd(_logger, Request, StatusCodes.Status200OK, 
                    new { menu_count = menus.Count, tag_count = tags.Count }, 
                    stopwatch.ElapsedMilliseconds, nameof(GetMenu));

                // ビジネスイベントをログ記録
                StructuredLogger.LogBusinessEvent(_logger, "TrainingMenusRetrieved", 
                    new { menu_count = menus.Count, tag_count = tags.Count });

                return HttpResponseHelper.CreateSuccessResponse(responseData);
            }
            catch (AppException ex)
            {
                // アプリケーション固有の例外処理
                stopwatch.Stop();
                StructuredLogger.LogError(_logger, ex, Request, new { elapsed_ms = stopwatch.ElapsedMilliseconds });
                StructuredLogger.LogRequestEnd(_logger, Request, (int)ex.ErrorInfo.StatusCode, 
                    null, stopwatch.ElapsedMilliseconds, nameof(GetMenu));
                
                return HttpResponseHelper.CreateErrorResponse(ex);
            }
            catch (Exception ex)
            {
                // 予期しない例外処理
                stopwatch.Stop();
                var appEx = new AppException(ApplicationConstants.ErrorCodes.ServerError, ex);
                StructuredLogger.LogUnhandledException(_logger, ex, Request, nameof(GetMenu));
                StructuredLogger.LogRequestEnd(_logger, Request, (int)appEx.ErrorInfo.StatusCode, 
                    null, stopwatch.ElapsedMilliseconds, nameof(GetMenu));
                
                return HttpResponseHelper.CreateErrorResponse(appEx);
            }
        }

        /// <summary>
        /// トレーニングメニューの一覧を取得
        /// タグ情報も含めて取得し、フロントエンドでの表示に必要な情報を提供
        /// </summary>
        /// <returns>メニューリスト</returns>
        private async Task<List<TrainingMenuResponse>> GetTrainingMenuListAsync()
        {
            try
            {
                // EF Coreのクエリを最適化：必要な関連データのみを効率的に取得
                var menus = await _context.TrainingMenus
                    .Include(m => m.TrainingTags) // タグ情報も含めて取得
                    .AsNoTracking() // 読み取り専用クエリでパフォーマンス向上
                    .Select(m => new TrainingMenuResponse
                    {
                        MenuId = m.MenuId ?? string.Empty,
                        JPName = m.Jpname ?? string.Empty,
                        ENName = m.Enname ?? string.Empty,
                        Description = m.Description,
                        CreatedAt = m.CreatedAt,
                        // タグIDのリストを作成
                        Tags = m.TrainingTags.Select(t => new TagReference 
                        { 
                            TagId = t.TagId ?? string.Empty 
                        }).ToList()
                    })
                    .OrderBy(m => m.JPName) // 日本語名でソート
                    .ToListAsync();

                _logger.LogDebug("トレーニングメニュー取得完了: {Count}件", menus.Count);
                return menus;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "トレーニングメニューの取得中にエラーが発生しました");
                throw new AppException(ApplicationConstants.ErrorCodes.ServerError, 
                    "トレーニングメニューの取得に失敗しました", ex);
            }
        }

        /// <summary>
        /// トレーニングタグの一覧を取得
        /// メニューのカテゴリ分類やフィルタリングに使用
        /// </summary>
        /// <returns>タグリスト</returns>
        private async Task<List<TrainingTagResponse>> GetTrainingTagListAsync()
        {
            try
            {
                var tags = await _context.TrainingTags
                    .AsNoTracking() // 読み取り専用クエリでパフォーマンス向上
                    .Select(t => new TrainingTagResponse
                    {
                        TagId = t.TagId ?? string.Empty,
                        JPName = t.Jpname ?? string.Empty,
                        ENName = t.Enname ?? string.Empty
                    })
                    .OrderBy(t => t.JPName) // 日本語名でソート
                    .ToListAsync();

                _logger.LogDebug("トレーニングタグ取得完了: {Count}件", tags.Count);
                return tags;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "トレーニングタグの取得中にエラーが発生しました");
                throw new AppException(ApplicationConstants.ErrorCodes.ServerError, 
                    "トレーニングタグの取得に失敗しました", ex);
            }
        }

        /// <summary>
        /// トレーニングメニューのレスポンス用データ転送オブジェクト
        /// APIレスポンスの形式を明確に定義
        /// </summary>
        public sealed class TrainingMenuResponse
        {
            /// <summary>メニューID（一意識別子）</summary>
            public string MenuId { get; set; } = string.Empty;
            
            /// <summary>日本語メニュー名</summary>
            public string JPName { get; set; } = string.Empty;
            
            /// <summary>英語メニュー名</summary>
            public string ENName { get; set; } = string.Empty;
            
            /// <summary>メニューの説明</summary>
            public string? Description { get; set; }
            
            /// <summary>作成日時</summary>
            public DateTime? CreatedAt { get; set; }
            
            /// <summary>関連タグのリスト</summary>
            public List<TagReference> Tags { get; set; } = new();
        }

        /// <summary>
        /// タグ参照用データ転送オブジェクト
        /// メニューに関連付けられたタグの情報
        /// </summary>
        public sealed class TagReference
        {
            /// <summary>タグID</summary>
            public string TagId { get; set; } = string.Empty;
        }

        /// <summary>
        /// トレーニングタグのレスポンス用データ転送オブジェクト
        /// </summary>
        public sealed class TrainingTagResponse
        {
            /// <summary>タグID（一意識別子）</summary>
            public string TagId { get; set; } = string.Empty;
            
            /// <summary>日本語タグ名</summary>
            public string JPName { get; set; } = string.Empty;
            
            /// <summary>英語タグ名</summary>
            public string ENName { get; set; } = string.Empty;
        }
    }
}