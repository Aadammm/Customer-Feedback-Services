using Customer_Feedback_Services.Configuration;
using Customer_Feedback_Services.Services;

var builder = WebApplication.CreateBuilder(args); 

builder.Configuration.SetBasePath(Directory.GetCurrentDirectory() + "/Configuration");
builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

// Add services to the container.
builder.Services.Configure<FeedbackDatabaseSettings>(
	builder.Configuration.GetSection("FeedbackDatabase"));

builder.Services.AddSingleton<FeedbackService>();
builder.Services.AddControllers();

var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

await app.RunAsync();

