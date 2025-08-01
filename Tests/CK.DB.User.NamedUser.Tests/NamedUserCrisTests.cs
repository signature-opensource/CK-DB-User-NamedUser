using CK.Core;
using CK.Cris;
using CK.DB.Actor;
using CK.IO.Actor;
using CK.SqlServer;
using CK.Testing;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Shouldly;
using System;
using System.Threading.Tasks;

namespace CK.DB.User.NamedUser.Tests;

[TestFixture]
public class NamedUserCrisTests
{
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    AsyncServiceScope _scope;
    CrisExecutionContext _executor;
    PocoDirectory _pocoDir;
    UserTable _userTable;
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        _scope = SharedEngine.AutomaticServices.CreateAsyncScope();
        var services = _scope.ServiceProvider;

        _pocoDir = services.GetRequiredService<PocoDirectory>();
        _executor = services.GetRequiredService<CrisExecutionContext>();
        _userTable = services.GetRequiredService<UserTable>();
    }

    [OneTimeTearDown]
    public async Task OneTimeTearDownAsync()
    {
        await _scope.DisposeAsync();
    }

    [Test]
    public async Task can_get_userProfile_Async()
    {
        var userId = 1;
        var cmd = _pocoDir.Create<IGetUserProfileQCommand>( cmd =>
        {
            cmd.ActorId = 1;
            cmd.UserId = userId;
        } );
        var executingCmd = await _executor.ExecuteRootCommandAsync( cmd );

        var profile = executingCmd.WithResult<IO.User.NamedUser.IUserProfile?>().Result;
        profile.ShouldNotBeNull();
        profile.UserId.ShouldBe( userId );
        profile.UserName.ShouldBe( "System" );
        profile.FirstName.ShouldBe( "" );
        profile.LastName.ShouldBe( "" );
    }

    [Test]
    public async Task can_create_user_Async()
    {
        var userName = Guid.NewGuid().ToString();
        var fName = Guid.NewGuid().ToString();
        var lName = Guid.NewGuid().ToString();
        var cmd = _pocoDir.Create<IO.User.NamedUser.ICreateUserCommand>( c =>
        {
            c.ActorId = 1;
            c.UserName = userName;
            c.FirstName = fName;
            c.LastName = lName;
        } );
        var executingCmd = await _executor.ExecuteRootCommandAsync( (IAbstractCommand)cmd );
        var res = executingCmd.WithResult<ICreateUserCommandResult>().Result;
        res.UserIdResult.ShouldBeGreaterThan( 1 );

        var getcmd = _pocoDir.Create<IGetUserProfileQCommand>( cmd =>
        {
            cmd.ActorId = 1;
            cmd.UserId = res.UserIdResult;
        } );

        var executingGetCmd = await _executor.ExecuteRootCommandAsync( getcmd );

        var profile = executingGetCmd.WithResult<IO.User.NamedUser.IUserProfile?>().Result;
        profile.ShouldNotBeNull();
        profile.FirstName.ShouldBe( fName );
        profile.LastName.ShouldBe( lName );
    }

    [Test]
    public async Task can_set_user_names_Async()
    {
        var createCmd = _pocoDir.Create<IO.User.NamedUser.ICreateUserCommand>( c =>
        {
            c.ActorId = 1;
            c.UserName = Guid.NewGuid().ToString();
        } );
        var executingCreateCmd = await _executor.ExecuteRootCommandAsync( (IAbstractCommand)createCmd ); 
        var createRes = executingCreateCmd.WithResult<ICreateUserCommandResult>().Result;

        var fName = Guid.NewGuid().ToString();
        var lName = Guid.NewGuid().ToString();
        var cmd = _pocoDir.Create<IO.User.NamedUser.ISetUserNamesCommand>( c =>
        {
            c.ActorId = 1;
            c.UserId = createRes.UserIdResult;
            c.FirstName = fName;
            c.LastName = lName;
        } );
        var executingCmd = await _executor.ExecuteRootCommandAsync( cmd );

        var res = executingCmd.WithResult<ICrisBasicCommandResult>().Result;
        res.UserMessages.ShouldNotBeNull();

        using var ctx = new SqlStandardCallContext();
        var profile = await _userTable.GetUserProfileAsync<IO.User.NamedUser.IUserProfile>( ctx, 1, createRes.UserIdResult );
        profile.ShouldNotBeNull();
        profile.FirstName.ShouldBe( fName );
        profile.LastName.ShouldBe( lName );
    }
}
