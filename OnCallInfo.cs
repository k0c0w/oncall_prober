namespace OnCallProber;

internal sealed record OnCallInfo
{
    public string Host { get; set; }
    
    public string AppName { get; set; }
    
    public string AppKey { get; set; }
}