using Danfe.NFe.Core.Elementos;
using Danfe.NFe.Core.Enumeracoes;
using Danfe.NFe.Core.Modelo;

namespace Danfe.NFe.Core.Blocos
{
    /// <summary>
    /// Bloco "Total do IBS / CBS / IS" (NT 2026.010, item 4.1).
    /// </summary>
    internal class BlocoTotalIbsCbsIs : BlocoBase
    {
        public BlocoTotalIbsCbsIs(DanfeViewModel viewModel, Estilo estilo) : base(viewModel, estilo)
        {
            var m = ViewModel.CalculoImposto;

            AdicionarLinhaCampos()
                .ComCampoNumerico("Valor da CBS", m.ValorCbs)
                .ComCampoNumerico("Valor do IBS UF", m.ValorIbsUf)
                .ComCampoNumerico("Valor do IBS Município", m.ValorIbsMun)
                .ComCampoNumerico("Valor do Imposto Seletivo", m.ValorImpostoSeletivo)
                .ComLargurasIguais();

            // Monofasia: exibir somente quando informado o grupo gMono (W57).
            if (AlgumValorInformado(m.ValorIbsMono, m.ValorCbsMono, m.ValorIbsMonoReten, m.ValorCbsMonoReten))
            {
                AdicionarLinhaCampos()
                    .ComCampoNumerico("Valor do IBS monofásico", m.ValorIbsMono)
                    .ComCampoNumerico("Valor da CBS monofásica", m.ValorCbsMono)
                    .ComCampoNumerico("Valor do IBS monofásico por retenção", m.ValorIbsMonoReten)
                    .ComCampoNumerico("Valor da CBS monofásica por retenção", m.ValorCbsMonoReten)
                    .ComLargurasIguais();
            }
        }

        public override PosicaoBloco Posicao => PosicaoBloco.Topo;
        public override string Cabecalho => "Total do IBS / CBS / IS";
    }
}
