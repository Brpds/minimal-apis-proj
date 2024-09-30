using MinimalApi.Dominio.Entities;

namespace Test.Domain.Entidades;

[TestClass]
public class AdministradorTest
{
    [TestMethod]
    public void TestarGetSetPropriedades()
    {
        //arrange - cria o item
        var adm = new Administrador();

        //Act - set de propriedades
        adm.Id = 1;
        adm.Email = "teste@teste.com";
        adm.Senha = "senhateste";
        adm.Perfil = "Adm";

        //Assert - get de propriedades:
        Assert.AreEqual(1, adm.Id);
        Assert.AreEqual("teste@teste.com", adm.Email);
        Assert.AreEqual("senhateste", adm.Senha);
        Assert.AreEqual("Adm", adm.Perfil);

    }
}