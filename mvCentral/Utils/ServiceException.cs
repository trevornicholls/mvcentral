using System;

namespace mvCentral.Utils
{
  public enum ServiceExceptionType
  {
    InvalidService = 2,
    InvalidMethod = 3,
    AuthenticationFailed = 4,
    InvalidFormat = 5,
    InvalidParameters = 6,
    InvalidResource = 7,
    TokenError = 8,
    InvalidSessionKey = 9,
    InvalidAPIKey = 10,
    ServiceOffline = 11,
    SubscribersOnly = 12,
    InvalidSignature = 13,
    UnauthorizedToken = 14,
    ExpiredToken = 15,
    FreeRadioExpired = 18,
    NotEnoughContent = 20,
    NotEnoughMembers = 21,
    NotEnoughFans = 22,
    NotEnoughNeighbours = 23
  }

  /// <summary>
  /// A Last.fm web service exception
  /// </summary>
  public class ServiceException : Exception
  {
    /// <summary>
    /// The exception type.
    /// </value>
    public ServiceExceptionType Type { get; private set; }

    /// <summary>
    /// The description of the exception.
    /// </summary>
    public string Description { get; private set; }

    public ServiceException(ServiceExceptionType type, string description)
      : base()
    {
      this.Type = type;
      this.Description = description;
    }

    public override string Message
    {
      get
      {
        return this.Type.ToString() + ": " + this.Description;
      }
    }
  }
}

