using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
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

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers()
                .AddMvcOptions(option =>
            {
                //replace Json Formatter. To Utf8Json
                option.InputFormatters.Clear();
                option.InputFormatters.Add(new Utf8JsonInputFormatter());
                option.OutputFormatters.Clear();
                option.OutputFormatters.Add(new Utf8JsonOutputFormatter());
            });

            //CORS(Cross-Origin Resource Sharing) 설정
            services.AddCors(options =>
            {
                options.AddDefaultPolicy(
                    builder =>
                    {
                        // 허용할 CORS 도메인 입력
                        builder.WithOrigins(WAS_Config.getWAS_CORS_URL_List());
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

            env.WebRootPath = env.ContentRootPath = WAS_Config.getWebRootDir();

            //WebRootPath의 정적인 파일들을 그대로 Url을 통하여 배포할건지 여부.
            app.UseStaticFiles();

            //본 앱의 통신범위는 내부망으로만 한정되어 있으므로 https연결은 비활성화.
            //app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
