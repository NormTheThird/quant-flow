namespace QuantFlow.Common.Enumerations;

public enum RefreshTokenType
{
    /// <summary>
    /// Represents an unknown or unspecified value.
    /// </summary>
    Unknown = 0,

    /// <summary>
    /// Indicates a standard refresh token used to obtain a new access token. 
    /// </summary>
    Refresh = 1,

    /// <summary>
    /// Represents a password reset operation, typically used to reset a user's password in an authentication system.
    /// </summary>
    PasswordReset = 2
}