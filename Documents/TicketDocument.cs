using QuestPDF.Fluent;
using QuestPDF.Infrastructure;
using QuestPDF.Helpers;
using OnlineTicket.Models;
using System.Collections.Generic;
using System;

public class TicketDocument : IDocument
{
    public List<Ticket> Tickets { get; set; }
    public TicketDocument(List<Ticket> tickets)
    {
        Tickets = tickets;
    }
    public DocumentMetadata GetMetadata() => DocumentMetadata.Default;
    public void Compose(IDocumentContainer container)
    {
        container.Page(page =>
        {
            page.Margin(30);
            page.Size(PageSizes.A4);
            page.PageColor(Colors.White);
            page.DefaultTextStyle(x => x.FontSize(12));

            page.Content().Column(column =>
            {
                // Title
                column.Item().Text("My Tickets")
                              .Bold()
                              .FontSize(18)
                              .AlignCenter();

                // Loop through tickets
                foreach (var ticket in Tickets)
                {
                    column.Item().PaddingVertical(10)
                          .BorderBottom(1)
                          .BorderColor(Colors.Grey.Lighten2)
                          .Row(row =>
                          {
                              // Left: Ticket Info
                              row.RelativeItem(2).Column(col =>
                              {
                                  col.Item().Text($"Event: {ticket.Event?.Title ?? "N/A"}").Bold();
                                  col.Item().Text($"Ticket Type: {ticket.TicketType?.Name ?? "N/A"}");
                                  col.Item().Text($"Event Date: {ticket.Event?.EventDate.ToString("MMM dd, yyyy @ hh:mm tt") ?? "N/A"}");
                                  col.Item().Text($"Venue: {ticket.Event?.Venue?.Name ?? "N/A"}");
                                  col.Item().Text($"Ticket ID: {ticket.TicketId}");
                              });
                              // Right: QR Code
                              row.RelativeItem(1).Column(col =>
                              {
                                  if (!string.IsNullOrEmpty(ticket.QrBase64))
                                  {
                                      try
                                      {
                                          // Remove data URI prefix if exists
                                          var base64 = ticket.QrBase64.Replace("data:image/png;base64,", "");
                                          var qrBytes = Convert.FromBase64String(base64);

                                          col.Item().Image(qrBytes, QuestPDF.Infrastructure.ImageScaling.FitArea);
                                      }
                                      catch
                                      {
                                          // Optional: show placeholder if QR fails
                                          col.Item().Text("QR not available").Italic().FontSize(10).AlignCenter();
                                      }
                                  }
                              });
                          });
                }
            });
        });
    }
}







