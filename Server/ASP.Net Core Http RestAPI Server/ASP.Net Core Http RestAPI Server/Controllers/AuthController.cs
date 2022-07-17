using System.Net.Mime;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ASP.Net_Core_Http_RestAPI_Server.JsonDataModels;
using ASP.Net_Core_Http_RestAPI_Server.Services;

namespace ASP.Net_Core_Http_RestAPI_Server.Controllers
{
    [ApiController]
    public class AuthController : ControllerBase
    {
        private ILogger<AuthController> log;
        private AuthService authService;

        //Construct Dependency Injections
        public AuthController(ILogger<AuthController> logger, AuthService authService)
        {
            this.authService = authService;
            this.log = logger;
        }

        #region 로그인
        
        // 요청 URL
        // https://serverAddress/signin
        [HttpPost("signin")]
        [Consumes(MediaTypeNames.Application.Json)]
        [Produces(MediaTypeNames.Application.Json)]
        public async Task<ResponseSignIn> Post(RequestSignIn request)
        {
            var response = await authService.signin(request);
            
            log.LogInformation($"AuthController result: {response.result}, {response.jwtAccess}, {response.jwtRefresh}");

            return response;
        }
        #endregion
        
        
        
        
        
        /*private static string ConvertToInsertIntoSQL(object obj)
        {
            PrimaryDataSource dbContext = dbContextFactory.CreateDbContext();
  
            var aa = dbContext.UserInfos.FromSqlInterpolated($"select * from user_info where 1=1").ToList();
  
            int affectRows = dbContext.Database.ExecuteSqlInterpolated($"insert into (a,b,c) values ()");
  
  
            var firstName = "John";
            var id = 12;
            int affectRow1 = dbContext.Database.ExecuteSqlInterpolated($"Update [User] SET FirstName = {firstName} WHERE Id = {id}");
  
  
            var test = dbContext.GetType().GetCustomAttribute(typeof(HttpPostAttribute)) as HttpPostAttribute;
  
            var test2 = response.GetType().GetMethod("").GetCustomAttribute(typeof(HttpPostAttribute)) as HttpPostAttribute;
  
            var data = test2.Template;
  
            var property = response.GetType().GetProperties();
  
            var attr = property[0].GetCustomAttribute(typeof(HttpPostAttribute)) as HttpPostAttribute;
  
  
  
  
  
  
            var table = obj.GetType().GetCustomAttribute(typeof(Table)) as Table;
            var sql = "insert into " + table.Name + "(";
            var columns = new List<string>();
            var values = new List<object>();
            foreach (var propertyInfo in obj.GetType().GetProperties())
            {
                var column = propertyInfo.GetCustomAttribute(typeof(Column)) as Column;
                columns.Add(column.Name);
                if (propertyInfo.PropertyType.Name == "String" || propertyInfo.PropertyType.Name == "Boolean")
                {
                    values.Add("\"" + propertyInfo.GetValue(obj).ToString() + "\"");
                }
                else if (propertyInfo.PropertyType.Name == "DateTime")
                {
                    var dateTime = (DateTime)propertyInfo.GetValue(obj);
                    values.Add("\"" + dateTime.ToString("yyyy-MM-dd") + "\"");
                }
                else
                {
                    values.Add(propertyInfo.GetValue(obj).ToString());
                }
            }
            sql += string.Join(", ", columns) + ") values(";
            sql += string.Join(", ", values) + ")";
            return sql;
        }*/
    }
}