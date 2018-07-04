<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="EPLDirectPrint.aspx.cs" Inherits="WebApplication1.WebForm1" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Teste Impressão EPL direto do navegador</title>
    <meta name="description" content="The description of my page" />
    <meta name="viewport" content="width=device-width, initial-scale=1.0" />
</head>
<body>
    <h1>Teste Impressão EPL direto do navegador</h1>

    <script type="text/javascript">
        function ImprimeEpl(epl)
        {
            var JanelaImpressao = window.open();
            JanelaImpressao.document.open('text/plain')
            JanelaImpressao.document.write(epl);
            JanelaImpressao.document.close();
            JanelaImpressao.focus();
            JanelaImpressao.print();
            JanelaImpressao.close();
        }

    </script>

    <input type="button" value="Imprimir EPL" onclick="ImprimeEpl(document.getElementById('codigoepl').value)" /><br/>
    <textarea id="codigoepl" cols="40" rows="20">A500,100,0,2,1,1,N,"TESTE"
P1,1
N

    </textarea><br/>
    
    <form id="NewForm" method="post" runat="server">
    <asp:Button Text="Imprimir PDF" runat="server" OnClick="ImprimirPDF_Click"/>
    </form>
            
    </body>
</html>
