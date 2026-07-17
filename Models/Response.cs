namespace MongoAPI.Models
{
    public class Response
    {
        public bool IsOk { get; set; }
        public object Message { get; set; }

        public Response(bool isOk, object message)
        {
            IsOk = isOk;
            Message = message;
        }
    }
}
