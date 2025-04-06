using FluentValidation.AspNetCore;
using PP_BC_C_H_1.Controllers;
using PP_BC_C_H_1.Middleware;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddControllers(); // Add this line to register controllers
builder.Services.AddEndpointsApiExplorer(); // Add this line to register API explorer
builder.Services.AddSwaggerGen(); // Add this line to register Swagger

// Add FluentValidation
builder.Services.AddControllers().AddFluentValidation(x =>
{
    x.RegisterValidatorsFromAssemblyContaining<ProductValidator>();
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.UseSwagger(); // Add this line to enable Swagger middleware
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1"); // Add this line to configure Swagger UI
    c.RoutePrefix = string.Empty; // Set Swagger UI at the app's root
});

app.UseMiddleware<ErrorHandlerMiddleware>(); // Add this line to use the custom error handler middleware

app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers(); // Add this line to map controller routes
    endpoints.MapRazorPages();
});

app.Run();
