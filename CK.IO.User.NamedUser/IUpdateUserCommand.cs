namespace CK.IO.User.NamedUser;

public interface IUpdateUserCommand : CK.IO.User.UserProfile.IUpdateUserCommand
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
}
