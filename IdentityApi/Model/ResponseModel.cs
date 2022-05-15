namespace IdentityApi.Model
{
    public class ResponseModel<T> where T : class
    {
        public int Code { get; set; }
        public string Message { get; set; }
        public bool Successful { get; set; }
        public T Data { get; set; }

        public static ResponseModel<T> Success(T data, string message = "successfully", int code = 200)
        {
            return new ResponseModel<T> { Successful = true, Code = code, Data = data, Message = message };
        }

        public static ResponseModel<T> Fail(string errorMessage, int errorCode = 500)
        {
            return new ResponseModel<T> { Successful = false, Message = errorMessage, Code = errorCode, Data = null };
        }
    }

    public class ResponseModel
    {
        public int Code { get; set; }
        public string Message { get; set; }
        public bool Successful { get; set; }

        public static ResponseModel Success(string message = "successfully", int code = 200)
        {
            return new ResponseModel { Successful = true, Code = 200, Message = message };
        }

        public static ResponseModel Fail(string errorMessage, int errorCode = 500)
        {
            return new ResponseModel { Successful = false, Message = errorMessage, Code = errorCode };
        }
    }
}
