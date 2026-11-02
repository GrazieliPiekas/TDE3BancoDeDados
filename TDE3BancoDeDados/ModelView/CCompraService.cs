using System;
using System.Collections.Generic;
using System.Text;

using SQLite;
using TDE3BancoDeDados.Model;

namespace TDE3BancoDeDados.ModelView
{
    public class CCompraService
    {
        // Controlar o acesso ao banco de dados SQLite
        private SQLiteAsyncConnection _db;

        // Construtor
        public CCompraService()
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

        // Método para inserir uma compra no banco de dados SQLite
        public Task<int> InsertCompra(CCompra c) => _db.InsertAsync(c);

        // Método para atualizar uma compra no banco de dados SQLite
        public Task<int> UpdateCompra(CCompra c) => _db.UpdateAsync(c);

        // Método para excluir uma compra do banco de dados SQLite
        public Task<int> DeleteCompra(CCompra c) => _db.DeleteAsync(c);

        // Método para obter todas as compras do banco de dados SQLite
        public Task<List<CCompra>> GetCompras() => _db.Table<CCompra>().ToListAsync();

        // Método para consultar uma compra pelo código
        public Task<List<CCompra>> ConsultarCodigo(string query)
        {
            string sql = $"SELECT * FROM CCompra WHERE CodigoCompra = {query}";
            return _db.QueryAsync<CCompra>(sql);
        }

        // Método para obter as compras de um cliente
        public Task<List<CCompra>> GetComprasDoCliente(int codigoCliente)
        {
            string sql = $"SELECT * FROM CCompra WHERE CodigoCliente = {codigoCliente}";
            return _db.QueryAsync<CCompra>(sql);
        }
    }
}
