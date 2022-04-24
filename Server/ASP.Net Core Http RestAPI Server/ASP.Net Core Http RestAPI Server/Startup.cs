using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
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
            services.AddControllers();

            //CORS(Cross-Origin Resource Sharing) ����
            services.AddCors(options =>
            {
                options.AddDefaultPolicy(
                    builder =>
                    {
                        // ����� CORS ������ �Է�
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

            //WebRootPath�� ������ ���ϵ��� �״�� Url�� ���Ͽ� �����Ұ��� ����.
            app.UseStaticFiles();

            //�� ���� ��Ź����� ���θ����θ� �����Ǿ� �����Ƿ� https������ ��Ȱ��ȭ.
            //app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
        }
    }
}