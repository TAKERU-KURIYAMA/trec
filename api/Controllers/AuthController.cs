using Microsoft.AspNetCore.Mvc;
using Api.Common;
using Services;
using System.ComponentModel.DataAnnotations;

namespace API.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly IJwtService _jwtService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IAuthService authService, IJwtService jwtService, ILogger<AuthController> logger)
        {
            _authService = authService;
            _jwtService = jwtService;
            _logger = logger;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            try
            {
                StructuredLogger.LogRequestStart(Request.Method, Request.Path.ToString(), nameof(Login));

                if (!ModelState.IsValid)
                {
                    return HttpResponseHelper.CreateErrorResponse(ApplicationConstants.ErrorCodes.ParameterError, "入力値が不正です");
                }

                var user = await _authService.AuthenticateUserAsync(request.LoginId, request.Password);
                if (user == null)
                {
                    return HttpResponseHelper.CreateErrorResponse(ApplicationConstants.ErrorCodes.LoginIdOrPasswordInvalid, "ログインIDまたはパスワードが正しくありません");
                }

                var token = _jwtService.GenerateToken(user.UserCommonId, user.LoginId, new Dictionary<string, string>
                {
                    { "user_id", user.UserCommonId },
                    { "login_id", user.LoginId },
                    { "display_name", user.DisplayName }
                });

                var responseData = new
                {
                    token,
                    user = new
                    {
                        id = user.UserCommonId,
                        loginId = user.LoginId,
                        displayName = user.DisplayName
                    }
                };

                StructuredLogger.LogRequestEnd(Request.Method, Request.Path.ToString(), 200, TimeSpan.Zero, responseData);
                return HttpResponseHelper.CreateSuccessResponse(responseData);
            }
            catch (AppException aex)
            {
                StructuredLogger.LogError($"Error in {nameof(Login)}: {aex.UserMessage}", aex, Request.Path.ToString());
                return HttpResponseHelper.CreateErrorResponse(aex.ErrorCode, aex.UserMessage);
            }
            catch (Exception ex)
            {
                var appEx = new AppException(ApplicationConstants.ErrorCodes.ServerError, "システムエラーが発生しました", ex);
                StructuredLogger.LogError($"Unhandled error in {nameof(Login)}: {ex.Message}", ex, Request.Path.ToString());
                return HttpResponseHelper.CreateErrorResponse(ApplicationConstants.ErrorCodes.ServerError, "システムエラーが発生しました");
            }
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            try
            {
                StructuredLogger.LogRequestStart(Request.Method, Request.Path.ToString(), nameof(Register));

                if (!ModelState.IsValid)
                {
                    return HttpResponseHelper.CreateErrorResponse(ApplicationConstants.ErrorCodes.ParameterError, "入力値が不正です");
                }

                var userCommonId = await _authService.GenerateUserCommonIdAsync();
                var user = await _authService.RegisterUserAsync(userCommonId, request.LoginId, request.Password, request.DisplayName);

                var token = _jwtService.GenerateToken(user.UserCommonId, user.LoginId, new Dictionary<string, string>
                {
                    { "display_name", user.DisplayName }
                });

                var responseData = new
                {
                    token,
                    user = new
                    {
                        id = user.UserCommonId,
                        loginId = user.LoginId,
                        displayName = user.DisplayName
                    }
                };

                StructuredLogger.LogRequestEnd(Request.Method, Request.Path.ToString(), 201, TimeSpan.Zero, responseData);
                var result = HttpResponseHelper.CreateSuccessResponse(responseData, "ユーザー登録が完了しました");
                result.StatusCode = 201;
                return result;
            }
            catch (AppException aex)
            {
                StructuredLogger.LogError($"Error in {nameof(Register)}: {aex.UserMessage}", aex, Request.Path.ToString());
                return HttpResponseHelper.CreateErrorResponse(aex.ErrorCode, aex.UserMessage);
            }
            catch (Exception ex)
            {
                var appEx = new AppException(ApplicationConstants.ErrorCodes.ServerError, "システムエラーが発生しました", ex);
                StructuredLogger.LogError($"Unhandled error in {nameof(Register)}: {ex.Message}", ex, Request.Path.ToString());
                return HttpResponseHelper.CreateErrorResponse(ApplicationConstants.ErrorCodes.ServerError, "システムエラーが発生しました");
            }
        }

        [HttpPost("validate")]
        public IActionResult ValidateToken([FromHeader(Name = "Authorization")] string? authorizationHeader)
        {
            try
            {
                StructuredLogger.LogRequestStart(Request.Method, Request.Path.ToString(), nameof(ValidateToken));

                var token = _jwtService.ExtractTokenFromAuthorizationHeader(authorizationHeader);
                if (string.IsNullOrEmpty(token))
                {
                    return HttpResponseHelper.CreateErrorResponse(ApplicationConstants.ErrorCodes.AuthenticationError, "認証トークンが見つかりません");
                }

                var (isValid, userId) = _jwtService.ValidateToken(token);
                if (!isValid || string.IsNullOrEmpty(userId))
                {
                    return HttpResponseHelper.CreateErrorResponse(ApplicationConstants.ErrorCodes.AuthenticationError, "無効な認証トークンです");
                }

                var claims = _jwtService.GetTokenClaims(token);
                var responseData = new
                {
                    isValid = true,
                    userId,
                    claims
                };

                StructuredLogger.LogRequestEnd(Request.Method, Request.Path.ToString(), 200, TimeSpan.Zero, responseData);
                return HttpResponseHelper.CreateSuccessResponse(responseData);
            }
            catch (Exception ex)
            {
                StructuredLogger.LogError($"Unhandled error in {nameof(ValidateToken)}: {ex.Message}", ex, Request.Path.ToString());
                return HttpResponseHelper.CreateErrorResponse(ApplicationConstants.ErrorCodes.ServerError, "システムエラーが発生しました");
            }
        }
    }

    public class LoginRequest
    {
        [Required(ErrorMessage = "ログインIDは必須です")]
        [StringLength(256, ErrorMessage = "ログインIDは256文字以下で入力してください")]
        public string LoginId { get; set; } = string.Empty;

        [Required(ErrorMessage = "パスワードは必須です")]
        [StringLength(128, MinimumLength = 8, ErrorMessage = "パスワードハッシュは8文字以上128文字以下である必要があります")]
        public string Password { get; set; } = string.Empty;
    }

    public class RegisterRequest
    {
        [Required(ErrorMessage = "ログインIDは必須です")]
        [StringLength(32, ErrorMessage = "ログインIDは32文字以下で入力してください")]
        [RegularExpression(@"^[a-zA-Z0-9]+$", ErrorMessage = "ログインIDは英数字のみ使用可能です")]
        public string LoginId { get; set; } = string.Empty;

        [Required(ErrorMessage = "パスワードは必須です")]
        [StringLength(64, MinimumLength = 64, ErrorMessage = "パスワードハッシュは64文字である必要があります")]
        [RegularExpression(@"^[a-fA-F0-9]{64}$", ErrorMessage = "パスワードハッシュは64文字の16進数文字列である必要があります")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "表示名は必須です")]
        [StringLength(64, ErrorMessage = "表示名は64文字以下で入力してください")]
        public string DisplayName { get; set; } = string.Empty;
    }
}