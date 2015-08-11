namespace WykopSDK.API.Models
{
    public class ErrorInternal
    {
        public int code { get; set; }
        public string message { get; set; }
    }

    public class Error
    {
        public ErrorInternal error { get; set; }
    }
}
