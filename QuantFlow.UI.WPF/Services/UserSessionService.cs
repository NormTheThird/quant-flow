namespace QuantFlow.UI.WPF.Services;

/// <summary>
/// Service to manage current user session state
/// </summary>
public class UserSessionService : IUserSessionService
{
    private Guid? _currentUserId;
    private string? _currentUsername;

    public Guid CurrentUserId
    {
        get => _currentUserId ?? throw new InvalidOperationException("No user is currently logged in");
        private set => _currentUserId = value;
    }

    public string CurrentUsername
    {
        get => _currentUsername ?? throw new InvalidOperationException("No user is currently logged in");
        private set => _currentUsername = value;
    }

    public bool IsLoggedIn => _currentUserId.HasValue;

    public void SetCurrentUser(Guid userId, string username)
    {
        CurrentUserId = userId;
        CurrentUsername = username;
    }

    public void ClearCurrentUser()
    {
        _currentUserId = null;
        _currentUsername = null;
    }
}