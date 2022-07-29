using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace ASP.Net_Core_Http_RestAPI_Server.DBContexts.Mappers
{
    public class AccountInfoMapper
    {
        private IDbContextFactory<projectaContext> dbContextFactory;
        
        
        //Construct Dependency Injections
        public AccountInfoMapper(IDbContextFactory<projectaContext> dbContextFactory)
        {
            this.dbContextFactory = dbContextFactory;
        }

        
        
        
        //db에 새로운 데이터를 insert
        public async Task<int> InsertAccountInfo(AccountInfo accountInfo)
        {
            using (var dbContext = dbContextFactory.CreateDbContext())
            {
                await dbContext.AccountInfos.AddAsync(accountInfo);
                dbContext.Entry(accountInfo).State = EntityState.Added;
                return await dbContext.SaveChangesAsync();
            }
        }
        

        //guest token으로 AccountInfo를 디비에서 조회
        public AccountInfo GetAccountInfoByGuestToken(string guestToken)
        {
            using (var dbContext = dbContextFactory.CreateDbContext())
            {
                var accountQuery = dbContext.AccountInfos
                    .Where(table =>
                        table.AccountGuestToken.Equals(guestToken)
                    ).AsNoTracking();

                if (accountQuery.Any())
                    return accountQuery.FirstOrDefault();
                else
                    return null;
            }
        }

        //accountUniqueID 으로 AccountInfo를 디비에서 조회
        public AccountInfo GetAccountInfoByAccountUniqueID(string accountUniqueID)
        {
            using (var dbContext = dbContextFactory.CreateDbContext())
            {
                var accountQuery = dbContext.AccountInfos
                    .Where(table =>
                        table.AccountUniqueId.Equals(uint.Parse(accountUniqueID))
                    ).AsNoTracking();

                if (accountQuery.Any())
                    return accountQuery.FirstOrDefault();
                else
                    return null;
            }
        }
        
        //db변경사항 update 반영
        public async Task<int> UpdateAccountInfo(AccountInfo accountInfo)
        {
            using (var dbContext = dbContextFactory.CreateDbContext())
            {
                dbContext.Entry(accountInfo).State = EntityState.Modified;
                return await dbContext.SaveChangesAsync();
            }
        }
        
        



    }
}
