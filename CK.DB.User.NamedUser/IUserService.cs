using CK.Core;
using CK.SqlServer;
using System.Threading.Tasks;

namespace CK.DB.User.NamedUser;

public interface IUserService : CK.DB.User.UserProfile.IUserService
{
    Task<int> CreateUserAsync( ISqlCallContext ctx, UserMessageCollector collector, int actorId, string userName, string firstName, string lastName );
    new Task<IO.User.NamedUser.IUserProfile> GetUserProfileAsync( ISqlCallContext ctx, int actorId, int userId );
    Task UpdateUserAsync( ISqlCallContext ctx, UserMessageCollector collector, int actorId, int userId, string? userName, string? firstName, string? lastName );
}
