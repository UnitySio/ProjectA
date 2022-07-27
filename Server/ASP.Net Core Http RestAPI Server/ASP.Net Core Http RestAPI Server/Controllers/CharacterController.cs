using System.Net.Mime;
using System.Threading.Tasks;
using ASP.Net_Core_Http_RestAPI_Server.JsonDataModels;
using ASP.Net_Core_Http_RestAPI_Server.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace ASP.Net_Core_Http_RestAPI_Server.Controllers
{
    [ApiController]
    public class CharacterController : ControllerBase
    {
        private readonly ILogger<CharacterController> log;
        private CharacterService characterService;

        
        
        //Construct Dependency Injections
        public CharacterController(ILogger<CharacterController> log,
            CharacterService characterService)
        {
            this.characterService = characterService;
            this.log = log;
        }

        
        
        
        
        
        
        

        #region 캐릭터 추가

        //요청 URL
        // http://serverAddress/userdata/character/add
        [HttpPost("userdata/character/add")]
        [Consumes(MediaTypeNames.Application.Json)]
        public async Task<ResponseAddCharacter> AddCharacter(RequestAddCharacter request)
        {
            var response = await characterService.AddCharacter(request);

            return response;
        }

        #endregion


        
        
        #region 캐릭터 목록 요청

        //요청 URL
        // http://serverAddress/userdata/character
        [HttpPost("userdata/character")]
        [Consumes(MediaTypeNames.Application.Json)]
        public async Task<ResponseGetCharacter> GetCharacter(RequestGetCharacter request)
        {
            var response = await characterService.GetCharacter(request);

            return response;
        }

        #endregion
    }
}