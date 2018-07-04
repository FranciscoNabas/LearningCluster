using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Spire.Pdf;
using Spire.Pdf.Annotations;
using Spire.Pdf.Widget;
using System.Drawing.Printing;
using System.Windows.Forms;

namespace WebApplication1
{
    public partial class WebForm1 : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            
        }

        protected void ImprimirPDF_Click(object sender, EventArgs e)
        {
            PrintDialog DialogoImpressao = new PrintDialog();
            PdfDocument Documento = new PdfDocument();
            Documento.LoadFromFile(@"C:\Users\fenabas\Desktop\CardGenPrint.pdf");
            DialogoImpressao.AllowPrintToFile = true;
            DialogoImpressao.AllowSomePages = true;
            DialogoImpressao.PrinterSettings.MaximumPage = 1;
            DialogoImpressao.PrinterSettings.MaximumPage = Documento.Pages.Count;

            if (DialogoImpressao.ShowDialog() == DialogResult.OK)
            {
                Documento.PrintSettings.SelectPageRange(1, Documento.Pages.Count);
                Documento.Print();
            }
            
        }
    }
}