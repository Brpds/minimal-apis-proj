using Microsoft.IdentityModel.Tokens;
using MinimalApi.Dominio.Entities;
using MinimalApi.Dominio.Interfaces;

namespace Test.Mocks;

public class VeiculoServicoMock : IVeiculoServico
{
    public static List<Veiculo> veiculos = new List<Veiculo>();
    public void Apagar(Veiculo veiculo)
    {
        veiculos.Remove(veiculo);
    }

    public void Atualizar(Veiculo veiculo)
    {
        if(!veiculos.IsNullOrEmpty()){
            veiculo.Marca = "Ford";
            veiculo.Nome = "Ranger";
            veiculo.Ano = 2021;
        }
    }

    public Veiculo BuscaPorId(int id)
    {
        return veiculos.Find(v => v.Id == id);
    }

    public void Incluir(Veiculo veiculo)
    {
        veiculo.Id = veiculos.Count() + 1;
        veiculos.Add(veiculo);

    }

    public List<Veiculo> Todos(int? pagina = 1, string nome = null, string marca = null)
    {
        return veiculos;
    }
}