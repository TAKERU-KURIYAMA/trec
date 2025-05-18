namespace Common
{
    public class ApiResponse<T>
    {
        public string Code { get; set; } = "0000";  // アプリ内成功コード（"0000" = 成功）
        public string Message { get; set; } = "成功";
        public T? Data { get; set; }
    }

    public class ApiResponse
    {
        public string Code { get; set; } = "0000";
        public string Message { get; set; } = "成功";
    }
}
