using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi;
using NexusOS.DAL.Models;
using NexusOS.Util;

var builder = WebApplication.CreateBuilder(args);

#region DI & Database

// Cấu hình kết nối cơ sở dữ liệu SQL Server thông qua Entity Framework Core
builder.Services.AddDbContext<NexusOsContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString(AppConstants.Context)));

#endregion

#region Base

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
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "ZenOS Solution API Swagger", Version = "v1" });

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

#endregion

#region Packages

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
}

app.UseHttpsRedirection(); // 🔐 Chuyển hướng HTTPS

app.UseAuthorization(); // 🔐 Phân quyền

app.MapControllers();

app.Run();
