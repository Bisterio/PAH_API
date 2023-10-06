using API.ErrorHandling;
using DataAccess;
using DataAccess.Implement;
using DataAccess.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Service;
using Service.Implement;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddHttpClient("GHN", httpClient => {
    httpClient.BaseAddress = new Uri(builder.Configuration["API3rdParty:GHN:dev"]);
});
builder.Services.AddDbContext<PlatformAntiquesHandicraftsContext>(options => options.UseSqlServer("name=ConnectionStrings:dev"));
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddRouting(options => options.LowercaseUrls = true);
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

builder.Services.AddScoped<ValidateModelAttribute>();
builder.Services.Configure<ApiBehaviorOptions>(options => {
    options.SuppressModelStateInvalidFilter = true;
});

builder.Services.AddScoped<IUserDAO, UserDAO>();
builder.Services.AddScoped<ITokenDAO, TokenDAO>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IImageService, ImageService>();
builder.Services.AddScoped<ICategoryDAO, CategoryDAO>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IMaterialDAO, MaterialDAO>();
builder.Services.AddScoped<IMaterialService, MaterialService>();
builder.Services.AddScoped<IProductDAO, ProductDAO>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IAuctionDAO, AuctionDAO>();
builder.Services.AddScoped<IAuctionService, AuctionService>();
builder.Services.AddScoped<IProductImageDAO, ProductImageDAO>();
builder.Services.AddScoped<IOrderCancelDAO, OrderCancelDAO>();
builder.Services.AddScoped<IAddressDAO, AddressDAO>();
builder.Services.AddScoped<IAddressService, AddressService>();
builder.Services.AddScoped<IBuyerDAO, BuyerDAO>();
builder.Services.AddScoped<IOrderDAO, OrderDAO>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<IAuctionDAO, AuctionDAO>();
builder.Services.AddScoped<IAuctionService, AuctionService>();
builder.Services.AddScoped<IBidDAO, BidDAO>();
builder.Services.AddScoped<IBidService, BidService>();
builder.Services.AddScoped<ISellerDAO, SellerDAO>();
builder.Services.AddScoped<ISellerService, SellerService>();
builder.Services.AddScoped<IFeedbackDAO, FeedbackDAO>();
builder.Services.AddScoped<IFeedbackService, FeedbackService>();

builder.Services.AddAuthentication(x => {
    x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options => {
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters() {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidAudience = builder.Configuration["Jwt:Audience"],
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidateLifetime = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
    };
    options.Events = new JwtBearerEvents {
        OnAuthenticationFailed = context => {
            if (context.Exception.GetType() == typeof(SecurityTokenExpiredException)) {
                context.Response.Headers.Add("IS-TOKEN-EXPIRED", "true");
            }
            return Task.CompletedTask;
        }
    };
});

if (builder.Environment.IsDevelopment())
    builder.Services.AddHostedService<API.Tunnel.TunnelService>();

var app = builder.Build();
var logger = app.Services.GetRequiredService<ILogger<Program>>();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment()) {
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.ConfigureExceptionHandler(logger);

app.UseHttpsRedirection();


app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
