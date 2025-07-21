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
    public class PrintingSaleInvoice
    {
        Inventorifo.Lib.LibCore CoreCl = new Inventorifo.Lib.LibCore ();

        public string transaction_id;
        public bool dialog;
        private double fontSize = 8.5;
        public string strContent;
        private int linesPerPage;
        private string[] lines;
        private int numLines;
        private int numPages;
        private PrintOperation print;
        DataTable dtTrans;
        DataTable dtItems;
        TransactionSale parent;


        public PrintingSaleInvoice(object parent, DataTable dtTrans, DataTable dtItems)
        {
            this.parent=(TransactionSale)parent;
            this.dtTrans = dtTrans;
            this.dtItems = dtItems;
            this.strContent = "";
            this.strContent += "NoTrans\t" + transaction_id + "\n";
            this.strContent += "Tgl\t\t 2025-01-01 \n\n";
        }
        public void DoPrint(bool dialog)
        {
            var print = new PrintOperation
            {
                JobName = "Print Job transaction_id " + dtTrans.Rows[0].ItemArray[0].ToString()                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                   
            };

            print.BeginPrint += OnBeginPrint;
            print.DrawPage += OnDrawPage;

            if(dialog){
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
            // var op = (PrintOperation)obj;
            // op.NPages = 1;
            string contents;
            double height;

            PrintContext context = args.Context;
            height = context.Height;
        
            linesPerPage = (int)Math.Floor(height / fontSize);

            numLines = this.dtTrans.Rows.Count+20;
            numPages = (numLines - 1) / linesPerPage + 1;     
            var op = (PrintOperation)obj;       
            //op.NPages = numPages;    
            op.NPages = 1;        
        }

        public void OnDrawPage(object obj, DrawPageArgs args)
        {
            var cr = args.Context.CairoContext;
            double pageWidth = args.Context.Width;
            double pageHeight = args.Context.Height;


            // // Table data
            // string[] headers = { "Name", "Age", "City" };
            // string[,] data =
            // {
            //     { "Alice", "30", "New York" },
            //     { "Bob", "25", "Berlin" },
            //     { "Charlie", "40", "Tokyo" }
            // };
            // // rows
            // // foreach (DataRow dr in this.dtTrans.Rows)
            // // {
            // //     data.add ({})
            // // } 

            double payment_amount = 0;
            double tax_amount = 0;
            double total_price = 0;

            double lineSpace = 10;
            double startX = 8;
            double startY = 10;
            double PosX = startX;
            double PosY = startY;
            
            //title
            cr.SelectFontFace("Sans", FontSlant.Normal, FontWeight.Bold);
            cr.SetFontSize(9);
            TextExtents te = cr.TextExtents(this.parent.parent.conf.organization_name);
            PosX = PosX = (pageWidth - te.Width) / 2 - te.XBearing; 
            cr.MoveTo(PosX, PosY);
            cr.ShowText(this.parent.parent.conf.organization_name);

            //subtitle
            cr.SetFontSize(6);
            PosY = PosY+lineSpace;
            te = cr.TextExtents(this.parent.parent.conf.organization_address + " Telp." + this.parent.parent.conf.organization_phone_number);
            PosX = PosX = (pageWidth - te.Width) / 2 - te.XBearing;
            cr.MoveTo(PosX, PosY);
            cr.ShowText(this.parent.parent.conf.organization_address + " Telp." + this.parent.parent.conf.organization_phone_number );
            
            //draw line
            PosY = PosY+4;
            PosX = startX;
            cr.MoveTo(PosX, PosY);
            cr.LineTo(pageWidth-PosX, PosY);
            cr.Stroke();

            //customer
            startX=0;
            PosX = startX;
            cr.SetFontSize(7);
            cr.SelectFontFace("Sans", FontSlant.Normal, FontWeight.Normal);
            foreach (DataRow dr in this.dtTrans.Rows){
                PosY = PosY+lineSpace;
                cr.MoveTo(PosX, PosY);
                cr.ShowText(dr["id"].ToString()+"/"+dr["transaction_date"].ToString()+"/"+dr["user_name"].ToString());
                PosY = PosY+lineSpace;
                cr.MoveTo(PosX, PosY);
                cr.ShowText(dr["customer_id"].ToString()+"/"+dr["person_name"].ToString());
                payment_amount = CoreCl.GetPaymentAmount(dr["id"].ToString());
                tax_amount = Convert.ToDouble(dr["tax_amount"].ToString()) ;
            }
            double total_item = 0;
            PosY = PosY+lineSpace+lineSpace;
            foreach (DataRow dr in this.dtItems.Rows){
                PosX = startX;
                cr.MoveTo(PosX, PosY);
                cr.ShowText(dr["product_short_name"].ToString() );
                //PosX = PosX+90;
                total_item = total_item+ Convert.ToDouble(dr["quantity"].ToString());       
                cr.ShowText(dr["final_price"].ToString() );
                cr.MoveTo(startX+110, PosY);
                double subtotal= Convert.ToDouble(dr["final_price"].ToString())*Convert.ToDouble(dr["quantity"].ToString());
                cr.ShowText(subtotal.ToString() );
                total_price=total_price+subtotal;
                PosY = PosY+lineSpace;
            }

            //draw line
            PosX = startX+90;
            cr.MoveTo(PosX, PosY);
            cr.LineTo(PosX+50, PosY);
            cr.Stroke();

            cr.SelectFontFace("Sans", FontSlant.Normal, FontWeight.Bold);
            PosY = PosY+lineSpace;
            PosX = startX;
            cr.MoveTo(PosX, PosY);
            cr.ShowText("Item  "+total_item.ToString());

            cr.SelectFontFace("Sans", FontSlant.Normal, FontWeight.Normal);
            PosX = startX+50;
            cr.MoveTo(PosX, PosY);
            cr.ShowText("Subtotal  ");

            PosX = startX+110;
            cr.MoveTo(PosX, PosY);
            cr.ShowText(total_price.ToString());

            PosY = PosY+lineSpace;
            PosX = startX+50;
            cr.MoveTo(PosX, PosY);
            cr.ShowText("Tax  ");

            PosX = startX+110;
            cr.MoveTo(PosX, PosY);
            cr.ShowText( tax_amount.ToString() );

            //draw line
            PosY = PosY+3;
            PosX = startX+90;
            cr.MoveTo(PosX, PosY);
            cr.LineTo(PosX+50, PosY);
            cr.Stroke();

            cr.SelectFontFace("Sans", FontSlant.Normal, FontWeight.Bold);
            PosY = PosY+lineSpace;
            PosX = startX+50;
            cr.MoveTo(PosX, PosY);
            cr.ShowText("Total  ");

            PosX = startX+110;
            cr.MoveTo(PosX, PosY);
            cr.ShowText( (total_price+tax_amount).ToString() );

            cr.SelectFontFace("Sans", FontSlant.Normal, FontWeight.Normal);
            PosY = PosY+lineSpace;
            PosX = startX+50;
            cr.MoveTo(PosX, PosY);
            cr.ShowText("Payment  ");

            PosX = startX+110;
            cr.MoveTo(PosX, PosY);
            cr.ShowText( payment_amount.ToString() );

            PosY = PosY+lineSpace;
            PosX = startX+50;
            cr.MoveTo(PosX, PosY);
            cr.ShowText("Change  ");

            PosX = startX+110;
            cr.MoveTo(PosX, PosY);
            cr.ShowText( (payment_amount-total_price+tax_amount).ToString() );
            
            //subtitle
            cr.SetFontSize(6);
            PosY = startX;
            cr.MoveTo(PosX, PosY);
            cr.ShowText("Barang yang sudah dibeli tidak bisa ditukar/kembalikan");
            PosY = PosY+lineSpace+lineSpace;
            cr.MoveTo(PosX, PosY);
            cr.ShowText("tidak bisa ditukar/kembalikan");
            PosY = PosY+lineSpace+lineSpace;
            cr.MoveTo(PosX, PosY);
            cr.ShowText("TERIMAKASIH");
            PosY = PosY+lineSpace+lineSpace;
            cr.ShowText(" ");
            (cr as IDisposable).Dispose ();

            // // Draw horizontal lines
            // for (int r = 0; r <= rows; r++)
            // {
            //     double y = startY + r * cellHeight;
            //     cr.MoveTo(startX, y);
            //     cr.LineTo(startX + cols * cellWidth, y);
            // }

            // // Draw vertical lines
            // for (int c = 0; c <= cols; c++)
            // {
            //     double x = startX + c * cellWidth;
            //     cr.MoveTo(x, startY);
            //     cr.LineTo(x, startY + rows * cellHeight);
            // }

            // cr.Stroke();

            // Draw header text
            // cr.SelectFontFace("Sans", FontSlant.Normal, FontWeight.Bold);
            // cr.SetFontSize(12);
            // for (int c = 0; c < cols; c++)
            // {
            //     // center
            //     //double x = startX + c * cellWidth + (cellWidth - te.Width) / 2; 
            //     double x = startX + c * cellWidth + 5;
            //     double y = startY + cellHeight / 2 + 5;
            //     cr.MoveTo(x, y);
            //     cr.ShowText(headers[c]);
            // }

            // // Draw cell text
            // cr.SelectFontFace("Sans", FontSlant.Normal, FontWeight.Normal);
            // for (int r = 0; r < data.GetLength(0); r++)
            // {
            //     for (int c = 0; c < cols; c++)
            //     {
            //         double x = startX + c * cellWidth + 5;
            //         double y = startY + (r + 1) * cellHeight + cellHeight / 2 + 5;
            //         cr.MoveTo(x, y);
            //         cr.ShowText(data[r, c]);
            //     }
            // }

            // cr.Stroke();
        }
    }
}