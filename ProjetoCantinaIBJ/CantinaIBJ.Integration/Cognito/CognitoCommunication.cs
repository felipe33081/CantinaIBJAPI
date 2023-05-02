using Amazon.CognitoIdentityProvider;
using Amazon.CognitoIdentityProvider.Model;
using CantinaIBJ.Integration.Cognito.Model;
using CantinaIBJ.Integration.Cognito.Model.Group;
using CantinaIBJ.Integration.Cognito.Model.User;
using CantinaIBJ.Model;
using CantinaIBJ.Model.AppSettings;
using Microsoft.Extensions.Options;
using System.Drawing;

namespace CantinaIBJ.Integration.Cognito;

public class CognitoCommunication : ICognitoCommunication
{
    readonly CognitoSettings _cognitoSettings;
    readonly AmazonCognitoIdentityProviderClient _cognitoProviderClient;

    public CognitoCommunication(IOptions<CognitoSettings> cognitoSettings)
    {
        _cognitoSettings = cognitoSettings.Value ?? throw new ArgumentNullException(nameof(cognitoSettings.Value));
        _cognitoProviderClient = GetCognitoProviderClient();
    }

    #region Groups

    public async Task<List<GroupGetResponseModel>> GetGroupsAsync(int _start, int _end, string searchString, string userPoolId)
    {
        List<GroupGetResponseModel> response = new();

        var cognitoResponse = await ListGroups(userPoolId, _end, searchString);

        if (cognitoResponse.Groups is not null)
        {
            response = GetData(cognitoResponse.Groups);
        }

        response.Skip(_start)
            .Take(_end)
            .ToList();

        return response;
    }

    public async Task<List<UserGetResponseModel>> GetUsersInGroupAsync(string groupName, string nextToken, int _start, int _end, string userPoolId)
    {
        List<UserGetResponseModel> response = new();

        var totalUsersInGroup = (await _cognitoProviderClient.ListUsersInGroupAsync(new() { UserPoolId = userPoolId, Limit = 60, GroupName = groupName })).Users.Count;

        ListUsersInGroupResponse cognitoResponse = await _cognitoProviderClient.ListUsersInGroupAsync(new()
        {
            UserPoolId = userPoolId,
            GroupName = groupName
        });

        if (cognitoResponse.Users is not null)
        {
            response = GetData(cognitoResponse.Users);
        }

        response
            .Skip(_start)
            .Take(_end)
            .ToList();

        return response;
    }

    public async Task<GroupGetDetailResponseModel> GetGroupAsync(string id, string userPoolId)
    {
        var response = new GroupGetDetailResponseModel();

        var cognitoResponse = await _cognitoProviderClient
            .GetGroupAsync(new() { UserPoolId = userPoolId, GroupName = id, });

        if (cognitoResponse is not null)
        {
            response.CreationDate = cognitoResponse.Group.CreationDate;
            response.Description = cognitoResponse.Group.Description;
            response.GroupName = cognitoResponse.Group.GroupName;
            response.LastModifiedDate = cognitoResponse.Group.LastModifiedDate;
            response.Precedence = cognitoResponse.Group.Precedence;
            response.UserPoolId = cognitoResponse.Group.UserPoolId;
        }

        return response;
    }

    public async Task<GroupPostResponseModel> CreateGroupAsync(string description, string name, int precedence, string userPoolId)
    {
        var response = new GroupPostResponseModel();

        var cognitoResponse = await _cognitoProviderClient
            .CreateGroupAsync(new CreateGroupRequest
            {
                Description = description,
                GroupName = name,
                Precedence = precedence,
                UserPoolId = userPoolId
            });

        if (cognitoResponse is not null)
        {
            response.CreationDate = cognitoResponse.Group.CreationDate;
            response.Description = cognitoResponse.Group.Description;
            response.GroupName = cognitoResponse.Group.GroupName;
            response.LastModifiedDate = cognitoResponse.Group.LastModifiedDate;
            response.Precedence = cognitoResponse.Group.Precedence;
            response.UserPoolId = cognitoResponse.Group.UserPoolId;
        }

        return response;
    }

    public async Task UpdateGroupAsync(string groupNameId, string description, int precedence, string userPoolId) =>
        await _cognitoProviderClient.UpdateGroupAsync(new UpdateGroupRequest
        {
            Description = description,
            GroupName = groupNameId,
            Precedence = precedence,
            UserPoolId = userPoolId
        });

    public async Task DeleteGroupAsync(string name, string userPoolId) =>
        await _cognitoProviderClient.DeleteGroupAsync(new DeleteGroupRequest
        {
            UserPoolId = userPoolId,
            GroupName = name
        });

    public async Task<ListGroupsResponse> ListGroups(string userPoolId, int _end, string searchString)
    {
        if (String.IsNullOrEmpty(searchString))
        {
            return await _cognitoProviderClient
                .ListGroupsAsync(new ListGroupsRequest()
                {
                    UserPoolId = userPoolId
                });
        }

        var resp = await _cognitoProviderClient
            .ListGroupsAsync(new ListGroupsRequest()
            {
                UserPoolId = userPoolId
            });
        
        resp.Groups = resp.Groups.Where(x => x.GroupName.ToLower().Contains((searchString ?? "").ToLower())).ToList();
        resp.NextToken = null;
        return resp;
    }

    #endregion

    #region Users 

    public async Task<ListDataPagination<UserGetResponseModel>> GetUsersAsync(int page, int size, string userPoolId, string? filter = null)
    {
        var fixPagination = FixPagination(page, size);

        var totalUser = (await _cognitoProviderClient
            .DescribeUserPoolAsync(new() { UserPoolId = userPoolId }))
                .UserPool.EstimatedNumberOfUsers;

        ListUsersResponse cognitoResponse = await _cognitoProviderClient
            .ListUsersAsync(new()
            {
                UserPoolId = userPoolId,
                AttributesToGet = new(),
                Limit = fixPagination.Size,
                Filter = filter
            });

        if (!string.IsNullOrEmpty(filter))
            totalUser = cognitoResponse.Users.Count;

        return new ListDataPagination<UserGetResponseModel>
        {
            Data = GetData(cognitoResponse.Users),
            Page = fixPagination.Page,
            TotalItems = totalUser,
            TotalPages = (int)Math.Ceiling((double)totalUser / fixPagination.Size)
        };
    }

    public async Task<UserGetResponseModel> GetUserAsync(string id, string userPoolId)
    {
        var response = new UserGetResponseModel();

        var cognitoResponse = await _cognitoProviderClient
            .AdminGetUserAsync(new() { UserPoolId = userPoolId, Username = id });

        if (cognitoResponse is not null)
        {
            return new UserGetResponseModel()
            {
                Enabled = cognitoResponse.Enabled,
                UserCreateDate = cognitoResponse.UserCreateDate,
                UserLastModifiedDate = cognitoResponse.UserLastModifiedDate,
                Id = cognitoResponse.Username,
                Name = cognitoResponse.UserAttributes.FirstOrDefault(x => x.Name == "name")?.Value ?? "N/D",
                PhoneNumber = cognitoResponse.UserAttributes.FirstOrDefault(x => x.Name == "phone_number")?.Value.Replace("+55", "") ?? "N/D",
                Email = cognitoResponse.UserAttributes.FirstOrDefault(x => x.Name == "email")?.Value ?? "N/D",
                UserStatus = cognitoResponse.UserStatus.Value,
                EmailVerified = cognitoResponse.UserAttributes.FirstOrDefault(x => x.Name == "email_verified")?.Value == "true",
                UserAttributes = cognitoResponse.UserAttributes
                        .Select(x => new AttributeModel() { Name = x.Name, Value = x.Value })
                        .ToList()
            };
        }

        return null;
    }

    public async Task<string> GetUserSub(string id, string userPoolId)
    {
        var response = new UserGetResponseModel();

        var cognitoResponse = await _cognitoProviderClient
            .AdminGetUserAsync(new() { UserPoolId = userPoolId, Username = id });

        return cognitoResponse.UserAttributes.Where(x => x.Name == "sub").FirstOrDefault()?.Value;
    }

    public async Task<List<UserGroupResponseModel>> GetUserGroupsAsync(string id, string userPoolId, int _start, int _end)
    {
        var response = new List<UserGroupResponseModel>();

        var cognitoResponse = await _cognitoProviderClient
            .AdminListGroupsForUserAsync(new AdminListGroupsForUserRequest
            {
                UserPoolId = userPoolId,
                Username = id,
                Limit = _end
            });

        if (cognitoResponse is not null)
        {
            response = cognitoResponse.Groups.Select(g => new UserGroupResponseModel { GroupName = g.GroupName, Precedence = g.Precedence }).ToList();
        }

        response.Skip(_start)
                .Take(_end)
                .ToList();

        return response;
    }


    public async Task<UserPostResponseModel> CreateUserAsync(UserPostRequestModel model, string userPoolId, MessageActionType messageActionType)
    {
        var response = new UserPostResponseModel();

        var cognitoResponse = await _cognitoProviderClient.AdminCreateUserAsync(new()
        {
            Username = model.Email,
            TemporaryPassword = model.Password,
            UserPoolId = userPoolId,
            MessageAction = messageActionType.Value,
            DesiredDeliveryMediums = new List<string>() { "EMAIL" },
            UserAttributes = new List<AttributeType> {
                    new AttributeType { Name = "email_verified", Value = model.EmailVerified.ToString() },
                    new AttributeType { Name = "email", Value = model.Email.ToLower().Trim() },
                    new AttributeType { Name = "name", Value = model.Name },
                    new AttributeType { Name = "phone_number", Value = model.PhoneNumber }
                }
        });

        if (cognitoResponse is not null)
        {
            response.Enabled = cognitoResponse.User.Enabled;
            response.Username = cognitoResponse.User.Username;
            response.UserStatus = cognitoResponse.User.UserStatus;
        }

        return response;
    }

    public async Task SetEmailVerified(string userId, string userPoolId)
    {
        var attrs = new List<AttributeType>
                {
                    new AttributeType { Name = "email_verified", Value = "true" }
                };
        await _cognitoProviderClient.AdminUpdateUserAttributesAsync(new()
        {
            Username = userId,
            UserPoolId = userPoolId,
            UserAttributes = attrs
        });
    }

    public async Task UpdateUserAsync(string userId, UserPutRequestModel model, string userPoolId)
    {
        var attrs = new List<AttributeType>
            {
                new AttributeType { Name = "email", Value = model.Email.ToLower().Trim() },
                new AttributeType { Name = "name", Value = model.Name },
                new AttributeType { Name = "phone_number", Value = model.PhoneNumber },
                new AttributeType { Name = "email_verified", Value = model.EmailVerified == true ? "true" : "false" }
            };
        await _cognitoProviderClient.AdminUpdateUserAttributesAsync(new()
        {
            Username = userId,
            UserPoolId = userPoolId,
            UserAttributes = attrs
        });
    }

    public async Task RemoveUserAsync(string id, string userPoolId) =>
        await _cognitoProviderClient.AdminDeleteUserAsync(new()
        {
            Username = id,
            UserPoolId = userPoolId
        });

    public async Task ResetPasswordUserAsync(string id, string userPoolId) =>
        await _cognitoProviderClient.AdminResetUserPasswordAsync(new()
        {
            Username = id,
            UserPoolId = userPoolId
        });
        

    public async Task AdminSetUserPasswordAsync(string id, string userPoolId, string newPassword, bool permanent)
    {
        var request = new AdminSetUserPasswordRequest
        {
            Password = newPassword,
            Permanent = permanent,
            Username = id,
            UserPoolId = userPoolId
        };
        await _cognitoProviderClient.AdminSetUserPasswordAsync(request);
    }

    public async Task AddtUserToAnExistingGroupAsync(string poolId, string groupName, string userId) =>
        await _cognitoProviderClient.AdminAddUserToGroupAsync(new()
        {
            Username = userId,
            UserPoolId = poolId,
            GroupName = groupName
        });

    public async Task RemoveUserFromAnExistingGroupAsync(string poolId, string groupName, string userId) =>
        await _cognitoProviderClient.AdminRemoveUserFromGroupAsync(new()
        {
            Username = userId,
            UserPoolId = poolId,
            GroupName = groupName
        });

    #endregion

    #region [ Extensions ]

    private AmazonCognitoIdentityProviderClient GetCognitoProviderClient() =>
        new(_cognitoSettings.AccessKey, _cognitoSettings.SecretKey,
            Amazon.RegionEndpoint.GetBySystemName(_cognitoSettings.Region));

    private static List<GroupGetResponseModel> GetData(List<GroupType> groupTypes)
    {
        var response = new List<GroupGetResponseModel>();

        groupTypes.ForEach(group =>
        {
            response.Add(new()
            {
                CreationDate = group.CreationDate,
                Description = group.Description,
                GroupName = group.GroupName,
                LastModifiedDate = group.LastModifiedDate,
                Precedence = group.Precedence,
                UserPoolId = group.UserPoolId
            });
        });

        return response;
    }

    private static List<UserGetResponseModel> GetData(List<UserType> userTypes)
    {
        var response = new List<UserGetResponseModel>();

        userTypes.ForEach(user =>
        {
            response.Add(new()
            {
                Enabled = user.Enabled,
                UserCreateDate = user.UserCreateDate,
                UserLastModifiedDate = user.UserLastModifiedDate,
                Id = user.Username,
                Name = user.Attributes.FirstOrDefault(x => x.Name == "name")?.Value ?? "N/D",
                PhoneNumber = user.Attributes.FirstOrDefault(x => x.Name == "phone_number")?.Value.Replace("+55", "") ?? "N/D",
                Email = user.Attributes.FirstOrDefault(x => x.Name == "email")?.Value ?? "N/D",
                UserStatus = user.UserStatus.Value,
                EmailVerified = user.Attributes.FirstOrDefault(x => x.Name == "email_verified")?.Value == "true"
            });
        });

        return response;
    }

    private static (int Page, int Size) FixPagination(int page, int size)
    {
        if (size <= 0 || size > 60) size = 60;
        if (page < 0) page = 0;

        return new(page, size);
    }
    #endregion
}