using CK.Core;
using CK.Cris;
using CK.IO.User.UserProfile;
using CK.SqlServer;
using CK.Testing;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Shouldly;
using System;
using System.Linq;
using System.Threading.Tasks;
using static CK.Testing.MonitorTestHelper;

namespace CK.DB.User.NamedUser.Tests;

[TestFixture]
public class NamedUserCrisTests
{
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    AsyncServiceScope _scope;
    CrisExecutionContext _crisExecutionContext;
    PocoDirectory _pocoDir;
    Package _package;
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        _scope = SharedEngine.AutomaticServices.CreateAsyncScope();
        var services = _scope.ServiceProvider;
        _pocoDir = services.GetRequiredService<PocoDirectory>();
        _crisExecutionContext = services.GetRequiredService<CrisExecutionContext>();
        _package = services.GetRequiredService<CK.DB.User.NamedUser.Package>();
    }

    [OneTimeTearDown]
    public async Task OneTimeTearDownAsync()
    {
        await _scope.DisposeAsync();
    }

    [Test]
    public async Task can_get_userProfile_through_Cris_Async()
    {
        var userId = 1;
        var cmd = _pocoDir.Create<IGetUserProfileQCommand>( cmd =>
        {
            cmd.ActorId = 1;
            cmd.UserId = userId;
        } );
        var executedCmd = await _crisExecutionContext.ExecuteRootCommandAsync( cmd );

        var profile = executedCmd.WithResult<IO.User.NamedUser.IUserProfile?>().Result;
        profile.ShouldNotBeNull();
        profile.UserId.ShouldBe( userId );
        profile.UserName.ShouldBe( "System" );
        profile.FirstName.ShouldBe( "" );
        profile.LastName.ShouldBe( "" );
    }

    [Test]
    public async Task can_create_user_through_Cris_Async()
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
        var executedCmd = await _crisExecutionContext.ExecuteRootCommandAsync( cmd );

        var res = executedCmd.WithResult<ICreateUserCommandResult>().Result;
        res.CreatedUserId.ShouldBeGreaterThan( 1 );
        res.UserMessages.ShouldNotBeNull();

        var getcmd = _pocoDir.Create<IGetUserProfileQCommand>( cmd =>
        {
            cmd.ActorId = 1;
            cmd.UserId = res.CreatedUserId;
        } );

        var executedGetCmd = await _crisExecutionContext.ExecuteRootCommandAsync( getcmd );

        var profile = executedGetCmd.WithResult<IO.User.NamedUser.IUserProfile?>().Result;
        profile.ShouldNotBeNull();
        profile.FirstName.ShouldBe( fName );
        profile.LastName.ShouldBe( lName );
    }

    [Test]
    public async Task can_update_user_through_Cris_Async()
    {
        var createCmd = _pocoDir.Create<IO.User.NamedUser.ICreateUserCommand>( c =>
        {
            c.ActorId = 1;
            c.UserName = Guid.NewGuid().ToString();
        } );
        var executingCreateCmd = await _crisExecutionContext.ExecuteRootCommandAsync( createCmd );

        var createRes = executingCreateCmd.WithResult<ICreateUserCommandResult>().Result;

        var newName = Guid.NewGuid().ToString();
        var fName = Guid.NewGuid().ToString();
        var lName = Guid.NewGuid().ToString();
        var cmd = _pocoDir.Create<IO.User.NamedUser.IUpdateUserCommand>( c =>
        {
            c.ActorId = 1;
            c.UserId = createRes.CreatedUserId;
            c.UserName = newName;
            c.FirstName = fName;
            c.LastName = lName;
        } );
        var executingCmd = await _crisExecutionContext.ExecuteRootCommandAsync( cmd );
        var res = executingCmd.WithResult<ICrisBasicCommandResult>().Result;

        res.UserMessages.ShouldNotBeNull();

        var profile = await _package.GetUserProfileAsync( new SqlStandardCallContext(), 1, createRes.CreatedUserId );
        profile.ShouldNotBeNull();
        profile.UserName.ShouldBe( newName );
        profile.FirstName.ShouldBe( fName );
        profile.LastName.ShouldBe( lName );
    }

    [Test]
    public async Task can_destroy_user_through_Cris_Async()
    {
        var createCmd = _pocoDir.Create<IO.User.NamedUser.ICreateUserCommand>( c =>
        {
            c.ActorId = 1;
            c.UserName = Guid.NewGuid().ToString();
        } );
        var executingCreateCmd = await _crisExecutionContext.ExecuteRootCommandAsync( createCmd );
        var createRes = executingCreateCmd.WithResult<ICreateUserCommandResult>().Result;
        var cmd = _pocoDir.Create<IDestroyUserCommand>( c =>
        {
            c.ActorId = 1;
            c.UserId = createRes.CreatedUserId;
        } );
        var executingCmd = await _crisExecutionContext.ExecuteRootCommandAsync( cmd );
        var res = executingCmd.WithResult<ICrisBasicCommandResult>().Result;
        res.UserMessages.Where( um => um.Level == UserMessageLevel.Error ).ShouldBeEmpty();

        var getcmd = _pocoDir.Create<IGetUserProfileQCommand>( cmd =>
        {
            cmd.ActorId = 1;
            cmd.UserId = createRes.CreatedUserId;
        } );
        var executingGetCmd = await _crisExecutionContext.ExecuteRootCommandAsync( getcmd );

        var profile = executingGetCmd.WithResult<IO.User.NamedUser.IUserProfile?>().Result;
        profile.ShouldBeNull();
    }
}
