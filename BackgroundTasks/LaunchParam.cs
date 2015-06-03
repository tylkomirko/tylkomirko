using Newtonsoft.Json;

namespace BackgroundTasks
{
    public sealed class LaunchParam
    {
        public string Type { get; set; }
        public string Author { get; set; }
        public string AuthorSex { get; set; }
        public int AuthorGroup { get; set; }
        public int EntryID { get; set; }
        public int CommentID { get; set; }
        public int NotificationID { get; set; }
    }

    public static class LaunchParamExtensions
    {
        public static string toString(this LaunchParam param)
        {
            return JsonConvert.SerializeObject(param);
        }
    }
}
