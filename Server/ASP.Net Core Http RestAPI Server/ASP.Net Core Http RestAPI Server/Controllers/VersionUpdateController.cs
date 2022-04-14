using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ASP.Net_Core_Http_RestAPI_Server.DBContexts;
using ASP.Net_Core_Http_RestAPI_Server.JsonDataModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace ASP.Net_Core_Http_RestAPI_Server.Controllers
{
    /*

    VersionUpdateController

    VersionUpdate (클라이언트 버전) 관련 로직을 처리하는 컨트롤러 클래스.


    API 리스트

    - 버전 및 업데이트 목록 체크


    */



    [ApiController]
    public class VersionUpdateController : ControllerBase
    {
        private static DBContextPoolManager<siogames_mainContext> dbPoolManager;
        private readonly ILogger<AuthController> debugLogger;

        public VersionUpdateController(ILogger<AuthController> logger)
        {
            this.debugLogger = logger;

            if(dbPoolManager == null)
                dbPoolManager = new DBContextPoolManager<siogames_mainContext>();
        }


        //클라버전은 1.0.0 으로 간결하게
        //버전 문자열 정규식 (1.0.0)
        Regex versionPattern = new Regex("^([0-9]+.[0-9]+.[0-9]+)$");


        //요청 URI
        // http://serverAddress/version-check
        [HttpPost("version-check")]
        public async Task<Response_VersionUpdate> Post(Request_VersionCheck versionData)
        {
            //DB에 접속하여 데이터를 조작하는 DBContext객체를 가져옴.
            var dbContext = dbPoolManager.Rent();

            Response_VersionUpdate responseData = new Response_VersionUpdate();

            if (versionData == null)
            {
                responseData.result = "Error. 잘못된 형태의 데이터.";
            }
            else if (string.IsNullOrEmpty(versionData.currentClientVersion))
            {
                responseData.result = "currentClientVersion 의 값이 공백입니다.";
            }
            //버전 문자열 정규식 검사.  알맞은 형식의 문자열이 아니라면 false 반환.
            else if (versionPattern.IsMatch(versionData.currentClientVersion))
            {
                //클라이언트 버전 체크 및 업데이트 대상 파일의 목록, 다운로드 목록 URL등을 가공하여 client로 반환.

                //디비나 설정파일에서 최신버전 정보를 불러온후, 최신버전인지 체크.
                bool versionOK;
                
                if (versionData.currentClientVersion == "1.0.0")
                    versionOK = true;
                else
                    versionOK = false;

                //최신버전이라면
                if (versionOK)
                {
                    responseData.result = "ok";
                    responseData.needUpdate = false;
                }
                //업데이트가 필요하다면
                else
                {
                    responseData.result = "ok";
                    responseData.needUpdate = true;

                    //디비나 설정파일에서 업데이트 대상 파일들의 정보를 불러온후, 지정한다.
                    int filecount = 200;
                    for (int i = 0; i < filecount; i++)
                    {
                        responseData.updateFileList.Add(new UpdateFileData()
                        {
                            filePath = "/Assets/Texture/aaaaa",
                            fileName = $"test_{i}",
                            fileSize = 12345678,
                            fileHash = "kjhf23489glkq3hjiofug09qtggt==",
                            fileURL = "https://s3.amazon.com/ap-southeast-2/bucketname/test어쩌구저쩌구"
                        });
                    }
                }
            }
            else
            {
                responseData.result = "currentClientVersion 의 값이 정상적이지 않습니다.";
            }

            //사용이 끝난 DBContext객체를 반환.
            dbPoolManager.Return(dbContext);

            //클라이언트에게 Json문자열 응답.
            return responseData;
        }
    }
}
