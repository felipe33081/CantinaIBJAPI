using CantinaIBJ.Model.Core;

namespace CantinaIBJ.Model.User;

public class User
{
    #region [States]
    public int UserId { get; set; }

    public string Name { get; set; }

    public string PhoneNumber { get; set; }

    public string Aud { get; set; }
    
    public string Group { get; set; }

    public DateTimeOffset? TokenCreatedIn { get; set; }
    public DateTimeOffset? TokenExpiresIn { get; set; }

    #endregion

    #region [Behaviors]

    public string GetCurrentUser() => string.IsNullOrEmpty(Name) ? "" : Name;

    #endregion
}