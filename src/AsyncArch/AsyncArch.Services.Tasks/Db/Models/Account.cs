namespace AsyncArch.Services.Tasks.Db.Models;

public class Account
{
    public int Id { get; set; }
    public Guid UserId { get; set; }
    public string? UserName { get; set; }
    public string? Role { get; set; }
}