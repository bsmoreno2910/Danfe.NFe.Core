using System;
using System.Collections;
using System.Collections.Generic;
using Danfe.NFe.Core.Graphics;
using QRCoder;

namespace Danfe.NFe.Core.Elementos
{
    /// <summary>
    /// Desenha um QR Code vetorial centralizado no retângulo do elemento.
    /// </summary>
    internal class QrCode : ElementoBase
    {
        public const float Margem = 1.5F;

        private readonly List<BitArray> _Modulos;

        public QrCode(Estilo estilo, string conteudo) : base(estilo)
        {
            if (string.IsNullOrWhiteSpace(conteudo)) throw new ArgumentException("O conteúdo do QR Code não pode ser vazio.", nameof(conteudo));

            using (var gerador = new QRCodeGenerator())
            using (var dados = gerador.CreateQrCode(conteudo, QRCodeGenerator.ECCLevel.M))
            {
                // A matriz já inclui a zona de silêncio de 4 módulos em cada lado.
                _Modulos = dados.ModuleMatrix;
            }
        }

        public override void Draw(Gfx gfx)
        {
            base.Draw(gfx);

            int n = _Modulos.Count;
            float lado = Math.Min(Width, Height) - 2 * Margem;
            if (lado <= 0 || n == 0) return;

            float modulo = lado / n;
            float x0 = X + (Width - lado) / 2F;
            float y0 = Y + (Height - lado) / 2F;

            for (int linha = 0; linha < n; linha++)
            {
                var bits = _Modulos[linha];
                int coluna = 0;

                while (coluna < n)
                {
                    if (!bits[coluna])
                    {
                        coluna++;
                        continue;
                    }

                    int inicio = coluna;
                    while (coluna < n && bits[coluna]) coluna++;

                    gfx.FillRectangle(x0 + inicio * modulo, y0 + linha * modulo, (coluna - inicio) * modulo, modulo);
                }
            }
        }
    }
}
