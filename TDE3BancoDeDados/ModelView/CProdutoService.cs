using System;
using System.Collections.Generic;
using System.Text;

using SQLite;
using TDE3BancoDeDados.Model;

namespace TDE3BancoDeDados.ModelView
{
    public class CProdutoService
    {
        // Controlar o acesso ao banco de dados SQLite
        private SQLiteAsyncConnection _db;

        // Construtor
        public CProdutoService()
        {
            Init();
        }

        public async Task Init()
        {
            // Se a conexão com o banco de dados já foi inicializada,
            // não faz nada
            if (_db != null)
                return;

            // Definir o nome do arquivo do banco de dados SQLite
            const string DatabaseFilename = "loja.db3";

            // Define Flags para o SQLite
            const SQLiteOpenFlags Flags =
                // Abre o banco de dados para leitura e escrita
                SQLite.SQLiteOpenFlags.ReadWrite |
                // Cria o banco de dados se ele não existir
                SQLite.SQLiteOpenFlags.Create |
                // Permite acesso compartilhado ao banco de dados
                SQLite.SQLiteOpenFlags.SharedCache;

            // Caminho do arquivo do banco de dados
            string DatabasePath = Path.Combine(FileSystem.AppDataDirectory,
                 DatabaseFilename);

            // Cria a conexão com o banco de dados SQLite
            _db = new SQLiteAsyncConnection(DatabasePath, Flags);

            // Cria as 4 tabelas do banco de dados (se ainda não existirem)
            await _db.CreateTableAsync<CCliente>();
            await _db.CreateTableAsync<CProduto>();
            await _db.CreateTableAsync<CCompra>();
            await _db.CreateTableAsync<CItemCompra>();
        }

        // Método para inserir um produto no banco de dados SQLite
        public Task<int> InsertProduto(CProduto p) => _db.InsertAsync(p);

        // Método para atualizar um produto no banco de dados SQLite
        public Task<int> UpdateProduto(CProduto p) => _db.UpdateAsync(p);

        // Método para excluir um produto do banco de dados SQLite
        public Task<int> DeleteProduto(CProduto p) => _db.DeleteAsync(p);

        // Método para obter todos os produtos do banco de dados SQLite
        public Task<List<CProduto>> GetProdutos() => _db.Table<CProduto>().ToListAsync();

        // Método para consultar um produto pelo código
        public Task<List<CProduto>> ConsultarCodigo(string query)
        {
            string sql = $"SELECT * FROM CProduto WHERE CodigoProduto = {query}";
            return _db.QueryAsync<CProduto>(sql);
        }
    }
}
