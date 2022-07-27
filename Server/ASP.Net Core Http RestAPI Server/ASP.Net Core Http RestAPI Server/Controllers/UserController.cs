using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Linq;
using System.Net.Mime;
using System.Threading.Tasks;
using ASP.Net_Core_Http_RestAPI_Server.DBContexts;
using ASP.Net_Core_Http_RestAPI_Server.JsonDataModels;
using ASP.Net_Core_Http_RestAPI_Server.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;

namespace ASP.Net_Core_Http_RestAPI_Server.Controllers
{
    /*

    UserController

    User (사용자) 관련 로직을 처리하는 컨트롤러 클래스.


    API 리스트

    - 유저 데이터 요청
    - 닉네임 생성 및 변경

    */


    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly ILogger<UserController> log;
        private UserService userService;

        
        //Construct Dependency Injections
        public UserController(ILogger<UserController> log, UserService userService)
        {
            this.userService = userService;
            this.log = log;
        }


        
        #region 유저 데이터 요청
        
        //요청 URL
        // http://serverAddress/userdata
        [HttpPost("userdata")]
        [Consumes(MediaTypeNames.Application.Json)] // application/json
        public async Task<ResponseUserData> GetUserData(RequestUserData request)
        {
            var response = await userService.GetUserData(request);
            
            return response;
        }
        
        #endregion
        
        #region 닉네임 생성 및 변경
        
        //요청 URL
        // http://serverAddress/userdata/nickname/update
        [HttpPost("userdata/nickname/update")]
        [Consumes(MediaTypeNames.Application.Json)] // application/json
        public async Task<ResponseUpdateUserNickname> UpdateNickName(RequestUpdateUserNickname request)
        {
            var response = await userService.UpdateNickName(request);
            
            return response;
        }
        
        #endregion
    }
}