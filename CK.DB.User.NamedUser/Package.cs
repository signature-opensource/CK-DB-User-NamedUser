using CK.Core;
using CK.SqlServer;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace CK.DB.User.NamedUser;

/// <summary>
/// Package that adds a Firstname and a Lastname.
/// </summary>
[SqlPackage( Schema = "CK", ResourcePath = "Res" )]
[Versions( "1.0.1" )]
public partial class Package : SqlPackage, IUserService
{
    [AllowNull]
    UserProfile.Package _userProfilePackage;

    [InjectObject, AllowNull]
    public NamedUserTable NamedUserTable { get; protected set; }

    void StObjConstruct( Actor.Package actorPackage, UserProfile.Package userProfilePackage )
    {
        _userProfilePackage = userProfilePackage;
    }

    public async Task<int> CreateUserAsync( ISqlCallContext ctx, UserMessageCollector collector, int actorId, string userName, string firstName, string lastName )
    {
        var userId = await NamedUserTable.CreateUserAsync( ctx, actorId, userName, lastName, firstName );
        ctx.Monitor.Info( $"Named User successfully created. (ActorId: {actorId}, UserId: {userId}, FirstName: {firstName}, LastName: {lastName})" );

        return userId;
    }

    public Task<int> CreateUserAsync( ISqlCallContext ctx, UserMessageCollector collector, int actorId, string userName )
        => _userProfilePackage.CreateUserAsync( ctx, collector, actorId, userName );

    public async Task<CK.IO.User.NamedUser.IUserProfile> GetUserProfileAsync( ISqlCallContext ctx, int actorId, int userId )
        => (CK.IO.User.NamedUser.IUserProfile)await _userProfilePackage.ReadUserProfileAsync( ctx, actorId, userId );

    Task<CK.IO.User.UserProfile.IUserProfile> UserProfile.IUserService.GetUserProfileAsync( ISqlCallContext ctx, int actorId, int userId )
        => _userProfilePackage.GetUserProfileAsync( ctx, actorId, userId );

    public async Task UpdateUserAsync( ISqlCallContext ctx, UserMessageCollector collector, int actorId, int userId, string? userName, string? firstName, string? lastName )
    {
        if( !string.IsNullOrWhiteSpace( userName ) )
        {
            await _userProfilePackage.UpdateUserAsync( ctx, collector, actorId, userId, userName );
        }
        await NamedUserTable.SetNamesAsync( ctx, actorId, userId, firstName, lastName );
        ctx.Monitor.Info( $"Named User has successfully updated. (UserId: {userId}, FirstName: {firstName}, LastName: {lastName})" );

        collector.Info( $"Named user successfully updated (UserId: {userId}, FirstName: {firstName}, LastName: {lastName})", "User.UserNamedUserUpdated" );
    }

    public Task UpdateUserAsync( ISqlCallContext ctx, UserMessageCollector collector, int actorId, int userId, string? userName )
        => _userProfilePackage.UpdateUserAsync( ctx, collector, actorId, userId, userName );

    public Task DestroyUserAsync( ISqlCallContext ctx, UserMessageCollector collector, int actorId, int userId )
        => DestroyUserAsync( ctx, collector, actorId, userId );
}
