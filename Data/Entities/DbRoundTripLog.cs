namespace StudyMgt.Data.Entities;

public class DbRoundTripLog
{
    public int Id { get; set; }
    public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;
    public string Message { get; set; } = string.Empty;
}
