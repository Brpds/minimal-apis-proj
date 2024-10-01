using MinimalApi.Dominio.Entities;

namespace Test.Domain.Entidades;

[TestClass]
public class VeiculoTest
{
    [TestMethod]
    public void TestarGetSetPropriedades()
    {
        //Arrange
        var veiculo = new Veiculo
        {
            //Act
            Id = 1,
            Marca = "Mitsubishi",
            Nome = "Pajero Sport",
            Ano = 2025
        };

        //Assert
        Assert.AreEqual(1, veiculo.Id);
        Assert.AreEqual("Mitsubishi", veiculo.Marca);
        Assert.AreEqual("Pajero Sport", veiculo.Nome);
        Assert.AreEqual(2025, veiculo.Ano);
    }

}