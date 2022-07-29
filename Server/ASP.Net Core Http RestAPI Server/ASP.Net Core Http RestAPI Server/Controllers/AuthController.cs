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
        public AuthController(ILogger<AuthController> log, AuthService authService)
        {
            this.authService = authService;
            this.log = log;
        }

        
        
        
        
        
        #region 로그인
        
        // 요청 URL
        // https://serverAddress/SignIn
        [HttpPost("SignIn")]
        [Consumes(MediaTypeNames.Application.Json)]
        [Produces(MediaTypeNames.Application.Json)]
        public async Task<ResponseSignIn> Post(RequestSignIn request)
        {
            var response = await authService.SignIn(request);
            
            log.LogInformation($"AuthController result: {response.result}, {response.jwtAccess}, {response.jwtRefresh}");

            return response;
        }
        #endregion
    }
}