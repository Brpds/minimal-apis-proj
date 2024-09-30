using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using MinimalApi.Dominio.Entities;
using MinimalApi.Dominio.Services;
using MinimalApi.Infraestrutura.Db;

namespace Test.Domain.Servicos;

[TestClass]
public class AdministradorServicoTest
{
    private DbContexto CriarContextoTeste()
    {
        //Configurar o ConfigBuilder
        var assemblyPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        var path = Path.GetFullPath(Path.Combine(assemblyPath ?? "", "..", "..", ".."));

        var builder = new ConfigurationBuilder()
            .SetBasePath(path ?? Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddEnvironmentVariables();

        var configuration = builder.Build();

        return new DbContexto(configuration);
    }
    [TestMethod]
    public void TestandoSalvarAdministrador()
    {
        //arrange - cria o item
        var context = CriarContextoTeste();
        context.Database.ExecuteSqlRaw("TRUNCATE TABLE ADMINISTRADORES");

        var adm = new Administrador();
        adm.Id = 1;
        adm.Email = "teste@teste.com";
        adm.Senha = "senhateste";
        adm.Perfil = "Adm";

        var administradorServico = new AdministradorServico(context);

        //Act - set de propriedades
        administradorServico.Incluir(adm);

        //Assert - get de propriedades:
        Assert.AreEqual(1, administradorServico.Todos(1).Count());
        // Assert.AreEqual("teste@teste.com", adm.Email);
        // Assert.AreEqual("senhateste", adm.Senha);
        // Assert.AreEqual("Adm", adm.Perfil);
    }


    [TestMethod]
    public void TestandoBuscarAdministradorPorId()
    {
        //arrange - cria o item
        var context = CriarContextoTeste();
        context.Database.ExecuteSqlRaw("TRUNCATE TABLE ADMINISTRADORES");

        var adm = new Administrador();
        adm.Id = 1;
        adm.Email = "teste@teste.com";
        adm.Senha = "senhateste";
        adm.Perfil = "Adm";

        var administradorServico = new AdministradorServico(context);

        //Act - set de propriedades
        administradorServico.Incluir(adm);
        var admin = administradorServico.BuscaPorId(adm.Id);

        //Assert - get de propriedades:
        Assert.AreEqual(1, admin.Id);
    }
}