namespace CK.IO.User.NamedUser;

public interface IUserProfile : CK.IO.User.UserProfile.IUserProfile
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
}
