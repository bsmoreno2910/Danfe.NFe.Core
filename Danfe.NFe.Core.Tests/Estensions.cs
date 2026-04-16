
using System.Diagnostics;

namespace Danfe.NFe.Core.Tests
{
    public static class Extentions
    {
        public static void SalvarTestePdf(this Danfe.NFe.Core.DanfeDoc d)
        {
            d.Salvar(new StackTrace().GetFrame(1).GetMethod().Name + ".pdf");
        }
    }
}
