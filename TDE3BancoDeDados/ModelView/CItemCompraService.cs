using System;
using System.Collections.Generic;
using System.Text;

using SQLite;
using TDE3BancoDeDados.Model;

namespace TDE3BancoDeDados.ModelView
{
    public class CItemCompraService
    {
        // Controlar o acesso ao banco de dados SQLite
        private SQLiteAsyncConnection _db;

        // Construtor
        public CItemCompraService()
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

        // Método para inserir um item no banco de dados SQLite
        public Task<int> InsertItem(CItemCompra i) => _db.InsertAsync(i);

        // Método para atualizar um item no banco de dados SQLite
        public Task<int> UpdateItem(CItemCompra i) => _db.UpdateAsync(i);

        // Método para excluir um item do banco de dados SQLite
        public Task<int> DeleteItem(CItemCompra i) => _db.DeleteAsync(i);

        // Método para obter os itens de uma compra
        public Task<List<CItemCompra>> GetItensDaCompra(int codigoCompra)
        {
            string sql = $"SELECT * FROM CItemCompra WHERE CodigoCompra = {codigoCompra}";
            return _db.QueryAsync<CItemCompra>(sql);
        }

        // Método para obter os itens que usam um produto
        public Task<List<CItemCompra>> GetItensDoProduto(int codigoProduto)
        {
            string sql = $"SELECT * FROM CItemCompra WHERE CodigoProduto = {codigoProduto}";
            return _db.QueryAsync<CItemCompra>(sql);
        }
    }
}
