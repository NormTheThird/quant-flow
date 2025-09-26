namespace QuantFlow.Api.Rest.Request;

/// <summary>
/// Request model for creating new users
/// </summary>
public class CreateUserRequest
{
    /// <summary>
    /// User's email address (must be unique)
    /// </summary>
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Please provide a valid email address")]
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// User's password (minimum 6 characters)
    /// </summary>
    [Required(ErrorMessage = "Password is required")]
    [MinLength(6, ErrorMessage = "Password must be at least 6 characters long")]
    public string Password { get; set; } = string.Empty;

    /// <summary>
    /// User's first name (optional)
    /// </summary>
    [MaxLength(50, ErrorMessage = "First name cannot exceed 50 characters")]
    public string? FirstName { get; set; }

    /// <summary>
    /// User's last name (optional)
    /// </summary>
    [MaxLength(50, ErrorMessage = "Last name cannot exceed 50 characters")]
    public string? LastName { get; set; }

    /// <summary>
    /// Username (optional - will default to email prefix if not provided)
    /// </summary>
    [MaxLength(50, ErrorMessage = "Username cannot exceed 50 characters")]
    public string? Username { get; set; }

    /// <summary>
    /// Whether the user should have system administrator privileges
    /// </summary>
    public bool IsSystemAdmin { get; set; } = false;
}