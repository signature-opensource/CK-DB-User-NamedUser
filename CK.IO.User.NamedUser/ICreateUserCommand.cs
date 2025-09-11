namespace CK.IO.User.NamedUser;

public interface ICreateUserCommand : Actor.ICreateUserCommand
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
}
