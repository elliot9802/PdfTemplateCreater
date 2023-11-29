using Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddHttpClient();
builder.Services.AddLogging();
builder.Services.AddControllers();
builder.Services.AddTransient<IPdfTemplateService, PdfTemplateService>();
builder.Services.AddTransient<IFileService, FileService>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Enable CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("DefaultCorsPolicy", builder =>
    {
        builder.WithOrigins("http://127.0.0.1:5500")
               .SetIsOriginAllowedToAllowWildcardSubdomains()
               .WithMethods("POST")
               .WithHeaders("Content-Type");
    });
});
/* 
CORS allows specific external origins access to your API via browsers. 
*/

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

// Use CORS middleware
app.UseCors("DefaultCorsPolicy");

app.MapControllers();

app.Run();