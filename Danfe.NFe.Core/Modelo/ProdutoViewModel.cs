namespace Danfe.NFe.Core.Modelo
{
    public class ProdutoViewModel
    {
        /// <summary>
        /// <para>Código do produto ou serviço</para>
        /// <para>Tag cProd</para>
        /// </summary>
        public string Codigo { get; set; }

        /// <summary>
        /// <para>Informações Adicionais do Produto</para>
        /// <para>Tag infAdProd</para>
        /// </summary>
        public string InformacoesAdicionais { get; set; }

        /// <summary>
        /// <para>Descrição do produto ou serviço</para>
        /// <para>Tag xProd</para>
        /// </summary>
        public string Descricao { get; set; }

        /// <summary>
        /// <para>Código NCM com 8 dígitos ou 2 dígitos (gênero)</para>
        /// <para>Tag NCM</para>
        /// </summary>
        public string Ncm { get; set; }


        /// <summary>
        /// <para>Origem da mercadoria + Tributação do ICMS</para>
        /// <para>Tag orig e CST</para>
        /// </summary>
        public string OCst { get; set; }

        /// <summary>
        /// <para>Código Fiscal de Operações e Prestações</para>
        /// <para>Tag CFOP</para>
        /// </summary>
        public int Cfop { get; set; }

        /// <summary>
        /// <para>Unidade Comercial</para>
        /// <para>Tag uCom</para>
        /// </summary>
        public string Unidade { get; set; }

        /// <summary>
        /// <para>Quantidade Comercial</para>
        /// <para>Tag qCom</para>
        /// </summary>
        public double Quantidade { get; set; }

        /// <summary>
        /// <para>Valor Unitário de Comercialização</para>
        /// <para>Tag vUnCom</para>
        /// </summary>
        public double ValorUnitario { get; set; }

        /// <summary>
        /// <para>Valor Total Bruto dos Produtos ou Serviços</para>
        /// <para>Tag vProd</para>
        /// </summary>
        public double ValorTotal { get; set; }

        /// <summary>
        /// <para>Valor da BC do ICMS</para>
        /// <para>Tag vBC</para>
        /// </summary>
        public double BaseIcms { get; set; }

        /// <summary>
        /// <para>Valor do ICMS</para>
        /// <para>Tag vICMS</para>
        /// </summary>
        public double ValorIcms { get; set; }

        /// <summary>
        /// <para>Alíquota do imposto</para>
        /// <para>Tag pICMS</para>
        /// </summary>
        public double AliquotaIcms { get; set; }

        /// <summary>
        /// <para>Valor do IPI</para>
        /// <para>Tag vIPI</para>
        /// </summary>
        public double? ValorIpi { get; set; }

        /// <summary>
        /// <para>Alíquota do IPI</para>
        /// <para>Tag pIPI</para>
        /// </summary>
        public double? AliquotaIpi { get; set; }

        /// <summary>
        /// <para>Valor da BC do IPI</para>
        /// <para>Tag vBC (grupo IPITrib)</para>
        /// </summary>
        public double? BaseIpi { get; set; }

        /// <summary>
        /// <para>Valor aproximado total de tributos federais, estaduais e municipais. [NT2013.003]</para>
        /// <para>Tag vTotTrib</para>
        /// </summary>
        public double? ValorAproximadoTributos { get; set; }

        #region Reforma Tributária - IBS / CBS / IS (NT 2026.010, item 4.3)

        /// <summary>
        /// <para>Código de Classificação Tributária do IBS e da CBS</para>
        /// <para>Tag cClassTrib (UB14)</para>
        /// </summary>
        public string ClassificacaoTributaria { get; set; }

        /// <summary>
        /// <para>Base de cálculo do IBS e da CBS</para>
        /// <para>Tag vBC (UB16)</para>
        /// </summary>
        public double? BaseIbsCbs { get; set; }

        /// <summary>
        /// <para>Alíquota do IBS da UF a ser impressa: pAliqEfet (UB28) quando informado o grupo gRed, senão pIBSUF (UB18).</para>
        /// </summary>
        public double? AliquotaIbsUf { get; set; }

        /// <summary>
        /// <para>Valor do IBS da UF</para>
        /// <para>Tag vIBSUF (UB35)</para>
        /// </summary>
        public double? ValorIbsUf { get; set; }

        /// <summary>
        /// <para>Alíquota do IBS do Município a ser impressa: pAliqEfet (UB47) quando informado o grupo gRed, senão pIBSMun (UB37).</para>
        /// </summary>
        public double? AliquotaIbsMun { get; set; }

        /// <summary>
        /// <para>Valor do IBS do Município</para>
        /// <para>Tag vIBSMun (UB54)</para>
        /// </summary>
        public double? ValorIbsMun { get; set; }

        /// <summary>
        /// <para>Alíquota da CBS a ser impressa: pAliqEfet (UB66) quando informado o grupo gRed, senão pCBS (UB56).</para>
        /// </summary>
        public double? AliquotaCbs { get; set; }

        /// <summary>
        /// <para>Valor da CBS</para>
        /// <para>Tag vCBS (UB67)</para>
        /// </summary>
        public double? ValorCbs { get; set; }

        /// <summary>
        /// <para>Base de cálculo do Imposto Seletivo</para>
        /// <para>Tag vBCIS (UB05)</para>
        /// </summary>
        public double? BaseIs { get; set; }

        /// <summary>
        /// <para>Alíquota do Imposto Seletivo</para>
        /// <para>Tag pIS (UB06)</para>
        /// </summary>
        public double? AliquotaIs { get; set; }

        /// <summary>
        /// <para>Valor do Imposto Seletivo</para>
        /// <para>Tag vIS (UB11)</para>
        /// </summary>
        public double? ValorIs { get; set; }

        #endregion

        public ProdutoViewModel()
        {
            AliquotaIpi = null;
            ValorIpi = null;
        }

        public string DescricaoCompleta
        {
            get
            {
                string descriCaoCompleta = Descricao;

                if (!string.IsNullOrWhiteSpace(InformacoesAdicionais))
                {
                    descriCaoCompleta += "\r\n" + InformacoesAdicionais;
                }

                return descriCaoCompleta;
            }
        }
    }
}
