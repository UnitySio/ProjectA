using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mime;
using System.Threading.Tasks;
using ASP.Net_Core_Http_RestAPI_Server.DBContexts;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace ASP.Net_Core_Http_RestAPI_Server.Controllers
{
    //test용 Controller.

    [ApiController]
    [Route("[controller]")]
    public class _TestController : ControllerBase
    {
        private static DBContextPoolManager<siogames_mainContext> dbPoolManager;
        private readonly ILogger<_TestController> debugLogger;

        public _TestController(ILogger<_TestController> logger)
        {
            this.debugLogger = logger;
            if (dbPoolManager == null)
                dbPoolManager = new DBContextPoolManager<siogames_mainContext>();
        }
        
        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        /*[HttpGet]
        public async Task<IEnumerable<TestData>> Get()
        {
            //http 요청헤더를 가져와서 검색.
            var headers = Request.Headers;
            if (headers.ContainsKey("HeaderParam"))
            {
                var header_value = headers["HeaderParam"];
                debugLogger.LogInformation($"11 WeatherForecast2 HeaderParam : {header_value}");
            }

            Random aa = new Random();

            //테스트용 유저.
            string targetUserName = $"test_{aa.Next(0, 50000)}";

            var dbContext = dbPoolManager.Rent();

            //해당 유저가 존재하는지 체크후, 존재한다면 pass, 존재하지 않다면 insert함.
            var result = dbContext.TestTables
                .Where((table) =>
                    table.UserName.Equals(targetUserName))
                .AsNoTracking()
                .Count();

            //존재한다면.
            if (result > 0)
            {
                debugLogger.LogInformation("11 Test Get");
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

            dbPoolManager.Return(dbContext);


            //http응답 헤더에 삽입 추가.
            if (!Response.Headers.ContainsKey("HeaderKey"))
                Response.Headers.Add("HeaderKey", $"HeaderValue:{DateTime.Now.ToString("yyyy-MM-dd_HH:mm:ss")}");
            else
                Response.Headers["HeaderKey"] = $"HeaderValue:{DateTime.Now.ToString("yyyy-MM-dd_HH:mm:ss")}";


            return Enumerable.Range(1, 5).Select(index => new TestData
            {
                Date = DateTime.Now.AddDays(index),
                TemperatureC = aa.Next(-20, 55),
                Summary = Summaries[aa.Next(Summaries.Length)]
            })
                .ToArray();
        }*/



        //route방식.
        [HttpGet("{id}/{id_str}/{pw_str}")] //=> https://localhost/ControllerName/id/id_str/pw_str
        public async Task<string> Get_params_Route(int id, string id_str, string pw_str)
        {
            var dbContext = dbPoolManager.Rent();

            debugLogger.LogInformation($"Get_params => [{id},{id_str},{pw_str}]");

            var changedCount = await dbContext.SaveChangesAsync();

            dbPoolManager.Return(dbContext);

            return $"Get_params => [{id},{id_str},{pw_str}]";
        }

        //query방식.
        [HttpGet("tests-value")] //=> https://localhost/ControllerName/tests-value?id=aa&id_str=bb&pw_str=cc
        //=> https://localhost/ControllerName/tests?id=aa&id_str=bb 만 보낼시에는 pw_str은 공백으로 전달됨.
        public async Task<string> Get_params_Query(int id, string id_str, string pw_str)
        {
            var dbContext = dbPoolManager.Rent();

            debugLogger.LogInformation($"Get_params => [{id},{id_str},{pw_str}]");

            var changedCount = await dbContext.SaveChangesAsync();

            dbPoolManager.Return(dbContext);

            return $"Get_params => [{id},{id_str},{pw_str}]";
        }

        //route + query방식.
        [HttpGet("{id2}/{id_str2}/{pw_str2}/tests")] //=> https://localhost/ControllerName/id2/id_str2/pw_str2/tests?id=aa&id_str=bb&pw_str=cc
        public async Task<string> Get_params_RouteAndQuery(int id, string id_str, string pw_str, int id2, string id_str2, string pw_str2)
        {
            var dbContext = dbPoolManager.Rent();

            debugLogger.LogInformation($"Get_params => [{id},{id_str},{pw_str},{id2},{id_str2},{pw_str2}]");

            var changedCount = await dbContext.SaveChangesAsync();

            dbPoolManager.Return(dbContext);

            return $"Get_params => [{id},{id_str},{pw_str},{id2},{id_str2},{pw_str2}]";
        }


        // [HttpPost("Test/Login-11")]
        // //[Consumes(MediaTypeNames.Application.Octet, MediaTypeNames.Application.Zip)]
        // //[Produces(MediaTypeNames.Application.Octet, MediaTypeNames.Application.Zip)]
        // public async Task<TestLogin> Post_binary()
        // {
        //     string filePath = Path.Combine(WAS_Config.getWebRootDir(), "fileName.mkv");
        //
        //     if (!Directory.Exists(Path.GetDirectoryName(filePath)))
        //         Directory.CreateDirectory(Path.GetDirectoryName(filePath));
        //
        //     if (System.IO.File.Exists(filePath))
        //     {
        //         System.IO.File.Delete(filePath);
        //     }
        //
        //     byte[] data;
        //     using (var ms = new MemoryStream())
        //     {
        //         await Request.Body.CopyToAsync(ms);
        //         data = ms.ToArray();
        //     }
        //     await System.IO.File.WriteAllBytesAsync(filePath, data);
        //
        //     return new TestLogin();
        // }
        //
        // [HttpPost("Test/Login11")]
        // //[Consumes(MediaTypeNames.Application.Octet, MediaTypeNames.Application.Zip)]
        // public async Task<TestLogin> Post_binary([FromForm] IFormFile file)
        // {
        //     debugLogger.LogInformation($"file => {file.FileName}, {file.Name}, {file.Length}, {file.Headers}");
        //
        //     string filePath = Path.Combine(WAS_Config.getWebRootDir(), file.FileName);
        //
        //     if (!Directory.Exists(Path.GetDirectoryName(filePath)))
        //         Directory.CreateDirectory(Path.GetDirectoryName(filePath));
        //
        //     if (System.IO.File.Exists(filePath))
        //     {
        //         System.IO.File.Delete(filePath);
        //     }
        //
        //     using (FileStream fs = new FileStream(filePath, FileMode.CreateNew))
        //     {
        //         byte[] buffer = new byte[1048576];
        //         int pos = 0;
        //         while (true)
        //         {
        //             int count = await file.OpenReadStream().ReadAsync(buffer, pos, buffer.Length);
        //             debugLogger.LogInformation($"buffer => {buffer.Length}, pos => {pos} count => {count}");
        //
        //             if (count <= 0)
        //                 break;
        //
        //             pos += count;
        //             await fs.WriteAsync(buffer, 0, count);
        //             await fs.FlushAsync();
        //         }
        //
        //         debugLogger.LogInformation($"userLoginDataLength => {fs.Position}, {pos}");
        //     }
        //
        //     return new TestLogin();
        // }
        //
        //
        // [HttpPost("test/login-1string123")] //https://localhost/ControllerName/test/login-1string123
        // [Consumes(MediaTypeNames.Application.Json, MediaTypeNames.Text.Plain)]
        // public async Task<string> Post_string(string data)
        // {
        //     using (var reader = new StreamReader(Request.Body))
        //     {
        //         data = await reader.ReadToEndAsync();
        //
        //         debugLogger.LogInformation($"userLoginDatastring => {data}");
        //
        //         return data;
        //     }
        // }
        //
        //
        //
        // [HttpPost("Test/Login2")]
        // //[Consumes(MediaTypeNames.Application.Json, MediaTypeNames.Text.Plain)]
        // public async Task<TestLogin> Post_json(TestLogin data)
        // {
        //     var dbContext = dbPoolManager.Rent();
        //
        //     debugLogger.LogInformation($"userLoginData => {data.AccountId}");
        //
        //     var changedCount = await dbContext.SaveChangesAsync();
        //
        //     dbPoolManager.Return(dbContext);
        //
        //     return new TestLogin();
        // }
        //


    }

    public class TestData
    {
        public DateTime Date { get; set; }

        public int TemperatureC { get; set; }

        public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);

        public string Summary { get; set; }
    }
}
