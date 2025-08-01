using CK.Auth;
using CK.Cris;

namespace CK.IO.User.NamedUser;

public interface ISetUserNamesCommand : ICommand<ICrisBasicCommandResult>, ICommandAuthNormal
{
    public int UserId { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
}
