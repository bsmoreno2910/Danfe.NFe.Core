using System;
using Danfe.NFe.Core.Elementos;
using Danfe.NFe.Core.Enumeracoes;
using Danfe.NFe.Core.Graphics;
using Danfe.NFe.Core.Modelo;

namespace Danfe.NFe.Core.Blocos
{
    /// <summary>
    /// Define um bloco básico do DANFE.
    /// </summary>
    internal abstract class BlocoBase : ElementoBase
    {
        /// <summary>
        /// Constante de proporção dos campos para o formato retrato A4, porcentagem dividida pela largura desenhável.
        /// </summary>
        public const float Proporcao = 100F / 200F;

        public DanfeViewModel ViewModel { get; private set; }

        public abstract PosicaoBloco Posicao { get; }

        /// <summary>
        /// Pilha principal.
        /// </summary>
        public VerticalStack MainVerticalStack { get; private set; }

        /// <summary>
        /// Quando verdadeiro, o bloco é mostrado apenas na primeira página, caso contário é mostrado em todas elas.
        /// </summary>
        public virtual Boolean VisivelSomentePrimeiraPagina => true;

        public virtual String Cabecalho => null;

        public BlocoBase(DanfeViewModel viewModel, Estilo estilo) : base(estilo)
        {
            MainVerticalStack = new VerticalStack();
            ViewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));

            if (!String.IsNullOrWhiteSpace(Cabecalho))
            {
                MainVerticalStack.Add(new CabecalhoBloco(estilo, Cabecalho));
            }
        }

        /// <summary>
        /// Indica se algum dos valores foi informado e é diferente de zero. Usado para os
        /// campos de exibição condicionada (NT 2026.010, item 4.4).
        /// </summary>
        protected static bool AlgumValorInformado(params double?[] valores)
        {
            foreach (var v in valores)
            {
                if (v.HasValue && v.Value != 0) return true;
            }

            return false;
        }

        public LinhaCampos AdicionarLinhaCampos()
        {
            var l = new LinhaCampos(Estilo, Width);
            l.Width = Width;
            l.Height = Estilo.AlturaCampo;
            MainVerticalStack.Add(l);
            return l;
        }

        public override void Draw(Gfx gfx)
        {
            base.Draw(gfx);
            MainVerticalStack.SetPosition(X, Y);
            MainVerticalStack.Width = Width;
            MainVerticalStack.Draw(gfx);
        }

        public override float Height { get => MainVerticalStack.Height; set => throw new NotSupportedException(); }
        public override bool PossuiContono => false;
    }
}
