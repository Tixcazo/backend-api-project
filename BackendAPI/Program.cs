

using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Backend.Models;
using Backend.Data;
using Backend.Config;
using Backend.Services;
using System.Reflection;
using System.Text.Json.Serialization;
using System.Text;

#nullable disable
var builder = WebApplication.CreateBuilder(args);

//เพิ่ม DbContext ของเราเข้าไปในบริการของแอปพลิเคชัน โดยใช้ SQL Server เป็นฐานข้อมูล
builder.Services.AddDbContext<DataContext>(options =>
{
    options.UseMySQL(builder.Configuration.GetConnectionString("DefaultConnection"));
});

//เพิ่ม Register AuthService เข้าไปในบริการของแอปพลิเคชัน เพื่อให้เราสามารถใช้บริการนี้ในคอนโทรลเลอร์ของเราได้
builder.Services.AddScoped<AuthService>();

//เพิ่ม Register ValidationService เข้าไปในบริการของแอปพลิเคชัน เพื่อให้เราสามารถใช้บริการนี้ในคอนโทรลเลอร์ของเราได้
builder.Services.AddScoped<ValidationService>();

//เพิ่ม AddControllers และ แก้ปัญหา circular reference ในการแปลงข้อมูลเป็น JSON
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    });

//Service.AddEndpointsApiExplorer() เป็นเมธอดที่ใช้ในการเพิ่มบริการสำหรับการสำรวจและสร้างเอกสาร API 
//โดยอัตโนมัติในแอปพลิเคชัน ASP.NET Core Web API ของเรา ซึ่งจะช่วยให้เราสามารถดูและทดสอบ API 
//ของเราได้ง่ายขึ้นผ่าน Swagger UI หรือเครื่องมืออื่นๆ ที่รองรับ OpenAPI Specification
builder.Services.AddEndpointsApiExplorer();

//เราใช้ AddSwaggerGen เพื่อเพิ่มบริการสำหรับการสร้างเอกสาร API โดยอัตโนมัติในแอปพลิเคชันของเรา
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Version = "v1",
        Title = "Backend API Swagger",
        Description = "A simple example ASP.NET Core Web API",
    });

    var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename));

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Please enter a valid token",
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        BearerFormat = "JWT",
        Scheme = "Bearer"
    });
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type=ReferenceType.SecurityScheme,
                    Id="Bearer"
                }
            },
            new string[]{}
        }
    });
});

//เราใช้ AddCore เพื่อเพิ่มบริการหลักของ ASP.NET Core ลงในแอปพลิเคชันของเรา 
//ซึ่งจะช่วยให้เราสามารถใช้ฟีเจอร์ต่างๆ ของ ASP.NET Core ได้อย่างเต็มที่ เช่น การจัดการเส้นทาง การจัดการข้อผิดพลาด และอื่นๆ
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", builder =>
    {
        builder.AllowAnyOrigin()
          .AllowAnyMethod()
          .AllowAnyHeader();
    });
});

var jwtSettings = new JwtSettings(); // เราสร้างอ็อบเจ็กต์ jwtSettings เพื่อเก็บค่าการตั้งค่า JWT ที่เราจะดึงมาจากไฟล์การตั้งค่า

//เราใช้ GetSection เพื่อดึงข้อมูลจากส่วนที่ชื่อว่า "JwtSettings" 
//ในไฟล์การตั้งค่า และใช้ Bind เพื่อผูกข้อมูลเหล่านั้นกับอ็อบเจ็กต์ jwtSettings ของเรา
builder.Configuration.GetSection("JwtSettings").Bind(jwtSettings);

//เราใช้ AddSingleton เพื่อเพิ่ม jwtSettings เข้าไปในบริการของแอปพลิเคชันของเรา
builder.Services.AddSingleton(jwtSettings);

//เราใช้ AddAuthentication เพื่อเพิ่มบริการสำหรับการตรวจสอบสิทธิ์ในแอปพลิเคชันของเรา 
//โดยกำหนดให้ใช้ JWT Bearer Authentication เป็นค่าเริ่มต้น
builder.Services.AddAuthentication(options =>
{
    //options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    //เป็นการกำหนดให้ใช้ JWT Bearer Authentication เป็นค่าเริ่มต้นสำหรับการตรวจสอบสิทธิ์ในแอปพลิเคชันของเรา
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;

    //options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    //เป็นการกำหนดให้ใช้ JWT Bearer Authentication เป็นค่าเริ่มต้นสำหรับการท้าทายผู้ใช้ที่ยังไม่ได้รับการตรวจสอบสิทธิ์
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true, // ตรวจสอบว่าโทเค็นมาจากแหล่งที่เชื่อถือได้หรือไม่
        ValidateAudience = true, // ตรวจสอบว่าโทเค็นถูกส่งไปยังผู้รับที่ถูกต้องหรือไม่
        ValidateLifetime = true, // ตรวจสอบว่าโทเค็นยังไม่หมดอายุหรือไม่
        ValidateIssuerSigningKey = true, // ตรวจสอบว่าโทเค็นถูกเซ็นด้วยคีย์ที่ถูกต้องหรือไม่
        ValidIssuer = jwtSettings.Issuer, // ตรวจสอบว่าโทเค็นมาจากแหล่งที่เชื่อถือได้หรือไม่
        ValidAudience = jwtSettings.Audience, // ตรวจสอบว่าโทเค็นถูกส่งไปยังผู้รับที่ถูกต้องหรือไม่
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8
            .GetBytes(jwtSettings.SecretKey)
        ), // ใช้คีย์ลับในการตรวจสอบความถูกต้องของโทเค็น
    };
});

//เราใช้ AddAuthorization เพื่อเพิ่มบริการสำหรับการอนุญาตในแอปพลิเคชันของเรา
builder.Services.AddAuthorization();

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Database Seeding - สร้างข้อมูลพื้นฐาน
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<DataContext>();

    // ตรวจสอบว่ายังไม่มี Roles อยู่ในฐานข้อมูล
    // ถ้าไม่มี Roles เราจะเพิ่ม Roles เริ่มต้นเข้าไปในฐานข้อมูล
    if (!context.Roles.Any())
    {
        var defaultRoles = new List<Role>
        {
            new Role { Id = 1, Name = "User" },      // ผู้ใช้ปกติ
            new Role { Id = 2, Name = "Guest" },     // ผู้ใช้แบบ Guest
            new Role { Id = 3, Name = "Admin" }      // ผู้ดูแลระบบ
        };

        context.Roles.AddRange(defaultRoles);
        await context.SaveChangesAsync();
    }

    // ตรวจสอบว่ายังไม่มี Admin อยู่ในฐานข้อมูล
    // ถ้าไม่มี Admin เราจะเพิ่ม Admin เริ่มต้นเข้าไปในฐานข้อมูล
    if (!context.Users.Any(u => u.RoleId == 3))
    {
        var passwordHasher = new PasswordHasher<User>();
        var tempUser = new User(); // สร้าง temp user สำหรับการแฮช password
        var hashedPassword = passwordHasher.HashPassword(tempUser, "1234");

        var adminUser = new User
        {
            Email = "admin@example.com",
            FirstName = "Admin",
            LastName = "Admin",
            Address = null,
            PhoneNumber = null,
            PasswordHash = hashedPassword, // ใช้ hashed password แทน plain text
            Birthdate = null,
            RoleId = 3, // กำหนด RoleId สำหรับ Admin เป็น 3
            isRegisteredUser = true // กำหนดให้เป็นผู้ใช้ที่ลงทะเบียนแล้ว
        };

        context.Users.Add(adminUser);
        await context.SaveChangesAsync();
    }
}

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowAll");  // Enable CORS
app.UseAuthentication();  // Enable Authentication
app.UseAuthorization();   // Enable Authorization
app.MapControllers();     // Map controller endpoints

app.Run();

