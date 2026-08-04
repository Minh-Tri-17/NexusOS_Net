using AutoMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using NexusOS.BLL.Services;
using NexusOS.DAL.Models;
using NexusOS.Util;
using System.Globalization;

var builder = WebApplication.CreateBuilder(args);

#region DI & Database

builder.Services.AddScoped<DevUserSeeder>();

builder.Services.Scan(scan => scan
    .FromAssemblyOf<CatCountryService>() // Định vị Assembly (file .dll) chứa lớp AccountService (tức là toàn bộ project ZenOS.BLL)
    .AddClasses(classes => classes.Where(type => type.Name.EndsWith("Service")))
    .AsImplementedInterfaces() // Tự động bắt cặp Class đó với Interface tương ứng mà nó kế thừa
    .WithScopedLifetime()); // Đăng ký với vòng đời là Scoped

// Cấu hình kết nối cơ sở dữ liệu SQL Server thông qua Entity Framework Core
builder.Services.AddDbContext<NexusOsContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString(AppConstants.Context)));

#endregion

#region Base

// Cấu hình dịch vụ gửi Email
builder.Services.AddSingleton<MailHelpers>(new MailHelpers(builder.Configuration));

// Cung cấp khả năng truy cập thông tin HTTP Context
builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

// Cấu hình chính sách CORS: Cho phép các ứng dụng Frontend từ danh sách origins cụ thể
builder.Services.AddCors(p => p.AddPolicy("FrontendCorsPolicy", build =>
{
    build.WithOrigins("http://localhost:4200").AllowAnyMethod().AllowAnyHeader();
}));

// Cấu hình giới hạn kích thước dữ liệu gửi lên (Body Size) cho Multipart (thường là Upload file)
builder.Services.Configure<FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = 209715200; // 200 MB
});

// Cấu hình giới hạn kích thước nhận dữ liệu ở tầng Web Server (Kestrel)
builder.WebHost.ConfigureKestrel(options =>
{
    options.Limits.MaxRequestBodySize = 209715200; // 200 MB
});

builder.Services.AddEndpointsApiExplorer();

// Thiết lập hệ thống nạp cấu hình đa tầng cho ứng dụng
builder.Configuration
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables();

#endregion

#region JWT Auth + Swagger

// Cấu hình bộ tạo tài liệu Swagger (Swagger Generator)
builder.Services.AddSwaggerGen(options =>
{
    // 1. Định nghĩa thông tin cơ bản của tài liệu API
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "NexusOS Solution API Swagger", Version = "v1" });

    // 2. Cấu hình định nghĩa cơ chế bảo mật (Security Scheme)
    var scheme = new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Nhập mã JWT Access Token để xác thực hệ thống.\n\n" +
                      "**Lưu ý:**\n" +
                      "- Chỉ nhập chuỗi Token (Ví dụ: `eyJhbGci...`)\n" +
                      "- **KHÔNG** gõ thêm chữ 'Bearer' ở phía trước (hệ thống tự thêm).\n\n"
    };

    // Đăng ký định nghĩa trên vào Swagger với ID là "Bearer"
    options.AddSecurityDefinition("Bearer", scheme);

    // 3. Thiết lập yêu cầu bảo mật (Security Requirement) cho toàn bộ API
    // Trong Microsoft.OpenApi 3.x, AddSecurityRequirement yêu cầu một Lambda (doc => ...)
    options.AddSecurityRequirement(doc =>
    {
        // Tạo một tham chiếu (Reference) tới định nghĩa "Bearer" đã tạo ở bước 2
        // 'doc' giúp tham chiếu này liên kết đúng với cấu trúc của toàn bộ tài liệu OpenAPI
        var schemeReference = new OpenApiSecuritySchemeReference("Bearer", doc);

        return new OpenApiSecurityRequirement
        {
            {
                schemeReference, // Khóa là đối tượng tham chiếu (Reference)
                new List<string>()
            }
        };
    });

    // 4. Loại bỏ các Navigation Property (toàn hệ thống)
    options.SchemaFilter<HideNavigationPropertiesSchemaFilter>();
});

string issuer = builder.Configuration.GetValue<string>("Tokens:Issuer");
string signingKey = builder.Configuration.GetValue<string>("Tokens:Key");
byte[] signingKeyBytes = System.Text.Encoding.UTF8.GetBytes(signingKey);

// Cấu hình hệ thống xác thực (Authentication)
builder.Services.AddAuthentication(opt =>
{
    opt.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    opt.DefaultSignInScheme = JwtBearerDefaults.AuthenticationScheme;
    opt.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
// Cấu hình chi tiết các quy tắc kiểm tra Token
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters()
    {
        ValidateIssuer = true,
        ValidIssuer = issuer,
        ValidateAudience = true,
        ValidAudience = issuer,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ClockSkew = System.TimeSpan.Zero,
        IssuerSigningKey = new SymmetricSecurityKey(signingKeyBytes)
    };
});

#endregion

#region Packages

#region AutoMapper

// Cấu hình AutoMapper bằng cách đăng ký trực tiếp lớp MappingProfiles vào hệ thống Dependency Injection.
builder.Services.AddAutoMapper(cfg => cfg.AddProfile<MappingProfiles>());

#endregion

#region Localizer

builder.Services.AddJsonLocalization(options =>
{
    options.ResourcesPath = new[] { "Resources" };
});

#endregion

#endregion

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();

    app.UseSwaggerUI();

    app.MapOpenApi();

    // Tạo user mặc định cho dev
    using (var scope = app.Services.CreateScope())
    {
        var seeder = scope.ServiceProvider.GetRequiredService<DevUserSeeder>();
        seeder.Seed();
    }
}

var cultures = new[]
{
    new CultureInfo("vi"),
    new CultureInfo("en")
};

app.UseRequestLocalization(new RequestLocalizationOptions
{
    DefaultRequestCulture = new RequestCulture("vi"),
    SupportedCultures = cultures,
    SupportedUICultures = cultures
});

var mapper = app.Services.GetRequiredService<IMapper>();
DataHelpers.ConfigureMapper(mapper);

app.UseHttpsRedirection(); // 🔐 Chuyển hướng HTTPS

app.UseRouting(); // 🧭 Routing

app.UseCors("FrontendCorsPolicy"); // 🌐 CORS cho phép truy cập từ frontend

app.UseAuthentication(); // 🔑 Xác thực

app.UseAuthorization(); // 🔐 Phân quyền

app.MapControllers();

app.Run();
