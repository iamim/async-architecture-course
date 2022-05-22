namespace AsyncArch.Services.Analytics.Db.Models;

public class Account
{
    public int Id { get; set; }
    public Guid AccountGuid { get; set; }
    public string? Name { get; set; }
    public string? Role { get; set; }
}