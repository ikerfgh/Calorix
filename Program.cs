using Calorix.Data;
using Calorix.Patterns.Facade;
using Calorix.Patterns.Singleton;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Falta ConnectionStrings:DefaultConnection en appsettings.json");
//Singleton
DatabaseConnection.Initialize(connectionString);

builder.Services.AddControllersWithViews().AddRazorRuntimeCompilation();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(60);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});
builder.Services.AddDbContext<CalorixDbContext>(options =>
    options.UseSqlite(DatabaseConnection.Instance.ConnectionString));
builder.Services.AddScoped<ICalorixFacade, CalorixFacade>();
builder.Services.AddScoped<Calorix.Services.IReportePdfService, Calorix.Services.ReportePdfService>();
builder.Services.AddHttpContextAccessor();
// API REST documentada con Swagger (/swagger)
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "Calorix REST API", Version = "v1",
        Description = "Endpoints REST de Calorix. Los recursos personales usan la cookie de sesión: inicia sesión en la app antes de probarlos." });
    var xml = System.IO.Path.Combine(AppContext.BaseDirectory, "Calorix.xml");
    if (System.IO.File.Exists(xml)) c.IncludeXmlComments(xml);
});
builder.Services.AddGraphQLServer()
    .AddQueryType<Calorix.GraphQL.Query>()
    .AddMutationType<Calorix.GraphQL.Mutation>();
QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community; // licencia gratuita (proyecto académico)

var app = builder.Build();

using (var db = DatabaseConnection.Instance.CreateContext())
{
    DbInitializer.Seed(db);
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
    app.UseHttpsRedirection();
}

app.UseStaticFiles();
app.UseRouting();
app.UseSession();
app.UseAuthorization();

app.UseSwagger();
app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Calorix REST v1"));

app.MapControllers();  // /api/... — API REST
app.MapGraphQL(); // /graphql — incluye el explorador Nitro en el navegador

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
