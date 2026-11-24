using Danfe.NFe.Core.Elementos;
using Danfe.NFe.Core.Enumeracoes;
using Danfe.NFe.Core.Modelo;

namespace Danfe.NFe.Core.Blocos
{
    /// <summary>
    /// Bloco "Total do ICMS / IPI" (NT 2026.010).
    /// </summary>
    internal class BlocoTotalIcmsIpi : BlocoBase
    {
        public BlocoTotalIcmsIpi(DanfeViewModel viewModel, Estilo estilo) : base(viewModel, estilo)
        {
            var m = ViewModel.CalculoImposto;

            AdicionarLinhaCampos()
                .ComCampoNumerico("Base de Cálculo do ICMS", m.BaseCalculoIcms)
                .ComCampoNumerico("Valor do ICMS", m.ValorIcms)
                .ComCampoNumerico("Base de Cálculo do ICMS ST", m.BaseCalculoIcmsSt)
                .ComCampoNumerico("Valor do ICMS ST", m.ValorIcmsSt)
                .ComCampoNumerico("Valor do IPI", m.ValorIpi)
                .ComLargurasIguais();

            // Campos de exibição condicionada (NT 2026.010, item 4.4).
            var vIcmsUfDest = ViewModel.ExibirIcmsInterestadual ? m.vICMSUFDest : null;
            var vFcpUfDest = ViewModel.ExibirIcmsInterestadual ? m.vFCPUFDest : null;

            if (AlgumValorInformado(m.ValorFcp, m.ValorFcpSt, vIcmsUfDest, vFcpUfDest))
            {
                AdicionarLinhaCampos()
                    .ComCampoNumerico("Valor do FCP", m.ValorFcp)
                    .ComCampoNumerico("Valor do FCP retido por ST", m.ValorFcpSt)
                    .ComCampoNumerico("Valor do DIFAL na UF de destino", vIcmsUfDest)
                    .ComCampoNumerico("Valor do FCP na UF de destino", vFcpUfDest)
                    .ComLargurasIguais();
            }

            if (AlgumValorInformado(m.BaseCalculoIcmsMono, m.ValorIcmsMono, m.BaseCalculoIcmsMonoReten, m.ValorIcmsMonoReten))
            {
                AdicionarLinhaCampos()
                    .ComCampoNumerico("BC do ICMS monofásico", m.BaseCalculoIcmsMono, 4)
                    .ComCampoNumerico("Valor do ICMS monofásico", m.ValorIcmsMono)
                    .ComCampoNumerico("BC do ICMS monofásico por retenção", m.BaseCalculoIcmsMonoReten, 4)
                    .ComCampoNumerico("Valor do ICMS monofásico por retenção", m.ValorIcmsMonoReten)
                    .ComLargurasIguais();
            }
        }

        public override PosicaoBloco Posicao => PosicaoBloco.Topo;
        public override string Cabecalho => "Total do ICMS / IPI";
    }
}
