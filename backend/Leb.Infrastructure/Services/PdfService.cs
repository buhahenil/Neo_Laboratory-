using System;
using System.Collections.Generic;
using System.IO;
using PdfSharp.Drawing;
using PdfSharp.Pdf;
using Leb.Core.Entities;
using Leb.Core.Interfaces;

namespace Leb.Infrastructure.Services
{
    public class PdfService : IPdfService
    {
        public byte[] GenerateReportPdf(Appointment appointment, IEnumerable<Report> reports, Branch? branch = null, bool preprinted = false)
        {
            var document = new PdfDocument();
            var page = document.AddPage();
            var gfx = XGraphics.FromPdfPage(page);

            double pageWidth = page.Width.Point;
            double pageHeight = page.Height.Point;

            // Fonts
            var fontTitle = new XFont("Arial", 18, XFontStyleEx.Bold);
            var fontSubtitle = new XFont("Arial", 11, XFontStyleEx.Bold);
            var fontLabel = new XFont("Arial", 9, XFontStyleEx.Bold);
            var fontValue = new XFont("Arial", 9, XFontStyleEx.Regular);
            var fontTableHeader = new XFont("Arial", 9, XFontStyleEx.Bold);
            var fontTableBody = new XFont("Arial", 9, XFontStyleEx.Regular);
            var fontWatermark = new XFont("Arial", 8, XFontStyleEx.Italic);
            var fontDoctorName = new XFont("Arial", 10, XFontStyleEx.Bold);
            var fontDoctorDegree = new XFont("Arial", 8, XFontStyleEx.Regular);

            // 1. Render Background Graphic / Header (Only in Digital Mode)
            if (!preprinted)
            {
                bool hasImage = branch != null && !string.IsNullOrEmpty(branch.LetterheadImagePath) && File.Exists(branch.LetterheadImagePath);
                if (hasImage)
                {
                    try
                    {
                        using (var img = XImage.FromFile(branch.LetterheadImagePath!))
                        {
                            gfx.DrawImage(img, 0, 0, pageWidth, pageHeight);
                        }
                    }
                    catch
                    {
                        DrawDefaultLetterhead(gfx, pageWidth, pageHeight, branch, fontTitle, fontWatermark, fontDoctorName, fontDoctorDegree);
                    }
                }
                else
                {
                    DrawDefaultLetterhead(gfx, pageWidth, pageHeight, branch, fontTitle, fontWatermark, fontDoctorName, fontDoctorDegree);
                }
            }

            // 2. Patient Demographics & Report Metadata
            double yPos = 145; // Start offset below letterhead header

            gfx.DrawRectangle(XBrushes.GhostWhite, new XRect(20, yPos, pageWidth - 40, 55));
            gfx.DrawRectangle(XPens.LightGray, new XRect(20, yPos, pageWidth - 40, 55));

            double pY = yPos + 16;
            gfx.DrawString("PATIENT NAME:", fontLabel, XBrushes.DarkSlateGray, 30, pY);
            gfx.DrawString($"{appointment.PatientFirstName} {appointment.PatientLastName}", fontSubtitle, XBrushes.Black, 120, pY);

            gfx.DrawString("REPORT ID:", fontLabel, XBrushes.DarkSlateGray, pageWidth - 200, pY);
            gfx.DrawString($"REP-{appointment.AppointmentId}", fontLabel, XBrushes.DarkRed, pageWidth - 120, pY);

            pY += 18;
            int age = DateTime.Today.Year - appointment.DateOfBirth.Year;
            if (appointment.DateOfBirth > DateTime.Today.AddYears(-age)) age--;

            gfx.DrawString("AGE / GENDER:", fontLabel, XBrushes.DarkSlateGray, 30, pY);
            gfx.DrawString($"{age} Yrs / {appointment.Gender}", fontValue, XBrushes.Black, 120, pY);

            gfx.DrawString("DATE:", fontLabel, XBrushes.DarkSlateGray, pageWidth - 200, pY);
            gfx.DrawString(appointment.AppointmentDate.ToString("yyyy-MM-dd"), fontValue, XBrushes.Black, pageWidth - 120, pY);

            pY += 18;
            gfx.DrawString("REF. DOCTOR:", fontLabel, XBrushes.DarkSlateGray, 30, pY);
            string docName = !string.IsNullOrEmpty(appointment.DoctorFirstName) ? $"Dr. {appointment.DoctorFirstName} {appointment.DoctorLastName}" : "Self Referrer";
            gfx.DrawString(docName, fontValue, XBrushes.Black, 120, pY);

            gfx.DrawString("PHONE:", fontLabel, XBrushes.DarkSlateGray, pageWidth - 200, pY);
            gfx.DrawString(appointment.PatientPhone, fontValue, XBrushes.Black, pageWidth - 120, pY);

            // 3. Test Results Table
            yPos += 70;
            gfx.DrawString("DIAGNOSTIC PATHOLOGY REPORT", fontSubtitle, XBrushes.Maroon, 20, yPos);

            double xCode = 20;
            double xName = 90;
            double xSample = 270;
            double xValue = 370;
            double xNormal = 470;

            yPos += 12;
            gfx.DrawRectangle(XBrushes.Maroon, new XRect(20, yPos, pageWidth - 40, 20));
            gfx.DrawString("CODE", fontTableHeader, XBrushes.White, xCode + 5, yPos + 14);
            gfx.DrawString("TEST NAME", fontTableHeader, XBrushes.White, xName + 5, yPos + 14);
            gfx.DrawString("SAMPLE TYPE", fontTableHeader, XBrushes.White, xSample + 5, yPos + 14);
            gfx.DrawString("RESULT VALUE", fontTableHeader, XBrushes.White, xValue + 5, yPos + 14);
            gfx.DrawString("REFERENCE RANGE", fontTableHeader, XBrushes.White, xNormal + 5, yPos + 14);

            yPos += 24;
            bool hasOutsourced = false;

            foreach (var rep in reports)
            {
                gfx.DrawString(rep.TestCode, fontTableBody, XBrushes.Black, xCode + 5, yPos + 12);

                string testNameStr = rep.TestName;
                if (rep.IsOutsourced)
                {
                    testNameStr += " [Outsourced]";
                    hasOutsourced = true;
                }
                gfx.DrawString(testNameStr, fontTableBody, XBrushes.Black, xName + 5, yPos + 12);
                gfx.DrawString(rep.SampleType, fontTableBody, XBrushes.Black, xSample + 5, yPos + 12);

                string res = rep.ResultValue ?? "Pending";
                var valColor = res == "Pending" ? XBrushes.Red : XBrushes.DarkGreen;
                gfx.DrawString(res, fontLabel, valColor, xValue + 5, yPos + 12);

                gfx.DrawString(rep.NormalRange ?? "N/A", fontTableBody, XBrushes.Black, xNormal + 5, yPos + 12);

                if (rep.IsOutsourced && !string.IsNullOrEmpty(rep.ExternalLabName))
                {
                    yPos += 14;
                    gfx.DrawString($"Ref. Lab: {rep.ExternalLabName} | Barcode: {rep.ExternalBarcode ?? "N/A"}", fontWatermark, XBrushes.DarkBlue, xName + 5, yPos + 10);
                }

                if (!string.IsNullOrEmpty(rep.Remarks))
                {
                    yPos += 14;
                    gfx.DrawString($"Remarks: {rep.Remarks}", fontWatermark, XBrushes.DimGray, xName + 5, yPos + 10);
                }

                yPos += 22;
                gfx.DrawLine(XPens.LightGray, 20, yPos - 5, pageWidth - 20, yPos - 5);
            }

            // 4. Outsourced Footnote & Verification QR Section
            if (hasOutsourced)
            {
                yPos += 10;
                gfx.DrawString("* Note: Tests marked with [Outsourced] were dispatched to authorized third-party reference laboratories.", fontWatermark, XBrushes.DarkBlue, 20, yPos);
            }

            // Digital QR Verification block near footer
            double qrY = pageHeight - 110;
            gfx.DrawRectangle(XBrushes.WhiteSmoke, new XRect(20, qrY, pageWidth - 40, 55));
            gfx.DrawRectangle(XPens.Silver, new XRect(20, qrY, pageWidth - 40, 55));

            double qrX = 30;
            gfx.DrawRectangle(XPens.Maroon, XBrushes.Maroon, new XRect(qrX, qrY + 8, 40, 40));
            gfx.DrawRectangle(XPens.White, XBrushes.White, new XRect(qrX + 7, qrY + 15, 26, 26));
            gfx.DrawRectangle(XPens.Maroon, XBrushes.Maroon, new XRect(qrX + 13, qrY + 21, 14, 14));

            gfx.DrawString("DIGITALLY SIGNED & VERIFIED REPORT", fontSubtitle, XBrushes.Maroon, qrX + 55, qrY + 20);
            gfx.DrawString($"Scan OR visit https://neoleboretory.lab/verify/REP-{appointment.AppointmentId} to verify authenticity.", fontWatermark, XBrushes.SlateGray, qrX + 55, qrY + 36);

            using var stream = new MemoryStream();
            document.Save(stream);
            return stream.ToArray();
        }

        private static void DrawDefaultLetterhead(XGraphics gfx, double pageWidth, double pageHeight, Branch? branch, XFont fontTitle, XFont fontWatermark, XFont fontDoctorName, XFont fontDoctorDegree)
        {
            // Default Header Bar
            var rectHeader = new XRect(0, 0, pageWidth, 75);
            gfx.DrawRectangle(XBrushes.Maroon, rectHeader);

            string title = branch?.GujaratiTitle ?? "નીઓ લેબોરેટરી";
            gfx.DrawString(title, fontTitle, XBrushes.White, 35, 45);

            // Doctors Box
            double docX = pageWidth - 220;
            string doc1 = branch?.Doctor1Name ?? "Ankur Ramani";
            string deg1 = branch?.Doctor1Degree ?? "B.Voc , PGDMLT";
            string doc2 = branch?.Doctor2Name ?? "Hardik Ramani";
            string deg2 = branch?.Doctor2Degree ?? "BSC. Micro, MSC. Embryo, PGDMLT";

            gfx.DrawString(doc1, fontDoctorName, XBrushes.White, docX, 25);
            gfx.DrawString($"({deg1})", fontDoctorDegree, XBrushes.LightPink, docX, 37);

            gfx.DrawString(doc2, fontDoctorName, XBrushes.White, docX, 52);
            gfx.DrawString($"({deg2})", fontDoctorDegree, XBrushes.LightPink, docX, 64);

            // Footer Bar
            var rectFooter = new XRect(0, pageHeight - 35, pageWidth, 35);
            gfx.DrawRectangle(XBrushes.Maroon, rectFooter);

            string phone = branch?.ContactNumber ?? "+91 83203 23244";
            string timing = branch?.TimingInfo ?? "8:00 AM to 8:00 PM";
            string address = branch != null ? $"{branch.Address}, {branch.City}" : "Opp. Neo Hospital, Nikava, Tal. Kalavad, Dist. Jamnagar.";

            gfx.DrawString($"Ph: {phone} | Timings: {timing}", fontDoctorDegree, XBrushes.White, 20, pageHeight - 15);
            gfx.DrawString(address, fontDoctorDegree, XBrushes.White, pageWidth / 2 - 60, pageHeight - 15);
        }

        public byte[] GenerateInvoicePdf(Appointment appointment, Invoice invoice)
        {
            var document = new PdfDocument();
            var page = document.AddPage();
            var gfx = XGraphics.FromPdfPage(page);

            double pageWidth = page.Width.Point;
            double pageHeight = page.Height.Point;

            // Fonts
            var fontTitle = new XFont("Arial", 22, XFontStyleEx.Bold);
            var fontSubtitle = new XFont("Arial", 12, XFontStyleEx.Bold);
            var fontLabel = new XFont("Arial", 10, XFontStyleEx.Bold);
            var fontValue = new XFont("Arial", 10, XFontStyleEx.Regular);
            var fontTableHeader = new XFont("Arial", 10, XFontStyleEx.Bold);
            var fontTableBody = new XFont("Arial", 9, XFontStyleEx.Regular);

            // Draw Header Banner
            var rectHeader = new XRect(20, 20, pageWidth - 40, 60);
            gfx.DrawRectangle(XBrushes.RoyalBlue, rectHeader);
            gfx.DrawString("LABORATORY RECEIPT / INVOICE", fontTitle, XBrushes.White, 35, 57);

            // Invoice Metadata
            double yPos = 95;
            gfx.DrawString($"Invoice No: {invoice.InvoiceNumber}", fontLabel, XBrushes.Black, 20, yPos);
            gfx.DrawString($"Date Generated: {invoice.GeneratedAt.ToString("yyyy-MM-dd HH:mm")}", fontValue, XBrushes.Black, 20, yPos + 15);
            gfx.DrawString($"Appointment ID: {appointment.AppointmentId}", fontValue, XBrushes.Black, 20, yPos + 30);

            gfx.DrawString($"Patient: {appointment.PatientFirstName} {appointment.PatientLastName}", fontLabel, XBrushes.Black, pageWidth - 250, yPos);
            gfx.DrawString($"Branch: {appointment.BranchName}", fontValue, XBrushes.Black, pageWidth - 250, yPos + 15);
            gfx.DrawString($"Payment Status: {appointment.PaymentStatus}", fontLabel, XBrushes.DarkGreen, pageWidth - 250, yPos + 30);

            // Separator Line
            gfx.DrawLine(XPens.DarkGray, 20, 140, pageWidth - 20, 140);

            // Services Purchased List
            gfx.DrawString("SERVICES PURCHASED", fontSubtitle, XBrushes.RoyalBlue, 20, 165);

            yPos = 190;
            gfx.DrawRectangle(XBrushes.LightGray, new XRect(20, yPos, pageWidth - 40, 20));
            gfx.DrawString("CODE", fontTableHeader, XBrushes.Black, 30, yPos + 14);
            gfx.DrawString("ITEM DESCRIPTION", fontTableHeader, XBrushes.Black, 130, yPos + 14);
            gfx.DrawString("TYPE", fontTableHeader, XBrushes.Black, 350, yPos + 14);
            gfx.DrawString("PRICE (INR)", fontTableHeader, XBrushes.Black, 470, yPos + 14);

            yPos = 215;

            // Draw Tests
            foreach (var test in appointment.Tests)
            {
                gfx.DrawString(test.Code, fontTableBody, XBrushes.Black, 30, yPos + 12);
                gfx.DrawString(test.Name, fontTableBody, XBrushes.Black, 130, yPos + 12);
                gfx.DrawString("Individual Test", fontTableBody, XBrushes.Black, 350, yPos + 12);
                gfx.DrawString(test.Price.ToString("F2"), fontTableBody, XBrushes.Black, 470, yPos + 12);
                yPos += 20;
            }

            // Draw Packages
            foreach (var pkg in appointment.Packages)
            {
                gfx.DrawString(pkg.Code, fontTableBody, XBrushes.Black, 30, yPos + 12);
                gfx.DrawString(pkg.Name, fontTableBody, XBrushes.Black, 130, yPos + 12);
                gfx.DrawString("Test Package", fontTableBody, XBrushes.Black, 350, yPos + 12);
                gfx.DrawString(pkg.Price.ToString("F2"), fontTableBody, XBrushes.Black, 470, yPos + 12);
                yPos += 20;
            }

            gfx.DrawLine(XPens.LightGray, 20, yPos + 5, pageWidth - 20, yPos + 5);
            yPos += 15;

            // Pricing totals
            double totalXLabel = pageWidth - 220;
            double totalXValue = pageWidth - 90;

            gfx.DrawString("Subtotal Amount:", fontValue, XBrushes.Black, totalXLabel, yPos);
            gfx.DrawString($"INR {invoice.TotalAmount.ToString("F2")}", fontValue, XBrushes.Black, totalXValue, yPos);

            gfx.DrawString("Discount Applied:", fontValue, XBrushes.Black, totalXLabel, yPos + 15);
            gfx.DrawString($"INR -{invoice.DiscountAmount.ToString("F2")}", fontValue, XBrushes.Black, totalXValue, yPos + 15);

            gfx.DrawString("Tax (18% GST):", fontValue, XBrushes.Black, totalXLabel, yPos + 30);
            gfx.DrawString($"INR {invoice.TaxAmount.ToString("F2")}", fontValue, XBrushes.Black, totalXValue, yPos + 30);

            gfx.DrawLine(XPens.DarkGray, totalXLabel, yPos + 38, pageWidth - 20, yPos + 38);

            gfx.DrawString("Total Paid:", fontLabel, XBrushes.Black, totalXLabel, yPos + 52);
            gfx.DrawString($"INR {invoice.FinalAmount.ToString("F2")}", fontLabel, XBrushes.Black, totalXValue, yPos + 52);

            // Footer notes
            double footerY = pageHeight - 80;
            gfx.DrawLine(XPens.LightGray, 20, footerY, pageWidth - 20, footerY);
            gfx.DrawString("Thank you for choosing Neo Leboretory.", fontLabel, XBrushes.RoyalBlue, 20, footerY + 20);
            gfx.DrawString("For queries or support, contact help@neoleboretory.lab or call 1800-200-NEOLEB.", fontTableBody, XBrushes.Black, 20, footerY + 35);

            using var stream = new MemoryStream();
            document.Save(stream);
            return stream.ToArray();
        }
    }
}
