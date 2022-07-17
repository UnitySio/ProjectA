using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace ASP.Net_Core_Http_RestAPI_Server.DBContexts.Mappers
{
    public class UserInfoMapper
    {
        private IDbContextFactory<projectaContext> dbContextFactory;
        
        //Construct Dependency Injections
        public UserInfoMapper(IDbContextFactory<projectaContext> dbContextFactory)
        {
            this.dbContextFactory = dbContextFactory;
        }

        
        
        
        
        //db에 새로운 데이터를 insert
        public async Task<int> InsertUserInfo(UserInfo userInfo)
        {
            using (var dbContext = dbContextFactory.CreateDbContext())
            {
                await dbContext.UserInfos.AddAsync(userInfo);
                dbContext.Entry(userInfo).State = EntityState.Added;
                return await dbContext.SaveChangesAsync();
            }
        }
        
        
        
        public UserInfo GetUserInfoByAccountUniqueID(string accountUniqueID)
        {
            using (var dbContext = dbContextFactory.CreateDbContext())
            {
                var userQuery = dbContext.UserInfos
                    .Where(table =>
                        table.AccountUniqueId.Equals(accountUniqueID)
                    ).AsNoTracking();
                

                if (userQuery.Any())
                    return userQuery.FirstOrDefault();
                else
                    return null;
            }
        }
        
        
        //db변경사항 update 반영
        public async Task<int> UpdateUserInfo(UserInfo userInfo)
        {
            using (var dbContext = dbContextFactory.CreateDbContext())
            {
                dbContext.Entry(userInfo).State = EntityState.Modified;
                return await dbContext.SaveChangesAsync();
            }
        }
        
        
        
        
        
    }
}
