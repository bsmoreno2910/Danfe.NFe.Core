using System;
using System.Collections.Generic;
using System.IO;
using PdfSharpCore.Drawing;
using PdfSharpCore.Pdf;
using Zion.NFe.Danfe.Blocos;
using Zion.NFe.Danfe.Elementos;
using Zion.NFe.Danfe.Enumeracoes;
using Zion.NFe.Danfe.Graphics;
using Zion.NFe.Danfe.Modelo;

namespace Zion.NFe.Danfe
{
    public class DanfeDoc : IDisposable
    {
        public DanfeViewModel ViewModel { get; private set; }

        /// <summary>
        /// Documento PDF (PdfSharpCore).
        /// </summary>
        public PdfDocument PdfDocument { get; private set; }

        internal BlocoCanhoto Canhoto { get; private set; }
        internal BlocoIdentificacaoEmitente IdentificacaoEmitente { get; private set; }

        internal List<BlocoBase> _Blocos;
        internal Estilo EstiloPadrao { get; private set; }

        internal List<DanfePagina> Paginas { get; private set; }

        private readonly string _FonteFamilia;
        private bool _FoiGerado;

        /// <summary>
        /// Imagem raster (JPG/PNG) do logo. Exclusivo com <see cref="_LogoPdfForm"/>.
        /// </summary>
        private XImage _LogoImage;

        /// <summary>
        /// Logo em formato vetorial — primeira página de um PDF. Renderizado como XImage.
        /// </summary>
        private XImage _LogoPdfForm;

        public DanfeDoc(DanfeViewModel viewModel)
        {
            ViewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));

            // PdfSharpCore precisa de um FontResolver configurado para localizar
            // fontes cross-platform (Linux/Mac). Idempotente: só roda uma vez.
            DanfeFontResolverSetup.EnsureInitialized();

            _Blocos = new List<BlocoBase>();
            PdfDocument = new PdfDocument();

            // De acordo com o item 7.7, a fonte deve ser Times New Roman ou Courier New.
            _FonteFamilia = "Times New Roman";

            EstiloPadrao = CriarEstilo();

            Paginas = new List<DanfePagina>();
            Canhoto = CriarBloco<BlocoCanhoto>();
            IdentificacaoEmitente = AdicionarBloco<BlocoIdentificacaoEmitente>();
            AdicionarBloco<BlocoDestinatarioRemetente>();

            if (ViewModel.LocalRetirada != null && ViewModel.ExibirBlocoLocalRetirada)
                AdicionarBloco<BlocoLocalRetirada>();

            if (ViewModel.LocalEntrega != null && ViewModel.ExibirBlocoLocalEntrega)
                AdicionarBloco<BlocoLocalEntrega>();

            if (ViewModel.Duplicatas.Count > 0)
                AdicionarBloco<BlocoDuplicataFatura>();

            AdicionarBloco<BlocoCalculoImposto>(ViewModel.Orientacao == Orientacao.Paisagem ? EstiloPadrao : CriarEstilo(4.75F));
            AdicionarBloco<BlocoTransportador>();
            AdicionarBloco<BlocoDadosAdicionais>(CriarEstilo(tFonteCampoConteudo: 8));

            if (ViewModel.CalculoIssqn.Mostrar)
                AdicionarBloco<BlocoCalculoIssqn>();

            AdicionarMetadata();

            _FoiGerado = false;
        }

        /// <summary>
        /// Define a logo a partir de um stream de imagem raster (JPG/PNG).
        /// </summary>
        public void AdicionarLogoImagem(Stream stream)
        {
            if (stream == null) throw new ArgumentNullException(nameof(stream));
            // XImage.FromStream copia os dados; não precisamos manter o stream vivo.
            _LogoImage = XImage.FromStream(() => stream);
        }

        /// <summary>
        /// Define a logo a partir da primeira página de um PDF (logo vetorial).
        /// </summary>
        public void AdicionarLogoPdf(Stream stream)
        {
            if (stream == null) throw new ArgumentNullException(nameof(stream));
            // No PdfSharpCore, o próprio XImage sabe lidar com PDFs de 1 página via XPdfForm,
            // mas a forma mais compatível é carregar via XImage.FromStream.
            _LogoPdfForm = XImage.FromStream(() => stream);
        }

        public void AdicionarLogoImagem(string path)
        {
            if (string.IsNullOrWhiteSpace(path)) throw new ArgumentException(nameof(path));
            _LogoImage = XImage.FromFile(path);
        }

        public void AdicionarLogoPdf(string path)
        {
            if (string.IsNullOrWhiteSpace(path)) throw new ArgumentException(nameof(path));
            _LogoPdfForm = XImage.FromFile(path);
        }

        private void AdicionarMetadata()
        {
            var info = PdfDocument.Info;
            info.CreationDate = DateTime.Now;
            info.Creator = string.Format("{0} {1} - {2}",
                "Zion.NFe.Danfe",
                System.Reflection.Assembly.GetExecutingAssembly().GetName().Version,
                "https://github.com/bsmoreno2910/Zion.NFe.Danfe");
            info.Title = "DANFE (Documento auxiliar da NFe)";
            info.Subject = $"DANFE - Chave: {ViewModel.ChaveAcesso}";
            info.Keywords = "DANFE;NFE;ChaveAcesso=" + ViewModel.ChaveAcesso;
        }

        private Estilo CriarEstilo(float tFonteCampoCabecalho = 6, float tFonteCampoConteudo = 10)
        {
            return new Estilo(_FonteFamilia, tFonteCampoCabecalho, tFonteCampoConteudo);
        }

        public void Gerar()
        {
            if (_FoiGerado) throw new InvalidOperationException("O Danfe já foi gerado.");

            IdentificacaoEmitente.Logo = _LogoPdfForm ?? _LogoImage;
            var tabela = new TabelaProdutosServicos(ViewModel, EstiloPadrao);

            while (true)
            {
                DanfePagina p = CriarPagina();

                tabela.SetPosition(p.RetanguloCorpo.Location);
                tabela.SetSize(p.RetanguloCorpo.Size);
                tabela.Draw(p.Gfx);

                // No PdfSharpCore cada página tem seu próprio XGraphics que precisa ser
                // fechado (Dispose) para que o conteúdo seja gravado no PDF.
                p.Dispose();

                if (tabela.CompletamenteDesenhada) break;
            }

            PreencherNumeroFolhas();
            _FoiGerado = true;
        }

        private DanfePagina CriarPagina()
        {
            DanfePagina p = new DanfePagina(this);
            Paginas.Add(p);
            p.DesenharBlocos(Paginas.Count == 1);
            p.DesenharCreditos();

            if (ViewModel.TipoAmbiente == 2)
                p.DesenharAvisoHomologacao();

            return p;
        }

        internal T CriarBloco<T>() where T : BlocoBase
        {
            return (T)Activator.CreateInstance(typeof(T), ViewModel, EstiloPadrao);
        }

        internal T CriarBloco<T>(Estilo estilo) where T : BlocoBase
        {
            return (T)Activator.CreateInstance(typeof(T), ViewModel, estilo);
        }

        internal T AdicionarBloco<T>() where T : BlocoBase
        {
            var bloco = CriarBloco<T>();
            _Blocos.Add(bloco);
            return bloco;
        }

        internal T AdicionarBloco<T>(Estilo estilo) where T : BlocoBase
        {
            var bloco = CriarBloco<T>(estilo);
            _Blocos.Add(bloco);
            return bloco;
        }

        internal void AdicionarBloco(BlocoBase bloco)
        {
            _Blocos.Add(bloco);
        }

        internal void PreencherNumeroFolhas()
        {
            int nFolhas = Paginas.Count;
            for (int i = 0; i < Paginas.Count; i++)
            {
                // Precisamos reabrir o XGraphics da página para gravar o número.
                Paginas[i].DesenhaNumeroPaginas(i + 1, nFolhas);
            }
        }

        public void Salvar(string path)
        {
            if (string.IsNullOrWhiteSpace(path)) throw new ArgumentException(nameof(path));
            PdfDocument.Save(path);
        }

        public void Salvar(Stream stream)
        {
            if (stream == null) throw new ArgumentNullException(nameof(stream));
            PdfDocument.Save(stream, false);
        }

        /// <summary>
        /// Retorna os bytes do PDF gerado.
        /// </summary>
        public byte[] ObterPdfBytes(Stream stream)
        {
            if (stream == null) throw new ArgumentNullException(nameof(stream));
            PdfDocument.Save(stream, false);
            if (stream is MemoryStream ms) return ms.ToArray();

            using (var tmp = new MemoryStream())
            {
                stream.Position = 0;
                stream.CopyTo(tmp);
                return tmp.ToArray();
            }
        }

        #region IDisposable Support
        private bool disposedValue = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    foreach (var p in Paginas) p.Dispose();
                    PdfDocument?.Dispose();
                    _LogoImage?.Dispose();
                    _LogoPdfForm?.Dispose();
                }
                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }
        #endregion
    }
}
