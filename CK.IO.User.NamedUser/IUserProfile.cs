namespace CK.IO.User.NamedUser;

public interface IUserProfile : Actor.IUserProfile
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
}
