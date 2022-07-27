using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace ASP.Net_Core_Http_RestAPI_Server.DBContexts.Mappers
{
    public class UserCharacterInfoMapper
    {
        private IDbContextFactory<projectaContext> dbContextFactory;
        
        //Construct Dependency Injections
        public UserCharacterInfoMapper(IDbContextFactory<projectaContext> dbContextFactory)
        {
            this.dbContextFactory = dbContextFactory;
        }
        
        
        
        
        
        //db에 새로운 데이터를 insert
        public async Task<int> InsertUserCharacterInfo(UserCharacterInfo userCharacterInfo)
        {
            using (var dbContext = dbContextFactory.CreateDbContext())
            {
                await dbContext.UserCharacterInfos.AddAsync(userCharacterInfo);
                dbContext.Entry(userCharacterInfo).State = EntityState.Added;
                return await dbContext.SaveChangesAsync();
            }
        }
        
        
        
        public UserCharacterInfo GetUserCharacterInfoByAccountUniqueIDAndCharacterUniqueID(string accountUniqueID, string characterUniqueID)
        {
            using (var dbContext = dbContextFactory.CreateDbContext())
            {
                var userCharacterInfoQuery = dbContext.UserCharacterInfos
                    .Where(table =>
                        table.AccountUniqueId.Equals(accountUniqueID) &&
                        table.CharacterUniqueId.Equals(uint.Parse(characterUniqueID)))
                    .AsNoTracking();

                if (userCharacterInfoQuery.Any())
                    return userCharacterInfoQuery.FirstOrDefault();
                else
                    return null;
            }
        }
        
        
        public List<UserCharacterInfo> GetUserCharacterInfoListByAccountUniqueID(string accountUniqueID)
        {
            using (var dbContext = dbContextFactory.CreateDbContext())
            {
                var userCharacterInfoQuery = dbContext.UserCharacterInfos
                    .Where(table => 
                        table.AccountUniqueId.Equals(accountUniqueID))
                    .AsNoTracking();

                if (userCharacterInfoQuery.Any())
                    return userCharacterInfoQuery.ToList();
                else
                    return null;
            }
        }
    }
}
