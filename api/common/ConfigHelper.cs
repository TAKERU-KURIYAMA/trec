using Microsoft.Extensions.Configuration;

namespace Utils
{
    public static class ConfigHelper
    {
        private static IConfigurationRoot? _config;

        /// <summary>
        /// 環境変数や appsettings.json を読み込む設定オブジェクトを返す（初回のみ構築）
        /// </summary>
        public static IConfigurationRoot GetConfiguration()
        {
            if (_config == null)
            {
                _config = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddEnvironmentVariables()
                    .AddJsonFile("appsettings.json", optional: true) // 開発用
                    .AddJsonFile("appsettings.Development.json", optional: true)
                    .Build();
            }

            return _config;
        }

        /// <summary>
        /// 接続文字列を取得
        /// </summary>
        public static string GetConnectionString()
        {
            return GetConfiguration().GetConnectionString("DefaultConnection") 
                   ?? throw new InvalidOperationException("接続文字列が見つかりません");
        }

        /// <summary>
        /// JWT のシークレットを取得
        /// </summary>
        public static string GetJwtSecret()
        {
            return GetConfiguration()["Jwt:Secret"] 
                   ?? throw new InvalidOperationException("JWTシークレットが見つかりません");
        }

        /// <summary>
        /// その他共通設定を取得
        /// </summary>
        public static string? Get(string key)
        {
            return GetConfiguration()[key];
        }
    }
}
