namespace TDE3BancoDeDados.View;

public partial class MenuPage : ContentPage
{
	public MenuPage()
	{
		InitializeComponent();
	}

    // Abre a página de Cadastro de Clientes
    private async void btClientes_Clicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new ClientePage());
    }

    // Abre a página de Cadastro de Produtos
    private async void btProdutos_Clicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new ProdutoPage());
    }

    // Abre a página de Compra
    private async void btCompra_Clicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new CompraPage());
    }
}
