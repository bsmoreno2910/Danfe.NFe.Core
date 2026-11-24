using System.IO;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Danfe.NFe.Core.Enumeracoes;
using Danfe.NFe.Core.Modelo;

namespace Danfe.NFe.Core.Tests
{
    /// <summary>
    /// Leiaute do DANFE da NT 2026.010 (Reforma Tributária: IBS, CBS e IS).
    /// </summary>
    [TestClass]
    public class ReformaTributariaTests
    {
        private const double Delta = 0.00001;
        private static readonly string XmlPath = Path.Combine("Xml", "NFe", "v4.00", "v4_ReformaTributaria.xml");
        private static readonly string OutputDirectory = Path.Combine("Output", "ReformaTributaria");

        private static DanfeViewModel CriarModelo() => DanfeViewModelCreator.CriarDeArquivoXml(XmlPath);

        private static void Gerar(DanfeViewModel model, string nome)
        {
            Directory.CreateDirectory(OutputDirectory);

            using (var danfe = new DanfeDoc(model))
            {
                danfe.Gerar();
                danfe.Salvar(Path.Combine(OutputDirectory, nome + ".pdf"));
            }
        }

        [TestMethod]
        public void Item_SemGrupoReducao_ImprimeAliquotaVigente()
        {
            var p = CriarModelo().Produtos[0];

            Assert.AreEqual("000001", p.ClassificacaoTributaria);
            Assert.AreEqual(1000, p.BaseIbsCbs.Value, Delta);
            Assert.AreEqual(0.10, p.AliquotaIbsUf.Value, Delta);
            Assert.AreEqual(1.00, p.ValorIbsUf.Value, Delta);
            Assert.AreEqual(0.00, p.AliquotaIbsMun.Value, Delta);
            Assert.AreEqual(0.90, p.AliquotaCbs.Value, Delta);
            Assert.AreEqual(9.00, p.ValorCbs.Value, Delta);
            Assert.AreEqual(1000, p.BaseIs.Value, Delta);
            Assert.AreEqual(2.00, p.AliquotaIs.Value, Delta);
            Assert.AreEqual(20.00, p.ValorIs.Value, Delta);
            Assert.AreEqual(1000, p.BaseIpi.Value, Delta);
        }

        [TestMethod]
        public void Item_ComGrupoReducao_ImprimeAliquotaEfetiva()
        {
            var p = CriarModelo().Produtos[1];

            Assert.AreEqual("200030", p.ClassificacaoTributaria);
            Assert.AreEqual(0.04, p.AliquotaIbsUf.Value, Delta);
            Assert.AreEqual(0.00, p.AliquotaIbsMun.Value, Delta);
            Assert.AreEqual(0.36, p.AliquotaCbs.Value, Delta);
            Assert.IsNull(p.BaseIs);
            Assert.IsNull(p.ValorIs);
        }

        [TestMethod]
        public void Totais_IbsCbsIs_E_CamposCondicionados()
        {
            var m = CriarModelo();
            var t = m.CalculoImposto;

            Assert.AreEqual(10.80, t.ValorCbs.Value, Delta);
            Assert.AreEqual(1.20, t.ValorIbsUf.Value, Delta);
            Assert.AreEqual(0.00, t.ValorIbsMun.Value, Delta);
            Assert.AreEqual(20.00, t.ValorImpostoSeletivo.Value, Delta);
            Assert.AreEqual(3.00, t.ValorIbsMono.Value, Delta);
            Assert.AreEqual(5.00, t.ValorCbsMono.Value, Delta);
            Assert.AreEqual(10.00, t.ValorFcp.Value, Delta);
            Assert.AreEqual(0.00, t.ValorFcpSt.Value, Delta);
            Assert.IsNull(t.BaseCalculoIcmsMonoReten);

            Assert.AreEqual("3 - REGIME NORMAL", m.Emitente.CrtDescricao);
            Assert.IsTrue(m.QrCode.StartsWith("https://"));
            Assert.IsFalse(m.Transportadora.PossuiDados);
        }

        [TestMethod]
        public void XmlSemGruposDaReforma_NaoPreencheCampos()
        {
            var m = DanfeViewModelCreator.CriarDeArquivoXml(Path.Combine("Xml", "NFe", "v4.00", "v4_ComLocalEntrega.xml"));

            Assert.IsTrue(m.Produtos.All(p => p.BaseIbsCbs == null && p.AliquotaCbs == null && p.ValorIs == null));
            Assert.IsNull(m.CalculoImposto.ValorCbs);
            Assert.IsNull(m.CalculoImposto.ValorImpostoSeletivo);
            Assert.IsNull(m.QrCode);
        }

        [TestMethod]
        public void Retrato()
        {
            var m = CriarModelo();
            m.Orientacao = Orientacao.Retrato;
            Gerar(m, "ReformaTributaria_Retrato");
        }

        [TestMethod]
        public void Paisagem()
        {
            var m = CriarModelo();
            m.Orientacao = Orientacao.Paisagem;
            Gerar(m, "ReformaTributaria_Paisagem");
        }

        [TestMethod]
        public void SemQrCode_SemCanhoto()
        {
            var m = CriarModelo();
            m.QrCode = null;
            m.QuantidadeCanhotos = 0;
            Gerar(m, "ReformaTributaria_SemQrCode_SemCanhoto");
        }

        [TestMethod]
        public void ItemMaiorQueAPagina_NaoTravaAGeracao()
        {
            var m = CriarModelo();
            m.Produtos[0].InformacoesAdicionais = string.Join("\n", Enumerable.Repeat("Linha de informação adicional do item.", 200));
            Gerar(m, "ReformaTributaria_ItemGigante");
        }
    }
}
