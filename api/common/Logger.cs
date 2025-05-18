using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Common
{
    /// <summary>
    /// ログクラス
    /// </summary>
    public static class Logger
    {
        private static List<string> headerList = new List<string>() { };

        /// <summary>
        /// 開始ログ
        /// </summary>
        public static void Entry(ILogger? argLog, HttpRequest argReq)
        {
            if (argLog == null) return;

            try
            {
                var sb = new StringBuilder();
                sb.Append("Entry( ");

                sb.Append("[Header](");
                IHeaderDictionary header = argReq.Headers;
                foreach (var o in headerList)
                {
                    if (!string.IsNullOrWhiteSpace(header[o]))
                    {
                        sb.Append($"{o}：{header[o]},");
                    }
                }

                if (argReq.ContentType != null)
                    sb.Append($"Content-Type:{argReq.ContentType}");

                sb.Append(")");

                IQueryCollection query = argReq.Query;
                if (query.Count > 0)
                {
                    sb.Append("[Query](");
                    foreach (var o in query)
                    {
                        sb.Append($"{o.Key}：{o.Value},");
                    }
                    sb.Append(")");
                }

                if (argReq.HasFormContentType)
                {
                    IFormCollection form = argReq.Form;
                    if (form.Count > 0)
                    {
                        sb.Append("[Form](");
                        foreach (var o in form)
                        {
                            sb.Append($"{o.Key}：{o.Value},");
                        }
                        sb.Append(")");
                    }
                }

                sb.Append(")");
                argLog.LogInformation(sb.ToString());
            }
            catch (Exception ex)
            {
                argLog.LogCritical(ex, "開始ログ生成エラー: " + ex.ToString());
            }
        }

        /// <summary>
        /// 終了ログ
        /// </summary>
        public static void Exit(ILogger? argLog, dynamic? argRes = null)
        {
            if (argLog == null) return;

            try
            {
                var sb = new StringBuilder();
                sb.Append("Exit( ");
                if (argRes != null)
                {
                    sb.Append(JsonConvert.SerializeObject(argRes));
                }
                sb.Append(")");
                argLog.LogInformation(sb.ToString());
            }
            catch (Exception ex)
            {
                argLog.LogCritical(ex, "終了ログ生成エラー: " + ex.ToString());
            }
        }

        /// <summary>
        /// エラーログ（コード＋例外）
        /// </summary>
        public static void Error(ILogger? argLog, AppException.Value argCode, Exception? argEx = null)
        {
            if (argLog == null) return;

            try
            {
                AppException fex = new AppException(argCode, argEx);
                argLog.LogError("Error( {Exception} )", fex);
            }
            catch (Exception ex)
            {
                argLog.LogCritical(ex, "エラーログ生成エラー: " + ex.ToString());
            }
        }

        /// <summary>
        /// エラーログ（AppException）
        /// </summary>
        public static void Error(ILogger? argLog, AppException argFex)
        {
            if (argLog == null) return;

            try
            {
                argLog.LogError("Error( {Exception} )", argFex);
            }
            catch (Exception ex)
            {
                argLog.LogCritical(ex, "エラーログ生成エラー: " + ex.ToString());
            }
        }
    }
}
