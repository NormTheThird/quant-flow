﻿namespace QuantFlow.Common.Exceptions;

/// <summary>
/// Exception thrown when a user is not authorized to perform an action
/// </summary>
public class UnauthorizedException : Exception
{
    /// <summary>
    /// Initializes a new instance of the UnauthorizedException class
    /// </summary>
    public UnauthorizedException() : base("You are not authorized to perform this action.")
    {
    }

    /// <summary>
    /// Initializes a new instance of the UnauthorizedException class with a specified error message
    /// </summary>
    /// <param name="message">The message that describes the error</param>
    public UnauthorizedException(string message) : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the UnauthorizedException class with a specified error message and a reference to the inner exception
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception</param>
    /// <param name="innerException">The exception that is the cause of the current exception</param>
    public UnauthorizedException(string message, Exception innerException) : base(message, innerException)
    {
    }
}