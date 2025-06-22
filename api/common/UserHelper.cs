using Api.Models;
using Api.Common;
using Microsoft.EntityFrameworkCore;

namespace Api.Common
{
    /// <summary>
    /// ユーザー関連のヘルパークラス
    /// データベースアクセスを伴うユーザー操作を提供
    /// </summary>
    public static class UserHelper
    {
        /// <summary>
        /// ログインIDからユーザーオブジェクトを取得
        /// ユーザーが見つからない場合は例外を発生させる
        /// </summary>
        /// <param name="context">データベースコンテキスト</param>
        /// <param name="loginId">ログインID</param>
        /// <returns>ユーザー情報</returns>
        /// <exception cref="AppException">ユーザーが見つからない場合</exception>
        public static User GetUser(TrecPlansRDBContext context, string loginId)
        {
            if (context == null) 
                throw new ArgumentNullException(nameof(context));
            
            if (string.IsNullOrWhiteSpace(loginId)) 
                throw new AppException(ApplicationConstants.ErrorCodes.ParameterError, "ログインIDが指定されていません");

            User? userData = context.Users
                .Where(x => x.LoginId == loginId)
                .FirstOrDefault();

            if (userData == null)
            {
                throw new AppException(ApplicationConstants.ErrorCodes.UserNotFound, "指定されたユーザーが見つかりません");
            }
            
            return userData;
        }

        /// <summary>
        /// ユーザー共通IDからユーザーオブジェクトを取得
        /// ユーザーが見つからない場合は例外を発生させる
        /// </summary>
        /// <param name="context">データベースコンテキスト</param>
        /// <param name="userCommonId">ユーザー共通ID</param>
        /// <returns>ユーザー情報</returns>
        /// <exception cref="AppException">ユーザーが見つからない場合</exception>
        public static User GetUserByCommonId(TrecPlansRDBContext context, string userCommonId)
        {
            if (context == null) 
                throw new ArgumentNullException(nameof(context));
            
            if (string.IsNullOrWhiteSpace(userCommonId)) 
                throw new AppException(ApplicationConstants.ErrorCodes.ParameterError, "ユーザー共通IDが指定されていません");

            User? userData = context.Users
                .Where(x => x.UserCommonId == userCommonId)
                .FirstOrDefault();

            if (userData == null)
            {
                throw new AppException(ApplicationConstants.ErrorCodes.UserNotFound, "指定されたユーザーが見つかりません");
            }
            
            return userData;
        }

        /// <summary>
        /// ログインIDからユーザーオブジェクトを非同期で取得
        /// パフォーマンス向上のための非同期版
        /// </summary>
        /// <param name="context">データベースコンテキスト</param>
        /// <param name="loginId">ログインID</param>
        /// <returns>ユーザー情報</returns>
        /// <exception cref="AppException">ユーザーが見つからない場合</exception>
        public static async Task<User> GetUserAsync(TrecPlansRDBContext context, string loginId)
        {
            if (context == null) 
                throw new ArgumentNullException(nameof(context));
            
            if (string.IsNullOrWhiteSpace(loginId)) 
                throw new AppException(ApplicationConstants.ErrorCodes.ParameterError, "ログインIDが指定されていません");

            User? userData = await context.Users
                .Where(x => x.LoginId == loginId)
                .FirstOrDefaultAsync();

            if (userData == null)
            {
                throw new AppException(ApplicationConstants.ErrorCodes.UserNotFound, "指定されたユーザーが見つかりません");
            }
            
            return userData;
        }

        /// <summary>
        /// ユーザー共通IDからユーザーオブジェクトを非同期で取得
        /// パフォーマンス向上のための非同期版
        /// </summary>
        /// <param name="context">データベースコンテキスト</param>
        /// <param name="userCommonId">ユーザー共通ID</param>
        /// <returns>ユーザー情報</returns>
        /// <exception cref="AppException">ユーザーが見つからない場合</exception>
        public static async Task<User> GetUserByCommonIdAsync(TrecPlansRDBContext context, string userCommonId)
        {
            if (context == null) 
                throw new ArgumentNullException(nameof(context));
            
            if (string.IsNullOrWhiteSpace(userCommonId)) 
                throw new AppException(ApplicationConstants.ErrorCodes.ParameterError, "ユーザー共通IDが指定されていません");

            User? userData = await context.Users
                .Where(x => x.UserCommonId == userCommonId)
                .FirstOrDefaultAsync();

            if (userData == null)
            {
                throw new AppException(ApplicationConstants.ErrorCodes.UserNotFound, "指定されたユーザーが見つかりません");
            }
            
            return userData;
        }

        /// <summary>
        /// ログインIDの重複チェック
        /// 新規ユーザー登録時の重複確認に使用
        /// </summary>
        /// <param name="context">データベースコンテキスト</param>
        /// <param name="loginId">チェックするログインID</param>
        /// <returns>重複している場合true</returns>
        public static async Task<bool> IsLoginIdDuplicateAsync(TrecPlansRDBContext context, string loginId)
        {
            if (context == null) 
                throw new ArgumentNullException(nameof(context));
            
            if (string.IsNullOrWhiteSpace(loginId)) 
                return false;

            return await context.Users
                .AnyAsync(x => x.LoginId == loginId);
        }

        /// <summary>
        /// ユーザー共通IDの重複チェック
        /// 新規ユーザー登録時の重複確認に使用
        /// </summary>
        /// <param name="context">データベースコンテキスト</param>
        /// <param name="userCommonId">チェックするユーザー共通ID</param>
        /// <returns>重複している場合true</returns>
        public static async Task<bool> IsUserCommonIdDuplicateAsync(TrecPlansRDBContext context, string userCommonId)
        {
            if (context == null) 
                throw new ArgumentNullException(nameof(context));
            
            if (string.IsNullOrWhiteSpace(userCommonId)) 
                return false;

            return await context.Users
                .AnyAsync(x => x.UserCommonId == userCommonId);
        }

        /// <summary>
        /// 有効なユーザーのみを取得（論理削除対応）
        /// 将来的に論理削除機能を追加する場合のための拡張ポイント
        /// </summary>
        /// <param name="context">データベースコンテキスト</param>
        /// <param name="loginId">ログインID</param>
        /// <returns>有効なユーザー情報</returns>
        public static async Task<User?> GetActiveUserAsync(TrecPlansRDBContext context, string loginId)
        {
            if (context == null) 
                throw new ArgumentNullException(nameof(context));
            
            if (string.IsNullOrWhiteSpace(loginId)) 
                return null;

            // 現在は論理削除フィールドがないため、通常の検索と同じ
            // 将来的に IsDeleted フィールドが追加された場合は、ここに条件を追加
            return await context.Users
                .Where(x => x.LoginId == loginId)
                .FirstOrDefaultAsync();
        }
    }
}