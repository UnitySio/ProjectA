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
        
        
        
        
        
        
    }
}
