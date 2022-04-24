using System.IO;


namespace ASP.Net_Core_Http_RestAPI_Server
{
    public class WASConfig
    {
        //WebApplicationServer 설정
        static string wasLISTENHTTPPort = "5001"; //(TCP Port)

        static string[] wasCORSURLList =
        {
            "https://api.wizard87.com",
            "http://api.wizard87.com"
        };

        //WebRootDirectory 경로 설정
        static string webRootDirectory = string.Empty;

        //연동할 RDBMS관련 접속정보 설정
<<<<<<< HEAD:Server/ASP.Net Core Http RestAPI Server/ASP.Net Core Http RestAPI Server/Config/WAS_Config.cs
        static string DBMSAddress = "db.wizard87.com";
        static string DBMSPort = "3333";
        static string DBMSUser = "siogames_admin";
        static string DBMSPassword = "qwert12345!Q";
        static string targetDatabaseName = "siogames_main";
        static string DBMSMaxConnectionPoolSize = "2000";
=======
        static string dbMSAddress = "projecta.cmn63t9z8mgo.ap-northeast-2.rds.amazonaws.com";
        static string dbMSPort = "3306";
        static string dbMSUser = "admin";
        static string dbMSPassword = "dnwls3388";
        static string targetDatabaseName = "projecta";
        static string dbMSMaxConnectionPoolSize = "1000";
>>>>>>> 029fd61... 리팩토링 1차 재작업:Server/ASP.Net Core Http RestAPI Server/ASP.Net Core Http RestAPI Server/Config/WASConfig.cs


        //SMTP 인증메일용 Gmail 계정 정보
        static string gmailAddress = "siogames2020@gmail.com";
        static string gmailPassword = "siogames!";

        //리슨 URL
        public static string GetWASURLInfo()
        {
            var wasURL = $"http://*:{wasLISTENHTTPPort}";
            return wasURL;
        }

        //CORS_허용URL
        public static string[] GetWASCORSURLList()
        {
            return wasCORSURLList;
        }

        //RDBMS 연결정보
        public static string GetDBConnectionInfo()
        {
            var connectionString =
                $"server={dbMSAddress};port={dbMSPort};" +
                $"user={dbMSUser};password={dbMSPassword};" +
                $"database={targetDatabaseName};maxpoolsize={dbMSMaxConnectionPoolSize}";

            return connectionString;
        }

        //웹루트 디렉토리 경로
        public static string GetWebRootDirectory()
        {
            webRootDirectory = Path.Combine(Directory.GetCurrentDirectory(), "wwwRoot");

            if (!Directory.Exists(webRootDirectory))
                Directory.CreateDirectory(webRootDirectory);

            return webRootDirectory;
        }

        //SMTP 인증메일용 Gmail 계정
        public static string GetGmailAddress()
        {
            return gmailAddress;
        }

        public static string GetGmailPassword()
        {
            return gmailPassword;
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
Install-Package Microsoft.EntityFrameworkCore.Tools


MySQL, MariaDB같은 디비에서 테이블을 생성.
이후 아래 명령어를 Nuget 패키지관리자 콘솔에 입력하여
DBContext 클래스 파일과, RDBMS의 테이블과 매핑되는 C# 클래스 파일을 자동생성.
MySQL, MariaDB 테이블 -> C# EntityFrameworkCore DBContext 마이그레이션 툴 명령어.

<<<<<<< HEAD:Server/ASP.Net Core Http RestAPI Server/ASP.Net Core Http RestAPI Server/Config/WAS_Config.cs
Scaffold-DbContext "server=db.wizard87.com;port=3333;user=siogames_admin;password=qwert12345!Q;database=siogames_main" Pomelo.EntityFrameworkCore.MySql -OutputDir DBContexts -Force

=======
Scaffold-DbContext "server=db.aruku.kro.kr;port=3333;user=siogames_admin;password=qwert12345!Q;database=siogames_main" Pomelo.EntityFrameworkCore.MySql -OutputDir DBContexts -Force
Scaffold-DbContext "server=projecta.cmn63t9z8mgo.ap-northeast-2.rds.amazonaws.com;port=3306;user=admin;password=dnwls3388;database=projecta" Pomelo.EntityFrameworkCore.MySql -OutputDir DBContexts -Force
>>>>>>> 029fd61... 리팩토링 1차 재작업:Server/ASP.Net Core Http RestAPI Server/ASP.Net Core Http RestAPI Server/Config/WASConfig.cs

ex) Scaffold-DbContext " [디비접속 문자열] " Pomelo.EntityFrameworkCore.MySql -OutputDir [출력 디렉토리 경로] -Force

*/