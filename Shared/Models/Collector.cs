namespace LendingApp.Shared.Models;

public class Collector
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string FullName { get; set; } = string.Empty;
    public string ContactNumber { get; set; } = string.Empty;
    public string AssignedArea { get; set; } = string.Empty;
}
