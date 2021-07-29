using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mime;
using System.Threading.Tasks;
using ASP.NET_Core_RestfulAPI_TestServer.DBContexts;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace ASP.Net_Core_Http_RestAPI_Server.Controllers
{
    //test용 Controller.

    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private static DBContextPool<aspnetcore_testContext> pool;
        private readonly ILogger<WeatherForecastController> debugLogger;

        public WeatherForecastController(ILogger<WeatherForecastController> logger)
        {
            this.debugLogger = logger;
            pool = new DBContextPool<aspnetcore_testContext>();
        }
        
        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        [HttpGet]
        [Consumes(MediaTypeNames.Application.Json, MediaTypeNames.Text.Plain)]
        [Produces(MediaTypeNames.Application.Json, MediaTypeNames.Text.Plain)]
        public async Task<IEnumerable<WeatherForecast>> Get()
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

            var dbContext = pool.Rent();

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


            //http응답 헤더에 삽입 추가.
            if (!Response.Headers.ContainsKey("HeaderKey"))
                Response.Headers.Add("HeaderKey", $"HeaderValue:{DateTime.Now.ToString("yyyy-MM-dd_HH:mm:ss")}");
            else
                Response.Headers["HeaderKey"] = $"HeaderValue:{DateTime.Now.ToString("yyyy-MM-dd_HH:mm:ss")}";


            return Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateTime.Now.AddDays(index),
                TemperatureC = aa.Next(-20, 55),
                Summary = Summaries[aa.Next(Summaries.Length)]
            })
                .ToArray();
        }

        [HttpPost("WeatherForecast/Login1")]
        //[Consumes(MediaTypeNames.Application.Octet, MediaTypeNames.Application.Zip)]
        public async Task<TestLogin> Post_binary()
        {
            string filePath = Path.Combine(WAS_Config.getWebRootDir(), "fileName.mkv");

            if (!Directory.Exists(Path.GetDirectoryName(filePath)))
                Directory.CreateDirectory(Path.GetDirectoryName(filePath));

            if (System.IO.File.Exists(filePath))
            {
                System.IO.File.Delete(filePath);
            }

            byte[] data;
            using (var ms = new MemoryStream())
            {
                await Request.Body.CopyToAsync(ms);
                data = ms.ToArray();
                await System.IO.File.WriteAllBytesAsync(filePath, data);
            }

            return new TestLogin();
        }

        [HttpPost("WeatherForecast/Login11")]
        //[Consumes(MediaTypeNames.Application.Octet, MediaTypeNames.Application.Zip)]
        public async Task<TestLogin> Post_binary([FromForm] IFormFile file)
        {
            debugLogger.LogInformation($"file => {file.FileName}, {file.Name}, {file.Length}, {file.Headers}");

            string filePath = Path.Combine(WAS_Config.getWebRootDir(), file.FileName);

            if (!Directory.Exists(Path.GetDirectoryName(filePath)))
                Directory.CreateDirectory(Path.GetDirectoryName(filePath));

            if (System.IO.File.Exists(filePath))
            {
                System.IO.File.Delete(filePath);
            }

            using (FileStream fs = new FileStream(filePath, FileMode.CreateNew))
            {
                byte[] buffer = new byte[1048576];
                int pos = 0;
                while (true)
                {
                    int count = await file.OpenReadStream().ReadAsync(buffer, pos, buffer.Length);
                    debugLogger.LogInformation($"buffer => {buffer.Length}, pos => {pos} count => {count}");

                    if (count <= 0)
                        break;

                    pos += count;
                    await fs.WriteAsync(buffer, 0, count);
                    await fs.FlushAsync();
                }

                debugLogger.LogInformation($"userLoginDataLength => {fs.Position}, {pos}");
            }

            return new TestLogin();
        }


        [HttpPost("WeatherForecast/Login")]
        [Consumes(MediaTypeNames.Application.Json, MediaTypeNames.Text.Plain)]
        public async Task<string> Post_string(string data)
        {
            using (var reader = new StreamReader(Request.Body))
            {
                data = await reader.ReadToEndAsync();

                debugLogger.LogInformation($"userLoginDatastring => {data}");

                return data;
            }
        }



        [HttpPost("WeatherForecast/Login2")]
        //[Consumes(MediaTypeNames.Application.Json, MediaTypeNames.Text.Plain)]
        public async Task<TestLogin> Post_json(TestLogin data)
        {
            var dbContext = pool.Rent();

            debugLogger.LogInformation($"userLoginData => {data.AccountId}");

            var changedCount = await dbContext.SaveChangesAsync();

            pool.Return(dbContext);

            return new TestLogin();
        }
    }
}
