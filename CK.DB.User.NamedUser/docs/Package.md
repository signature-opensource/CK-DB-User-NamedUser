Adds a first name and a last name to the users (`CK.tUser`), set at creation or later, and exposed by `CK.vUser` and the
user profile.

`NamedUserTable.CreateUserAsync` creates a user with its names and `NamedUserTable.SetNamesAsync` changes them; it also
handles the `ISetUserNamesCommand` of CK.IO.User.NamedUser.
