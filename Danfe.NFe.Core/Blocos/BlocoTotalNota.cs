using Danfe.NFe.Core.Elementos;
using Danfe.NFe.Core.Enumeracoes;
using Danfe.NFe.Core.Modelo;

namespace Danfe.NFe.Core.Blocos
{
    /// <summary>
    /// Bloco "Total dos Produtos e Total da Nota" (NT 2026.010).
    /// </summary>
    internal class BlocoTotalNota : BlocoBase
    {
        public BlocoTotalNota(DanfeViewModel viewModel, Estilo estilo) : base(viewModel, estilo)
        {
            var m = ViewModel.CalculoImposto;

            AdicionarLinhaCampos()
                .ComCampoNumerico("Valor Total dos Produtos", m.ValorTotalProdutos)
                .ComCampoNumerico("Valor do Frete", m.ValorFrete)
                .ComCampoNumerico("Valor do Seguro", m.ValorSeguro)
                .ComCampoNumerico("Desconto", m.Desconto)
                .ComCampoNumerico("Outras Despesas Acessórias", m.OutrasDespesas)
                .ComLargurasIguais();

            AdicionarLinhaCampos()
                .ComCampoNumerico("Valor Total da Nota", m.ValorTotalNota)
                .ComLargurasIguais();
        }

        public override PosicaoBloco Posicao => PosicaoBloco.Topo;
        public override string Cabecalho => "Total dos Produtos e Total da Nota";
    }
}
