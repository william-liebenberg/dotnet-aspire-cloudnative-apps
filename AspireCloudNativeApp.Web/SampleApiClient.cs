namespace AspireCloudNativeApp.Web;

public class SampleApiClient(HttpClient httpClient)
{
    public async Task<ApiEnvironment?> GetEnvironment()
    {
        var result = await httpClient.GetFromJsonAsync<ApiEnvironment>("/environment");
        return result;
    }
}


public class ApiEnvironment
{
    public string? RuntimeVersion { get; set; }
    public string? OsVersion { get; set; }
    public string? OsArchitecture { get; set; }
    public string? User { get; set; }
    public int ProcessorCount { get; set; }
    public long TotalAvailableMemoryBytes { get; set; }
    public long MemoryLimit { get; set; }
    public int MemoryUsage { get; set; }
    public string? HostName { get; set; }
}
