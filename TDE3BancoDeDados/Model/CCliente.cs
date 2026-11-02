using System;
using System.Collections.Generic;
using System.Text;

using SQLite;

namespace TDE3BancoDeDados.Model
{
    public class CCliente
    {
        [PrimaryKey, AutoIncrement]
        public int CodigoCliente { get; set; }

        [MaxLength(50)]
        public string Nome { get; set; }

        public long RG { get; set; }

        [MaxLength(14)]
        public string Telefone { get; set; }
    }
}
