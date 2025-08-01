namespace CK.IO.User.NamedUser;

public interface ICreateUserCommand : CK.IO.User.UserProfile.ICreateUserCommand
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
}
