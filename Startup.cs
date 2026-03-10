using Microsoft.AspNetCore.Mvc.Routing;
using ApiThiBangLaiXeOto.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
namespace ApiThiBangLaiXeOto;

public class Startup
{
    public Startup(IConfiguration configuration)
    {
        Configuration = configuration;
    }

    public IConfiguration Configuration { get; }

    // This method gets called by the runtime. Use this method to add services to the container
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddControllers();
        services.AddScoped<SqlHelper>();
        services.AddHttpLogging(options =>
        {
            options.LoggingFields = Microsoft.AspNetCore.HttpLogging.HttpLoggingFields.All;
        });
        // JWT + Cookie + Google Auth
        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = "BearerMain";
            options.DefaultChallengeScheme = "BearerMain";
            options.DefaultSignInScheme = "Cookies";
        })
        .AddJwtBearer("BearerMain", options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = Configuration["Jwt:Issuer"],
                ValidAudience = Configuration["Jwt:Audience"],
                IssuerSigningKey = new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(Configuration["Jwt:Key_Main"]!)
                ),
                ClockSkew = TimeSpan.Zero
            };
        });
    }

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILoggerFactory loggerFactory)
    {
        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }



        app.UseHttpsRedirection();

        app.UseRouting();

        app.UseAuthentication();
        app.UseAuthorization();

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
        });
    }
}