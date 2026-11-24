using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using PdfSharpCore.Drawing;
using Danfe.NFe.Core.Enumeracoes;
using Danfe.NFe.Core.Graphics;
using Danfe.NFe.Core.Tools.Extensions;

namespace Danfe.NFe.Core.Elementos
{
    internal class Tabela : ElementoBase
    {
        public List<TabelaColuna> Colunas { get; private set; }
        public List<CelulaTabela[]> Linhas { get; private set; }
        public float PaddingSuperior { get; private set; }
        public float PaddingInferior { get; private set; }
        public float PaddingHorizontal { get; private set; }

        public int LinhaAtual { get; private set; }

        /// <summary>
        /// Quando verdadeiro, a primeira linha da página é desenhada mesmo que não caiba, para que
        /// um item mais alto que a página não impeça o término da geração do DANFE.
        /// </summary>
        public bool ForcarPrimeiraLinha { get; set; }

        private float _DY;
        private float _DY1;

        public Fonte FonteCorpo { get; private set; }
        public Fonte FonteCabecalho { get; private set; }

        public Tabela(Estilo estilo) : base(estilo)
        {
            Colunas = new List<TabelaColuna>();
            Linhas = new List<CelulaTabela[]>();
            LinhaAtual = 0;

            PaddingHorizontal = 0.6F;
            PaddingSuperior = 0.75F;
            PaddingInferior = 0.3F;

            // 7.7.7 Conteúdo dos Campos do Quadro “Dados dos Produtos/Serviços”
            // Deverá ter tamanho mínimo de seis(6) pontos, ou 17 CPP.

            FonteCorpo = estilo.CriarFonteRegular(6F);
            FonteCabecalho = estilo.CriarFonteNegrito(5.5F);
        }

        public Tabela ComColuna(float larguraP, AlinhamentoHorizontal ah, params string[] cabecalho)
        {
            Colunas.Add(new TabelaColuna(cabecalho, larguraP, ah));
            return this;
        }

        public void AdicionarLinha(List<string> linha)
        {
            if (linha.Count != Colunas.Count) throw new ArgumentException(nameof(linha));

            var celulas = new CelulaTabela[linha.Count];
            for (int i = 0; i < linha.Count; i++)
            {
                if (!string.IsNullOrWhiteSpace(linha[i]))
                    celulas[i] = new CelulaTexto(linha[i], FonteCorpo, Colunas[i].AlinhamentoHorizontal);
            }

            Linhas.Add(celulas);
        }

        public void AdicionarLinha(params CelulaTabela[] celulas)
        {
            if (celulas.Length != Colunas.Count) throw new ArgumentException(nameof(celulas));
            Linhas.Add(celulas);
        }

        public void AjustarLarguraColunas()
        {
            var sw = Colunas.Sum(x => x.PorcentagemLargura);

            if (sw > 100F) throw new InvalidOperationException();

            var w = (100F - sw) / (float)Colunas.Where(x => x.PorcentagemLargura == 0).Count();

            foreach (var c in Colunas.Where(x => x.PorcentagemLargura == 0))
                c.PorcentagemLargura = w;

        }

        private Boolean DesenharLinha(Gfx gfx, bool forcar)
        {
            _DY1 = _DY;

            var celulas = Linhas[LinhaAtual];
            var larguras = new float[Colunas.Count];
            float tbm = 0;

            for (int i = 0; i < Colunas.Count; i++)
            {
                larguras[i] = (Width * Colunas[i].PorcentagemLargura) / 100F - 2F * PaddingHorizontal;

                if (celulas[i] != null)
                    tbm = Math.Max(tbm, celulas[i].CalcularAltura(larguras[i]));
            }

            if (!forcar && tbm + _DY + PaddingInferior + PaddingSuperior > BoundingBox.Bottom) return false;

            float x = X;

            for (int i = 0; i < Colunas.Count; i++)
            {
                if (celulas[i] != null)
                    celulas[i].Desenhar(gfx, x + PaddingHorizontal, _DY + PaddingSuperior, larguras[i]);

                x += (Width * Colunas[i].PorcentagemLargura) / 100F;
            }

            _DY += Math.Max(tbm, FonteCorpo.AlturaLinha) + PaddingSuperior + PaddingInferior;

            return true;
        }

        public void DesenharCabecalho(Gfx gfx)
        {
            var ml = Colunas.Max(c => c.Cabecalho.Length);
            float ac = ml * FonteCabecalho.AlturaLinha + 2F;

            float x = X;
            _DY = Y;

            foreach (var coluna in Colunas)
            {
                float w = (Width * coluna.PorcentagemLargura) / 100F;
                var r = new RectangleF(x, _DY, w, ac);

                var tb = new TextStack(r.InflatedRetangle(1F));
                tb.AlinhamentoVertical = AlinhamentoVertical.Centro;
                tb.AlinhamentoHorizontal = AlinhamentoHorizontal.Centro;

                foreach (var item in coluna.Cabecalho)
                {
                    tb.AddLine(item, FonteCabecalho);
                }

                tb.Draw(gfx);

                x += w;

                gfx.DrawRectangle(r);
                gfx.DrawRectangle(r.X, BoundingBox.Y, r.Width, BoundingBox.Height);
            }

            _DY += ac;

            gfx.Stroke();
        }


        public override void Draw(Gfx gfx)
        {

            base.Draw(gfx);
            gfx.SetLineWidth(0.25F);

            DesenharCabecalho(gfx);

            int linhasNestaPagina = 0;

            while (LinhaAtual < Linhas.Count)
            {
                Boolean r = DesenharLinha(gfx, ForcarPrimeiraLinha && linhasNestaPagina == 0);

                if (r)
                {
                    if (LinhaAtual > 0 && linhasNestaPagina > 0)
                    {
                        // Linha separadora entre produtos: cinza 50% com traço 6/1.
                        gfx.DrawDashedLine(
                            new PointF(BoundingBox.Left, _DY1),
                            new PointF(BoundingBox.Right, _DY1),
                            XColor.FromArgb(128, 128, 128),
                            0.25F,
                            new double[] { 6, 1 });
                    }

                    LinhaAtual++;
                    linhasNestaPagina++;
                }
                else
                {
                    break;
                }
            }
        }

        public override bool PossuiContono => false;
    }
}
