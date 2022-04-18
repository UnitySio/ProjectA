using System.IO;


namespace ASP.Net_Core_Http_RestAPI_Server
{
    public class WAS_Config
    {
        //WebApplicationServer 설정
        static string WAS_LISTEN_HTTP_Port = "5001"; //(TCP Port)
        static string[] WAS_CORS_URL_List = { 
            "https://api.wizard87.com",
            "http://api.wizard87.com"
        };

        //WebRootDirectory 경로 설정
        static string WebRootDir = string.Empty;

        //연동할 RDBMS관련 접속정보 설정
        static string DBMSAddress = "db.aruku.kro.kr";
        static string DBMSPort = "3333";
        static string DBMSUser = "siogames_admin";
        static string DBMSPassword = "qwert12345!Q";
        static string targetDatabaseName = "siogames_main";
        static string DBMSMaxConnectionPoolSize = "2000";


        //SMTP 인증메일용 Gmail 계정 정보
        static string GmailAddr = "siogames2020@gmail.com";
        static string GmailPassword = "siogames!";

        //리슨 URL
        public static string getWAS_URLInfo()
        {
            var WAS_URL = $"http://*:{WAS_LISTEN_HTTP_Port}";
            return WAS_URL;
        }

        //CORS_허용URL
        public static string[] getWAS_CORS_URL_List()
        {
            return WAS_CORS_URL_List;
        }

        //RDBMS 연결정보
        public static string getDBConnInfo()
        {
            var ConnectionString =
                $"server={DBMSAddress};port={DBMSPort};" +
                $"user={DBMSUser};password={DBMSPassword};" +
                $"database={targetDatabaseName};maxpoolsize={DBMSMaxConnectionPoolSize}";

            return ConnectionString;
        }

        //웹루트 디렉토리 경로
        public static string getWebRootDir()
        {
            WebRootDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwRoot");

            if (!Directory.Exists(WebRootDir))
                Directory.CreateDirectory(WebRootDir);

            return WebRootDir;
        }

        //SMTP 인증메일용 Gmail 계정
        public static string getGmailAddr()
        {
            return GmailAddr;
        }
        public static string getGmailPassword()
        {
            return GmailPassword;
        }



    }
}


/*
Visual Studio 2019 기준.

아래 명령어를 Nuget 패키지관리자 콘솔에 입력하여 Nuget Package 설치.

Install-Package Microsoft.EntityFrameworkCore
Install-Package Microsoft.EntityFrameworkCore.Design
Install-Package Microsoft.EntityFrameworkCore.Tools
Install-Package Pomelo.EntityFrameworkCore.MySql
Install-Package Pomelo.EntityFrameworkCore.MySql.Design


MySQL, MariaDB같은 디비에서 테이블을 생성.
이후 아래 명령어를 Nuget 패키지관리자 콘솔에 입력하여
DBContext 클래스 파일과, RDBMS의 테이블과 매핑되는 C# 클래스 파일을 자동생성.
MySQL, MariaDB 테이블 -> C# EntityFrameworkCore DBContext 마이그레이션 툴 명령어.

Scaffold-DbContext "server=db.aruku.kro.kr;port=3333;user=siogames_admin;password=qwert12345!Q;database=siogames_main" Pomelo.EntityFrameworkCore.MySql -OutputDir DBContexts -Force


ex) Scaffold-DbContext " [디비접속 문자열] " Pomelo.EntityFrameworkCore.MySql -OutputDir [출력 디렉토리 경로] -Force

*/