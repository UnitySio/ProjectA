using ASP.Net_Core_Http_RestAPI_Server.DBContexts;
using ASP.Net_Core_Http_RestAPI_Server.DBContexts.Mappers;
using ASP.Net_Core_Http_RestAPI_Server.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace ASP.Net_Core_Http_RestAPI_Server
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        
        public void ConfigureServices(IServiceCollection services)
        {
            //ControllerBase 를 상속받는 모든 Controller객체 주입.
            services.AddControllers();
            
            //로직 처리용 Service객체 싱글톤으로 주입.
            services.AddSingleton<AuthService>();
            services.AddSingleton<CharacterService>();
            services.AddSingleton<UserService>();
            services.AddSingleton<TransactionService>();
            
            //DB 접근용 Mapper객체 싱글톤으로 주입. 
            services.AddSingleton<AccountInfoMapper>();
            services.AddSingleton<UserCharacterInfoMapper>();
            services.AddSingleton<UserInfoMapper>();
            services.AddSingleton<UserSigninLogMapper>();
            
            
            //DB 접근용 dbcontext의 pooling factory객체  주입.
            services.AddPooledDbContextFactory<projectaContext>((provider, options) =>
            {
                if (!options.IsConfigured)
                {
                    //db 접속 설정
                    options.UseMySql(
                        WASConfig.GetDBConnectionInfo(),
                        ServerVersion.AutoDetect(WASConfig.GetDBConnectionInfo()),
                        builder =>
                        {
                            builder.EnableRetryOnFailure(5);
                        }).EnableServiceProviderCaching(false);
                }
            }, 5000);

            //CORS(Cross-Origin Resource Sharing) 설정
            services.AddCors(options =>
            {
                options.AddDefaultPolicy(
                    builder =>
                    {
                        // 허용할 CORS 도메인 입력
                        builder.WithOrigins(WASConfig.GetWASCORSURLList());
                    });
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            env.WebRootPath = env.ContentRootPath = WASConfig.GetWebRootDirectory();

            //WebRootPath의 정적인 파일들을 그대로 Url을 통하여 배포할건지 여부.
            app.UseStaticFiles();

            //본 앱의 통신범위는 내부망으로만 한정되어 있으므로 https연결은 비활성화.
            //app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
        }
    }
}