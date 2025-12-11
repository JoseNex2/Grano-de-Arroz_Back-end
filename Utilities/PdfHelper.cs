using Entities.Domain;
using iText.IO.Font.Constants;
using iText.IO.Image;
using iText.Kernel.Font;
using iText.Kernel.Geom;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Borders;
using iText.Layout.Element;
using iText.Layout.Properties;

namespace Utilities
{
    public class PdfHelper
    {
        public PdfHelper()
        {
        
        }

        public static byte[] PdfReportGenerate(Report report)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                PdfWriter writer = new PdfWriter(ms);
                PdfDocument pdf = new PdfDocument(writer);
                Document documento = new Document(pdf, PageSize.A4);
                documento.SetMargins(50, 50, 25, 25);

                string imagePathLogo = System.IO.Path.Combine(AppContext.BaseDirectory, "Resources", "Logo_Quilino.jpg");
                Image imgLogo = new Image(ImageDataFactory.Create(imagePathLogo));
                string imagePathFirma = System.IO.Path.Combine(AppContext.BaseDirectory, "Resources", "Firma.jpg");
                Image imgFirma = new Image(ImageDataFactory.Create(imagePathFirma));
                PdfFont fuenteNormal = PdfFontFactory.CreateFont(StandardFonts.HELVETICA);
                PdfFont fuenteNegrita = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD);
                Table tablaPrincipal = new Table(1).UseAllAvailableWidth();

                Table tablaInfoEmpleador = new Table(2).UseAllAvailableWidth();
                tablaInfoEmpleador.AddCell(new Cell()
                    .Add(imgLogo.SetAutoScale(true))
                    .SetBorder(Border.NO_BORDER)
                    .SetWidth(80)
                    );
                Table tablaDatosEmpleador = new Table(1).UseAllAvailableWidth();
                tablaDatosEmpleador.AddCell(new Cell()
                    .Add(new Paragraph("Reporte de bateria")
                    .SetFont(fuenteNegrita)
                    .SetFontSize(15))
                    .SetTextAlignment(TextAlignment.CENTER)
                    .SetBorder(Border.NO_BORDER)
                    );

                documento.Close();
                return ms.ToArray();
            }
        }
    }
}
