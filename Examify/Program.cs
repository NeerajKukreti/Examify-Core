using Examify.Services;
using Examify.Common;
using Examify.Helpers;
using Examify.Handlers;
using Examify.Middleware;
using Serilog;
 

var builder = WebApplication.CreateBuilder(args);

LoggerConfigurator.ConfigureLogger(null, 12201, Path.Combine(AppContext.BaseDirectory, "Logs", "MVC-app-.log"));

builder.Host.UseSerilog();

// Add services to the container.
builder.Services.Configure<AppSettings>(
    builder.Configuration.GetSection("AppSettings"));
builder.Services.Configure<Examify.Common.ApiSettings>(
    builder.Configuration.GetSection("ApiSettings"));

builder.Services.AddHttpContextAccessor();
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = builder.Environment.IsDevelopment()
        ? CookieSecurePolicy.None
        : CookieSecurePolicy.Always;
    options.Cookie.SameSite = SameSiteMode.Strict;
    options.Cookie.IsEssential = true;
});

builder.Services.AddAuthentication("SessionScheme")
    .AddCookie("SessionScheme", options =>
    {
        options.LoginPath = "/Auth/Index";
        options.AccessDeniedPath = "/Auth/Index";
        options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
        options.SlidingExpiration = true;
    });

builder.Services.AddAuthorization();

// Register the token handler
builder.Services.AddTransient<AuthTokenHandler>();

// HttpClient for Exam Portal API with token handler
var apiBaseUrl = builder.Configuration["ExamifyAPI:BaseUrl"] ?? "https://localhost:7271/api/";
builder.Services.AddHttpClient("ExamifyAPI", client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
})
.AddHttpMessageHandler<AuthTokenHandler>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
    });
});

builder.Services.AddControllersWithViews().AddRazorRuntimeCompilation();
builder.Services.AddScoped<IFileService, FileService>();
builder.Services.AddScoped<IExamService, ExamService>();
builder.Services.AddScoped<IInstituteLoginService, InstituteLoginService>();
builder.Services.AddScoped<IStudentLoginService, StudentLoginService>();
//builder.Services.AddScoped<IInstituteService, InstituteService>(); 
builder.Services.AddScoped<ISubjectService, SubjectService>();  
builder.Services.AddScoped<IStateService, StateService>();
builder.Services.AddScoped<IAdminLoginService, AdminLoginService>();
builder.Services.AddScoped<Examify.Services.AuthService>();
builder.Services.AddScoped<IQuestionService, QuestionService>();
builder.Services.AddScoped<IStudentService, StudentService>();
builder.Services.AddScoped<IClassService, ClassService>();
builder.Services.AddScoped<IBatchService, BatchService>();

// Add caching services
builder.Services.AddMemoryCache();
builder.Services.AddSingleton<ICacheService, CacheService>();

var app = builder.Build();

app.UseSerilogRequestLogging();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}
else
{
    app.UseDeveloperExceptionPage();
}

app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseStaticFiles();
app.UseSession();
app.UseRouting();

app.UseAuthentication();
app.UseJwtCookieValidation(); // Check JWT cookie after authentication
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Dashboard}/{action=Index}/{id?}");

app.Run();
