using System;
using System.Collections.Generic;
using System.Linq;

namespace HotelBookingSystem.TemplateMethod
{
    // ─── DATA MODELS ────────────────────────────────────────────────────────
    public record AuditEntry(string TxnId, decimal Amount, string Type, string AccountId);
    public record ReportSummary(int Count, decimal TotalDebits, decimal TotalCredits, decimal NetFlow);

    // ─── ABSTRACT CLASS cu Template Method ───────────────────────────────
    public abstract class AuditReportGenerator
    {
        // Template Method — defineste scheletul, apeleaza pasii in ordine fixa
        // Metoda este sealed — subclasele NU pot modifica ordinea pasilor!
        public void GenerateReport(string reportId, DateOnly period)
        {
            Console.WriteLine($"\n=== Generare raport: {reportId} | {period:yyyy-MM} ===");

            var data = FetchData(reportId, period);          // pas 1 — abstract
            var processed = ProcessData(data);               // pas 2 — abstract
            var formatted = FormatReport(reportId, processed); // pas 3 — abstract
            
            OnBeforeExport(reportId);                        // hook optional
            Export(formatted, reportId);                      // pas 4 — abstract
            OnAfterExport(reportId);                         // hook optional

            Console.WriteLine($"Raport {reportId} generat cu succes.");
        }

        // Pasi abstracti — subclasele TREBUIE sa ii implementeze
        protected abstract IEnumerable<AuditEntry> FetchData(string id, DateOnly period);
        protected abstract ReportSummary ProcessData(IEnumerable<AuditEntry> data);
        protected abstract string FormatReport(string id, ReportSummary summary);
        protected abstract void Export(string content, string reportId);

        // Hook-uri optionale — subclasele POT sa le suprascrie
        protected virtual void OnBeforeExport(string reportId) { }
        protected virtual void OnAfterExport(string reportId) { }
    }

    // ─── CONCRETE SUBCLASS — PDF ──────────────────────────────────────────
    public class PdfAuditReportGenerator : AuditReportGenerator
    {
        protected override IEnumerable<AuditEntry> FetchData(string id, DateOnly period)
        {
            Console.WriteLine("[PDF] Fetch date din DB...");
            return new[] {
                new AuditEntry("T1", 5_000m,  "DEBIT",  "ACC1"),
                new AuditEntry("T2", 12_000m, "CREDIT", "ACC1"),
                new AuditEntry("T3", 3_500m,  "DEBIT",  "ACC2"),
            };
        }

        protected override ReportSummary ProcessData(IEnumerable<AuditEntry> data)
        {
            var list = data.ToList();
            decimal deb = list.Where(e => e.Type == "DEBIT").Sum(e => e.Amount);
            decimal cred = list.Where(e => e.Type == "CREDIT").Sum(e => e.Amount);
            Console.WriteLine("[PDF] Procesare date...");
            return new ReportSummary(list.Count, deb, cred, cred - deb);
        }

        protected override string FormatReport(string id, ReportSummary s)
        {
            Console.WriteLine("[PDF] Formatare HTML->PDF cu header, footer, semnaturi...");
            return $"<PDF>Raport #{id} | Debit:{s.TotalDebits:C} | Credit:{s.TotalCredits:C}</PDF>";
        }

        protected override void Export(string content, string reportId)
            => Console.WriteLine($"[PDF] Export fisier: audit_{reportId}.pdf");

        // Suprascrie hook — trimite email dupa export
        protected override void OnAfterExport(string reportId)
            => Console.WriteLine($"[PDF] Email trimis: audit_{reportId}.pdf -> audit@bank.md");
    }

    // ─── CONCRETE SUBCLASS — CSV ──────────────────────────────────────────
    public class CsvAuditReportGenerator : AuditReportGenerator
    {
        protected override IEnumerable<AuditEntry> FetchData(string id, DateOnly period)
        {
            Console.WriteLine("[CSV] Fetch date din API...");
            return new[] {
                new AuditEntry("T1", 5_000m, "DEBIT", "ACC1"),
                new AuditEntry("T2", 8_000m, "DEBIT", "ACC2"),
            };
        }

        protected override ReportSummary ProcessData(IEnumerable<AuditEntry> data)
        {
            var list = data.ToList();
            decimal deb = list.Sum(e => e.Amount);
            Console.WriteLine("[CSV] Procesare date...");
            return new ReportSummary(list.Count, deb, 0, -deb);
        }

        protected override string FormatReport(string id, ReportSummary s)
        {
            Console.WriteLine("[CSV] Formatare CSV...");
            return $"id,debits,credits\n{id},{s.TotalDebits},{s.TotalCredits}";
        }

        protected override void Export(string content, string reportId)
            => Console.WriteLine($"[CSV] Export: audit_{reportId}.csv -> FTP server");
    }
}
