using CK.Core;
using CK.Cris;
using CK.IO.User.NamedUser;
using CK.SqlServer;
using Microsoft.Data.SqlClient;
using System;
using System.Threading.Tasks;

namespace CK.DB.User.NamedUser;

public partial class Package
{
    [CommandHandler]
    public async Task<IO.User.UserProfile.ICreateUserCommandResult> HandleCreateUserAsync( ISqlTransactionCallContext ctx, UserMessageCollector collector, ICreateUserCommand cmd )
    {
        using( ctx.Monitor.OpenInfo( $"Handling ICreateUserCommand. (ActorId: {cmd.ActorId})" ) )
        {
            var res = cmd.CreateResult();
            try
            {
                using( var transaction = ctx[NamedUserTable].BeginTransaction() )
                {
                    var userId = await CreateUserAsync( ctx,
                                                        collector,
                                                        cmd.ActorId.GetValueOrDefault(),
                                                        cmd.UserName,
                                                        cmd.FirstName,
                                                        cmd.LastName );
                    transaction.Commit();
                    res.CreatedUserId = userId;
                    collector.Info( $"User successfully created. (UserId: {userId})", "User.UserCreated" );
                }
            }
            catch( SqlDetailedException ex ) when( ex.InnerSqlException is SqlException sqlEx )
            {
                ctx.Monitor.Error( $"Error while handling ICreateUserCommand: {ex.Message}", ex );
                collector.Error( ex );
            }
            catch( Exception ex )
            {
                ctx.Monitor.Error( $"Error while handling ICreateUserCommand: {ex.Message}", ex );
                collector.Error( "An error occurred while creating the user.", "User.UserCreationFailed" );
            }

            res.SetUserMessages( collector );
            return res;
        }
    }

    [CommandHandler]
    public async Task<ICrisBasicCommandResult> HandleUpdateUserAsync( ISqlTransactionCallContext ctx, UserMessageCollector collector, IUpdateUserCommand cmd )
    {
        using( ctx.Monitor.OpenInfo( $"Handling IUpdateUserCommand. (ActorId: {cmd.ActorId})" ) )
        {
            var res = cmd.CreateResult();
            try
            {
                using( var transaction = ctx[NamedUserTable].BeginTransaction() )
                {
                    await UpdateUserAsync( ctx, collector, cmd.ActorId.GetValueOrDefault(), cmd.UserId, cmd.UserName, cmd.FirstName, cmd.LastName );
                    transaction.Commit();
                }
            }
            catch( SqlDetailedException ex ) when( ex.InnerSqlException is not null )
            {
                ctx.Monitor.Error( $"Error while handling IUpdateUserCommand: {ex.Message}", ex );
                collector.Error( ex );
            }
            catch( Exception ex )
            {
                ctx.Monitor.Error( $"Error while handling IUpdateUserCommand: {ex.Message}", ex );
                collector.Error( "An error occurred while updating the user.", "User.UserUpdateFailed" );
            }

            res.SetUserMessages( collector );
            return res;
        }
    }
}
