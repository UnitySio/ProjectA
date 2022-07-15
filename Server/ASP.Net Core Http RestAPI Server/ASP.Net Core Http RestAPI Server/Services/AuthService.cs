using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ASP.Net_Core_Http_RestAPI_Server.Controllers;
using ASP.Net_Core_Http_RestAPI_Server.DBContexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ASP.Net_Core_Http_RestAPI_Server.Services
{
    public class AuthService
    {
        private IDbContextFactory<PrimaryDataSource> dbContextFactory;
        private ILogger<AuthService> log;

        //Construct Dependency Injections
        public AuthService(ILogger<AuthService> logger, IDbContextFactory<PrimaryDataSource> dbContextFactory)
        {
            this.log = logger;
            this.dbContextFactory = dbContextFactory;
        }










    }
}
