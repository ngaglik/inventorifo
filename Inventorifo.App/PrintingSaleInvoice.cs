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
            var op = (PrintOperation)obj;
            op.NPages = 1;
        }

        public void OnDrawPage(object obj, DrawPageArgs args)
        {
            var cr = args.Context.CairoContext;
            double pageWidth = args.Context.Width;
            double pageHeight = args.Context.Height;

            // Table data
            string[] headers = { "Name", "Age", "City" };
            string[,] data =
            {
                { "Alice", "30", "New York" },
                { "Bob", "25", "Berlin" },
                { "Charlie", "40", "Tokyo" }
            };
            // rows
            // foreach (DataRow dr in this.dtTrans.Rows)
            // {
            //     data.add ({})
            // } 

            double payment_amount = 0;
            double tax_amount = 0;
            double total_price = 0;

            double lineSpace = 10;
            double startX = 10;
            double startY = 10;
            double PosX = startX;
            double PosY = startY;
            
            //title
            cr.SelectFontFace("Sans", FontSlant.Normal, FontWeight.Bold);
            cr.SetFontSize(10);
            TextExtents te = cr.TextExtents(this.parent.parent.conf.organization_name);
            PosX = PosX = (pageWidth - te.Width) / 2 - te.XBearing; 
            cr.MoveTo(PosX, PosY);
            cr.ShowText(this.parent.parent.conf.organization_name);

            //subtitle
            cr.SetFontSize(8);
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
                PosX = PosX+200;
                cr.MoveTo(PosX, PosY);
                cr.ShowText(dr["quantity"].ToString() );
                total_item = total_item+ Convert.ToDouble(dr["quantity"].ToString());
                PosX = PosX+20;
                cr.MoveTo(PosX+10, PosY);
                cr.ShowText(dr["final_price"].ToString() );
                PosX = PosX+20;
                cr.MoveTo(PosX+60, PosY);
                double subtotal= Convert.ToDouble(dr["final_price"].ToString())*Convert.ToDouble(dr["quantity"].ToString());
                cr.ShowText(subtotal.ToString() );
                total_price=total_price+subtotal;
                PosY = PosY+lineSpace;
            }

            //draw line
            PosX = startX+180;
            cr.MoveTo(PosX, PosY);
            cr.LineTo(PosX+150, PosY);
            cr.Stroke();

            PosY = PosY+lineSpace;
            PosX = startX+150;
            cr.MoveTo(PosX, PosY);
            cr.ShowText("Subtotal  ");

            PosX = startX+200;
            cr.MoveTo(PosX, PosY);
            cr.ShowText(total_item.ToString());

            PosX = PosX+100;
            cr.MoveTo(PosX, PosY);
            cr.ShowText(total_price.ToString());

            PosY = PosY+lineSpace;
            PosX = startX+150;
            cr.MoveTo(PosX, PosY);
            cr.ShowText("Tax  ");

            PosX = startX+300;
            cr.MoveTo(PosX, PosY);
            cr.ShowText( tax_amount.ToString() );

            //draw line
            PosY = PosY+3;
            PosX = startX+280;
            cr.MoveTo(PosX, PosY);
            cr.LineTo(PosX+50, PosY);
            cr.Stroke();

            cr.SelectFontFace("Sans", FontSlant.Normal, FontWeight.Bold);
            PosY = PosY+lineSpace;
            PosX = startX+150;
            cr.MoveTo(PosX, PosY);
            cr.ShowText("Total  ");

            PosX = startX+300;
            cr.MoveTo(PosX, PosY);
            cr.ShowText( (total_price+tax_amount).ToString() );

            cr.SelectFontFace("Sans", FontSlant.Normal, FontWeight.Normal);
            PosY = PosY+lineSpace;
            PosX = startX+150;
            cr.MoveTo(PosX, PosY);
            cr.ShowText("Payment  ");

            PosX = startX+300;
            cr.MoveTo(PosX, PosY);
            cr.ShowText( payment_amount.ToString() );

            PosY = PosY+lineSpace;
            PosX = startX+150;
            cr.MoveTo(PosX, PosY);
            cr.ShowText("Change  ");

            PosX = startX+300;
            cr.MoveTo(PosX, PosY);
            cr.ShowText( (payment_amount-total_price+tax_amount).ToString() );
            

            //cr.SetLineWidth(1);
            cr.SetSourceRGB(0, 0, 0);

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