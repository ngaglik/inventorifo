using System;
using System.Collections.Generic;
using Gtk;
using Gdk;
using System.Data;
using Pango;
using UI = Gtk.Builder.ObjectAttribute;

namespace Inventorifo.App
{
    //[Section(ContentType = typeof(EditableCellsSection), Category = Category.Widgets)]
    class TransactionPurchase : Gtk.Box
    {
        Inventorifo.Lib.LibDb DbCl = new Inventorifo.Lib.LibDb ();
        Inventorifo.Lib.LibGui GuiCl = new Inventorifo.Lib.LibGui ();
        public object parent;
        public string prm;
        public TransactionPurchase(object parent, string prm) : this(new Builder("TransactionPurchase.glade")) { 
            this.parent=parent;
            this.prm = prm;
        }
        private TreeView _treeView;
        private ListStore _transModel;
        private TreeView _treeViewItems;
        private ListStore _itemsModel;
        //private ListStore numbers_model;
        private Dictionary<CellRenderer, int> _cellColumnsRender;
        private List<Item> _articles;

        private Entry entSearch;
        private Entry entBarcode;
        private Popover popoverSupplier ;
        private Popover popoverProduct ;

        public TransactionPurchase(Builder builder) : base(builder.GetRawOwnedObject("TransactionPurchase"))
        {
            builder.Autoconnect(this);

            Label lbTitle = (Label)builder.GetObject("LbTitle");
            lbTitle.Text = "Purchase";
            lbTitle.ModifyFont(FontDescription.FromString("Arial 18"));

            Box  boxMiddle = (Box)builder.GetObject("BoxMiddle");
            boxMiddle.SetSizeRequest(-1, -1); // Allow dynamic resizing
            boxMiddle.Expand = true;

            Button  btnProduct = (Button)builder.GetObject("BtnProduct");
            popoverProduct = new Popover(btnProduct);    
            btnProduct.Clicked += AddProduct;

            Button  btnSupplier = (Button)builder.GetObject("BtnSupplier");
            popoverSupplier = new Popover(btnSupplier);    
            btnSupplier.Clicked += AddSupplier;

            Button  btnAdd = (Button)builder.GetObject("BtnAdd");
            btnAdd.Clicked += AddItem;
            
        }

        private class Item
        { //
            public Item(string id, string name){
                this.id = id;
                this.name = name;
            }
            public string id;
            public string name;
        }

        private enum ColumnItem
        { //
            id,
            name,
            Num
        };

        private enum ColumnNumber
        {
            Text,
            Num
        };
        
        private void HandleEntSearchChanged(object sender, EventArgs e)
        {
            CreateItemsModel(false,entSearch.Text.Trim());
        }
        private void CreateItemsModel(Boolean showAll,string strfind)
        {      
            if(strfind=="" && !showAll) {          
                _treeView.Model = null;
            }else{
                //ListStore model;
                _transModel = null;
                TreeIter iter;
                /* create array */
                _articles = new List<Item>();

                string whrfind = "";
                if(strfind!="") whrfind = "and upper(prodgr.name) like upper('" + strfind + "%')  ";

                string sql = "SELECT prodgr.id , prodgr.name "+
                        "FROM product_group prodgr "+
                        "WHERE 1=1 "+ whrfind +
                        "ORDER by prodgr.name asc";
                        Console.WriteLine(sql);
              
                DataTable dttv = DbCl.fillDataTable(DbCl.getConn(), sql);
                foreach (DataRow dr in dttv.Rows)
                {                    
                    string id=dr[0].ToString();
                    string name=dr[1].ToString();
                                    
                    _articles.Add(new Item(id, name));
                }

                /* create list store */
                //
                _transModel = new ListStore(typeof(string), typeof(string));

                /* add items */
                for (int i = 0; i < _articles.Count; i++)
                {
                    iter = _transModel.Append();
                    _transModel.SetValue(iter, (int)ColumnItem.id, _articles[i].id);
                    _transModel.SetValue(iter, (int)ColumnItem.name, _articles[i].name);
                }
                _treeView.Model = _transModel;           

            }
        }

        private static ListStore CreateNumbersModel()
        {
            ListStore model;
            TreeIter iter;

            /* create list store */
            model = new ListStore(typeof(string), typeof(int));

            /* add numbers */
            for (int i = 0; i < 10; i++)
            {
                iter = model.Append();
                model.SetValue(iter, (int)ColumnNumber.Text, i.ToString());
            }
            return model;
        }

       
        private void AddColumns()
        {
            CellRendererText rendererText = new CellRendererText();
            _cellColumnsRender.Add(rendererText, (int)ColumnItem.id);
            _treeView.InsertColumn(-1, "ID", rendererText, "text", (int)ColumnItem.id);

            rendererText = new CellRendererText
            {
                Editable = true
            };
            rendererText.Foreground = "green";
            rendererText.Edited += CellEdited;
            _cellColumnsRender.Add(rendererText, (int)ColumnItem.name);
            _treeView.InsertColumn(-1, "Name", rendererText, "text", (int)ColumnItem.name);            

        }

        private void AddItem(object sender, EventArgs e)
        {

            string sql = "insert into transaction (transaction_type) values ('') ";
            Console.WriteLine (sql);
            DbCl.ExecuteTrans(DbCl.getConn(), sql);
            CreateItemsModel(true,entSearch.Text.Trim());
        }

        private void RemoveItem(object sender, EventArgs e)
        {
            
        }

        private void CellEdited(object data, EditedArgs args)
        {
           TreePath path = new TreePath(args.Path);
            int column = _cellColumnsRender[(CellRenderer)data];
            _transModel.GetIter(out TreeIter iter, path);

            switch (column)
            {
                case (int)ColumnItem.name:
                    {
                        int i = path.Indices[0];
                        _articles[i].name = args.NewText;
                        _transModel.SetValue(iter, column, _articles[i].name);
                        string sql = "update product_group set name = '"+args.NewText+"' where id='"+_articles[i].id+"' ";
                        Console.WriteLine (sql);
                        DbCl.ExecuteTrans(DbCl.getConn(), sql);
                    }
                    break;

            }
        }

        public void doChildProduct(object o,string prm){
            GLib.Timeout.Add(0, () =>
            {
                GuiCl.RemoveAllWidgets(popoverProduct);
                Label popLabel = new Label((string)o);
                popoverProduct.Add(popLabel);
                popoverProduct.SetSizeRequest(200, 20);
                popLabel.Show();
                Console.WriteLine("Your'e select "+prm);
                return false;
            });
        }
        
        private void AddProduct(object sender, EventArgs e)
        {
            //GuiCl.RemoveAllWidgets(popover);        
            //                  
            GuiCl.RemoveAllWidgets(popoverProduct);        
            ReferenceProduct refWidget = new ReferenceProduct(this,"dialog","purchase");
            popoverProduct.Add(refWidget);
            popoverProduct.SetSizeRequest(800, 300);
            refWidget.Show();          
            popoverProduct.ShowAll();
        }

        public void doChildSupplier(object o,string prm){
            GLib.Timeout.Add(0, () =>
            {
                GuiCl.RemoveAllWidgets(popoverSupplier);
                Label popLabel = new Label((string)o);
                popoverSupplier.Add(popLabel);
                popoverSupplier.SetSizeRequest(200, 20);
                popLabel.Show();
                string sql = "update transaction set supplier_id='"+prm+"' where id= ";
                Console.WriteLine (sql);
                DbCl.ExecuteTrans(DbCl.getConn(), sql);
                
                return false;
            });
           // CreateItemsModel(true,entSearch.Text.Trim());
        }
        
        private void AddSupplier(object sender, EventArgs e)
        {
            //GuiCl.RemoveAllWidgets(popover);        
            //                  
            GuiCl.RemoveAllWidgets(popoverSupplier);        
            ReferenceSupplier refWidget = new ReferenceSupplier(this,"dialog","purchase");
            popoverSupplier.Add(refWidget);
            popoverSupplier.SetSizeRequest(800, 300);
            refWidget.Show();          
            popoverSupplier.ShowAll();
        }

        private void EditingStarted(object o, EditingStartedArgs args)
        {
           //((ComboBox)args.Editable).RowSeparatorFunc += SeparatorRow;
        }

        private bool SeparatorRow(ITreeModel model, TreeIter iter)
        {
            TreePath path = model.GetPath(iter);
            int idx = path.Indices[0];

            return idx == 5;
        }
    }
}