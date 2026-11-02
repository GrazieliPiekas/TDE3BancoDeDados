using System;
using System.Collections.Generic;
using System.Text;

using SQLite;
using TDE3BancoDeDados.Model;

namespace TDE3BancoDeDados.ModelView
{
    public class CClienteService
    {
        // Controlar o acesso ao banco de dados SQLite
        private SQLiteAsyncConnection _db;

        // Construtor
        public CClienteService()
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

        // Método para inserir um cliente no banco de dados SQLite
        public Task<int> InsertCliente(CCliente c) => _db.InsertAsync(c);

        // Método para atualizar um cliente no banco de dados SQLite
        public Task<int> UpdateCliente(CCliente c) => _db.UpdateAsync(c);

        // Método para excluir um cliente do banco de dados SQLite
        public Task<int> DeleteCliente(CCliente c) => _db.DeleteAsync(c);

        // Método para obter todos os clientes do banco de dados SQLite
        public Task<List<CCliente>> GetClientes() => _db.Table<CCliente>().ToListAsync();

        // Método para consultar um cliente pelo código
        public Task<List<CCliente>> ConsultarCodigo(string query)
        {
            string sql = $"SELECT * FROM CCliente WHERE CodigoCliente = {query}";
            return _db.QueryAsync<CCliente>(sql);
        }
    }
}
