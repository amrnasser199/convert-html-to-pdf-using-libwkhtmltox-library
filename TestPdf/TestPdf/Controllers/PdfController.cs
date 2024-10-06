using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Drawing.Printing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using static System.Net.Mime.MediaTypeNames;
using System.Web.UI.WebControls;
using DinkToPdf;
using DinkToPdf.Contracts;
using Orientation = DinkToPdf.Orientation;
using System.Reflection;
using System.Threading.Tasks;

namespace TestPdf.Controllers
{
	public class PdfController : ApiController
	{
		private static readonly IConverter _converter = new SynchronizedConverter(new PdfTools());

		public PdfController()
		{
			// Initialize the DinkToPdf converter
			//_converter = new SynchronizedConverter(new PdfTools());
		}

		[HttpPost]
		[Route("api/pdf/generate")]
		public async Task<IHttpActionResult> GeneratePdfAsync(TicketModel ticket)
		{

			// HTML Template with placeholders
			string htmlTemplate = await Task.Run(() =>
				File.ReadAllText(System.Web.Hosting.HostingEnvironment.MapPath("~/Templates/TrainTicketTemplate.html"))
			);
			// Replace placeholders with actual ticket data
			string htmlContent = htmlTemplate
				.Replace("{{PassengerName}}", "amr")
				.Replace("{{BookingID}}", "123")
				.Replace("{{TrainName}}","aswan to cairo")
				.Replace("{{TrainNumber}}", "126")
				.Replace("{{DepartureStation}}", "aswan")
				.Replace("{{DepartureTime}}", DateTime.Now.ToString())
				.Replace("{{ArrivalStation}}","cairo")
				.Replace("{{ArrivalTime}}", DateTime.Now.AddDays(2).ToString())
				.Replace("{{SeatNumber}}", "124");
			
			// Generate the PDF
			var pdfDocument = new HtmlToPdfDocument()
			{
				GlobalSettings = {
					ColorMode = DinkToPdf.ColorMode.Color,
					Orientation = Orientation.Portrait,
					PaperSize = DinkToPdf.PaperKind.A4
				},
				Objects = {
					new ObjectSettings() {
						PagesCount = true,
						HtmlContent = htmlContent,
						WebSettings = { DefaultEncoding = "utf-8" }
					}
				}
			};

			try
			{
				//Convert html to pdf
				byte[] pdf = await Task.Run(() => _converter.Convert(pdfDocument));

				// Return PDF as a file response
				var result = new HttpResponseMessage(HttpStatusCode.OK)
				{
					Content = new ByteArrayContent(pdf)
				};
				result.Content.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("attachment")
				{
					FileName = "TrainTicket.pdf"
				};
				result.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/pdf");

				return ResponseMessage(result);
			}
			catch (Exception ex)
			{
				// Log or handle the error
				return InternalServerError(ex);
			}
		}
	
	}

	// Model class for ticket data
	public class TicketModel
	{
		public string PassengerName { get; set; }
		public string BookingID { get; set; }
		public string TrainName { get; set; }
		public string TrainNumber { get; set; }
		public string DepartureStation { get; set; }
		public string DepartureTime { get; set; }
		public string ArrivalStation { get; set; }
		public string ArrivalTime { get; set; }
		public string SeatNumber { get; set; }
	}

}










