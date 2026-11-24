using System;
using Danfe.NFe.Core.Elementos;
using Danfe.NFe.Core.Enumeracoes;
using Danfe.NFe.Core.Graphics;
using Danfe.NFe.Core.Modelo;

namespace Danfe.NFe.Core.Blocos
{
    /// <summary>
    /// Bloco "Dados Adicionais", acompanhado do quadro "QR Code" quando o XML possui o qrCode (NT 2026.010, item 4.5).
    /// </summary>
    internal class BlocoDadosAdicionais : BlocoBase
    {
        public const float AlturaMinima = 25;
        public const float InfComplementaresLarguraPorcentagem = 75;
        public const float QrCodeLarguraPorcentagemRetrato = 20;
        public const float QrCodeLarguraPorcentagemPaisagem = 14;

        private readonly CampoMultilinha _cInfComplementares;
        private readonly Campo _cReservadoFisco;
        private readonly FlexibleLine _Linha;
        private readonly CabecalhoBloco _CabecalhoDadosAdicionais;

        private readonly QrCode _QrCode;
        private readonly CabecalhoBloco _CabecalhoQrCode;
        private readonly FlexibleLine _LinhaComQrCode;
        private readonly float _QrCodeLarguraPorcentagem;

        public BlocoDadosAdicionais(DanfeViewModel viewModel, Estilo estilo) : base(viewModel, estilo)
        {
            _cInfComplementares = new CampoMultilinha("Informações Complementares", ViewModel.TextoAdicional(), estilo);
            _cReservadoFisco = new CampoMultilinha("Reservado ao fisco", ViewModel.TextoAdicionalFisco(), estilo);

            _Linha = new FlexibleLine() { Height = _cInfComplementares.Height }
            .ComElemento(_cInfComplementares)
            .ComElemento(_cReservadoFisco)
            .ComLarguras(InfComplementaresLarguraPorcentagem, 0);

            _CabecalhoDadosAdicionais = new CabecalhoBloco(estilo, "Dados adicionais");

            if (string.IsNullOrWhiteSpace(ViewModel.QrCode))
            {
                MainVerticalStack.Add(_CabecalhoDadosAdicionais, _Linha);
                return;
            }

            _QrCodeLarguraPorcentagem = ViewModel.Orientacao == Orientacao.Paisagem ? QrCodeLarguraPorcentagemPaisagem : QrCodeLarguraPorcentagemRetrato;
            _QrCode = new QrCode(estilo, ViewModel.QrCode);
            _CabecalhoQrCode = new CabecalhoBloco(estilo, "QR Code");

            var esquerda = new VerticalStack();
            esquerda.Add(_CabecalhoDadosAdicionais, _Linha);

            var direita = new VerticalStack();
            direita.Add(_CabecalhoQrCode, _QrCode);

            _LinhaComQrCode = new FlexibleLine()
                .ComElemento(esquerda)
                .ComElemento(direita)
                .ComLarguras(0, _QrCodeLarguraPorcentagem);

            MainVerticalStack.Add(_LinhaComQrCode);
        }

        public override float Width
        {
            get => base.Width;
            set
            {
                base.Width = value;

                // Força o ajuste da altura do InfComplementares
                if (_cInfComplementares == null || _Linha == null) return;

                _Linha.Width = _LinhaComQrCode == null ? value : value * (100F - _QrCodeLarguraPorcentagem) / 100F;
                _Linha.Posicionar();
                _cInfComplementares.Height = AlturaMinima;
                _Linha.Height = _cInfComplementares.Height;

                if (_LinhaComQrCode != null)
                {
                    _QrCode.Height = _Linha.Height;
                    _LinhaComQrCode.Width = value;
                    _LinhaComQrCode.Height = _CabecalhoDadosAdicionais.Height + _Linha.Height;
                }
            }
        }

        public override void Draw(Gfx gfx)
        {
            base.Draw(gfx);
        }

        public override PosicaoBloco Posicao => PosicaoBloco.Base;
    }
}
