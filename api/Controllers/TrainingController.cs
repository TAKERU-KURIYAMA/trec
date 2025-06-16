using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Api.Models;
using Api.Common;
using System.Diagnostics;

namespace API.Controllers
{
    /// <summary>
    /// トレーニング関連のAPIエンドポイントを提供するコントローラー
    /// トレーニングメニューの取得、記録の管理などを担当
    /// </summary>
    [Route("training")]
    public class TrainingController : BaseController
    {
        private readonly MessageRDBContext _context;

        /// <summary>
        /// TrainingControllerのコンストラクタ
        /// </summary>
        /// <param name="logger">ロガー</param>
        /// <param name="context">データベースコンテキスト</param>
        public TrainingController(ILogger<TrainingController> logger, MessageRDBContext context)
            : base(logger)
        {
            _context = context;
        }

        /// <summary>
        /// トレーニングメニューとタグの一覧を取得
        /// クライアントアプリでメニュー選択画面を表示する際に使用
        /// </summary>
        /// <returns>メニューリストとタグリストを含むレスポンス</returns>
        [HttpGet("menu")]
        [AllowAnonymous] // メニュー一覧は認証不要
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetMenu()
        {
            var stopwatch = Stopwatch.StartNew();
            
            try
            {
                // 構造化ログでリクエスト開始を記録
                StructuredLogger.LogRequestStart(Request.Method, Request.Path.ToString(), nameof(GetMenu));

                // データベース接続確認（ヘルスチェックの意味も含む）
                var isConnected = await _context.Database.CanConnectAsync();
                if (!isConnected)
                {
                    throw new AppException(ApplicationConstants.ErrorCodes.ServerError, "データベースに接続できません");
                }

                _logger.LogDebug("データベース接続確認完了");

                // メニューとタグを順次取得（DbContext同時実行問題を回避）
                var menus = await GetTrainingMenuListAsync();
                var tags = await GetTrainingTagListAsync();

                // レスポンスデータを構築（フロントエンド期待形式に合わせる）
                var responseData = new
                {
                    menus = menus,
                    tags = tags,
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
                StructuredLogger.LogRequestEnd(Request.Method, Request.Path.ToString(), StatusCodes.Status200OK, 
                    stopwatch.Elapsed, new { menu_count = menus.Count, tag_count = tags.Count });

                // ビジネスイベントをログ記録
                StructuredLogger.LogBusinessEvent("TrainingMenusRetrieved", 
                    new { menu_count = menus.Count, tag_count = tags.Count });

                return HttpResponseHelper.CreateSuccessResponse(responseData);
            }
            catch (AppException ex)
            {
                // アプリケーション固有の例外処理
                stopwatch.Stop();
                StructuredLogger.LogError($"Error in {nameof(GetMenu)}: {ex.UserMessage}", ex, new { elapsed_ms = stopwatch.ElapsedMilliseconds });
                StructuredLogger.LogRequestEnd(Request.Method, Request.Path, 500, 
                    stopwatch.Elapsed, null);
                
                return HttpResponseHelper.CreateErrorResponse(ex.ErrorCode, ex.UserMessage);
            }
            catch (Exception ex)
            {
                // 予期しない例外処理
                stopwatch.Stop();
                var appEx = new AppException(ApplicationConstants.ErrorCodes.ServerError, "トレーニングメニューの取得中にエラーが発生しました", ex);
                StructuredLogger.LogUnhandledException(ex, new { action = nameof(GetMenu), path = Request.Path.ToString() });
                StructuredLogger.LogRequestEnd(Request.Method, Request.Path, 500, 
                    stopwatch.Elapsed, null);
                
                return HttpResponseHelper.CreateErrorResponse(appEx.ErrorCode, appEx.UserMessage);
            }
        }

        /// <summary>
        /// アイソレーション種目の部位別一覧を取得
        /// 単関節運動を部位別に分類して表示する際に使用
        /// </summary>
        /// <param name="bodyPartTag">部位タグ（任意フィルタ）</param>
        /// <returns>アイソレーション種目の部位別リスト</returns>
        [HttpGet("menu/isolation")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetIsolationExercisesByBodyPart([FromQuery] string? bodyPartTag = null)
        {
            var stopwatch = Stopwatch.StartNew();
            
            try
            {
                StructuredLogger.LogRequestStart(Request.Method, Request.Path.ToString(), nameof(GetIsolationExercisesByBodyPart));

                // データベース接続確認
                var isConnected = await _context.Database.CanConnectAsync();
                if (!isConnected)
                {
                    throw new AppException(ApplicationConstants.ErrorCodes.ServerError, "データベースに接続できません");
                }

                // アイソレーション種目を取得（isolationタグを持つメニュー）
                var isolationQuery = _context.TrainingMenus
                    .Include(m => m.TrainingTags)
                    .ThenInclude(tt => tt.Tag)
                    .Where(m => m.TrainingTags.Any(tt => tt.TagId == "isolation"))
                    .AsNoTracking();

                // 部位フィルタが指定されている場合は追加フィルタを適用
                if (!string.IsNullOrEmpty(bodyPartTag))
                {
                    isolationQuery = isolationQuery.Where(m => m.TrainingTags.Any(tt => tt.TagId == bodyPartTag));
                }

                var isolationMenus = await isolationQuery.ToListAsync();

                // 部位別にグループ化
                var bodyPartGroups = isolationMenus
                    .SelectMany(m => m.TrainingTags
                        .Where(tt => IsBodyPartTag(tt.TagId))
                        .Select(tt => new { Menu = m, BodyPartTag = tt }))
                    .GroupBy(x => new { x.BodyPartTag.TagId, x.BodyPartTag.Jpname, x.BodyPartTag.Enname })
                    .Select(g => new IsolationBodyPartGroup
                    {
                        BodyPartId = g.Key.TagId ?? string.Empty,
                        BodyPartName = g.Key.Jpname ?? string.Empty,
                        BodyPartNameEn = g.Key.Enname ?? string.Empty,
                        ExerciseCount = g.Count(),
                        Exercises = g.Select(x => new IsolationExercise
                        {
                            MenuId = x.Menu.MenuId ?? string.Empty,
                            MenuName = x.Menu.Jpname ?? string.Empty,
                            MenuNameEn = x.Menu.Enname ?? string.Empty,
                            Description = x.Menu.Description,
                            AllTags = x.Menu.TrainingTags.Select(tt => new ExerciseTag
                            {
                                TagId = tt.TagId ?? string.Empty,
                                TagName = tt.Jpname ?? string.Empty,
                                TagNameEn = tt.Enname ?? string.Empty
                            }).ToList(),
                            Equipment = GetEquipmentTag(x.Menu.TrainingTags),
                            Difficulty = GetDifficultyTag(x.Menu.TrainingTags),
                            CreatedAt = x.Menu.CreatedAt
                        }).ToList()
                    })
                    .OrderBy(g => GetBodyPartDisplayOrder(g.BodyPartId))
                    .ToList();

                stopwatch.Stop();

                var responseData = new
                {
                    body_part_groups = bodyPartGroups,
                    summary = new
                    {
                        total_body_parts = bodyPartGroups.Count,
                        total_exercises = bodyPartGroups.Sum(g => g.ExerciseCount),
                        filter_applied = !string.IsNullOrEmpty(bodyPartTag) ? bodyPartTag : null
                    },
                    meta = new
                    {
                        retrieved_at = DateTime.UtcNow,
                        exercise_type = "isolation"
                    }
                };

                StructuredLogger.LogRequestEnd(Request.Method, Request.Path.ToString(), StatusCodes.Status200OK,
                    stopwatch.Elapsed, new { 
                        body_part_count = bodyPartGroups.Count, 
                        exercise_count = bodyPartGroups.Sum(g => g.ExerciseCount),
                        filter = bodyPartTag
                    });

                StructuredLogger.LogBusinessEvent("IsolationExercisesRetrieved", 
                    new { 
                        body_part_count = bodyPartGroups.Count, 
                        exercise_count = bodyPartGroups.Sum(g => g.ExerciseCount),
                        filter = bodyPartTag
                    });

                return HttpResponseHelper.CreateSuccessResponse(responseData);
            }
            catch (AppException ex)
            {
                stopwatch.Stop();
                StructuredLogger.LogError($"Error in {nameof(GetIsolationExercisesByBodyPart)}: {ex.UserMessage}", ex, 
                    new { elapsed_ms = stopwatch.ElapsedMilliseconds });
                StructuredLogger.LogRequestEnd(Request.Method, Request.Path, 500, stopwatch.Elapsed, null);
                
                return HttpResponseHelper.CreateErrorResponse(ex.ErrorCode, ex.UserMessage);
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                var appEx = new AppException(ApplicationConstants.ErrorCodes.ServerError, 
                    "アイソレーション種目の取得中にエラーが発生しました", ex);
                StructuredLogger.LogUnhandledException(ex, new { action = nameof(GetIsolationExercisesByBodyPart), path = Request.Path.ToString() });
                StructuredLogger.LogRequestEnd(Request.Method, Request.Path, 500, stopwatch.Elapsed, null);
                
                return HttpResponseHelper.CreateErrorResponse(appEx.ErrorCode, appEx.UserMessage);
            }
        }

        /// <summary>
        /// タグが部位タグかどうかを判定
        /// </summary>
        private static bool IsBodyPartTag(string? tagId)
        {
            if (string.IsNullOrEmpty(tagId)) return false;
            
            var bodyPartTags = new[] { "chest", "back", "shoulders", "arms", "legs", "core", "glutes" };
            return bodyPartTags.Contains(tagId);
        }

        /// <summary>
        /// 器具タグを取得
        /// </summary>
        private static string GetEquipmentTag(ICollection<TrainingTag> tags)
        {
            var equipmentTags = new[] { "barbell", "dumbbell", "machine", "bodyweight", "cable", "band" };
            var equipmentTag = tags.FirstOrDefault(t => equipmentTags.Contains(t.TagId ?? string.Empty));
            return equipmentTag?.Jpname ?? "その他";
        }

        /// <summary>
        /// 難易度タグを取得
        /// </summary>
        private static string GetDifficultyTag(ICollection<TrainingTag> tags)
        {
            var difficultyTags = new[] { "beginner", "intermediate", "advanced" };
            var difficultyTag = tags.FirstOrDefault(t => difficultyTags.Contains(t.TagId ?? string.Empty));
            return difficultyTag?.Jpname ?? "未設定";
        }

        /// <summary>
        /// 部位の表示順序を取得
        /// </summary>
        private static int GetBodyPartDisplayOrder(string bodyPartId)
        {
            return bodyPartId switch
            {
                "chest" => 1,
                "back" => 2,
                "shoulders" => 3,
                "arms" => 4,
                "legs" => 5,
                "glutes" => 6,
                "core" => 7,
                _ => 999
            };
        }

        /// <summary>
        /// アイソレーション種目の部位別表示用データ転送オブジェクト
        /// </summary>
        public sealed class IsolationBodyPartGroup
        {
            /// <summary>部位ID</summary>
            public string BodyPartId { get; set; } = string.Empty;
            
            /// <summary>部位名（日本語）</summary>
            public string BodyPartName { get; set; } = string.Empty;
            
            /// <summary>部位名（英語）</summary>
            public string BodyPartNameEn { get; set; } = string.Empty;
            
            /// <summary>アイソレーション種目数</summary>
            public int ExerciseCount { get; set; }
            
            /// <summary>アイソレーション種目リスト</summary>
            public List<IsolationExercise> Exercises { get; set; } = new();
        }

        /// <summary>
        /// アイソレーション種目データ転送オブジェクト
        /// </summary>
        public sealed class IsolationExercise
        {
            /// <summary>メニューID</summary>
            public string MenuId { get; set; } = string.Empty;
            
            /// <summary>メニュー名（日本語）</summary>
            public string MenuName { get; set; } = string.Empty;
            
            /// <summary>メニュー名（英語）</summary>
            public string MenuNameEn { get; set; } = string.Empty;
            
            /// <summary>説明</summary>
            public string? Description { get; set; }
            
            /// <summary>全タグリスト</summary>
            public List<ExerciseTag> AllTags { get; set; } = new();
            
            /// <summary>使用器具</summary>
            public string Equipment { get; set; } = string.Empty;
            
            /// <summary>難易度</summary>
            public string Difficulty { get; set; } = string.Empty;
            
            /// <summary>作成日時</summary>
            public DateTime? CreatedAt { get; set; }
        }

        /// <summary>
        /// エクササイズタグデータ転送オブジェクト
        /// </summary>
        public sealed class ExerciseTag
        {
            /// <summary>タグID</summary>
            public string TagId { get; set; } = string.Empty;
            
            /// <summary>タグ名（日本語）</summary>
            public string TagName { get; set; } = string.Empty;
            
            /// <summary>タグ名（英語）</summary>
            public string TagNameEn { get; set; } = string.Empty;
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
                        TagIds = m.TrainingTags.Select(t => t.TagId ?? string.Empty).ToList()
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
                var tags = await _context.TagMasters
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
            
            /// <summary>関連タグIDのリスト（フロントエンド互換性）</summary>
            public List<string> TagIds { get; set; } = new();
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

        /// <summary>
        /// トレーニングレコード登録のテスト用エンドポイント
        /// </summary>
        [HttpGet("record/test")]
        public IActionResult TestRecordEndpoint()
        {
            return HttpResponseHelper.CreateSuccessResponse(new { message = "Record endpoint is working" });
        }

        /// <summary>
        /// トレーニングレコードを登録
        /// ワークアウトセッション完了時にフロントエンドから呼び出される
        /// </summary>
        /// <param name="request">トレーニングレコード登録リクエスト</param>
        /// <returns>登録されたレコードID</returns>
        [HttpPost("record")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CreateTrainingRecord([FromBody] CreateTrainingRecordRequest request)
        {
            var stopwatch = Stopwatch.StartNew();
            
            try
            {
                StructuredLogger.LogRequestStart(Request.Method, Request.Path.ToString(), nameof(CreateTrainingRecord));
                
                // リクエスト検証
                if (string.IsNullOrEmpty(request.MenuId))
                {
                    throw new AppException(ApplicationConstants.ErrorCodes.ParameterError, "メニューIDは必須です");
                }
                
                if (string.IsNullOrEmpty(request.TrainingDate))
                {
                    throw new AppException(ApplicationConstants.ErrorCodes.ParameterError, "トレーニング日は必須です");
                }
                
                if (request.Sets == null || !request.Sets.Any())
                {
                    throw new AppException(ApplicationConstants.ErrorCodes.ParameterError, "セットデータは必須です");
                }

                // データベース接続確認
                var isConnected = await _context.Database.CanConnectAsync();
                if (!isConnected)
                {
                    throw new AppException(ApplicationConstants.ErrorCodes.ServerError, "データベースに接続できません");
                }

                // メニューの存在確認
                var menu = await _context.TrainingMenus
                    .FirstOrDefaultAsync(m => m.MenuId == request.MenuId);
                
                if (menu == null)
                {
                    throw new AppException(ApplicationConstants.ErrorCodes.DataNotFound, $"メニューID '{request.MenuId}' が見つかりません");
                }

                // 認証されたユーザーIDを取得
                var userCommonId = GetRequiredUserId();
                
                // ExecutionStrategyを使用してトランザクションを実行
                var strategy = _context.Database.CreateExecutionStrategy();
                var recordId = string.Empty;
                var result = await strategy.ExecuteAsync(async () =>
                {
                    using var transaction = await _context.Database.BeginTransactionAsync();
                    
                    try
                    {
                        recordId = Guid.NewGuid().ToString();
                        var trainingDate = DateTime.Parse(request.TrainingDate);

                        // 各セットを TrainingRecordSets テーブルに保存
                        foreach (var set in request.Sets)
                        {
                            var recordSet = new TrainingRecordSet
                            {
                                UserCommonId = userCommonId,
                                MenuId = request.MenuId,
                                TrainingDate = DateOnly.FromDateTime(trainingDate),
                                SetNumber = set.SetNumber,
                                Reps = set.Reps,
                                Weight = set.Weight,
                                CreatedAt = DateTime.UtcNow
                            };

                            _context.TrainingRecordSets.Add(recordSet);
                        }

                        // 日次サマリーを DailyTrainingRecord テーブルに保存
                        var dailyRecord = await CreateOrUpdateDailyRecord(userCommonId, request.MenuId, trainingDate, request.Sets);
                        
                        await _context.SaveChangesAsync();
                        await transaction.CommitAsync();
                        
                        return new { recordId, dailyRecord };
                    }
                    catch
                    {
                        await transaction.RollbackAsync();
                        throw;
                    }
                });

                stopwatch.Stop();

                var response = new CreateTrainingRecordResponse
                {
                    RecordId = result.recordId,
                    DailyRecordId = $"{userCommonId}_{request.MenuId}_{request.TrainingDate}",
                    SetCount = request.Sets.Count,
                    TotalReps = request.Sets.Sum(s => s.Reps),
                    TotalVolume = request.Sets.Sum(s => s.Reps * (s.Weight ?? 0)),
                    MaxWeight = request.Sets.Max(s => s.Weight) ?? 0,
                    TrainingDate = request.TrainingDate
                };

                StructuredLogger.LogRequestEnd(Request.Method, Request.Path.ToString(), StatusCodes.Status200OK,
                    stopwatch.Elapsed, new { record_id = result.recordId, set_count = request.Sets.Count });

                StructuredLogger.LogBusinessEvent("TrainingRecordCreated", 
                    new { record_id = result.recordId, menu_id = request.MenuId, set_count = request.Sets.Count });

                return HttpResponseHelper.CreateSuccessResponse(response, "トレーニングレコードが正常に登録されました");
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                StructuredLogger.LogRequestEnd(Request.Method, Request.Path, 500, stopwatch.Elapsed, null);
                return HandleException(ex, nameof(CreateTrainingRecord));
            }
        }

        /// <summary>
        /// 日次トレーニングレコードを作成または更新
        /// </summary>
        private async Task<DailyTrainingRecord> CreateOrUpdateDailyRecord(string userCommonId, string menuId, DateTime trainingDate, List<TrainingSetRequest> sets)
        {
            var dateOnly = DateOnly.FromDateTime(trainingDate);
            
            // 既存の日次レコードを検索
            var existingRecord = await _context.DailyTrainingRecords
                .FirstOrDefaultAsync(d => d.UserCommonId == userCommonId 
                                       && d.MenuId == menuId 
                                       && d.TrainingDate == dateOnly);

            var setCount = sets.Count;
            var totalReps = sets.Sum(s => s.Reps);
            var maxReps = sets.Max(s => s.Reps);
            var maxWeight = sets.Max(s => s.Weight) ?? 0;
            var maxRepsWeight = sets.Where(s => s.Reps == maxReps).Max(s => s.Weight) ?? 0;
            var maxWeightReps = sets.Where(s => s.Weight == maxWeight).Max(s => s.Reps);
            var totalLoadAmount = sets.Sum(s => s.Reps * (s.Weight ?? 0));

            if (existingRecord != null)
            {
                // 既存レコードを更新
                existingRecord.SetCount = setCount;
                existingRecord.MaxReps = maxReps;
                existingRecord.MaxRepsWeight = maxRepsWeight;
                existingRecord.MaxWeight = maxWeight;
                existingRecord.MaxWeightReps = maxWeightReps;
                existingRecord.TotalLoadAmount = totalLoadAmount;
                existingRecord.TotalReps = totalReps;
                existingRecord.UpdatedAt = DateTime.UtcNow;
                
                return existingRecord;
            }
            else
            {
                // 新規レコードを作成
                var newRecord = new DailyTrainingRecord
                {
                    UserCommonId = userCommonId,
                    MenuId = menuId,
                    TrainingDate = dateOnly,
                    SetCount = setCount,
                    MaxReps = maxReps,
                    MaxRepsWeight = maxRepsWeight,
                    MaxWeight = maxWeight,
                    MaxWeightReps = maxWeightReps,
                    TotalLoadAmount = totalLoadAmount,
                    TotalReps = totalReps,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _context.DailyTrainingRecords.Add(newRecord);
                return newRecord;
            }
        }

        /// <summary>
        /// トレーニングレコード登録リクエスト
        /// </summary>
        public sealed class CreateTrainingRecordRequest
        {
            /// <summary>メニューID</summary>
            public string MenuId { get; set; } = string.Empty;
            
            /// <summary>トレーニング日（YYYY-MM-DD形式）</summary>
            public string TrainingDate { get; set; } = string.Empty;
            
            /// <summary>セットデータのリスト</summary>
            public List<TrainingSetRequest> Sets { get; set; } = new();
        }

        /// <summary>
        /// セットデータリクエスト
        /// </summary>
        public sealed class TrainingSetRequest
        {
            /// <summary>セット番号</summary>
            public int SetNumber { get; set; }
            
            /// <summary>回数</summary>
            public int Reps { get; set; }
            
            /// <summary>重量（kg）</summary>
            public decimal? Weight { get; set; }
            
            /// <summary>メモ</summary>
            public string? Note { get; set; }
        }

        /// <summary>
        /// トレーニングレコード登録レスポンス
        /// </summary>
        public sealed class CreateTrainingRecordResponse
        {
            /// <summary>登録されたレコードID</summary>
            public string RecordId { get; set; } = string.Empty;
            
            /// <summary>日次レコードID</summary>
            public string DailyRecordId { get; set; } = string.Empty;
            
            /// <summary>セット数</summary>
            public int SetCount { get; set; }
            
            /// <summary>総回数</summary>
            public int TotalReps { get; set; }
            
            /// <summary>総負荷量</summary>
            public decimal TotalVolume { get; set; }
            
            /// <summary>最大重量</summary>
            public decimal MaxWeight { get; set; }
            
            /// <summary>トレーニング日</summary>
            public string TrainingDate { get; set; } = string.Empty;
        }

        /// <summary>
        /// トレーニング履歴を取得
        /// 期間・メニュー・ユーザーでフィルタ可能
        /// </summary>
        /// <param name="menuId">メニューID（任意）</param>
        /// <param name="startDate">開始日（任意）</param>
        /// <param name="endDate">終了日（任意）</param>
        /// <param name="limit">取得件数制限（任意、デフォルト50）</param>
        /// <returns>履歴レコードリスト</returns>
        [HttpGet("history")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetTrainingHistory(
            [FromQuery] string? menuId = null,
            [FromQuery] string? startDate = null,
            [FromQuery] string? endDate = null,
            [FromQuery] int limit = 50)
        {
            var stopwatch = Stopwatch.StartNew();
            
            try
            {
                StructuredLogger.LogRequestStart(Request.Method, Request.Path.ToString(), nameof(GetTrainingHistory));
                
                // パラメータ検証
                if (limit <= 0 || limit > 100)
                {
                    throw new AppException(ApplicationConstants.ErrorCodes.ParameterError, "取得件数は1-100の範囲で指定してください");
                }

                DateOnly? startDateParsed = null;
                DateOnly? endDateParsed = null;

                if (!string.IsNullOrEmpty(startDate))
                {
                    if (!DateOnly.TryParse(startDate, out var parsed))
                    {
                        throw new AppException(ApplicationConstants.ErrorCodes.ParameterError, "開始日の形式が正しくありません");
                    }
                    startDateParsed = parsed;
                }

                if (!string.IsNullOrEmpty(endDate))
                {
                    if (!DateOnly.TryParse(endDate, out var parsed))
                    {
                        throw new AppException(ApplicationConstants.ErrorCodes.ParameterError, "終了日の形式が正しくありません");
                    }
                    endDateParsed = parsed;
                }

                if (startDateParsed.HasValue && endDateParsed.HasValue && startDateParsed > endDateParsed)
                {
                    throw new AppException(ApplicationConstants.ErrorCodes.ParameterError, "開始日は終了日より前の日付である必要があります");
                }

                // データベース接続確認
                var isConnected = await _context.Database.CanConnectAsync();
                if (!isConnected)
                {
                    throw new AppException(ApplicationConstants.ErrorCodes.ServerError, "データベースに接続できません");
                }

                var userCommonId = GetRequiredUserId(); // 認証されたユーザーIDを取得

                // 日次レコードを取得
                var query = _context.DailyTrainingRecords
                    .Include(d => d.Menu)
                    .Where(d => d.UserCommonId == userCommonId);

                // フィルタ適用
                if (!string.IsNullOrEmpty(menuId))
                {
                    query = query.Where(d => d.MenuId == menuId);
                }

                if (startDateParsed.HasValue)
                {
                    query = query.Where(d => d.TrainingDate >= startDateParsed.Value);
                }

                if (endDateParsed.HasValue)
                {
                    query = query.Where(d => d.TrainingDate <= endDateParsed.Value);
                }

                var records = await query
                    .OrderByDescending(d => d.TrainingDate)
                    .ThenByDescending(d => d.CreatedAt)
                    .Take(limit)
                    .AsNoTracking()
                    .ToListAsync();

                // レスポンス用データに変換
                var historyResponse = records.Select(r => new TrainingHistoryResponse
                {
                    RecordId = $"{r.UserCommonId}_{r.MenuId}_{r.TrainingDate:yyyy-MM-dd}",
                    MenuId = r.MenuId ?? string.Empty,
                    MenuName = r.Menu?.Jpname ?? "不明なメニュー",
                    TrainingDate = r.TrainingDate.ToString("yyyy-MM-dd"),
                    SetCount = r.SetCount,
                    TotalReps = r.TotalReps,
                    MaxReps = r.MaxReps,
                    MaxWeight = r.MaxWeight ?? 0,
                    MaxRepsWeight = r.MaxRepsWeight ?? 0,
                    MaxWeightReps = r.MaxWeightReps ?? 0,
                    TotalLoadAmount = r.TotalLoadAmount ?? 0,
                    CreatedAt = r.CreatedAt,
                    UpdatedAt = r.UpdatedAt
                }).ToList();

                stopwatch.Stop();

                var responseData = new
                {
                    records = historyResponse,
                    meta = new
                    {
                        total_count = historyResponse.Count,
                        has_more = historyResponse.Count == limit,
                        filters = new
                        {
                            menu_id = menuId,
                            start_date = startDate,
                            end_date = endDate,
                            limit = limit
                        }
                    }
                };

                StructuredLogger.LogRequestEnd(Request.Method, Request.Path.ToString(), StatusCodes.Status200OK,
                    stopwatch.Elapsed, new { record_count = historyResponse.Count });

                StructuredLogger.LogBusinessEvent("TrainingHistoryRetrieved", 
                    new { record_count = historyResponse.Count, menu_id = menuId });

                return HttpResponseHelper.CreateSuccessResponse(responseData);
            }
            catch (AppException ex)
            {
                stopwatch.Stop();
                StructuredLogger.LogError($"Error in {nameof(GetTrainingHistory)}: {ex.UserMessage}", ex, 
                    new { elapsed_ms = stopwatch.ElapsedMilliseconds });
                StructuredLogger.LogRequestEnd(Request.Method, Request.Path, 400, stopwatch.Elapsed, null);
                
                return HttpResponseHelper.CreateErrorResponse(ex.ErrorCode, ex.UserMessage);
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                var appEx = new AppException(ApplicationConstants.ErrorCodes.ServerError, 
                    "トレーニング履歴の取得中にエラーが発生しました", ex);
                StructuredLogger.LogUnhandledException(ex, new { action = nameof(GetTrainingHistory), path = Request.Path.ToString() });
                StructuredLogger.LogRequestEnd(Request.Method, Request.Path, 500, stopwatch.Elapsed, null);
                
                return HttpResponseHelper.CreateErrorResponse(appEx.ErrorCode, appEx.UserMessage);
            }
        }

        /// <summary>
        /// 特定日のトレーニング詳細セット記録を取得
        /// </summary>
        /// <param name="menuId">メニューID</param>
        /// <param name="trainingDate">トレーニング日（YYYY-MM-DD形式）</param>
        /// <returns>セット詳細記録リスト</returns>
        [HttpGet("history/details")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetTrainingHistoryDetails(
            [FromQuery] string menuId,
            [FromQuery] string trainingDate)
        {
            var stopwatch = Stopwatch.StartNew();
            
            try
            {
                StructuredLogger.LogRequestStart(Request.Method, Request.Path.ToString(), nameof(GetTrainingHistoryDetails));
                
                // パラメータ検証
                if (string.IsNullOrEmpty(menuId))
                {
                    throw new AppException(ApplicationConstants.ErrorCodes.ParameterError, "メニューIDは必須です");
                }
                
                if (string.IsNullOrEmpty(trainingDate))
                {
                    throw new AppException(ApplicationConstants.ErrorCodes.ParameterError, "トレーニング日は必須です");
                }

                if (!DateOnly.TryParse(trainingDate, out var dateParsed))
                {
                    throw new AppException(ApplicationConstants.ErrorCodes.ParameterError, "トレーニング日の形式が正しくありません");
                }

                // データベース接続確認
                var isConnected = await _context.Database.CanConnectAsync();
                if (!isConnected)
                {
                    throw new AppException(ApplicationConstants.ErrorCodes.ServerError, "データベースに接続できません");
                }

                var userCommonId = GetRequiredUserId(); // 認証されたユーザーIDを取得

                // セット詳細を取得
                var setRecords = await _context.TrainingRecordSets
                    .Where(s => s.UserCommonId == userCommonId 
                             && s.MenuId == menuId 
                             && s.TrainingDate == dateParsed)
                    .OrderBy(s => s.SetNumber)
                    .AsNoTracking()
                    .ToListAsync();

                if (!setRecords.Any())
                {
                    throw new AppException(ApplicationConstants.ErrorCodes.DataNotFound, 
                        "指定された日付・メニューのトレーニング記録が見つかりません");
                }

                // レスポンス用データに変換
                var detailsResponse = setRecords.Select(s => new TrainingSetDetailsResponse
                {
                    SetNumber = s.SetNumber,
                    Reps = s.Reps,
                    Weight = s.Weight,
                    CreatedAt = s.CreatedAt
                }).ToList();

                stopwatch.Stop();

                var responseData = new
                {
                    menu_id = menuId,
                    training_date = trainingDate,
                    sets = detailsResponse,
                    summary = new
                    {
                        total_sets = detailsResponse.Count,
                        total_reps = detailsResponse.Sum(s => s.Reps),
                        max_weight = detailsResponse.Max(s => s.Weight) ?? 0,
                        total_volume = detailsResponse.Sum(s => s.Reps * (s.Weight ?? 0))
                    }
                };

                StructuredLogger.LogRequestEnd(Request.Method, Request.Path.ToString(), StatusCodes.Status200OK,
                    stopwatch.Elapsed, new { set_count = detailsResponse.Count });

                return HttpResponseHelper.CreateSuccessResponse(responseData);
            }
            catch (AppException ex)
            {
                stopwatch.Stop();
                StructuredLogger.LogError($"Error in {nameof(GetTrainingHistoryDetails)}: {ex.UserMessage}", ex, 
                    new { elapsed_ms = stopwatch.ElapsedMilliseconds });
                
                var statusCode = ex.ErrorCode == ApplicationConstants.ErrorCodes.DataNotFound ? 404 : 400;
                StructuredLogger.LogRequestEnd(Request.Method, Request.Path, statusCode, stopwatch.Elapsed, null);
                
                return HttpResponseHelper.CreateErrorResponse(ex.ErrorCode, ex.UserMessage);
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                var appEx = new AppException(ApplicationConstants.ErrorCodes.ServerError, 
                    "トレーニング詳細の取得中にエラーが発生しました", ex);
                StructuredLogger.LogUnhandledException(ex, new { action = nameof(GetTrainingHistoryDetails), path = Request.Path.ToString() });
                StructuredLogger.LogRequestEnd(Request.Method, Request.Path, 500, stopwatch.Elapsed, null);
                
                return HttpResponseHelper.CreateErrorResponse(appEx.ErrorCode, appEx.UserMessage);
            }
        }

        /// <summary>
        /// トレーニング履歴レスポンス用データ転送オブジェクト
        /// </summary>
        public sealed class TrainingHistoryResponse
        {
            /// <summary>レコードID（ユニーク識別子）</summary>
            public string RecordId { get; set; } = string.Empty;
            
            /// <summary>メニューID</summary>
            public string MenuId { get; set; } = string.Empty;
            
            /// <summary>メニュー名</summary>
            public string MenuName { get; set; } = string.Empty;
            
            /// <summary>トレーニング日</summary>
            public string TrainingDate { get; set; } = string.Empty;
            
            /// <summary>セット数</summary>
            public int SetCount { get; set; }
            
            /// <summary>総回数</summary>
            public int TotalReps { get; set; }
            
            /// <summary>最大回数</summary>
            public int MaxReps { get; set; }
            
            /// <summary>最大重量</summary>
            public decimal MaxWeight { get; set; }
            
            /// <summary>最大回数時の重量</summary>
            public decimal MaxRepsWeight { get; set; }
            
            /// <summary>最大重量時の回数</summary>
            public int MaxWeightReps { get; set; }
            
            /// <summary>総負荷量</summary>
            public decimal TotalLoadAmount { get; set; }
            
            /// <summary>作成日時</summary>
            public DateTime? CreatedAt { get; set; }
            
            /// <summary>更新日時</summary>
            public DateTime? UpdatedAt { get; set; }
        }

        /// <summary>
        /// トレーニングセット詳細レスポンス用データ転送オブジェクト
        /// </summary>
        public sealed class TrainingSetDetailsResponse
        {
            /// <summary>セット番号</summary>
            public int SetNumber { get; set; }
            
            /// <summary>回数</summary>
            public int Reps { get; set; }
            
            /// <summary>重量（kg）</summary>
            public decimal? Weight { get; set; }
            
            /// <summary>作成日時</summary>
            public DateTime? CreatedAt { get; set; }
        }

        /// <summary>
        /// トレーニングプリセット一覧を取得
        /// </summary>
        /// <returns>プリセットリスト</returns>
        [HttpGet("presets")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetTrainingPresets()
        {
            var stopwatch = Stopwatch.StartNew();
            
            try
            {
                StructuredLogger.LogRequestStart(Request.Method, Request.Path.ToString(), nameof(GetTrainingPresets));
                
                // データベース接続確認
                var isConnected = await _context.Database.CanConnectAsync();
                if (!isConnected)
                {
                    throw new AppException(ApplicationConstants.ErrorCodes.ServerError, "データベースに接続できません");
                }

                // 認証されたユーザーIDを取得（実際のDB実装時に使用）
                // var userCommonId = GetRequiredUserId();

                // シンプルなプリセット実装（実際のテーブルがない場合のダミーデータ）
                var presets = new List<TrainingPresetResponse>
                {
                    new()
                    {
                        PresetId = "preset_1",
                        Name = "胸トレーニング基本",
                        Description = "ベンチプレス中心の胸トレーニング",
                        MenuId = "bench_press",
                        MenuName = "ベンチプレス",
                        DefaultSets = new List<PresetSetData>
                        {
                            new() { SetNumber = 1, Reps = 10, Weight = 60 },
                            new() { SetNumber = 2, Reps = 8, Weight = 70 },
                            new() { SetNumber = 3, Reps = 6, Weight = 80 }
                        },
                        CreatedAt = DateTime.UtcNow.AddDays(-7),
                        IsDefault = true
                    },
                    new()
                    {
                        PresetId = "preset_2", 
                        Name = "スクワット強化",
                        Description = "下半身強化プログラム",
                        MenuId = "squat",
                        MenuName = "スクワット",
                        DefaultSets = new List<PresetSetData>
                        {
                            new() { SetNumber = 1, Reps = 12, Weight = 80 },
                            new() { SetNumber = 2, Reps = 10, Weight = 90 },
                            new() { SetNumber = 3, Reps = 8, Weight = 100 },
                            new() { SetNumber = 4, Reps = 6, Weight = 110 }
                        },
                        CreatedAt = DateTime.UtcNow.AddDays(-3),
                        IsDefault = false
                    }
                };

                stopwatch.Stop();

                var responseData = new
                {
                    presets = presets,
                    meta = new
                    {
                        total_count = presets.Count
                        // user_id は実際のDB実装時に追加
                    }
                };

                StructuredLogger.LogRequestEnd(Request.Method, Request.Path.ToString(), StatusCodes.Status200OK,
                    stopwatch.Elapsed, new { preset_count = presets.Count });

                return HttpResponseHelper.CreateSuccessResponse(responseData);
            }
            catch (AppException ex)
            {
                stopwatch.Stop();
                StructuredLogger.LogError($"Error in {nameof(GetTrainingPresets)}: {ex.UserMessage}", ex, 
                    new { elapsed_ms = stopwatch.ElapsedMilliseconds });
                StructuredLogger.LogRequestEnd(Request.Method, Request.Path, 500, stopwatch.Elapsed, null);
                
                return HttpResponseHelper.CreateErrorResponse(ex.ErrorCode, ex.UserMessage);
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                var appEx = new AppException(ApplicationConstants.ErrorCodes.ServerError, 
                    "プリセットの取得中にエラーが発生しました", ex);
                StructuredLogger.LogUnhandledException(ex, new { action = nameof(GetTrainingPresets), path = Request.Path.ToString() });
                StructuredLogger.LogRequestEnd(Request.Method, Request.Path, 500, stopwatch.Elapsed, null);
                
                return HttpResponseHelper.CreateErrorResponse(appEx.ErrorCode, appEx.UserMessage);
            }
        }

        /// <summary>
        /// 新しいトレーニングプリセットを作成
        /// </summary>
        /// <param name="request">プリセット作成リクエスト</param>
        /// <returns>作成されたプリセットID</returns>
        [HttpPost("presets")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CreateTrainingPreset([FromBody] CreatePresetRequest request)
        {
            var stopwatch = Stopwatch.StartNew();
            
            try
            {
                StructuredLogger.LogRequestStart(Request.Method, Request.Path.ToString(), nameof(CreateTrainingPreset));
                
                // リクエスト検証
                if (string.IsNullOrEmpty(request.Name))
                {
                    throw new AppException(ApplicationConstants.ErrorCodes.ParameterError, "プリセット名は必須です");
                }
                
                if (string.IsNullOrEmpty(request.MenuId))
                {
                    throw new AppException(ApplicationConstants.ErrorCodes.ParameterError, "メニューIDは必須です");
                }
                
                if (request.DefaultSets == null || !request.DefaultSets.Any())
                {
                    throw new AppException(ApplicationConstants.ErrorCodes.ParameterError, "セットデータは必須です");
                }

                // データベース接続確認
                var isConnected = await _context.Database.CanConnectAsync();
                if (!isConnected)
                {
                    throw new AppException(ApplicationConstants.ErrorCodes.ServerError, "データベースに接続できません");
                }

                // メニューの存在確認
                var menu = await _context.TrainingMenus
                    .FirstOrDefaultAsync(m => m.MenuId == request.MenuId);
                
                if (menu == null)
                {
                    throw new AppException(ApplicationConstants.ErrorCodes.DataNotFound, $"メニューID '{request.MenuId}' が見つかりません");
                }

                // 認証されたユーザーIDを取得（実際のDB実装時に使用）
                // var userCommonId = GetRequiredUserId();
                var presetId = Guid.NewGuid().ToString();

                // シンプルなプリセット保存（実際のテーブル実装は後で）
                stopwatch.Stop();

                var response = new CreatePresetResponse
                {
                    PresetId = presetId,
                    Name = request.Name,
                    MenuId = request.MenuId,
                    SetCount = request.DefaultSets.Count,
                    CreatedAt = DateTime.UtcNow
                };

                StructuredLogger.LogRequestEnd(Request.Method, Request.Path.ToString(), StatusCodes.Status200OK,
                    stopwatch.Elapsed, new { preset_id = presetId });

                StructuredLogger.LogBusinessEvent("TrainingPresetCreated", 
                    new { preset_id = presetId, menu_id = request.MenuId, set_count = request.DefaultSets.Count });

                return HttpResponseHelper.CreateSuccessResponse(response, "プリセットが正常に作成されました");
            }
            catch (AppException ex)
            {
                stopwatch.Stop();
                StructuredLogger.LogError($"Error in {nameof(CreateTrainingPreset)}: {ex.UserMessage}", ex, 
                    new { elapsed_ms = stopwatch.ElapsedMilliseconds });
                StructuredLogger.LogRequestEnd(Request.Method, Request.Path, 400, stopwatch.Elapsed, null);
                
                return HttpResponseHelper.CreateErrorResponse(ex.ErrorCode, ex.UserMessage);
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                var appEx = new AppException(ApplicationConstants.ErrorCodes.ServerError, 
                    "プリセットの作成中にエラーが発生しました", ex);
                StructuredLogger.LogUnhandledException(ex, new { action = nameof(CreateTrainingPreset), path = Request.Path.ToString() });
                StructuredLogger.LogRequestEnd(Request.Method, Request.Path, 500, stopwatch.Elapsed, null);
                
                return HttpResponseHelper.CreateErrorResponse(appEx.ErrorCode, appEx.UserMessage);
            }
        }

        /// <summary>
        /// トレーニングプリセットレスポンス用データ転送オブジェクト
        /// </summary>
        public sealed class TrainingPresetResponse
        {
            /// <summary>プリセットID</summary>
            public string PresetId { get; set; } = string.Empty;
            
            /// <summary>プリセット名</summary>
            public string Name { get; set; } = string.Empty;
            
            /// <summary>説明</summary>
            public string? Description { get; set; }
            
            /// <summary>メニューID</summary>
            public string MenuId { get; set; } = string.Empty;
            
            /// <summary>メニュー名</summary>
            public string MenuName { get; set; } = string.Empty;
            
            /// <summary>デフォルトセットデータ</summary>
            public List<PresetSetData> DefaultSets { get; set; } = new();
            
            /// <summary>作成日時</summary>
            public DateTime? CreatedAt { get; set; }
            
            /// <summary>デフォルトプリセットかどうか</summary>
            public bool IsDefault { get; set; }
        }

        /// <summary>
        /// プリセットセットデータ
        /// </summary>
        public sealed class PresetSetData
        {
            /// <summary>セット番号</summary>
            public int SetNumber { get; set; }
            
            /// <summary>回数</summary>
            public int Reps { get; set; }
            
            /// <summary>重量（kg）</summary>
            public decimal? Weight { get; set; }
            
            /// <summary>メモ</summary>
            public string? Note { get; set; }
        }

        /// <summary>
        /// プリセット作成リクエスト
        /// </summary>
        public sealed class CreatePresetRequest
        {
            /// <summary>プリセット名</summary>
            public string Name { get; set; } = string.Empty;
            
            /// <summary>説明</summary>
            public string? Description { get; set; }
            
            /// <summary>メニューID</summary>
            public string MenuId { get; set; } = string.Empty;
            
            /// <summary>デフォルトセットデータ</summary>
            public List<PresetSetData> DefaultSets { get; set; } = new();
        }

        /// <summary>
        /// プリセット作成レスポンス
        /// </summary>
        public sealed class CreatePresetResponse
        {
            /// <summary>作成されたプリセットID</summary>
            public string PresetId { get; set; } = string.Empty;
            
            /// <summary>プリセット名</summary>
            public string Name { get; set; } = string.Empty;
            
            /// <summary>メニューID</summary>
            public string MenuId { get; set; } = string.Empty;
            
            /// <summary>セット数</summary>
            public int SetCount { get; set; }
            
            /// <summary>作成日時</summary>
            public DateTime CreatedAt { get; set; }
        }

        // ===================================
        // Schedule Management
        // ===================================

        /// <summary>
        /// トレーニング予定一覧を取得
        /// </summary>
        /// <param name="startDate">開始日（任意）</param>
        /// <param name="endDate">終了日（任意）</param>
        /// <param name="menuId">メニューID（任意）</param>
        /// <returns>予定リスト</returns>
        [HttpGet("schedules")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetTrainingSchedules(
            [FromQuery] string? startDate = null,
            [FromQuery] string? endDate = null,
            [FromQuery] string? menuId = null)
        {
            var stopwatch = Stopwatch.StartNew();
            
            try
            {
                StructuredLogger.LogRequestStart(Request.Method, Request.Path.ToString(), nameof(GetTrainingSchedules));
                
                // パラメータ検証
                DateOnly? startDateParsed = null;
                DateOnly? endDateParsed = null;

                if (!string.IsNullOrEmpty(startDate))
                {
                    if (!DateOnly.TryParse(startDate, out var parsed))
                    {
                        throw new AppException(ApplicationConstants.ErrorCodes.ParameterError, "開始日の形式が正しくありません");
                    }
                    startDateParsed = parsed;
                }

                if (!string.IsNullOrEmpty(endDate))
                {
                    if (!DateOnly.TryParse(endDate, out var parsed))
                    {
                        throw new AppException(ApplicationConstants.ErrorCodes.ParameterError, "終了日の形式が正しくありません");
                    }
                    endDateParsed = parsed;
                }

                // データベース接続確認
                var isConnected = await _context.Database.CanConnectAsync();
                if (!isConnected)
                {
                    throw new AppException(ApplicationConstants.ErrorCodes.ServerError, "データベースに接続できません");
                }

                // 認証されたユーザーIDを取得（実際のDB実装時に使用）
                // var userCommonId = GetRequiredUserId();

                // シンプルなスケジュール実装（実際のテーブルがない場合のダミーデータ）
                var schedules = new List<TrainingScheduleResponse>
                {
                    new()
                    {
                        ScheduleId = "sch_1",
                        MenuId = "bench_press",
                        MenuName = "ベンチプレス",
                        PresetId = "preset_1",
                        PresetName = "胸トレーニング基本",
                        ScheduledDate = "2024-01-15",
                        ScheduledTime = "10:00",
                        Notes = "胸のトレーニング強化週間",
                        CreatedAt = DateTime.UtcNow.AddDays(-2),
                        IsCompleted = false
                    },
                    new()
                    {
                        ScheduleId = "sch_2",
                        MenuId = "squat",
                        MenuName = "スクワット",
                        ScheduledDate = "2024-01-16",
                        ScheduledTime = "14:00",
                        Notes = "下半身強化",
                        CreatedAt = DateTime.UtcNow.AddDays(-1),
                        IsCompleted = false
                    }
                };

                // フィルタ適用
                if (!string.IsNullOrEmpty(menuId))
                {
                    schedules = schedules.Where(s => s.MenuId == menuId).ToList();
                }

                if (startDateParsed.HasValue)
                {
                    schedules = schedules.Where(s => DateOnly.Parse(s.ScheduledDate) >= startDateParsed.Value).ToList();
                }

                if (endDateParsed.HasValue)
                {
                    schedules = schedules.Where(s => DateOnly.Parse(s.ScheduledDate) <= endDateParsed.Value).ToList();
                }

                stopwatch.Stop();

                var responseData = new
                {
                    schedules = schedules.OrderBy(s => s.ScheduledDate).ToList(),
                    meta = new
                    {
                        total_count = schedules.Count,
                        filters = new
                        {
                            start_date = startDate,
                            end_date = endDate,
                            menu_id = menuId
                        }
                    }
                };

                StructuredLogger.LogRequestEnd(Request.Method, Request.Path.ToString(), StatusCodes.Status200OK,
                    stopwatch.Elapsed, new { schedule_count = schedules.Count });

                return HttpResponseHelper.CreateSuccessResponse(responseData);
            }
            catch (AppException ex)
            {
                stopwatch.Stop();
                StructuredLogger.LogError($"Error in {nameof(GetTrainingSchedules)}: {ex.UserMessage}", ex, 
                    new { elapsed_ms = stopwatch.ElapsedMilliseconds });
                StructuredLogger.LogRequestEnd(Request.Method, Request.Path, 400, stopwatch.Elapsed, null);
                
                return HttpResponseHelper.CreateErrorResponse(ex.ErrorCode, ex.UserMessage);
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                var appEx = new AppException(ApplicationConstants.ErrorCodes.ServerError, 
                    "予定の取得中にエラーが発生しました", ex);
                StructuredLogger.LogUnhandledException(ex, new { action = nameof(GetTrainingSchedules), path = Request.Path.ToString() });
                StructuredLogger.LogRequestEnd(Request.Method, Request.Path, 500, stopwatch.Elapsed, null);
                
                return HttpResponseHelper.CreateErrorResponse(appEx.ErrorCode, appEx.UserMessage);
            }
        }

        /// <summary>
        /// 新しいトレーニング予定を作成
        /// </summary>
        /// <param name="request">予定作成リクエスト</param>
        /// <returns>作成された予定ID</returns>
        [HttpPost("schedules")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CreateTrainingSchedule([FromBody] CreateScheduleRequest request)
        {
            var stopwatch = Stopwatch.StartNew();
            
            try
            {
                StructuredLogger.LogRequestStart(Request.Method, Request.Path.ToString(), nameof(CreateTrainingSchedule));
                
                // リクエスト検証
                if (string.IsNullOrEmpty(request.MenuId))
                {
                    throw new AppException(ApplicationConstants.ErrorCodes.ParameterError, "メニューIDは必須です");
                }
                
                if (string.IsNullOrEmpty(request.ScheduledDate))
                {
                    throw new AppException(ApplicationConstants.ErrorCodes.ParameterError, "予定日は必須です");
                }

                if (!DateOnly.TryParse(request.ScheduledDate, out var scheduledDate))
                {
                    throw new AppException(ApplicationConstants.ErrorCodes.ParameterError, "予定日の形式が正しくありません");
                }

                // データベース接続確認
                var isConnected = await _context.Database.CanConnectAsync();
                if (!isConnected)
                {
                    throw new AppException(ApplicationConstants.ErrorCodes.ServerError, "データベースに接続できません");
                }

                // メニューの存在確認
                var menu = await _context.TrainingMenus
                    .FirstOrDefaultAsync(m => m.MenuId == request.MenuId);
                
                if (menu == null)
                {
                    throw new AppException(ApplicationConstants.ErrorCodes.DataNotFound, $"メニューID '{request.MenuId}' が見つかりません");
                }

                // 認証されたユーザーIDを取得（実際のDB実装時に使用）
                // var userCommonId = GetRequiredUserId();
                var scheduleId = Guid.NewGuid().ToString();

                // シンプルな予定保存（実際のテーブル実装は後で）
                stopwatch.Stop();

                var response = new CreateScheduleResponse
                {
                    ScheduleId = scheduleId,
                    MenuId = request.MenuId,
                    ScheduledDate = request.ScheduledDate,
                    ScheduledTime = request.ScheduledTime,
                    CreatedAt = DateTime.UtcNow
                };

                StructuredLogger.LogRequestEnd(Request.Method, Request.Path.ToString(), StatusCodes.Status200OK,
                    stopwatch.Elapsed, new { schedule_id = scheduleId });

                StructuredLogger.LogBusinessEvent("TrainingScheduleCreated", 
                    new { schedule_id = scheduleId, menu_id = request.MenuId, scheduled_date = request.ScheduledDate });

                return HttpResponseHelper.CreateSuccessResponse(response, "予定が正常に作成されました");
            }
            catch (AppException ex)
            {
                stopwatch.Stop();
                StructuredLogger.LogError($"Error in {nameof(CreateTrainingSchedule)}: {ex.UserMessage}", ex, 
                    new { elapsed_ms = stopwatch.ElapsedMilliseconds });
                StructuredLogger.LogRequestEnd(Request.Method, Request.Path, 400, stopwatch.Elapsed, null);
                
                return HttpResponseHelper.CreateErrorResponse(ex.ErrorCode, ex.UserMessage);
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                var appEx = new AppException(ApplicationConstants.ErrorCodes.ServerError, 
                    "予定の作成中にエラーが発生しました", ex);
                StructuredLogger.LogUnhandledException(ex, new { action = nameof(CreateTrainingSchedule), path = Request.Path.ToString() });
                StructuredLogger.LogRequestEnd(Request.Method, Request.Path, 500, stopwatch.Elapsed, null);
                
                return HttpResponseHelper.CreateErrorResponse(appEx.ErrorCode, appEx.UserMessage);
            }
        }

        /// <summary>
        /// トレーニング予定レスポンス用データ転送オブジェクト
        /// </summary>
        public sealed class TrainingScheduleResponse
        {
            /// <summary>予定ID</summary>
            public string ScheduleId { get; set; } = string.Empty;
            
            /// <summary>メニューID</summary>
            public string MenuId { get; set; } = string.Empty;
            
            /// <summary>メニュー名</summary>
            public string MenuName { get; set; } = string.Empty;
            
            /// <summary>プリセットID</summary>
            public string? PresetId { get; set; }
            
            /// <summary>プリセット名</summary>
            public string? PresetName { get; set; }
            
            /// <summary>予定日（YYYY-MM-DD形式）</summary>
            public string ScheduledDate { get; set; } = string.Empty;
            
            /// <summary>予定時刻（HH:MM形式）</summary>
            public string? ScheduledTime { get; set; }
            
            /// <summary>メモ</summary>
            public string? Notes { get; set; }
            
            /// <summary>作成日時</summary>
            public DateTime CreatedAt { get; set; }
            
            /// <summary>完了フラグ</summary>
            public bool IsCompleted { get; set; }
        }

        /// <summary>
        /// 予定作成リクエスト
        /// </summary>
        public sealed class CreateScheduleRequest
        {
            /// <summary>メニューID</summary>
            public string MenuId { get; set; } = string.Empty;
            
            /// <summary>プリセットID（任意）</summary>
            public string? PresetId { get; set; }
            
            /// <summary>予定日（YYYY-MM-DD形式）</summary>
            public string ScheduledDate { get; set; } = string.Empty;
            
            /// <summary>予定時刻（HH:MM形式、任意）</summary>
            public string? ScheduledTime { get; set; }
            
            /// <summary>メモ（任意）</summary>
            public string? Notes { get; set; }
        }

        /// <summary>
        /// 予定作成レスポンス
        /// </summary>
        public sealed class CreateScheduleResponse
        {
            /// <summary>作成された予定ID</summary>
            public string ScheduleId { get; set; } = string.Empty;
            
            /// <summary>メニューID</summary>
            public string MenuId { get; set; } = string.Empty;
            
            /// <summary>予定日</summary>
            public string ScheduledDate { get; set; } = string.Empty;
            
            /// <summary>予定時刻</summary>
            public string? ScheduledTime { get; set; }
            
            /// <summary>作成日時</summary>
            public DateTime CreatedAt { get; set; }
        }
    }
}