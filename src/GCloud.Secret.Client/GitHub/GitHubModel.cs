using System.Net;

namespace GCloud.Secret.Client.GitHub;

public class GitHubModel
{
    public class Response
    {
        public string RequestUrl { get; set; }
        
        public bool IsSuccessStatusCode { get; set; }
        
        public HttpStatusCode StatusCode { get; set; }
    }
    
    public class Response<TData> : Response
    {
        public TData Data { get; set; }
    }
    
    public class Release
    {
        public string Html_Url { get; set; }

        public string Tag_Name { get; set; }
    }
}