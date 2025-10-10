namespace QuantFlow.Common.Enumerations;

/// <summary>
/// Represents the mode a dialog is operating in
/// </summary>
public enum DialogMode
{
    /// <summary>
    /// Represents an unknown or unspecified value.
    /// </summary>
    Unknown = 0,

    /// <summary>
    /// Represents the current state of the dialog is in create mode.
    /// </summary>
    Create,

    /// <summary>
    /// Represents the current state of the dialog is in edit mode.
    /// </summary>
    Edit,

    /// <summary>
    /// Represents the current state of the dialog is in view mode.
    /// </summary>
    View
}