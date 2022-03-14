
public class ApiMovieResponse
{
    public bool success { get; set; }
    public ApiMovie[] results { get; set; }
}

public class ApiMovie
{
    public string poster_path { get; set; }
    public string title { get; set; }
    public string overview { get; set; }
}
