using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using MinimalApi.Dominio.Entities;
using MinimalApi.Dominio.Enums;
using MinimalApi.Dominio.Interfaces;
using MinimalApi.Dominio.ModelViews;
using MinimalApi.Dominio.Services;
using MinimalApi.DTOs;
using MinimalApi.Infraestrutura.Db;

namespace MinimalApi;

public class Startup 
{
    public Startup(IConfiguration configuration){

            Configuration = configuration;
            key = Configuration.GetSection("Jwt").ToString() ?? "";
    }

    private string key = "";

    public IConfiguration Configuration {get; set;} = default!;

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddControllers();
        //para JWT
        services.AddAuthentication(option => {
        option.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        option.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        }).AddJwtBearer(option => {
        option.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateLifetime = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
            ValidateAudience = false,
            ValidateIssuer = false
        };
        });


        // para JWT 
        services.AddAuthorization();

        services.AddScoped<IAdministradorServico, AdministradorServico>();
        services.AddScoped<IVeiculoServico, VeiculoServico>();

        //configura o swagger
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(options => {
        //bearer que recebe o token de definição de segurança
        options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme{
            Name = "Authorization",
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT",
            In = ParameterLocation.Header,
            Description = "Insira o token JWT aqui: "
        });

        //adiciona os requisitos de segurança
        options.AddSecurityRequirement(new OpenApiSecurityRequirement{
            {
                new OpenApiSecurityScheme{
                    Reference = new OpenApiReference{
                        Type = ReferenceType.SecurityScheme,
                        Id = "Bearer"
                    }
                },
                new string[] {}
            }
        });
        });

        services.AddDbContext<DbContexto>(options => {
        options.UseMySql(
            Configuration.GetConnectionString("MySql"),
                ServerVersion.AutoDetect(Configuration.GetConnectionString("mysql"))
                
        );
        });

        services.AddCors(options =>
        {
            options.AddPolicy("AllowAll", builder =>
            {
                builder.AllowAnyOrigin()
                       .AllowAnyMethod()
                       .AllowAnyHeader();
            });
        });

}

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        app.UseSwagger();
        app.UseSwaggerUI();

        app.UseCors("AllowAll");

        app.UseRouting();

        //para JWT - Authentication DEVE vir sempre antes de Authorization
        app.UseAuthentication();
        app.UseAuthorization();

        app.UseEndpoints(endpoints => {
            endpoints.MapControllers();
            #region Home
            //allow anonymous permite acesso irrestrito à rota
            endpoints.MapGet("/", () => Results.Json(new Home())).AllowAnonymous().WithTags("Home");

            #endregion

            #region Administradores

            string GerarTokenJwt(Administrador administrador){
                if(string.IsNullOrEmpty(key)) return string.Empty;
                //encoding
                var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
                //credenciais criptografia
                var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

                var claims = new List<Claim>(){
                    // claims de definição de email e perfil
                    new Claim("Email", administrador.Email),
                    new Claim("Perfil", administrador.Perfil),
                    //limita o acesso às rotas de acordo com o tipo de role/perfil do usuário
                    new Claim(ClaimTypes.Role, administrador.Perfil)
                };

                //definição do token com credenciais, expiração e claims
                var token = new JwtSecurityToken(
                    claims : claims,
                    expires : DateTime.Now.AddDays(1),
                    signingCredentials : credentials
                );

                return new JwtSecurityTokenHandler().WriteToken(token);
            }

            endpoints.MapPost("/administradores/login", ([FromBody] LoginDTO loginDTO, IAdministradorServico administradorServico) => {
                var adm = administradorServico.Login(loginDTO);
                if(adm != null){
                    string token = GerarTokenJwt(adm);
                    return Results.Ok(new AdministradorLogado{
                        Email = adm.Email,
                        Perfil = adm.Perfil,
                        Token = token
                    });
                }else{
                return Results.Unauthorized();
                }
            }).AllowAnonymous().WithTags("Administradores");

            endpoints.MapGet("/administradores", ([FromQuery] int? pagina, IAdministradorServico administradorServico) => 
            {
                var adms = new List<AdministradorModelView>();
                var administradores = administradorServico.Todos(pagina);
                foreach(var adm in administradores)
                {
                    adms.Add(new AdministradorModelView{
                        Id = adm.Id,
                        Email = adm.Email,
                        Perfil = adm.Perfil
                    });
                }
                return Results.Ok(adms);
                // .RequireAuthorization() é o que protege a rota de acessos não autorizados
            }).RequireAuthorization()
            //requireauthorization com authorize attribute restringe o acesso de acordo com o parâmetro definido
            .RequireAuthorization(new AuthorizeAttribute {Roles = "Adm"})
            .WithTags("Administradores");

            endpoints.MapGet("/administradores/{id}", ([FromRoute] int id, IAdministradorServico administradorServico) => {
                var administrador = administradorServico.BuscaPorId(id);

                if(administrador == null) return Results.NotFound();

                return Results.Ok(new AdministradorModelView{
                        Id = administrador.Id,
                        Email = administrador.Email,
                        Perfil = administrador.Perfil
                    });
            }).RequireAuthorization()
            .RequireAuthorization(new AuthorizeAttribute {Roles = "Adm"})
            .WithTags("Administradores");

            endpoints.MapPost("/administradores", ([FromBody] AdministradorDTO administradorDTO, IAdministradorServico administradorServico) => 
            {
                var validacao = new ErrosDeValidacao{
                    Mensagens = new List<string>()
                };

                if(string.IsNullOrEmpty(administradorDTO.Email)){
                    validacao.Mensagens.Add("Email não pode ser vazio");
                }

                if(string.IsNullOrEmpty(administradorDTO.Senha)){
                    validacao.Mensagens.Add("Senha não pode ser vazia");
                }

                if(administradorDTO.Perfil == null){
                    validacao.Mensagens.Add("Perfil não pode ser vazio");
                }

                if(validacao.Mensagens.Count > 0){
                    return Results.BadRequest(validacao);
                }

                var administrador = new Administrador{
                    Email = administradorDTO.Email,
                    Senha = administradorDTO.Senha,
                    Perfil = administradorDTO.Perfil.ToString() ?? Perfil.Editor.ToString()
                };

                administradorServico.Incluir(administrador);

                return Results.Created($"/administrador/{administrador.Id}",
                new AdministradorModelView{
                        Id = administrador.Id,
                        Email = administrador.Email,
                        Perfil = administrador.Perfil
                    });    

            }).RequireAuthorization()
            .RequireAuthorization(new AuthorizeAttribute {Roles = "Adm"})
            .WithTags("Administradores");

            #endregion

            #region Veiculos

            ErrosDeValidacao validaDTO(VeiculoDTO veiculoDTO)
            {
                var validacao = new ErrosDeValidacao{
                    Mensagens = new List<string>()
                };

                if(string.IsNullOrEmpty(veiculoDTO.Nome)){
                    validacao.Mensagens.Add("O nome não pode ser vazio.");
                }

                if(string.IsNullOrEmpty(veiculoDTO.Marca)){
                    validacao.Mensagens.Add("A marca não pode ficar em branco.");
                }

                if(veiculoDTO.Ano < 1950){
                    validacao.Mensagens.Add("Veículo muito antigo. Somente veículos superiores a 1950.");
                }

                return validacao;
            }

            //cria um veículo
            endpoints.MapPost("/veiculos", ([FromBody] VeiculoDTO veiculoDTO, IVeiculoServico veiculoServico) => 
            {
                var validacao = validaDTO(veiculoDTO);

                if(validacao.Mensagens.Count > 0){
                    return Results.BadRequest(validacao);
                }

                var veiculo = new Veiculo{
                    Nome = veiculoDTO.Nome,
                    Marca = veiculoDTO.Marca,
                    Ano = veiculoDTO.Ano
                };

                veiculoServico.Incluir(veiculo);

                return Results.Created($"/veiculo/{veiculo}", veiculo);
            }).RequireAuthorization()
            //múltiplos authorize attributes indicam múltiplas autorizações de acesso às rotas
            .RequireAuthorization(new AuthorizeAttribute {Roles = "Adm,Editor"})
            .WithTags("Veiculos");

            //retorna todos
            endpoints.MapGet("/veiculos", ([FromQuery] int? pagina, IVeiculoServico veiculoServico) => {
                var veiculos = veiculoServico.Todos(pagina);

                return Results.Ok(veiculos);
            }).RequireAuthorization().WithTags("Veiculos");

            //retorna por ID
            endpoints.MapGet("/veiculos/{id}", ([FromRoute] int id, IVeiculoServico veiculoServico) => {
                var veiculo = veiculoServico.BuscaPorId(id);

                if(veiculo == null) return Results.NotFound();

                return Results.Ok(veiculo);
            }).RequireAuthorization()
            .RequireAuthorization(new AuthorizeAttribute {Roles = "Adm,Editor"})
            .WithTags("Veiculos");

            endpoints.MapPut("/veiculos/{id}", ([FromRoute] int id, VeiculoDTO veiculoDTO, IVeiculoServico veiculoServico) => 
            {
                var veiculo = veiculoServico.BuscaPorId(id);
                if(veiculo == null) return Results.NotFound();

                var validacao = validaDTO(veiculoDTO);

                if(validacao.Mensagens.Count > 0){
                    return Results.BadRequest(validacao);
                }

                veiculo.Nome = veiculoDTO.Nome;
                veiculo.Marca = veiculoDTO.Marca;
                veiculo.Ano = veiculoDTO.Ano;

                veiculoServico.Atualizar(veiculo);

                return Results.Ok(veiculo);
            }).RequireAuthorization()
            .RequireAuthorization(new AuthorizeAttribute {Roles = "Adm"})
            .WithTags("Veiculos");

            endpoints.MapDelete("/veiculos/{id}", ([FromRoute] int id, IVeiculoServico veiculoServico) => {
                var veiculo = veiculoServico.BuscaPorId(id);

                if(veiculo == null) return Results.NotFound();

                veiculoServico.Apagar(veiculo);

                return Results.NoContent();
            }).RequireAuthorization()
            .RequireAuthorization(new AuthorizeAttribute {Roles = "Adm"})
            .WithTags("Veiculos");
            #endregion

        });
    }
}