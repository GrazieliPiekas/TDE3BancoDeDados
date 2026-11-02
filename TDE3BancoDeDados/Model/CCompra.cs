using System;
using System.Collections.Generic;
using System.Text;

using SQLite;
using SQLiteNetExtensions.Attributes;

namespace TDE3BancoDeDados.Model
{
    public class CCompra
    {
        [PrimaryKey, AutoIncrement]
        public int CodigoCompra { get; set; }

        public DateTime DataCompra { get; set; }

        public double ValorTotal { get; set; }

        // Chave estrangeira: cliente que fez a compra
        [ForeignKey(typeof(CCliente))]
        public int CodigoCliente { get; set; }
    }
}
