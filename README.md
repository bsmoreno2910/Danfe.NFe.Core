# Danfe.NFe.Core (refactor PdfSharpCore / .NET Core)

Gerador de **DANFE NF-e modelo 55** em PDF.

Este repositório é uma continuação do trabalho original de
[Laranjeiras/Zion.NFe.Danfe](https://github.com/Laranjeiras/Zion.NFe.Danfe)
— que por sua vez é um fork do
[SilverCard/DanfeSharp](https://github.com/SilverCard/DanfeSharp).

## O que mudou nessa versão (2.0)

- Removida a dependência do **PDFClown.NET 2.0.0** (biblioteca descontinuada
  e incompatível com .NET Core/8).
- Substituída por **PdfSharpCore 1.3.x**, com licença MIT e suporte
  multiplataforma (Windows, Linux, macOS).
- Biblioteca compilando em **multi-target**: `netstandard2.0` e `net8.0`.
- Projeto `WebService` migrado para **.NET 8** com minimal hosting.
- Projeto de testes migrado para **MSTest 3.x SDK-style** em `net8.0`.
- Adicionado `FontResolver` padrão do PdfSharpCore para descoberta
  cross-platform de fontes (Times New Roman/Courier).
- Encoding Unicode no XFont — acentuação brasileira preservada.

## Instalação

```bash
dotnet add package Danfe.NFe.Core
```

## Exemplo de uso

```csharp
using Danfe.NFe.Core;
using Danfe.NFe.Core.Modelo;

// Cria o modelo a partir de um arquivo XML da NF-e.
var modelo = DanfeViewModelCreator.CriarDeArquivoXml("nfe.xml");

// O modelo também pode ser construído manualmente.
var modelo2 = new DanfeViewModel()
{
    NfNumero = 123456,
    NfSerie = 123,
    ChaveAcesso = "12345698700000000000000000000000000000000000",
    Emitente = new EmpresaViewModel()
    {
        CnpjCpf = "12345678000100",
        RazaoSocial = "Minha Empresa Ltda",
        // ...
    },
    // ...
};

modelo.DefinirTextoCreditos("Gerado por Minha Empresa Ltda.");

// Gera o PDF e salva em arquivo.
using (var danfe = new DanfeDoc(modelo))
{
    danfe.Gerar();
    danfe.Salvar("danfe.pdf");
}

// Ou obtém os bytes do PDF em memória.
using (var ms = new MemoryStream())
using (var danfe = new DanfeDoc(modelo))
{
    danfe.Gerar();
    byte[] bytes = danfe.ObterPdfBytes(ms);
    // bytes contém o PDF pronto para envio.
}
```

### Adicionar logo (opcional)

```csharp
using (var danfe = new DanfeDoc(modelo))
{
    // Logo raster (JPG/PNG):
    danfe.AdicionarLogoImagem("logo.jpg");

    // Ou logo vetorial (primeira página de um PDF):
    danfe.AdicionarLogoPdf("logo.pdf");

    danfe.Gerar();
    danfe.Salvar("danfe.pdf");
}
```

## Estrutura da solução

| Projeto | Framework | Descrição |
|---------|-----------|-----------|
| `Danfe.NFe.Core` | netstandard2.0 / net8.0 | Biblioteca principal. |
| `Danfe.NFe.Core.WebService` | net8.0 | API REST de exemplo (`POST /api/xml/pdf/gerar`). |
| `Danfe.NFe.Core.Tests` | net8.0 | Testes MSTest. |

## Font resolver (Linux / macOS / Docker)

O PdfSharpCore precisa localizar fontes TrueType do sistema. O projeto já
configura `GlobalFontSettings.FontResolver = new FontResolver()` do próprio
PdfSharpCore, que busca fontes em diretórios padrão de cada SO.

Em imagens Docker mínimas (`alpine`, `debian-slim` etc.) geralmente você
precisa instalar as fontes manualmente:

```dockerfile
RUN apt-get update && apt-get install -y fonts-liberation fontconfig && fc-cache -f
```

Se você quiser usar um resolver customizado, basta sobrescrever antes da
primeira chamada à biblioteca:

```csharp
PdfSharpCore.Fonts.GlobalFontSettings.FontResolver = meuResolverCustomizado;
```

## Build

```bash
dotnet restore
dotnet build
dotnet test
```

## Licença

MIT. Baseado no trabalho original do [DanfeSharp](https://github.com/SilverCard/DanfeSharp) / [Zion.NFe.Danfe](https://github.com/Laranjeiras/Zion.NFe.Danfe).
