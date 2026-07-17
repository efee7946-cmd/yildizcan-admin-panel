using System.Text.Json;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using YildizCanAdmin.Shared;

namespace YildizCanAdmin.Web.Services;

// Tek sayfalık veli raporu: profil + özet + seans tabloları.
public static class ParentReportPdf
{
    private const string Accent = "#2a78d6";
    private const int MaxRows = 20;

    public static byte[] Generate(StudentDetailResponse detail, SessionsResponse sessions)
    {
        var s = detail.Student;
        var sum = detail.Summary;

        return Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(36);
                page.DefaultTextStyle(t => t.FontSize(10).FontColor(Colors.Grey.Darken4));

                page.Header().Column(col =>
                {
                    col.Item().Row(row =>
                    {
                        row.RelativeItem().Text("YıldızCan Veli Raporu").FontSize(18).SemiBold().FontColor(Accent);
                        row.ConstantItem(140).AlignRight().AlignMiddle()
                            .Text(DateTime.Now.ToString("dd.MM.yyyy")).FontColor(Colors.Grey.Darken1);
                    });
                    col.Item().PaddingTop(6).BorderBottom(1).BorderColor(Colors.Grey.Lighten2);
                });

                page.Content().PaddingTop(14).Column(col =>
                {
                    col.Item().Text(string.IsNullOrWhiteSpace(s.FullName) ? "(isimsiz)" : s.FullName)
                        .FontSize(14).SemiBold();
                    if (s.BirthYear is not null)
                        col.Item().Text($"Doğum yılı: {s.BirthYear}").FontColor(Colors.Grey.Darken1);
                    if (!string.IsNullOrWhiteSpace(s.SupportNotes))
                        col.Item().PaddingTop(2).Text($"Destek notu: {s.SupportNotes}").FontColor(Colors.Grey.Darken1);

                    col.Item().PaddingTop(14).Text("Nesne Tanıma Özeti").FontSize(12).SemiBold();
                    col.Item().PaddingTop(6).Row(row =>
                    {
                        Tile(row, sum.Plays.ToString(), "Oyun");
                        Tile(row, sum.Accuracy is null ? "—" : $"%{sum.Accuracy}", "Doğruluk");
                        Tile(row, sum.Perfect.ToString(), "Hatasız oyun");
                        Tile(row, sum.LastPlayed?.ToLocalTime().ToString("dd.MM.yyyy") ?? "—", "Son oyun");
                    });

                    col.Item().PaddingTop(16).Text("Nesne Tanıma Seansları").FontSize(12).SemiBold();
                    if (sessions.ObjResults.Count == 0)
                    {
                        col.Item().PaddingTop(4).Text("Henüz seans yok.").FontColor(Colors.Grey.Darken1);
                    }
                    else
                    {
                        col.Item().PaddingTop(6).Table(table =>
                        {
                            table.ColumnsDefinition(c =>
                            {
                                c.RelativeColumn(3); c.RelativeColumn(2); c.RelativeColumn(2); c.RelativeColumn(2);
                            });
                            table.Header(h =>
                            {
                                Head(h.Cell(), "Tarih"); Head(h.Cell(), "Doğru"); Head(h.Cell(), "Hata"); Head(h.Cell(), "Doğruluk");
                            });
                            foreach (var r in sessions.ObjResults.Take(MaxRows))
                            {
                                Cell(table.Cell(), r.Date?.ToLocalTime().ToString("dd.MM.yyyy HH:mm") ?? "—");
                                Cell(table.Cell(), r.Items?.ToString() ?? "—");
                                Cell(table.Cell(), r.Errors?.ToString() ?? "—");
                                Cell(table.Cell(), r.Accuracy is null ? "—" : $"%{r.Accuracy}");
                            }
                        });
                        if (sessions.ObjResults.Count > MaxRows)
                            col.Item().PaddingTop(2).Text($"(son {MaxRows} seans gösteriliyor)").FontSize(8).FontColor(Colors.Grey.Darken1);
                    }

                    col.Item().PaddingTop(16).Text("Konuşma Pratiği Seansları").FontSize(12).SemiBold();
                    if (sessions.SpeechHistory.Count == 0)
                    {
                        col.Item().PaddingTop(4).Text("Henüz seans yok.").FontColor(Colors.Grey.Darken1);
                    }
                    else
                    {
                        col.Item().PaddingTop(6).Table(table =>
                        {
                            table.ColumnsDefinition(c =>
                            {
                                c.RelativeColumn(3); c.RelativeColumn(2); c.RelativeColumn(2); c.RelativeColumn(3);
                            });
                            table.Header(h =>
                            {
                                Head(h.Cell(), "Tarih"); Head(h.Cell(), "Süre (dk)"); Head(h.Cell(), "Konuşma turu"); Head(h.Cell(), "Hikâye");
                            });
                            foreach (var e in sessions.SpeechHistory.Take(MaxRows))
                            {
                                Cell(table.Cell(), PDate(e, "session_date"));
                                Cell(table.Cell(), P(e, "duration_min"));
                                Cell(table.Cell(), P(e, "total_turns"));
                                Cell(table.Cell(), P(e, "story_name"));
                            }
                        });
                        if (sessions.SpeechHistory.Count > MaxRows)
                            col.Item().PaddingTop(2).Text($"(son {MaxRows} seans gösteriliyor)").FontSize(8).FontColor(Colors.Grey.Darken1);
                    }
                });

                page.Footer().Row(row =>
                {
                    row.RelativeItem().Text("YıldızCan").FontSize(8).FontColor(Colors.Grey.Darken1);
                    row.RelativeItem().AlignRight().Text(t =>
                    {
                        t.DefaultTextStyle(x => x.FontSize(8).FontColor(Colors.Grey.Darken1));
                        t.Span("Sayfa ");
                        t.CurrentPageNumber();
                        t.Span(" / ");
                        t.TotalPages();
                    });
                });
            });
        }).GeneratePdf();
    }

    private static void Tile(RowDescriptor row, string value, string label)
    {
        row.RelativeItem().Border(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(8).Column(c =>
        {
            c.Item().AlignCenter().Text(value).FontSize(14).SemiBold();
            c.Item().AlignCenter().Text(label).FontSize(8).FontColor(Colors.Grey.Darken1);
        });
    }

    private static void Head(IContainer cell, string text)
        => cell.BorderBottom(1).BorderColor(Colors.Grey.Lighten1).PaddingVertical(4).PaddingHorizontal(2)
               .Text(text).SemiBold().FontSize(9);

    private static void Cell(IContainer cell, string text)
        => cell.BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).PaddingVertical(4).PaddingHorizontal(2)
               .Text(text).FontSize(9);

    private static string P(JsonElement e, string name)
    {
        if (e.ValueKind != JsonValueKind.Object || !e.TryGetProperty(name, out var v) || v.ValueKind == JsonValueKind.Null)
            return "—";
        var s = v.ToString();
        return string.IsNullOrWhiteSpace(s) ? "—" : s;
    }

    private static string PDate(JsonElement e, string name)
    {
        var s = P(e, name);
        return DateTime.TryParse(s, out var d) ? d.ToString("dd.MM.yyyy") : s;
    }
}
