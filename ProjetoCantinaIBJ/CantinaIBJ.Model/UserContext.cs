﻿using CantinaIBJ.Model.Core;

namespace CantinaIBJ.Model;

public class UserContext
{
    #region [States]
    public string UserId { get; set; }

    public string Name { get; set; }

    public string Tenant { get; set; }

    public string Email { get; set; }

    public string PhoneNumber { get; set; }

    public string JwtId { get; set; }

    public string Group { get; set; }

    public DateTimeOffset? TokenCreatedIn { get; set; }
    public DateTimeOffset? TokenExpiresIn { get; set; }

    #endregion

    #region [Behaviors]

    public string GetCurrentUser() => string.IsNullOrEmpty(Name) ? "" : Name;
    public string GetCurrentUserPoolId() => string.IsNullOrEmpty(Tenant) ? "" : Tenant;
    #endregion
}