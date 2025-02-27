using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using Gtk;
using Pango;
using UI = Gtk.Builder.ObjectAttribute;

namespace Inventorifo.App
{
    class ReferenceWindow : Gtk.Bin
    {
        Inventorifo.Lib.LibDb DbCl = new Inventorifo.Lib.LibDb();

        //[UI] private Label _label1 = null;
        //[UI] private Button _button1 = null;

        private int _counter;
        private TreeView treeViewData;
        private Gtk.ListStore lstItem;
        private ArrayList stocks;
        private Entry entFind;
        private Entry entBarcode;

        public ReferenceWindow(MainWindow parent,int jnstrans) : this(new Builder("ReferenceWindow.glade")) { }

        private ReferenceWindow(Builder builder) : base(builder.GetRawOwnedObject("ReferenceWindow"))
        {
            builder.Autoconnect(this);
            treeViewData = (TreeView)builder.GetObject("TreeViewData");
            entFind = (Entry)builder.GetObject("EntFind");
            entBarcode = (Entry)builder.GetObject("EntBarcode");
            Button BtnView = (Button)builder.GetObject("BtnView");
            BtnView.Clicked += BtnView_Clicked;
           

            //TreeVew product_id
            Gtk.CellRendererText product_id_cell = new Gtk.CellRendererText();
            Gtk.TreeViewColumn product_id_column = new Gtk.TreeViewColumn();
            product_id_column.Title = "product_id";
            product_id_column.PackStart(product_id_cell, true);


            product_id_column.SetCellDataFunc(product_id_cell, new Gtk.TreeCellDataFunc(RenderProductId));


            treeViewData.AppendColumn(product_id_column);

            
        }

        private void RenderProductId(Gtk.TreeViewColumn column, Gtk.CellRenderer cell, Gtk.ITreeModel model, Gtk.TreeIter iter)
        {
            Stock sto = (Stock)model.GetValue(iter, 0);
            (cell as Gtk.CellRendererText).Text = "Aaaaaaaaaaa";
        }

        public void populateTree(string strfind, string barcode)
        {
            string whrfind = "";
            Gtk.Application.Invoke(delegate
            {
                stocks = new ArrayList();
                if (strfind.Length > 0) {
                        whrfind = "and (upper(prod.name) like upper('" + strfind + "%') or upper(prod.name) like upper('" + strfind + "%')) " ;
                }else {
                        whrfind = "";
                }
                    
                string sql ="";
                if(barcode.Length>0){
                        sql = "SELECT prod.id product_id, prod.short_name,  prod.name prod_name, prod.barcode,stock.quantity,stock.unit, unit.name unit_name, stock.purchase_price, price.price, stock.expired_date,  prodgr.id product_group_id, prodgr.name product_group_name, stock.id stock_id, price.id price_id "+
                        "FROM product prod LEFT OUTER JOIN stock on prod.id = stock.product_id LEFT OUTER JOIN price on stock.id = price.stock_id left outer join unit on stock.unit = unit.id, product_group prodgr "+
                        "WHERE prod.product_group = prodgr.id "+
                        "and prod.barcode = '"+barcode+"' "+
                        "ORDER by prod.name asc";
                        Console.WriteLine(sql);
                }else{
                    sql = "SELECT prod.id product_id, prod.short_name, prod.name prod_name, prod.barcode,stock.quantity,stock.unit, unit.name unit_name, stock.purchase_price, price.price, stock.expired_date, prodgr.id product_group_id, prodgr.name product_group_name,stock.id stock_id, price.id price_id "+
                        "FROM product prod LEFT OUTER JOIN stock on prod.id = stock.product_id LEFT OUTER JOIN price on stock.id = price.stock_id left outer join unit on stock.unit = unit.id, product_group prodgr "+
                        "WHERE prod.product_group = prodgr.id "+ whrfind +
                        "ORDER by prod.name asc";
                        Console.WriteLine(sql);
                }
                DataTable dttv = DbCl.fillDataTable(DbCl.getConn(), sql);
                foreach (DataRow dr in dttv.Rows)
                {                    
                    double product_id=Convert.ToDouble(dr[0].ToString());
                    string short_name=dr[1].ToString();
                    string product_name= dr[2].ToString();
                    string barcode=dr[3].ToString();
                    double quantity=0;
                    if(dr[4].ToString()!=""){
                        quantity = Convert.ToDouble(dr[4].ToString());
                    }
                    int unit=0;
                    if(dr[5].ToString()!=""){
                        unit = Convert.ToInt32(dr[5].ToString());
                    }
                    string unit_name=dr[6].ToString();
                    double purchase_price = 0;
                    if( dr[7].ToString()!=""){
                        purchase_price=Convert.ToDouble( dr[7].ToString());
                    }
                    double price=0;
                    if( dr[8].ToString()!=""){
                        price=Convert.ToDouble( dr[8].ToString());
                    }
                    string expired_date="";
                    if(dr[9].ToString()!=""){
                        DateTime d = Convert.ToDateTime(dr[9].ToString());
                        expired_date = d.ToString("yyyy-MM-dd");
                    }    
                    int product_group_id=Convert.ToInt32( dr[10].ToString());
                    string product_group_name=dr[11].ToString();
                    double stock_id=0; 
                    if(dr[12].ToString()!=""){
                        stock_id=Convert.ToDouble( dr[12].ToString());
                    } 
                    double price_id=0;
                    if(dr[13].ToString()!=""){
                        price_id=Convert.ToDouble( dr[13].ToString());
                    } 
                                    
                    stocks.Add(new Stock(product_id,short_name ,product_name,barcode , quantity, unit , unit_name,purchase_price , price,expired_date, product_group_id, product_group_name, stock_id , price_id ));
                }
                lstItem = new Gtk.ListStore(typeof(Stock));
                foreach (Stock sto in stocks)
                {
                    lstItem.AppendValues(sto);
                    Console.WriteLine(sto.product_name);
                }
                treeViewData.Model = lstItem;
            });
        }

        private void BtnView_Clicked(object sender, EventArgs a)
        {
           populateTree(entFind.Text.Trim(),entBarcode.Text.Trim());
        }
    }
}
