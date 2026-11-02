using System;
using System.Collections.Generic;
using System.Text;

using SQLite;
using SQLiteNetExtensions.Attributes;

namespace TDE3BancoDeDados.Model
{
    public class CItemCompra
    {
        [PrimaryKey, AutoIncrement]
        public int CodigoItem { get; set; }

        // Chave estrangeira: produto comprado
        [ForeignKey(typeof(CProduto))]
        public int CodigoProduto { get; set; }

        // Chave estrangeira: compra a qual o item pertence
        [ForeignKey(typeof(CCompra))]
        public int CodigoCompra { get; set; }

        public int Quantidade { get; set; }

        public double ValorParcial { get; set; }
    }
}
