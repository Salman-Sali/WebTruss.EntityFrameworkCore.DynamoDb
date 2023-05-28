using ExampleWebApi;
using ExampleWebApi.Configurations;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var awsConfig = new AWSConfiguration();
builder.Configuration.Bind("AWS", awsConfig);
builder.Services.AddSingleton(awsConfig);

var dynamoDbTables = new DynamoDbTablesConfiguration();
builder.Configuration.Bind("DynamoDbTables", dynamoDbTables);
builder.Services.AddSingleton(dynamoDbTables);

builder.Services.AddSingleton<ApplicationDbContext>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
