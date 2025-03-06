using System;
using System.Collections.Generic;
using Gtk;
using Gdk;
using System.Data;
using Pango;

namespace Inventorifo.App
{
    //[Section(ContentType = typeof(EditableCellsSection), Category = Category.Widgets)]
    class ReferenceProduct : Gtk.Box
    {
        Inventorifo.Lib.LibDb DbCl = new Inventorifo.Lib.LibDb ();
        private TreeView _treeView;
        private ListStore _itemsModel;
        private Dictionary<CellRenderer, int> _cellColumnsRender;
        private List<Item> _articles;
        private Popover popover ;

        private Entry entSearch;
        private Entry entBarcode;

        public object parent;
        Boolean isEditable;
        string textForground;
        string prm;

        public ReferenceProduct(object parent, string mode, string prm) : base(Orientation.Vertical, 3)
        {
            this.parent=parent;
            this.prm = prm;

            Label lbTitle = new Label();
            lbTitle.Text = "Product";
            lbTitle.ModifyFont(FontDescription.FromString("Arial 18"));
            this.PackStart(lbTitle, false, true, 0);
            
             Box hbox = new Box(Orientation.Horizontal, 4)
            {
                Homogeneous = true
            };
            entSearch = new Entry();
            entSearch.PlaceholderText = "Search";
            entBarcode = new Entry();
            entBarcode.PlaceholderText = "Barcode";        
            hbox.PackStart(entBarcode, true, true, 0);   
            hbox.PackStart(entSearch, true, true, 0);    
           
             switch (mode)
            {
                case "dialog":
                    {
                        Button button = new Button("Select");
                        button.Clicked += SelectItem;
                        hbox.PackStart(button, true, true, 0);
                        isEditable = false;
                        textForground = "black";
                    } 
                    break;
                case "widget":
                    {
                        Button button = new Button("Add");
                        button.Clicked += AddItem;
                        hbox.PackStart(button, true, true, 0);
                        isEditable = true;
                        textForground = "green";
                        popover = new Popover(button);  
                    } 
                    break;
            }    
            _cellColumnsRender = new Dictionary<CellRenderer, int>();

            ScrolledWindow sw = new ScrolledWindow
            {
                ShadowType = ShadowType.EtchedIn
            };
            sw.SetPolicy(PolicyType.Automatic, PolicyType.Automatic);

            this.PackStart(sw, true, true, 0);

            /* create tree view */
            _treeView = new TreeView();
            _treeView.Selection.Mode = SelectionMode.Single;

            AddColumns();
            //_treeView.Columns[4].Visible = false;
            //_treeView.Columns[10].Visible = false;
            //_treeView.Columns[13].Visible = false;
            /* some buttons */
            
            
            CreateItemsModel(true,"","");
            sw.Add(_treeView);
            //_treeView.CanFocus = true;
            //_treeView.KeyPressEvent += HandleTreeViewKeyPressEvent;

            
                    
            this.PackStart(hbox, false, false, 0);

            entSearch.Changed += HandleEntSearchChanged;
            entBarcode.Changed += HandleEntBarcodeChanged;
            entBarcode.GrabFocus();
        }

        private class Item
        { //
            public Item(string id, string short_name, string product_name, string barcode, string product_group, string product_group_name){
                this.id = id;
                this.short_name = short_name;
                this.product_name = product_name;
                this.barcode = barcode;
                this.product_group = product_group;
                this.product_group_name = product_group_name;
            }
            public string id;
            public string short_name;
            public string product_name;
            public string barcode;
            public string product_group;
            public string product_group_name;
        }

        private enum ColumnItem
        { //
            id,
            short_name,
            product_name,
            barcode,           
            product_group,      
            product_group_name,
            Num
        };

        private enum ColumnNumber
        {
            Text,
            Num
        };
        

        private void HandleEntSearchChanged(object sender, EventArgs e)
        {
            CreateItemsModel(true,entSearch.Text.Trim(),"");
        }
        private void HandleEntBarcodeChanged(object sender, EventArgs e)
        {
            CreateItemsModel(false,"",entBarcode.Text.Trim());
        }
        private void CreateItemsModel(Boolean showAll,string strfind,string strbarcode)
        {      
            if((strfind=="" && strbarcode=="") && !showAll) {          
                _treeView.Model = null;
            }else{
                _itemsModel = null;
                TreeIter iter;
                /* create array */
                _articles = new List<Item>();

                string whrfind = "";
                if(strfind!="") whrfind = "and (upper(prod.name) like upper('" + strfind + "%') or upper(prod.short_name) like upper('" + strfind + "%')) ";
                string whrbarcode = "";
                if(strbarcode!="") whrbarcode = "and prod.barcode =  '" + strbarcode + "' ";
                        
                string sql = "SELECT prod.id, prod.short_name, prod.name prod_name, prod.barcode, prod.product_group, prodgr.name product_group_name "+
                        "FROM product prod, product_group prodgr "+
                        "WHERE prod.product_group = prodgr.id "+ whrfind + whrbarcode +
                        "ORDER by prod.name asc";
                        Console.WriteLine(sql);
              
                DataTable dttv = DbCl.fillDataTable(DbCl.getConn(), sql);
                foreach (DataRow dr in dttv.Rows)
                {                    
                    string id=dr[0].ToString();
                    string short_name=dr[1].ToString();
                    string product_name= dr[2].ToString();
                    string barcode=dr[3].ToString();
                    string product_group= dr[4].ToString();
                    string product_group_name= dr[5].ToString();
                                    
                    _articles.Add(new Item(id, short_name, product_name, barcode, product_group,product_group_name));
                }

                /* create list store */
                //
                _itemsModel = new ListStore(typeof(string), typeof(string), typeof(string), typeof(string),  typeof(string),  typeof(string));

                /* add items */
                for (int i = 0; i < _articles.Count; i++)
                {
                    iter = _itemsModel.Append();
                    _itemsModel.SetValue(iter, (int)ColumnItem.id, _articles[i].id);
                    _itemsModel.SetValue(iter, (int)ColumnItem.short_name, _articles[i].short_name);
                    _itemsModel.SetValue(iter, (int)ColumnItem.product_name, _articles[i].product_name);
                    _itemsModel.SetValue(iter, (int)ColumnItem.barcode, _articles[i].barcode);
                    _itemsModel.SetValue(iter, (int)ColumnItem.product_group, _articles[i].product_group);
                    _itemsModel.SetValue(iter, (int)ColumnItem.product_group_name, _articles[i].product_group_name);
                }
                _treeView.Model = _itemsModel;                
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
            _treeView.InsertColumn(-1, "Product ID", rendererText, "text", (int)ColumnItem.id);

            rendererText = new CellRendererText
            {
                Editable = isEditable
            };
            rendererText.Foreground = textForground;
            rendererText.Edited += CellEdited;
            _cellColumnsRender.Add(rendererText, (int)ColumnItem.short_name);
            _treeView.InsertColumn(-1, "Short name", rendererText, "text", (int)ColumnItem.short_name);

            rendererText = new CellRendererText
            {
                Editable = isEditable
            };
            rendererText.Foreground = textForground;
            rendererText.Edited += CellEdited;
            _cellColumnsRender.Add(rendererText, (int)ColumnItem.product_name);
            _treeView.InsertColumn(-1, "Product name", rendererText, "text", (int)ColumnItem.product_name);

            rendererText = new CellRendererText
            {
                Editable = isEditable
            };
            rendererText.Foreground = textForground;
            rendererText.Edited += CellEdited;
            _cellColumnsRender.Add(rendererText, (int)ColumnItem.barcode);
            _treeView.InsertColumn(-1, "Barcode", rendererText, "text", (int)ColumnItem.barcode);

            ListStore lstModelCombo = new ListStore(typeof(string), typeof(string));
            String sql = "Select id,name from product_group order by name asc";
            DataTable dt = DbCl.fillDataTable(DbCl.getConn(), sql);
            foreach (DataRow dr in dt.Rows)
            {
                lstModelCombo.AppendValues(dr[0].ToString(), dr[0].ToString() + ").  " + dr[1].ToString());
            }
            CellRendererCombo rendererCombo = new CellRendererCombo
            { 
                Model = lstModelCombo,
                TextColumn = 1,
                HasEntry = false,
                Editable = isEditable
            };
            rendererCombo.Foreground = textForground;
            rendererCombo.Edited += CellEdited;
            rendererCombo.EditingStarted += EditingStarted;           
            _cellColumnsRender.Add(rendererCombo, (int)ColumnItem.product_group_name);
            _treeView.InsertColumn(-1, "Product group", rendererCombo, "text", (int)ColumnItem.product_group_name);

        }
        private void SelectItem(object sender, EventArgs e)
        {
            TreeSelection selection = _treeView.Selection;
            TreeIter iter;
            if(selection.GetSelected( out iter)){
                Console.WriteLine("Selected Value:"+_itemsModel.GetValue (iter, 0).ToString()+_itemsModel.GetValue (iter, 1).ToString());
            }            
            TransactionPurchase o = (TransactionPurchase)this.parent;
            o.doChildProduct("Yeay! "+ _itemsModel.GetValue (iter, 1).ToString() +" selected",_itemsModel.GetValue (iter, 0).ToString());
        }
        private void HandleTreeViewKeyPressEvent(object sender, KeyPressEventArgs e)
        {
            if (e.Event.Key == Gdk.Key.Return || e.Event.Key == Gdk.Key.KP_Enter)  // Check if Enter key is pressed
            {
                TreeSelection selection = _treeView.Selection;
                TreeIter iter;
                if(selection.GetSelected( out iter)){
                    Console.WriteLine("Selected Value:"+_itemsModel.GetValue (iter, 0).ToString()+_itemsModel.GetValue (iter, 1).ToString());
                }            
                TransactionPurchase o = (TransactionPurchase)this.parent;
                o.doChildProduct("Yeay! "+ _itemsModel.GetValue (iter, 1).ToString() +" selected",_itemsModel.GetValue (iter, 0).ToString());
            }
        }
        private void AddItem(object sender, EventArgs e)
        {
            string sql = "insert into product (name,product_group) values ('',1) ";
            Console.WriteLine (sql);
            DbCl.ExecuteTrans(DbCl.getConn(), sql);
            CreateItemsModel(true,entSearch.Text.Trim(),"");
        }

        private void RemoveItem(object sender, EventArgs e)
        {
            
        }

        private void CellEdited(object data, EditedArgs args)
        {
           TreePath path = new TreePath(args.Path);
            int column = _cellColumnsRender[(CellRenderer)data];
            _itemsModel.GetIter(out TreeIter iter, path);

            switch (column)
            {
                case (int)ColumnItem.short_name:
                    {
                        int i = path.Indices[0];
                        //_articles[i].short_name = int.Parse(args.NewText);
                        _articles[i].short_name = args.NewText;
                        _itemsModel.SetValue(iter, column, _articles[i].short_name);
                        string sql = "update product set short_name = '"+args.NewText+"' where id='"+_articles[i].id+"' ";
                        Console.WriteLine (sql);
                        DbCl.ExecuteTrans(DbCl.getConn(), sql);
                    }
                    break;

                case (int)ColumnItem.product_name:
                    {
                        string oldText = (string)_itemsModel.GetValue(iter, column);
                        int i = path.Indices[0];
                        _articles[i].product_name = args.NewText;
                        _itemsModel.SetValue(iter, column, _articles[i].product_name);
                        string sql = "update product set name = '"+args.NewText+"' where id='"+_articles[i].id+"' ";
                        Console.WriteLine (sql);
                        DbCl.ExecuteTrans(DbCl.getConn(), sql);
                    }
                    break;
                case (int)ColumnItem.barcode:
                    {
                        string oldText = (string)_itemsModel.GetValue(iter, column);
                        int i = path.Indices[0];
                        _articles[i].barcode = args.NewText;
                        _itemsModel.SetValue(iter, column, _articles[i].barcode);
                        string sql = "update product set barcode = '"+args.NewText+"' where id='"+_articles[i].id+"' ";
                        Console.WriteLine (sql);
                        DbCl.ExecuteTrans(DbCl.getConn(), sql);
                    }
                    break;
                case (int)ColumnItem.product_group_name:
                    {
                        string oldText = (string)_itemsModel.GetValue(iter, column);
                        int i = path.Indices[0];                                              
                        if (args.NewText.Contains(")."))
                        {
                            String[] arr = args.NewText.Split(").");
                            _articles[i].product_group_name = arr[1].Trim();
                            _itemsModel.SetValue(iter, column, _articles[i].product_group_name );  
                            
                            string sql = "update product set product_group = '"+arr[0].Trim()+"' where id='"+_articles[i].id+"' ";
                            Console.WriteLine (sql);
                            DbCl.ExecuteTrans(DbCl.getConn(), sql);
                        }
                    }
                    break;
            }
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