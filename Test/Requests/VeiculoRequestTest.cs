using System.Text;
using System.Text.Json;
using MinimalApi.DTOs;
using Test.Helpers;

namespace Test.Requests;

[TestClass]
public class VeiculoRequestTest
{
    [ClassInitialize]
    public static void ClassInit(TestContext testContext)
    {
        Setup.ClassInit(testContext);
    }

    [ClassCleanup]
    public static void ClassCleanup()
    {
        Setup.ClassCleanup();
    }

    [TestMethod]
    public async Task TestarGetSetPropriedades()
    {
        //Arrange
        var veiculoDTO = new VeiculoDTO{
            Nome = "Teneré 700",
            Marca = "Yamaha",
            Ano = 2024
        };

        var content = new StringContent(JsonSerializer.Serialize(veiculoDTO), Encoding.UTF8, "Application/json");

        //Act
        var response = await Setup.client.PostAsync("veiculos", content);

        //Assert
        /*
            O teste passa quando há a tentativa de post para a rota
            sem login, significando que a rota só aceitará requests
            mediante login
        */
        Assert.AreEqual(401, (int)response.StatusCode);

        /*  
            para verificar a falha, comentar a linha acima
            e descomentar a linha abaixo
        */
        //Assert.AreEqual(200, (int)response.StatusCode);
    }
}