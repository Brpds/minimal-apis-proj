using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using MinimalApi.Dominio.Entities;
using MinimalApi.Dominio.Services;
using MinimalApi.Infraestrutura.Db;

namespace Test.Domain.Servicos;

[TestClass]
public class VeiculoServicoTest
{
    private DbContexto CriarContextoTeste()
    {
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
    public void TestandoSalvarVeiculo()
    {
        //Arrange 
        var context = CriarContextoTeste();
        context.Database.ExecuteSqlRaw("TRUNCATE TABLE Veiculos");

        var veiculo = new Veiculo{
            Id = 1,
            Marca = "Yamaha",
            Nome = "Teneré 700",
            Ano = 2025
        };

        var veiculoServico = new VeiculoServico(context);

        //Act

        veiculoServico.Incluir(veiculo);

        //Assert

        Assert.AreEqual(1, veiculoServico.Todos(1).Count());
    }

    [TestMethod]
    public void TestandoBuscarVeiculoPorId()
    {
        //Arrange 
        var context = CriarContextoTeste();
        context.Database.ExecuteSqlRaw("TRUNCATE TABLE Veiculos");

        var veiculo = new Veiculo{
            Id = 1,
            Marca = "Yamaha",
            Nome = "Teneré 700",
            Ano = 2025
        };

        var veiculoServico = new VeiculoServico(context);

        //Act

        veiculoServico.Incluir(veiculo);
        var buscaVeiculo = veiculoServico.BuscaPorId(veiculo.Id);

        //Assert

        Assert.AreEqual(1, buscaVeiculo.Id);
    }
}