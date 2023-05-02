using Amazon.CognitoIdentityProvider;
using CantinaIBJ.Integration.Cognito.Model.Group;
using CantinaIBJ.Integration.Cognito.Model.User;
using CantinaIBJ.Model;

namespace CantinaIBJ.Integration.Cognito;

public interface ICognitoCommunication
{
    #region Groups

    Task<List<GroupGetResponseModel>> GetGroupsAsync(int _start, int _end, string searchString, string userPoolId);
    Task<List<UserGetResponseModel>> GetUsersInGroupAsync(string groupName, string nextToken, int _start, int _end, string userPoolId);
    Task<GroupGetDetailResponseModel> GetGroupAsync(string id, string userPoolId);
    Task<GroupPostResponseModel> CreateGroupAsync(string description, string groupName, int precedence, string userPoolId);
    Task UpdateGroupAsync(string groupNameId, string description, int precedence, string userPoolId);
    Task DeleteGroupAsync(string name, string userPoolId);

    #endregion

    #region Users

    Task<ListDataPagination<UserGetResponseModel>> GetUsersAsync(int page, int size, string userPoolId, string? filter = null);
    Task<UserGetResponseModel> GetUserAsync(string id, string userPoolId);
    Task<string> GetUserSub(string userName, string userPoolId);
    Task<List<UserGroupResponseModel>> GetUserGroupsAsync(string id, string userPoolId, int _start, int _end);
    Task<UserPostResponseModel> CreateUserAsync(UserPostRequestModel model, string userPoolId, MessageActionType messageActionType);
    Task UpdateUserAsync(string userId, UserPutRequestModel model, string userPoolId);
    Task SetEmailVerified(string userId, string userPoolId);
    Task RemoveUserAsync(string id, string userPoolId);
    Task ResetPasswordUserAsync(string id, string userPoolId);
    Task AdminSetUserPasswordAsync(string id, string userPoolId, string newPassword, bool permanent);
    Task AddtUserToAnExistingGroupAsync(string poolId, string groupName, string userId);
    Task RemoveUserFromAnExistingGroupAsync(string poolId, string groupName, string userId);
    #endregion
}