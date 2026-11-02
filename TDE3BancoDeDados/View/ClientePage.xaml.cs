using TDE3BancoDeDados.Model;
using TDE3BancoDeDados.ModelView;
using System.Collections.ObjectModel;

namespace TDE3BancoDeDados.View;

public partial class ClientePage : ContentPage
{
    // Instância para tratamento da tabela CCliente
    CClienteService _clienteService;

    // Usado para verificar se o cliente possui compras antes de excluir
    CCompraService _compraService;

    // Lista para preencher o componente CollectionView
    ObservableCollection<CCliente> _ListaClientes =
        new ObservableCollection<CCliente>();


	public ClientePage()
	{
		InitializeComponent();

        // Inicializa os gerenciadores das tabelas
        _clienteService = new CClienteService();
        _compraService = new CCompraService();

        // Lista de clientes para exibição
        lvListaClientes.ItemsSource = _ListaClientes;
	}

    protected async override void OnAppearing()
    {
        try
        {
            _ListaClientes.Clear();
            List<CCliente> aux = await _clienteService.GetClientes();
            aux.ForEach(i => _ListaClientes.Add(i));
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
            string.IsNullOrWhiteSpace(txRG.Text) ||
            string.IsNullOrWhiteSpace(txTelefone.Text))
        {
            await DisplayAlertAsync("Erro", "Preencha todos os campos.", "OK");
            return false;
        }

        if (txNome.Text.Length > 50)
        {
            await DisplayAlertAsync("Erro", "O nome deve ter no máximo 50 caracteres.", "OK");
            return false;
        }

        if (!long.TryParse(txRG.Text, out long rg) || rg <= 0 || txRG.Text.Length > 10)
        {
            await DisplayAlertAsync("Erro", "O RG deve ter somente números (até 10 dígitos).", "OK");
            return false;
        }

        if (txTelefone.Text.Length > 14)
        {
            await DisplayAlertAsync("Erro", "O telefone deve ter no máximo 14 caracteres.", "OK");
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
            // Consulta o cliente pelo código
            List<CCliente> aux = await _clienteService.ConsultarCodigo(codigo.ToString());

            if (aux.Count == 0)
            {
                await DisplayAlertAsync("Erro", "Cliente não encontrado!", "OK");
                return;
            }

            // Atualiza os componentes do Form com os dados do cliente encontrado
            txNome.Text = aux[0].Nome;
            txRG.Text = aux[0].RG.ToString();
            txTelefone.Text = aux[0].Telefone;
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
        txRG.Text = string.Empty;
        txTelefone.Text = string.Empty;
        lvListaClientes.SelectedItem = null;
        txNome.Focus();
    }

    private async void btIncluir_Clicked(object sender, EventArgs e)
    {
        // Validação dos dados
        if (!await ValidarCampos())
            return;

        // Cria um objeto do tipo CCliente e preenche com os dados do Form
        CCliente c = new CCliente
        {
            Nome = txNome.Text,
            RG = Convert.ToInt64(txRG.Text),
            Telefone = txTelefone.Text
        };

        try
        {
            // Inserção dos dados no banco de dados
            await _clienteService.InsertCliente(c);
            await DisplayAlertAsync("Sucesso", "Cliente cadastrado!", "OK");
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("Erro", "Cliente não cadastrado! " + ex.Message, "OK");
        }

        // Limpa o form
        btLimpar_Clicked(sender, e);

        // Refresh na CollectionView
        rvListaClientes.IsRefreshing = true;
    }

    private async void btAlterar_Clicked(object sender, EventArgs e)
    {
        if (string.IsNullOrEmpty(txCodigo.Text))
        {
            await DisplayAlertAsync("Erro", "Selecione um cliente para alterar", "OK");
            return;
        }

        // Validação dos dados
        if (!await ValidarCampos())
            return;

        CCliente c = new CCliente
        {
            CodigoCliente = Convert.ToInt32(txCodigo.Text),
            Nome = txNome.Text,
            RG = Convert.ToInt64(txRG.Text),
            Telefone = txTelefone.Text
        };

        try
        {
            await _clienteService.UpdateCliente(c);
            await DisplayAlertAsync("Sucesso", "Cliente alterado com sucesso!", "OK");
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("Erro", "Não foi possível alterar o cliente! " + ex.Message, "OK");
        }

        // Limpa o form
        btLimpar_Clicked(sender, e);

        rvListaClientes.IsRefreshing = true;
    }

    private async void btExcluir_Clicked(object sender, EventArgs e)
    {
        if (string.IsNullOrEmpty(txCodigo.Text))
        {
            await DisplayAlertAsync("Erro", "Selecione um cliente para excluir", "OK");
            return;
        }

        int codigo = Convert.ToInt32(txCodigo.Text);

        try
        {
            // Não deixa excluir um cliente que possui compras
            List<CCompra> compras = await _compraService.GetComprasDoCliente(codigo);
            if (compras.Count > 0)
            {
                await DisplayAlertAsync("Erro", "Este cliente possui compras e não pode ser excluído!", "OK");
                return;
            }

            CCliente c = new CCliente
            {
                CodigoCliente = codigo
            };

            await _clienteService.DeleteCliente(c);
            await DisplayAlertAsync("Sucesso", "Cliente excluído com sucesso!", "OK");
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("Erro", "Não foi possível excluir o cliente! " + ex.Message, "OK");
        }

        // Limpa o form
        btLimpar_Clicked(sender, e);

        rvListaClientes.IsRefreshing = true;
    }

    private async void rvListaClientes_Refreshing(object sender, EventArgs e)
    {
        try
        {
            _ListaClientes.Clear();
            List<CCliente> aux = await _clienteService.GetClientes();
            aux.ForEach(i => _ListaClientes.Add(i));
            lvListaClientes.ItemsSource = _ListaClientes;
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("ERRO", ex.Message, "OK");
        }

        rvListaClientes.IsRefreshing = false;
    }


    private void lvListaClientes_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        var c = e.CurrentSelection.FirstOrDefault() as CCliente;

        if (c != null)
        {
            txCodigo.Text = c.CodigoCliente.ToString();
            txNome.Text = c.Nome;
            txRG.Text = c.RG.ToString();
            txTelefone.Text = c.Telefone;
            txNome.Focus();

            ((CollectionView)sender).SelectedItem = null;
        }
    }
}
