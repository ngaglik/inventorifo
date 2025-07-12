using System;
using System.Collections.Generic;
using Gtk;
using Gdk;
using System.Data;
using Pango;
using UI = Gtk.Builder.ObjectAttribute;
using System.Text.RegularExpressions;
using Inventorifo.Lib.Model;
using Cairo;

namespace Inventorifo.App
{
    public class PrintingTable
    {
        public string transaction_id;
        public bool dialog;
        private double fontSize = 8.5;
        public string strContent;
        private int linesPerPage;
        private string[] lines;
        private int numLines;
        private int numPages;

        public PrintingTable(string transaction_id)
        {
            this.dialog = dialog;
            this.transaction_id = transaction_id;
            this.strContent = "";
            this.strContent += "NoTrans\t" + transaction_id + "\n";
            this.strContent += "Tgl\t\t 2025-01-01 \n\n";
        }
        public void DoPrint(bool dialog)
        {
            var print = new PrintOperation
            {
                JobName = "Print Job transaction_id " + this.transaction_id
            };

            print.BeginPrint += OnBeginPrint;
            print.DrawPage += OnDrawPage;

            if(this.dialog){
                var result = print.Run(PrintOperationAction.PrintDialog, null);
                if (result == PrintOperationResult.Error)
                {
                    Console.WriteLine("An error occurred during printing.");
                }
            }else{
                var result = print.Run(PrintOperationAction.Print, null);
                if (result == PrintOperationResult.Error)
                {
                    Console.WriteLine("An error occurred during printing.");
                }
            }
            
        }
       
        public void OnBeginPrint(object obj, BeginPrintArgs args)
        {           
            var op = (PrintOperation)obj;
            op.NPages = 1;
        }

        public void OnDrawPage(object obj, DrawPageArgs args)
        {
            var cr = args.Context.CairoContext;

            // Table data
            string[] headers = { "Name", "Age", "City" };
            string[,] data =
            {
                { "Alice", "30", "New York" },
                { "Bob", "25", "Berlin" },
                { "Charlie", "40", "Tokyo" }
            };

            double startX = 50;
            double startY = 100;
            double cellWidth = 100;
            double cellHeight = 30;

            int rows = data.GetLength(0) + 1; // include header
            int cols = headers.Length;

            //cr.SetLineWidth(1);
            cr.SetSourceRGB(0, 0, 0);

            // Draw horizontal lines
            for (int r = 0; r <= rows; r++)
            {
                double y = startY + r * cellHeight;
                cr.MoveTo(startX, y);
                cr.LineTo(startX + cols * cellWidth, y);
            }

            // Draw vertical lines
            for (int c = 0; c <= cols; c++)
            {
                double x = startX + c * cellWidth;
                cr.MoveTo(x, startY);
                cr.LineTo(x, startY + rows * cellHeight);
            }

            cr.Stroke();

            // Draw header text
            cr.SelectFontFace("Sans", FontSlant.Normal, FontWeight.Bold);
            cr.SetFontSize(12);
            for (int c = 0; c < cols; c++)
            {
                // center
                //double x = startX + c * cellWidth + (cellWidth - te.Width) / 2; 
                double x = startX + c * cellWidth + 5;
                double y = startY + cellHeight / 2 + 5;
                cr.MoveTo(x, y);
                cr.ShowText(headers[c]);
            }

            // Draw cell text
            cr.SelectFontFace("Sans", FontSlant.Normal, FontWeight.Normal);
            for (int r = 0; r < data.GetLength(0); r++)
            {
                for (int c = 0; c < cols; c++)
                {
                    double x = startX + c * cellWidth + 5;
                    double y = startY + (r + 1) * cellHeight + cellHeight / 2 + 5;
                    cr.MoveTo(x, y);
                    cr.ShowText(data[r, c]);
                }
            }

            cr.Stroke();
        }
    }
}