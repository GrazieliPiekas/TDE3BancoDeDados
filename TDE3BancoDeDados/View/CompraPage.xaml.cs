using TDE3BancoDeDados.Model;
using TDE3BancoDeDados.ModelView;
using System.Collections.ObjectModel;

namespace TDE3BancoDeDados.View;

public partial class CompraPage : ContentPage
{
    // Instâncias para tratamento das tabelas
    CCompraService _compraService;
    CItemCompraService _itemService;
    CClienteService _clienteService;
    CProdutoService _produtoService;

    // Listas para preencher os componentes CollectionView
    ObservableCollection<CCompra> _ListaCompras =
        new ObservableCollection<CCompra>();
    ObservableCollection<CItemCompra> _ListaItens =
        new ObservableCollection<CItemCompra>();

    // Listas para preencher os Pickers de cliente e produto
    List<CCliente> _ListaClientes = new List<CCliente>();
    List<CProduto> _ListaProdutos = new List<CProduto>();


	public CompraPage()
	{
		InitializeComponent();

        // Inicializa os gerenciadores das tabelas
        _compraService = new CCompraService();
        _itemService = new CItemCompraService();
        _clienteService = new CClienteService();
        _produtoService = new CProdutoService();

        // Listas para exibição
        lvListaCompras.ItemsSource = _ListaCompras;
        lvListaItens.ItemsSource = _ListaItens;
	}

    protected async override void OnAppearing()
    {
        try
        {
            // Carrega os clientes e produtos nos Pickers
            _ListaClientes = await _clienteService.GetClientes();
            pkCliente.ItemsSource = _ListaClientes;

            _ListaProdutos = await _produtoService.GetProdutos();
            pkProduto.ItemsSource = _ListaProdutos;

            await CarregarCompras();
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("ERRO", ex.Message, "OK");
        }
    }

    // Atualiza a lista de compras
    private async Task CarregarCompras()
    {
        _ListaCompras.Clear();
        List<CCompra> aux = await _compraService.GetCompras();
        aux.ForEach(i => _ListaCompras.Add(i));
    }

    // Atualiza a lista de itens da compra informada
    private async Task CarregarItens(int codigoCompra)
    {
        _ListaItens.Clear();
        List<CItemCompra> aux = await _itemService.GetItensDaCompra(codigoCompra);
        aux.ForEach(i => _ListaItens.Add(i));
    }

    // Recalcula o valor total da compra (soma dos valores parciais dos itens)
    private async Task AtualizarValorTotal(int codigoCompra)
    {
        List<CItemCompra> itens = await _itemService.GetItensDaCompra(codigoCompra);
        double total = itens.Sum(i => i.ValorParcial);

        List<CCompra> aux = await _compraService.ConsultarCodigo(codigoCompra.ToString());
        if (aux.Count > 0)
        {
            aux[0].ValorTotal = total;
            await _compraService.UpdateCompra(aux[0]);
        }

        lbValorTotal.Text = "Valor Total: " + total.ToString("F2");
    }

    // Preenche o formulário com os dados de uma compra
    private async Task MostrarCompra(CCompra c)
    {
        txCodigoCompra.Text = c.CodigoCompra.ToString();
        pkCliente.SelectedItem = _ListaClientes.FirstOrDefault(x => x.CodigoCliente == c.CodigoCliente);
        dpData.Date = c.DataCompra;
        lbValorTotal.Text = "Valor Total: " + c.ValorTotal.ToString("F2");

        await CarregarItens(c.CodigoCompra);
    }

    // ----- Compra -----

    // Validação dos dados da compra antes de acessar o banco de dados
    private async Task<bool> ValidarCompra()
    {
        if (pkCliente.SelectedItem == null)
        {
            await DisplayAlertAsync("Erro", "Selecione o cliente.", "OK");
            return false;
        }

        if ((DateTime)dpData.Date > DateTime.Today)
        {
            await DisplayAlertAsync("Erro", "A data da compra não pode ser no futuro.", "OK");
            return false;
        }

        return true;
    }

    private async void btConsultar_Clicked(object sender, EventArgs e)
    {
        // Verifica se o código é válido
        if (!int.TryParse(txCodigoCompra.Text, out int codigo))
        {
            await DisplayAlertAsync("Erro", "Informe um código válido!", "OK");
            return;
        }

        try
        {
            List<CCompra> aux = await _compraService.ConsultarCodigo(codigo.ToString());

            if (aux.Count == 0)
            {
                await DisplayAlertAsync("Erro", "Compra não encontrada!", "OK");
                return;
            }

            await MostrarCompra(aux[0]);
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("Erro", ex.Message, "OK");
        }
    }

    private void btLimpar_Clicked(object sender, EventArgs e)
    {
        txCodigoCompra.Text = string.Empty;
        pkCliente.SelectedItem = null;
        dpData.Date = DateTime.Today;
        lbValorTotal.Text = "Valor Total: 0,00";
        _ListaItens.Clear();
        btLimparItem_Clicked(sender, e);
    }

    private async void btIncluir_Clicked(object sender, EventArgs e)
    {
        // Validação dos dados
        if (!await ValidarCompra())
            return;

        // Cria a compra (o valor total é calculado pelos itens)
        CCompra c = new CCompra
        {
            CodigoCliente = ((CCliente)pkCliente.SelectedItem).CodigoCliente,
            DataCompra = (DateTime)dpData.Date,
            ValorTotal = 0
        };

        try
        {
            await _compraService.InsertCompra(c);
            await DisplayAlertAsync("Sucesso", "Compra cadastrada! Agora adicione os itens.", "OK");

            // Mantém a compra no formulário para poder adicionar os itens
            await MostrarCompra(c);
            await CarregarCompras();
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("Erro", "Compra não cadastrada! " + ex.Message, "OK");
        }
    }

    private async void btAlterar_Clicked(object sender, EventArgs e)
    {
        if (string.IsNullOrEmpty(txCodigoCompra.Text))
        {
            await DisplayAlertAsync("Erro", "Selecione uma compra para alterar", "OK");
            return;
        }

        // Validação dos dados
        if (!await ValidarCompra())
            return;

        int codigo = Convert.ToInt32(txCodigoCompra.Text);

        try
        {
            // Valor total sempre vem da soma dos itens
            List<CItemCompra> itens = await _itemService.GetItensDaCompra(codigo);

            CCompra c = new CCompra
            {
                CodigoCompra = codigo,
                CodigoCliente = ((CCliente)pkCliente.SelectedItem).CodigoCliente,
                DataCompra = (DateTime)dpData.Date,
                ValorTotal = itens.Sum(i => i.ValorParcial)
            };

            await _compraService.UpdateCompra(c);
            await DisplayAlertAsync("Sucesso", "Compra alterada com sucesso!", "OK");
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("Erro", "Não foi possível alterar a compra! " + ex.Message, "OK");
        }

        btLimpar_Clicked(sender, e);
        await CarregarCompras();
    }

    private async void btExcluir_Clicked(object sender, EventArgs e)
    {
        if (string.IsNullOrEmpty(txCodigoCompra.Text))
        {
            await DisplayAlertAsync("Erro", "Selecione uma compra para excluir", "OK");
            return;
        }

        int codigo = Convert.ToInt32(txCodigoCompra.Text);

        try
        {
            // Exclui primeiro os itens da compra e depois a compra
            List<CItemCompra> itens = await _itemService.GetItensDaCompra(codigo);
            foreach (CItemCompra i in itens)
                await _itemService.DeleteItem(i);

            CCompra c = new CCompra
            {
                CodigoCompra = codigo
            };

            await _compraService.DeleteCompra(c);
            await DisplayAlertAsync("Sucesso", "Compra excluída com sucesso!", "OK");
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("Erro", "Não foi possível excluir a compra! " + ex.Message, "OK");
        }

        btLimpar_Clicked(sender, e);
        await CarregarCompras();
    }

    private async void lvListaCompras_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        var c = e.CurrentSelection.FirstOrDefault() as CCompra;

        if (c != null)
        {
            btLimparItem_Clicked(sender, e);
            await MostrarCompra(c);

            ((CollectionView)sender).SelectedItem = null;
        }
    }

    // ----- Itens da Compra -----

    // Validação dos dados do item antes de acessar o banco de dados
    private async Task<bool> ValidarItem()
    {
        // O item precisa pertencer a uma compra já cadastrada
        if (!int.TryParse(txCodigoCompra.Text, out int codigoCompra) ||
            (await _compraService.ConsultarCodigo(codigoCompra.ToString())).Count == 0)
        {
            await DisplayAlertAsync("Erro", "Inclua ou selecione uma compra antes de adicionar itens.", "OK");
            return false;
        }

        if (pkProduto.SelectedItem == null)
        {
            await DisplayAlertAsync("Erro", "Selecione o produto.", "OK");
            return false;
        }

        if (!int.TryParse(txQuantidade.Text, out int qtd) || qtd <= 0)
        {
            await DisplayAlertAsync("Erro", "Informe uma quantidade válida (maior que zero).", "OK");
            return false;
        }

        return true;
    }

    private void btLimparItem_Clicked(object sender, EventArgs e)
    {
        txCodigoItem.Text = string.Empty;
        pkProduto.SelectedItem = null;
        txQuantidade.Text = string.Empty;
    }

    private async void btIncluirItem_Clicked(object sender, EventArgs e)
    {
        // Validação dos dados
        if (!await ValidarItem())
            return;

        int codigoCompra = Convert.ToInt32(txCodigoCompra.Text);
        CProduto p = (CProduto)pkProduto.SelectedItem;
        int qtd = Convert.ToInt32(txQuantidade.Text);

        // Valor parcial = preço do produto x quantidade
        CItemCompra i = new CItemCompra
        {
            CodigoCompra = codigoCompra,
            CodigoProduto = p.CodigoProduto,
            Quantidade = qtd,
            ValorParcial = p.Preco * qtd
        };

        try
        {
            await _itemService.InsertItem(i);
            await AtualizarValorTotal(codigoCompra);
            await DisplayAlertAsync("Sucesso", "Item adicionado!", "OK");
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("Erro", "Item não adicionado! " + ex.Message, "OK");
        }

        btLimparItem_Clicked(sender, e);
        await CarregarItens(codigoCompra);
        await CarregarCompras();
    }

    private async void btAlterarItem_Clicked(object sender, EventArgs e)
    {
        if (string.IsNullOrEmpty(txCodigoItem.Text))
        {
            await DisplayAlertAsync("Erro", "Selecione um item para alterar", "OK");
            return;
        }

        // Validação dos dados
        if (!await ValidarItem())
            return;

        int codigoCompra = Convert.ToInt32(txCodigoCompra.Text);
        CProduto p = (CProduto)pkProduto.SelectedItem;
        int qtd = Convert.ToInt32(txQuantidade.Text);

        CItemCompra i = new CItemCompra
        {
            CodigoItem = Convert.ToInt32(txCodigoItem.Text),
            CodigoCompra = codigoCompra,
            CodigoProduto = p.CodigoProduto,
            Quantidade = qtd,
            ValorParcial = p.Preco * qtd
        };

        try
        {
            await _itemService.UpdateItem(i);
            await AtualizarValorTotal(codigoCompra);
            await DisplayAlertAsync("Sucesso", "Item alterado com sucesso!", "OK");
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("Erro", "Não foi possível alterar o item! " + ex.Message, "OK");
        }

        btLimparItem_Clicked(sender, e);
        await CarregarItens(codigoCompra);
        await CarregarCompras();
    }

    private async void btExcluirItem_Clicked(object sender, EventArgs e)
    {
        if (string.IsNullOrEmpty(txCodigoItem.Text))
        {
            await DisplayAlertAsync("Erro", "Selecione um item para excluir", "OK");
            return;
        }

        int codigoCompra = Convert.ToInt32(txCodigoCompra.Text);

        CItemCompra i = new CItemCompra
        {
            CodigoItem = Convert.ToInt32(txCodigoItem.Text)
        };

        try
        {
            await _itemService.DeleteItem(i);
            await AtualizarValorTotal(codigoCompra);
            await DisplayAlertAsync("Sucesso", "Item excluído com sucesso!", "OK");
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("Erro", "Não foi possível excluir o item! " + ex.Message, "OK");
        }

        btLimparItem_Clicked(sender, e);
        await CarregarItens(codigoCompra);
        await CarregarCompras();
    }

    private void lvListaItens_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        var i = e.CurrentSelection.FirstOrDefault() as CItemCompra;

        if (i != null)
        {
            txCodigoItem.Text = i.CodigoItem.ToString();
            pkProduto.SelectedItem = _ListaProdutos.FirstOrDefault(x => x.CodigoProduto == i.CodigoProduto);
            txQuantidade.Text = i.Quantidade.ToString();

            ((CollectionView)sender).SelectedItem = null;
        }
    }
}
