using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using PdfSharpCore.Drawing;
using Danfe.NFe.Core.Elementos;
using Danfe.NFe.Core.Enumeracoes;
using Danfe.NFe.Core.Graphics;
using Danfe.NFe.Core.Modelo;
using Danfe.NFe.Core.Tools;
using Danfe.NFe.Core.Tools.Extensions;

namespace Danfe.NFe.Core.Blocos
{
    /// <summary>
    /// Quadro "Dados dos Produtos / Serviços" no leiaute da NT 2026.010 (item 4.3).
    /// </summary>
    internal class TabelaProdutosServicos : ElementoBase
    {
        private static readonly XBrush CorSecundaria = new XSolidBrush(XColor.FromArgb(90, 90, 90));

        public CabecalhoBloco CabecalhoBloco { get; private set; }
        public Tabela Tabela { get; private set; }
        public DanfeViewModel ViewModel { get; private set; }

        public TabelaProdutosServicos(DanfeViewModel viewModel, Estilo estilo) : base(estilo)
        {
            ViewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
            CabecalhoBloco = new CabecalhoBloco(estilo, "DADOS DOS PRODUTOS / SERVIÇOS");

            var ad = AlinhamentoHorizontal.Direita;
            var ac = AlinhamentoHorizontal.Centro;
            var ae = AlinhamentoHorizontal.Esquerda;

            Tabela = new Tabela(Estilo);
            var rotuloCst = ViewModel.Emitente.CRT == "3" ? "CST" : "CSOSN";

            if (ViewModel.Orientacao == Orientacao.Retrato)
            {
                Tabela
                .ComColuna(0, ae, "DESCRIÇÃO DO PRODUTO / SERVIÇO")
                .ComColuna(6.4F, ac, rotuloCst + " /", "CFOP")
                .ComColuna(7.4F, ac, "QTD / UN")
                .ComColuna(8.2F, ad, "VLR UNIT")
                .ComColuna(8.2F, ad, "VLR TOTAL")
                .ComColuna(14.6F, ac, "BASES DE CÁLCULO")
                .ComColuna(14.8F, ac, "ALÍQUOTAS")
                .ComColuna(20.8F, ac, "VALOR DOS TRIBUTOS");
            }
            else
            {
                Tabela
                .ComColuna(0, ae, "DESCRIÇÃO DO PRODUTO / SERVIÇO")
                .ComColuna(5.4F, ac, rotuloCst + " /", "CFOP")
                .ComColuna(6.6F, ac, "QTD / UN")
                .ComColuna(6.8F, ad, "VLR UNIT")
                .ComColuna(6.8F, ad, "VLR TOTAL")
                .ComColuna(11.2F, ac, "BASES DE CÁLCULO")
                .ComColuna(12.2F, ac, "ALÍQUOTAS")
                .ComColuna(17F, ac, "VALOR DOS TRIBUTOS");
            }

            Tabela.AjustarLarguraColunas();

            var fonte = Tabela.FonteCorpo;

            foreach (var p in ViewModel.Produtos)
            {
                var descricao = new CelulaTexto(ae)
                    .AdicionarParagrafo(p.Descricao, fonte)
                    .AdicionarParagrafo(p.InformacoesAdicionais, fonte, CorSecundaria)
                    .AdicionarParagrafo(Identificadores(p), fonte, CorSecundaria);

                var cstCfop = new CelulaPares(fonte)
                    .AdicionarLinha(rotuloCst, p.OCst)
                    .AdicionarLinha("CFOP", p.Cfop.ToString(Formatador.Cultura));

                var quantidade = new CelulaTexto(ac)
                    .AdicionarParagrafo(p.Quantidade.Formatar(), fonte)
                    .AdicionarParagrafo(p.Unidade, fonte, CorSecundaria);

                var bases = new CelulaPares(fonte)
                    .AdicionarLinha("ICMS", p.BaseIcms.Formatar())
                    .AdicionarLinha("IBS / CBS", p.BaseIbsCbs.Formatar())
                    .AdicionarLinha("IS", p.BaseIs.Formatar())
                    .AdicionarLinha("IPI", p.BaseIpi.Formatar());

                var aliquotas = new CelulaPares(fonte)
                    .AdicionarLinha(CelulaPares.Par("ICMS", Percentual(p.AliquotaIcms)), CelulaPares.Par("CBS", Percentual(p.AliquotaCbs)))
                    .AdicionarLinha(CelulaPares.Par("IBS UF", Percentual(p.AliquotaIbsUf)), CelulaPares.Par("IPI", Percentual(p.AliquotaIpi)))
                    .AdicionarLinha(CelulaPares.Par("IBS MUN", Percentual(p.AliquotaIbsMun)), CelulaPares.Par("IS", Percentual(p.AliquotaIs)));

                var valores = new CelulaPares(fonte)
                    .AdicionarLinha(CelulaPares.Par("ICMS", p.ValorIcms.Formatar()), CelulaPares.Par("CBS", p.ValorCbs.Formatar()))
                    .AdicionarLinha(CelulaPares.Par("IBS UF", p.ValorIbsUf.Formatar()), CelulaPares.Par("IPI", p.ValorIpi.Formatar()))
                    .AdicionarLinha(CelulaPares.Par("IBS MUN", p.ValorIbsMun.Formatar()), CelulaPares.Par("IS", p.ValorIs.Formatar()));

                Tabela.AdicionarLinha(
                    descricao,
                    cstCfop,
                    quantidade,
                    new CelulaTexto(p.ValorUnitario.Formatar(), fonte, ad),
                    new CelulaTexto(p.ValorTotal.Formatar(), fonte, ad),
                    bases,
                    aliquotas,
                    valores);
            }
        }

        private static string Percentual(double? aliquota) => aliquota.HasValue ? aliquota.Formatar() + "%" : null;

        private static string Percentual(double aliquota) => aliquota.Formatar() + "%";

        /// <summary>
        /// Linha de identificação do item: código, NCM e classificação tributária do IBS/CBS.
        /// </summary>
        private static string Identificadores(ProdutoViewModel p)
        {
            var sb = new StringBuilder();

            void Adicionar(string rotulo, string valor)
            {
                if (string.IsNullOrWhiteSpace(valor)) return;
                if (sb.Length > 0) sb.Append(' ');
                sb.Append('[').Append(rotulo).Append(' ').Append(valor.Trim()).Append(']');
            }

            Adicionar("CÓD.", p.Codigo);
            Adicionar("NCM", p.Ncm);
            Adicionar("cClassTrib", p.ClassificacaoTributaria);

            return sb.ToString();
        }

        public override void Draw(Gfx gfx)
        {
            base.Draw(gfx);

            Tabela.SetPosition(RetanguloTabela.Location);
            Tabela.SetSize(RetanguloTabela.Size);
            Tabela.Draw(gfx);

            CabecalhoBloco.SetPosition(X, Y);
            CabecalhoBloco.Width = Width;
            CabecalhoBloco.Draw(gfx);
        }

        public RectangleF RetanguloTabela => BoundingBox.CutTop(CabecalhoBloco.Height);
        public Boolean CompletamenteDesenhada => Tabela.LinhaAtual == ViewModel.Produtos.Count;
        public override bool PossuiContono => false;
    }
}
