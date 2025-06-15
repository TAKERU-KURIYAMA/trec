using System.ComponentModel.DataAnnotations;

namespace Api.Models
{
    /// <summary>
    /// トレーニングメニュー作成リクエスト
    /// </summary>
    public class CreateMenuRequest
    {
        [Required(ErrorMessage = "メニューIDは必須です")]
        [StringLength(64, ErrorMessage = "メニューIDは64文字以内で入力してください")]
        public string MenuId { get; set; } = string.Empty;

        [Required(ErrorMessage = "メニュー名は必須です")]
        [StringLength(100, ErrorMessage = "メニュー名は100文字以内で入力してください")]
        public string MenuName { get; set; } = string.Empty;

        [StringLength(100, ErrorMessage = "英語名は100文字以内で入力してください")]
        public string? EnglishName { get; set; }

        [StringLength(500, ErrorMessage = "説明は500文字以内で入力してください")]
        public string? Description { get; set; }
    }

    /// <summary>
    /// トレーニングメニュー更新リクエスト
    /// </summary>
    public class UpdateMenuRequest
    {
        [Required(ErrorMessage = "メニュー名は必須です")]
        [StringLength(100, ErrorMessage = "メニュー名は100文字以内で入力してください")]
        public string MenuName { get; set; } = string.Empty;

        [StringLength(100, ErrorMessage = "英語名は100文字以内で入力してください")]
        public string? EnglishName { get; set; }

        [StringLength(500, ErrorMessage = "説明は500文字以内で入力してください")]
        public string? Description { get; set; }
    }

    /// <summary>
    /// タグ作成リクエスト
    /// </summary>
    public class CreateTagRequest
    {
        [Required(ErrorMessage = "タグIDは必須です")]
        [StringLength(64, ErrorMessage = "タグIDは64文字以内で入力してください")]
        public string TagId { get; set; } = string.Empty;

        [Required(ErrorMessage = "タグ名は必須です")]
        [StringLength(100, ErrorMessage = "タグ名は100文字以内で入力してください")]
        public string TagName { get; set; } = string.Empty;

        [StringLength(100, ErrorMessage = "英語名は100文字以内で入力してください")]
        public string? EnglishName { get; set; }
    }

    /// <summary>
    /// タグ更新リクエスト
    /// </summary>
    public class UpdateTagRequest
    {
        [Required(ErrorMessage = "タグ名は必須です")]
        [StringLength(100, ErrorMessage = "タグ名は100文字以内で入力してください")]
        public string TagName { get; set; } = string.Empty;

        [StringLength(100, ErrorMessage = "英語名は100文字以内で入力してください")]
        public string? EnglishName { get; set; }
    }

    /// <summary>
    /// メニューへのタグ追加リクエスト
    /// </summary>
    public class AssignTagToMenuRequest
    {
        [Required(ErrorMessage = "メニューIDは必須です")]
        [StringLength(64, ErrorMessage = "メニューIDは64文字以内で入力してください")]
        public string MenuId { get; set; } = string.Empty;

        [Required(ErrorMessage = "タグIDは必須です")]
        [StringLength(64, ErrorMessage = "タグIDは64文字以内で入力してください")]
        public string TagId { get; set; } = string.Empty;
    }

    /// <summary>
    /// メニューのタグ一括更新リクエスト
    /// </summary>
    public class UpdateMenuTagsRequest
    {
        [Required(ErrorMessage = "タグIDのリストは必須です")]
        public List<string> TagIds { get; set; } = new List<string>();
    }
}