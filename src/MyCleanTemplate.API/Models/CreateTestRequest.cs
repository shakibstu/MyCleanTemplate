namespace MyCleanTemplate.API.Models;

public class CreateTestRequest
{
    public string Name { get; set; }
    public string? Description { get; set; }

    public DateTime StartTime { get; set; }
    public CreateTestDetails? Details { get; set; }
}
public class CreateTestDetails
{
    public string Name { get; set; }
}