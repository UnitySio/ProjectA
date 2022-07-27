using System;
using System.Threading.Tasks;
using ASP.Net_Core_Http_RestAPI_Server.DBContexts;
using ASP.Net_Core_Http_RestAPI_Server.DBContexts.Mappers;
using ASP.Net_Core_Http_RestAPI_Server.JsonDataModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ASP.Net_Core_Http_RestAPI_Server.Services;

public class TransactionService
{
    private ILogger<TransactionService> log;

    private IDbContextFactory<projectaContext> dbContextFactory;
    private AccountInfoMapper accountInfoMapper;
    private UserCharacterInfoMapper userCharacterInfoMapper;
    private UserInfoMapper userInfoMapper;
    private UserSigninLogMapper userSigninLogMapper;

    //Construct Dependency Injections
    public TransactionService(ILogger<TransactionService> log,
        IDbContextFactory<projectaContext> dbContextFactory, AccountInfoMapper accountInfoMapper,
        UserCharacterInfoMapper userCharacterInfoMapper, UserInfoMapper userInfoMapper,
        UserSigninLogMapper userSigninLogMapper)
    {
        this.log = log;
        this.dbContextFactory = dbContextFactory;
        this.accountInfoMapper = accountInfoMapper;
        this.userCharacterInfoMapper = userCharacterInfoMapper;
        this.userInfoMapper = userInfoMapper;
        this.userSigninLogMapper = userSigninLogMapper;
    }


    //트랜잭션 처리 메소드.
    public async Task RunTaskWithTx(Func<Task> runTask)
    {
        Exception exception = null;
        
        using (var dbContext = dbContextFactory.CreateDbContext())
        {
            var strategy = dbContext.Database.CreateExecutionStrategy();

            Func<Task> dbTransactionTask = async () =>
            {
                using (var transaction = await dbContext.Database.BeginTransactionAsync())
                {
                    try
                    {
                        await runTask();
                        
                        await transaction.CommitAsync();
                    }
                    catch (Exception e)
                    {
                        exception = e;
                        await transaction.RollbackAsync();
                    }
                }
            };

            await strategy.ExecuteAsync(dbTransactionTask);
        }
        
        if (exception != null)
        {
            throw exception;
        }
    }
}