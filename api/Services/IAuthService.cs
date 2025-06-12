using Api.Models;

namespace Services;

/// <summary>
/// 認証関連サービスのインターフェース
/// ユーザー登録、認証、ID生成などの機能を定義
/// </summary>
public interface IAuthService
{
    /// <summary>
    /// 新規ユーザーを登録
    /// </summary>
    /// <param name="userCommonId">ユーザー共通ID（システム内部用）</param>
    /// <param name="loginId">ログインID（ユーザー入力）</param>
    /// <param name="plainPassword">平文パスワード</param>
    /// <param name="displayName">表示名</param>
    /// <returns>登録されたユーザー情報</returns>
    Task<User> RegisterUserAsync(string userCommonId, string loginId, string plainPassword, string displayName);

    /// <summary>
    /// ログイン認証を実行
    /// </summary>
    /// <param name="loginId">ログインID</param>
    /// <param name="plainPassword">平文パスワード</param>
    /// <returns>認証成功時はユーザー情報、失敗時はnull</returns>
    Task<User?> AuthenticateUserAsync(string loginId, string plainPassword);

    /// <summary>
    /// ユーザー共通IDの生成
    /// </summary>
    /// <returns>一意のユーザー共通ID</returns>
    Task<string> GenerateUserCommonIdAsync();
}
