using TDE3BancoDeDados.Model;
using TDE3BancoDeDados.ModelView;
using System.Collections.ObjectModel;

namespace TDE3BancoDeDados.View;

public partial class ProdutoPage : ContentPage
{
    // Instância para tratamento da tabela CProduto
    CProdutoService _produtoService;

    // Usado para verificar se o produto está em alguma compra antes de excluir
    CItemCompraService _itemService;

    // Lista para preencher o componente CollectionView
    ObservableCollection<CProduto> _ListaProdutos =
        new ObservableCollection<CProduto>();


	public ProdutoPage()
	{
		InitializeComponent();

        // Inicializa os gerenciadores das tabelas
        _produtoService = new CProdutoService();
        _itemService = new CItemCompraService();

        // Lista de produtos para exibição
        lvListaProdutos.ItemsSource = _ListaProdutos;
	}

    protected async override void OnAppearing()
    {
        try
        {
            _ListaProdutos.Clear();
            List<CProduto> aux = await _produtoService.GetProdutos();
            aux.ForEach(i => _ListaProdutos.Add(i));
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("ERRO", ex.Message, "OK");
        }
    }

    // Validação dos dados do formulário antes de acessar o banco de dados
    private async Task<bool> ValidarCampos()
    {
        if (string.IsNullOrWhiteSpace(txNome.Text) ||
            string.IsNullOrWhiteSpace(txTipo.Text) ||
            string.IsNullOrWhiteSpace(txPreco.Text) ||
            string.IsNullOrWhiteSpace(txEstoque.Text))
        {
            await DisplayAlertAsync("Erro", "Preencha todos os campos.", "OK");
            return false;
        }

        if (txNome.Text.Length > 50 || txTipo.Text.Length > 50)
        {
            await DisplayAlertAsync("Erro", "Nome e tipo devem ter no máximo 50 caracteres.", "OK");
            return false;
        }

        if (!double.TryParse(txPreco.Text, out double preco) || preco <= 0)
        {
            await DisplayAlertAsync("Erro", "Informe um preço válido (maior que zero).", "OK");
            return false;
        }

        if (!int.TryParse(txEstoque.Text, out int estoque) || estoque < 0)
        {
            await DisplayAlertAsync("Erro", "Informe uma quantidade em estoque válida.", "OK");
            return false;
        }

        return true;
    }

    private async void btConsultar_Clicked(object sender, EventArgs e)
    {
        // Verifica se o código é válido
        if (!int.TryParse(txCodigo.Text, out int codigo))
        {
            await DisplayAlertAsync("Erro", "Informe um código válido!", "OK");
            return;
        }

        try
        {
            // Consulta o produto pelo código
            List<CProduto> aux = await _produtoService.ConsultarCodigo(codigo.ToString());

            if (aux.Count == 0)
            {
                await DisplayAlertAsync("Erro", "Produto não encontrado!", "OK");
                return;
            }

            // Atualiza os componentes do Form com os dados do produto encontrado
            txNome.Text = aux[0].NomeProduto;
            txTipo.Text = aux[0].Tipo;
            txPreco.Text = aux[0].Preco.ToString();
            txEstoque.Text = aux[0].QtdEstoque.ToString();
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("Erro", ex.Message, "OK");
        }
    }

    private void btLimpar_Clicked(object sender, EventArgs e)
    {
        txCodigo.Text = string.Empty;
        txNome.Text = string.Empty;
        txTipo.Text = string.Empty;
        txPreco.Text = string.Empty;
        txEstoque.Text = string.Empty;
        lvListaProdutos.SelectedItem = null;
        txNome.Focus();
    }

    private async void btIncluir_Clicked(object sender, EventArgs e)
    {
        // Validação dos dados
        if (!await ValidarCampos())
            return;

        // Cria um objeto do tipo CProduto e preenche com os dados do Form
        CProduto p = new CProduto
        {
            NomeProduto = txNome.Text,
            Tipo = txTipo.Text,
            Preco = Convert.ToDouble(txPreco.Text),
            QtdEstoque = Convert.ToInt32(txEstoque.Text)
        };

        try
        {
            // Inserção dos dados no banco de dados
            await _produtoService.InsertProduto(p);
            await DisplayAlertAsync("Sucesso", "Produto cadastrado!", "OK");
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("Erro", "Produto não cadastrado! " + ex.Message, "OK");
        }

        // Limpa o form
        btLimpar_Clicked(sender, e);

        // Refresh na CollectionView
        rvListaProdutos.IsRefreshing = true;
    }

    private async void btAlterar_Clicked(object sender, EventArgs e)
    {
        if (string.IsNullOrEmpty(txCodigo.Text))
        {
            await DisplayAlertAsync("Erro", "Selecione um produto para alterar", "OK");
            return;
        }

        // Validação dos dados
        if (!await ValidarCampos())
            return;

        CProduto p = new CProduto
        {
            CodigoProduto = Convert.ToInt32(txCodigo.Text),
            NomeProduto = txNome.Text,
            Tipo = txTipo.Text,
            Preco = Convert.ToDouble(txPreco.Text),
            QtdEstoque = Convert.ToInt32(txEstoque.Text)
        };

        try
        {
            await _produtoService.UpdateProduto(p);
            await DisplayAlertAsync("Sucesso", "Produto alterado com sucesso!", "OK");
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("Erro", "Não foi possível alterar o produto! " + ex.Message, "OK");
        }

        // Limpa o form
        btLimpar_Clicked(sender, e);

        rvListaProdutos.IsRefreshing = true;
    }

    private async void btExcluir_Clicked(object sender, EventArgs e)
    {
        if (string.IsNullOrEmpty(txCodigo.Text))
        {
            await DisplayAlertAsync("Erro", "Selecione um produto para excluir", "OK");
            return;
        }

        int codigo = Convert.ToInt32(txCodigo.Text);

        try
        {
            // Não deixa excluir um produto que está em alguma compra
            List<CItemCompra> itens = await _itemService.GetItensDoProduto(codigo);
            if (itens.Count > 0)
            {
                await DisplayAlertAsync("Erro", "Este produto está em uma compra e não pode ser excluído!", "OK");
                return;
            }

            CProduto p = new CProduto
            {
                CodigoProduto = codigo
            };

            await _produtoService.DeleteProduto(p);
            await DisplayAlertAsync("Sucesso", "Produto excluído com sucesso!", "OK");
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("Erro", "Não foi possível excluir o produto! " + ex.Message, "OK");
        }

        // Limpa o form
        btLimpar_Clicked(sender, e);

        rvListaProdutos.IsRefreshing = true;
    }

    private async void rvListaProdutos_Refreshing(object sender, EventArgs e)
    {
        try
        {
            _ListaProdutos.Clear();
            List<CProduto> aux = await _produtoService.GetProdutos();
            aux.ForEach(i => _ListaProdutos.Add(i));
            lvListaProdutos.ItemsSource = _ListaProdutos;
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("ERRO", ex.Message, "OK");
        }

        rvListaProdutos.IsRefreshing = false;
    }


    private void lvListaProdutos_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        var p = e.CurrentSelection.FirstOrDefault() as CProduto;

        if (p != null)
        {
            txCodigo.Text = p.CodigoProduto.ToString();
            txNome.Text = p.NomeProduto;
            txTipo.Text = p.Tipo;
            txPreco.Text = p.Preco.ToString();
            txEstoque.Text = p.QtdEstoque.ToString();
            txNome.Focus();

            ((CollectionView)sender).SelectedItem = null;
        }
    }
}
