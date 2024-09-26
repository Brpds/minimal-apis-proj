namespace MinimalApi.Dominio.ModelViews;

public struct Home
{
    public string Mensagem {get => "Bem vindo à API de veículos - Minimal APIs";}

    //link completo foi necessário para poder acessar o swagger via JSON
    public string Doc {get => "http://localhost:5223/swagger";}
}