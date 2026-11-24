using System;
using System.Collections.Generic;
using System.Drawing;
using PdfSharpCore.Drawing;
using Danfe.NFe.Core.Enumeracoes;
using Danfe.NFe.Core.Graphics;

namespace Danfe.NFe.Core.Elementos
{
    /// <summary>
    /// Conteúdo de uma célula da <see cref="Tabela"/>.
    /// </summary>
    internal abstract class CelulaTabela
    {
        public abstract float CalcularAltura(float largura);
        public abstract void Desenhar(Gfx gfx, float x, float y, float largura);
    }

    /// <summary>
    /// Célula com um ou mais parágrafos de texto com quebra de linha automática.
    /// </summary>
    internal class CelulaTexto : CelulaTabela
    {
        private readonly List<TextBlock> _Paragrafos = new List<TextBlock>();
        private readonly List<XBrush> _Cores = new List<XBrush>();
        private readonly AlinhamentoHorizontal _Alinhamento;

        public CelulaTexto(AlinhamentoHorizontal alinhamento = AlinhamentoHorizontal.Esquerda)
        {
            _Alinhamento = alinhamento;
        }

        public CelulaTexto(string texto, Fonte fonte, AlinhamentoHorizontal alinhamento = AlinhamentoHorizontal.Esquerda) : this(alinhamento)
        {
            AdicionarParagrafo(texto, fonte);
        }

        public CelulaTexto AdicionarParagrafo(string texto, Fonte fonte, XBrush cor = null)
        {
            if (string.IsNullOrWhiteSpace(texto)) return this;

            _Paragrafos.Add(new TextBlock(texto, fonte) { AlinhamentoHorizontal = _Alinhamento });
            _Cores.Add(cor);
            return this;
        }

        public override float CalcularAltura(float largura)
        {
            float h = 0;

            foreach (var p in _Paragrafos)
            {
                p.Width = largura;
                h += p.Height;
            }

            return h;
        }

        public override void Desenhar(Gfx gfx, float x, float y, float largura)
        {
            for (int i = 0; i < _Paragrafos.Count; i++)
            {
                var p = _Paragrafos[i];
                p.Width = largura;
                p.SetPosition(x, y);

                var corAnterior = gfx.TextBrush;
                if (_Cores[i] != null) gfx.TextBrush = _Cores[i];
                p.Draw(gfx);
                gfx.TextBrush = corAnterior;

                y += p.Height;
            }
        }
    }

    /// <summary>
    /// Célula com linhas de pares "rótulo ..... valor". Cada linha pode ter um ou mais pares,
    /// separados por " / ", conforme os modelos de referência da NT 2026.010.
    /// Se o conteúdo não couber na largura, a fonte é reduzida até <see cref="TamanhoMinimo"/>.
    /// </summary>
    internal class CelulaPares : CelulaTabela
    {
        public const string Separador = " / ";
        public const float TamanhoMinimo = 4.5F;

        private readonly List<KeyValuePair<string, string>[]> _Linhas = new List<KeyValuePair<string, string>[]>();
        private readonly Fonte _FonteBase;

        private float _LarguraCalculada = -1;
        private Fonte _Fonte;

        public CelulaPares(Fonte fonteBase)
        {
            _FonteBase = fonteBase ?? throw new ArgumentNullException(nameof(fonteBase));
        }

        public CelulaPares AdicionarLinha(params KeyValuePair<string, string>[] pares)
        {
            if (pares == null || pares.Length == 0) throw new ArgumentException(nameof(pares));
            _Linhas.Add(pares);
            _LarguraCalculada = -1;
            return this;
        }

        public CelulaPares AdicionarLinha(string rotulo, string valor) => AdicionarLinha(Par(rotulo, valor));

        public static KeyValuePair<string, string> Par(string rotulo, string valor) => new KeyValuePair<string, string>(rotulo, valor);

        private float LarguraSlot(Fonte fonte, float largura, int quantidadePares)
        {
            float sep = fonte.MedirLarguraTexto(Separador);
            return (largura - sep * (quantidadePares - 1)) / quantidadePares;
        }

        private void AjustarFonte(float largura)
        {
            if (_LarguraCalculada == largura && _Fonte != null) return;

            _LarguraCalculada = largura;
            float escala = 1F;
            float espaco = _FonteBase.MedirLarguraTexto(" ");

            foreach (var linha in _Linhas)
            {
                float slot = LarguraSlot(_FonteBase, largura, linha.Length);

                foreach (var par in linha)
                {
                    float necessario = _FonteBase.MedirLarguraTexto(par.Key) + espaco + _FonteBase.MedirLarguraTexto(par.Value);
                    if (necessario > slot && necessario > 0)
                        escala = Math.Min(escala, slot / necessario);
                }
            }

            _Fonte = _FonteBase.Clonar();
            if (escala < 1F)
                _Fonte.Tamanho = Math.Max(TamanhoMinimo, _FonteBase.Tamanho * escala);
        }

        public override float CalcularAltura(float largura)
        {
            AjustarFonte(largura);
            return _Linhas.Count * _Fonte.AlturaLinha;
        }

        public override void Desenhar(Gfx gfx, float x, float y, float largura)
        {
            AjustarFonte(largura);

            float lh = _Fonte.AlturaLinha;
            float sep = _Fonte.MedirLarguraTexto(Separador);
            float ponto = _Fonte.MedirLarguraTexto(".");

            foreach (var linha in _Linhas)
            {
                float slot = LarguraSlot(_Fonte, largura, linha.Length);
                float xs = x;

                for (int i = 0; i < linha.Length; i++)
                {
                    if (i > 0)
                    {
                        gfx.DrawString(Separador, new RectangleF(xs, y, sep, lh), _Fonte, AlinhamentoHorizontal.Centro);
                        xs += sep;
                    }

                    var rotulo = linha[i].Key ?? string.Empty;
                    var valor = linha[i].Value ?? string.Empty;

                    float wValor = _Fonte.MedirLarguraTexto(valor);
                    float wRotulo = _Fonte.MedirLarguraTexto(rotulo);
                    int pontos = ponto > 0 ? (int)Math.Floor((slot - wRotulo - wValor - ponto) / ponto) : 0;
                    if (pontos > 0 && valor.Length > 0) rotulo += new string('.', pontos);

                    var r = new RectangleF(xs, y, slot, lh);
                    gfx.DrawString(rotulo, r, _Fonte, AlinhamentoHorizontal.Esquerda);
                    gfx.DrawString(valor, r, _Fonte, AlinhamentoHorizontal.Direita);

                    xs += slot;
                }

                y += lh;
            }
        }
    }
}
