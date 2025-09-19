using Examify.Services;
using OnlineExam.Common;
using OnlineExam.Helpers;
using OnlineExam.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.Configure<AppSettings>(
    builder.Configuration.GetSection("AppSettings"));
builder.Services.Configure<OnlineExam.Common.ApiSettings>(
    builder.Configuration.GetSection("ApiSettings"));

builder.Services.AddHttpContextAccessor();
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // session timeout
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// HttpClient for Exam Portal API
builder.Services.AddHttpClient("ExamifyAPI", client =>
{
    client.BaseAddress = new Uri("https://localhost:7271/api/");
});

builder.Services.AddControllersWithViews().AddRazorRuntimeCompilation();
builder.Services.AddScoped<IFileService, FileService>();
builder.Services.AddScoped<IExamService, ExamService>();
builder.Services.AddScoped<IInstituteLoginService, InstituteLoginService>();
builder.Services.AddScoped<IStudentLoginService, StudentLoginService>();
builder.Services.AddScoped<IInstituteService, InstituteService>();
builder.Services.AddScoped<IStudentService, StudentService>();
builder.Services.AddScoped<ISubjectService, SubjectService>();  
builder.Services.AddScoped<IStateService, StateService>();
builder.Services.AddScoped<IAdminLoginService, AdminLoginService>();
builder.Services.AddScoped<Examify.Services.AuthService>();
builder.Services.AddScoped<IQuestionService, QuestionService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseSession();
app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Dashboard}/{action=Index}/{id?}");

app.Run();
