namespace QuantFlow.UI.WPF.Interfaces;

/// <summary>
/// Service interface for managing current user session state
/// </summary>
public interface IUserSessionService
{
    /// <summary>
    /// Gets the current logged-in user's ID
    /// </summary>
    Guid CurrentUserId { get; }

    /// <summary>
    /// Gets the current logged-in user's username
    /// </summary>
    string CurrentUsername { get; }

    /// <summary>
    /// Gets whether a user is currently logged in
    /// </summary>
    bool IsLoggedIn { get; }

    /// <summary>
    /// Sets the current user session
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="username">Username</param>
    void SetCurrentUser(Guid userId, string username);

    /// <summary>
    /// Clears the current user session
    /// </summary>
    void ClearCurrentUser();
}