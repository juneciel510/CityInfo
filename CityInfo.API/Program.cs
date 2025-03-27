using Microsoft.AspNetCore.StaticFiles;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

//when user asks for some format that the server dosent support, return 406 error
builder.Services.AddControllers(options =>
{
    options.ReturnHttpNotAcceptable=true;
}).AddXmlDataContractSerializerFormatters();

builder.Services.AddProblemDetails(options => 
{
    options.CustomizeProblemDetails = ctx =>
    {
        ctx.ProblemDetails.Extensions.Add("additionalInfo", "Additional info example");
        ctx.ProblemDetails.Extensions.Add("server", Environment.MachineName);
    };
});
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSingleton<FileExtensionContentTypeProvider>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseRouting();

app.UseAuthorization();

app.UseEndpoints(endpoints =>
{
    //attribute routing
    endpoints.MapControllers();
});

//a concise way to enable attribute routing for controllers
//app.MapControllers();


app.Run();
