using System;
using System.Collections.Generic;
using System.Text;

using SQLite;

namespace TDE3BancoDeDados.Model
{
    public class CProduto
    {
        [PrimaryKey, AutoIncrement]
        public int CodigoProduto { get; set; }

        [MaxLength(50)]
        public string NomeProduto { get; set; }

        [MaxLength(50)]
        public string Tipo { get; set; }

        public double Preco { get; set; }

        public int QtdEstoque { get; set; }
    }
}
