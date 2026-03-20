namespace Auth.API.DTOs.Response;

public class UserResponseDto
{
    public Guid   Id          { get; set; }
    public string Email       { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string FullName    { get; set; } = string.Empty;
    public string Role        { get; set; } = string.Empty;
    public bool   IsActive    { get; set; }
    public string KYCStatus   { get; set; } = string.Empty;
}