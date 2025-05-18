using System.Net;
using System.Text;
using System.Runtime.Serialization;

namespace Common
{
    [Serializable()]
    public class AppException : System.Exception
    {
        public struct Value : IEquatable<Value>
        {
            public string Code { get; set; }
            public HttpStatusCode Status { get; set; }
            public string Msg { get; set; }

            public Value(string _Code, HttpStatusCode _Status, string _Msg)
            {
                this.Code = _Code;
                this.Status = _Status;
                this.Msg = _Msg;
            }

            public override bool Equals(object obj)
            {
                return obj is Value value && Equals(value);
            }

            public bool Equals(Value other)
            {
                return Code == other.Code &&
                       Status == other.Status &&
                       Msg == other.Msg;
            }

            public override int GetHashCode()
            {
                return HashCode.Combine(Code, Status, Msg);
            }

            public static bool operator ==(Value left, Value right)
            {
                return left.Equals(right);
            }

            public static bool operator !=(Value left, Value right)
            {
                return !(left == right);
            }
        }

        public Value Cause { get; set; }
        public int? CoreCode { get; set; }
        public Exception? ArgException { get; set; }
        public string? _message { get; set; }

        public AppException() : base()
        {
            this.CoreCode = 0;
            this.ArgException = null;
            this._message = null;
        }

        public AppException(Value code)
            : base(code.Msg)
        {
            this.Cause = code;
            this.CoreCode = 0;
            this.ArgException = null;
            this._message = null;
        }

        public AppException(Value code, int CoreCode)
            : base(code.Msg)
        {
            this.Cause = code;
            this.CoreCode = CoreCode;
            this.ArgException = null;
            this._message = null;
        }

        public AppException(Value code, string msg)
            : base(msg)
        {
            code.Msg = msg;
            this.Cause = code;
            this.CoreCode = 0;
            this.ArgException = null;
            this._message = null;
        }

        public AppException(Value code, Exception argEx)
            : base(code.Msg, argEx)
        {
            this.Cause = code;
            this.CoreCode = 0;
            this.ArgException = argEx;
            this._message = null;
        }

        public AppException(Value code, Exception argEx, string msg)
            : base(msg, argEx)
        {
            this.Cause = code;
            this.CoreCode = 0;
            this.ArgException = argEx;
            this._message = msg;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append($"[Code:{Cause.Code} ");
            sb.Append($"Status:{Cause.Status} ");
            sb.Append($"Msg:{Cause.Msg}] ");
            if (ArgException != null)
            {
                sb.Append($"ÅyException:{ArgException} ");
                if (ArgException.InnerException != null)
                {
                    sb.Append($"InnerException:{ArgException.InnerException} ");
                }
                sb.Append("Åz");
            }

            return sb.ToString();
        }

        protected static string ConstructEnvelope(int code, string message, string data)
        {
            var sb = new StringBuilder();
            sb.Append($"[StatusCode:{code}");
            sb.Append($", Message:{message}");
            sb.Append($", Data:{data}]");
            return sb.ToString();
        }

        protected AppException(SerializationInfo serializationInfo, StreamingContext streamingContext)
        {
            throw new NotImplementedException();
        }

        public AppException(string message) : base(message)
        {
        }

        public AppException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
