using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace ASP.Net_Core_Http_RestAPI_Server.DBContexts.Mappers
{
    public class UserSigninLogMapper
    {
        private IDbContextFactory<projectaContext> dbContextFactory;
        
        //Construct Dependency Injections
        public UserSigninLogMapper(IDbContextFactory<projectaContext> dbContextFactory)
        {
            this.dbContextFactory = dbContextFactory;
        }

        
        //db에 새로운 데이터를 insert
        public async Task<int> InsertUserSigninLog(UserSigninLog userSigninLog)
        {
            using (var dbContext = dbContextFactory.CreateDbContext())
            {
                await dbContext.UserSigninLogs.AddAsync(userSigninLog);
                dbContext.Entry(userSigninLog).State = EntityState.Added;
                return await dbContext.SaveChangesAsync();
            }
        }
        
        
        
        
    }
}
