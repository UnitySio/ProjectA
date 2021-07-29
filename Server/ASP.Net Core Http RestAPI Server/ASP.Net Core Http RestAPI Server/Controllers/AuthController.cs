using System;
using System.Linq;
using System.Net.Mime;
using System.Threading.Tasks;
using ASP.NET_Core_RestfulAPI_TestServer.DBContexts;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ASP.Net_Core_Http_RestAPI_Server.Controllers
{
    [ApiController]
    public class AuthController : ControllerBase
    {
        private static DBContextPool<aspnetcore_testContext> pool;
        private readonly ILogger<AuthController> debugLogger;

        public AuthController(ILogger<AuthController> logger)
        {
            this.debugLogger = logger;
            pool = new DBContextPool<aspnetcore_testContext>();
        }



        //추후 수정예정.
        [HttpPost("Auth/Login")]
        //[Consumes(MediaTypeNames.Application.Json, MediaTypeNames.Text.Plain)]
        public async Task<TestLogin> Post_json(TestLogin data)
        {
            var dbContext = pool.Rent();
            
            debugLogger.LogInformation($"userLoginData => {data.AccountId}");

            Random aa = new Random();

            //테스트용 유저.
            string targetUserName = $"test_{aa.Next(0, 50000)}";
            
            //해당 유저가 존재하는지 체크후, 존재한다면 pass, 존재하지 않다면 insert함.
            var result = dbContext.TestTables
                .Where((table) =>
                    table.UserName.Equals(targetUserName))
                .AsNoTracking()
                .Count();

            //존재한다면.
            if (result > 0)
            {
                debugLogger.LogInformation("11 WeatherForecast Get");
            }
            //없다면.
            else
            {
                var resultData = new TestTable()
                {
                    Level = 1,
                    Exp = aa.Next(0, 50000),
                    TestString = DateTime.Now.ToString(),
                    UserName = targetUserName,
                    UserPassword = "999999asdads!"
                };

                await dbContext.TestTables.AddAsync(resultData);

                dbContext.Entry(resultData).State = EntityState.Added;
            }

            var changedCount = await dbContext.SaveChangesAsync();

            pool.Return(dbContext);

            return new TestLogin();
        }
        


    }
}
