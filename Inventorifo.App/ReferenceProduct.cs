using System;
using System.Collections.Generic;
using Gtk;
using Gdk;
using System.Data;
using Pango;
using Inventorifo.Lib.Model;

namespace Inventorifo.App
{
    //[Section(ContentType = typeof(EditableCellsSection), Category = Category.Widgets)]
    class ReferenceProduct : Gtk.Box
    {
        Inventorifo.Lib.LibDb DbCl = new Inventorifo.Lib.LibDb ();
        Inventorifo.Lib.LibGui GuiCl = new Inventorifo.Lib.LibGui ();
        Inventorifo.Lib.LibCore CoreCl = new Inventorifo.Lib.LibCore ();

        private TreeView _treeView;
        private ListStore _lsModelProduct;
        private ListStore _lsModelIsActive;
        private Dictionary<CellRenderer, int> _cellColumnsRender;
        private List<clsProduct> _clsProduct;
        private Popover popover ;

        private Entry entSearch;
        private Entry entBarcode;

        public object parent;
        Boolean isEditable;
        string textForground;
        int transaction_type;
        string mode;
        clStock filter;
        private Boolean showAll;
        ComboBoxText cmbIsActive;

        public ReferenceProduct(object parent, string mode, int transaction_type, clStock filter) : base(Orientation.Vertical, 3)
        {
            this.parent=parent;
            this.transaction_type = transaction_type;
            this.mode = mode;
            this.filter = filter;

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

            cmbIsActive = new ComboBoxText();
            GuiCl.FillComboBoxText(cmbIsActive, "Select id,name from product_status order by id asc",0);
            cmbIsActive.Changed += HandleCmbIsActiveChanged;

            hbox.PackStart(cmbIsActive, true, true, 0);  
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
                        showAll = false;
                        if(transaction_type==1) cmbIsActive.Active = 0;
                        else cmbIsActive.Active = 1;
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
                        showAll = true;
                        cmbIsActive.Active = 0;
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
            
            //Console.WriteLine("filter.is_active "+ filter.is_active);
            CreateItemsModel(true,filter);
            sw.Add(_treeView);
            //_treeView.CanFocus = true;
            _treeView.KeyPressEvent += HandleTreeViewKeyPressEvent;

            
                    
            this.PackStart(hbox, false, false, 0);

            entSearch.Changed += HandleEntSearchChanged;
            entBarcode.Changed += HandleEntBarcodeChanged;
            entBarcode.GrabFocus();
        }

        private enum ColumnProduct
        { //
            id,
            short_name,
            product_name,
            global_quantity,
            barcode,
            product_group,      
            product_group_name,
            price1,
            price2,
            price3,
            Num
        };

        private enum ColumnNumber
        {
            Text,
            Num
        };
        
        private void HandleCmbIsActiveChanged(object sender, EventArgs e)
        {
            Gtk.Application.Invoke(delegate
            {
                this.filter.is_active=cmbIsActive.ActiveText;
                CreateItemsModel(this.showAll, this.filter);
            });
        }
        private void HandleEntSearchChanged(object sender, EventArgs e)
        {
            Gtk.Application.Invoke(delegate
            {
                this.filter.short_name=entSearch.Text.Trim();
                CreateItemsModel(this.showAll, this.filter);
            });
        }
        private void HandleEntBarcodeChanged(object sender, EventArgs e)
        {
            Gtk.Application.Invoke(delegate
            {
                this.filter.barcode = entBarcode.Text.Trim();
                CreateItemsModel(this.showAll, this.filter);
            });
        }
        private void CreateItemsModel(Boolean showAll,clStock filter)
        {   
            
            //Console.WriteLine("filter.is_active = "+ this.filter.is_active);
            if((this.filter.short_name=="" && this.filter.barcode=="") && !this.showAll) {          
                _treeView.Model = null;
            }else{
                _lsModelProduct = null;
                TreeIter iter;
                /* create array */
                _clsProduct = new List<clsProduct>();
                clsProduct prod;
                DataTable dttv = CoreCl.fillDtProduct(transaction_type,this.filter);
                foreach (DataRow dr in dttv.Rows)
                {            
                    prod = new clsProduct{        
                        id=dr["id"].ToString(),
                        short_name=dr["short_name"].ToString(),
                        product_name= dr["product_name"].ToString(),
                        //store_quantity=dr["store_quantity"].ToString(),
                        global_quantity=dr["global_quantity"].ToString(),
                        barcode=dr["barcode"].ToString(),
                        product_group= dr["product_group"].ToString(),
                        product_group_name= dr["product_group_name"].ToString(),
                        price1=dr["price1"].ToString(),
                        price2=dr["price2"].ToString(),
                        price3=dr["price3"].ToString(),
                    } ;                                 
                    _clsProduct.Add(prod);      
                }

                /* create list store */
                //
                _lsModelProduct = new ListStore( typeof(string),typeof(string), typeof(string), typeof(string),typeof(string), typeof(string), typeof(string), typeof(string),  typeof(string),  typeof(string));

                /* add items */
                for (int i = 0; i < _clsProduct.Count; i++)
                {
                    iter = _lsModelProduct.Append();
                    _lsModelProduct.SetValue(iter, (int)ColumnProduct.id, _clsProduct[i].id);
                    _lsModelProduct.SetValue(iter, (int)ColumnProduct.short_name, _clsProduct[i].short_name);
                    _lsModelProduct.SetValue(iter, (int)ColumnProduct.product_name, _clsProduct[i].product_name);
                   // _lsModelProduct.SetValue(iter, (int)ColumnProduct.store_quantity, _clsProduct[i].store_quantity);
                    _lsModelProduct.SetValue(iter, (int)ColumnProduct.global_quantity, _clsProduct[i].global_quantity);
                    _lsModelProduct.SetValue(iter, (int)ColumnProduct.barcode, _clsProduct[i].barcode);
                    _lsModelProduct.SetValue(iter, (int)ColumnProduct.product_group, _clsProduct[i].product_group);
                    _lsModelProduct.SetValue(iter, (int)ColumnProduct.product_group_name, _clsProduct[i].product_group_name);
                    _lsModelProduct.SetValue(iter, (int)ColumnProduct.price1, _clsProduct[i].price1);
                    _lsModelProduct.SetValue(iter, (int)ColumnProduct.price2, _clsProduct[i].price2);
                    _lsModelProduct.SetValue(iter, (int)ColumnProduct.price3, _clsProduct[i].price3);
                }
                _treeView.Model = _lsModelProduct;                
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
            _cellColumnsRender.Add(rendererText, (int)ColumnProduct.id);
            _treeView.InsertColumn(-1, "Product ID", rendererText, "text", (int)ColumnProduct.id);

            rendererText = new CellRendererText
            {
                Editable = isEditable
            };
            rendererText.Foreground = textForground;
            rendererText.Edited += CellEdited;
            _cellColumnsRender.Add(rendererText, (int)ColumnProduct.short_name);
            _treeView.InsertColumn(-1, "Short name", rendererText, "text", (int)ColumnProduct.short_name);

            
            // rendererText = new CellRendererText(); //3
            // _cellColumnsRender.Add(rendererText, (int)ColumnProduct.store_quantity);
            // _treeView.InsertColumn(-1, "Store quantity", rendererText, "text", (int)ColumnProduct.store_quantity);


            rendererText = new CellRendererText(); //3
            _cellColumnsRender.Add(rendererText, (int)ColumnProduct.global_quantity);
            _treeView.InsertColumn(-1, "Global quantity", rendererText, "text", (int)ColumnProduct.global_quantity);

            rendererText = new CellRendererText
            {
                Editable = isEditable
            };
            rendererText.Foreground = textForground;
            rendererText.Edited += CellEdited;
            _cellColumnsRender.Add(rendererText, (int)ColumnProduct.barcode);
            _treeView.InsertColumn(-1, "Barcode", rendererText, "text", (int)ColumnProduct.barcode);

            rendererText = new CellRendererText
            {
                Editable = isEditable
            };
            rendererText.Foreground = textForground;
            rendererText.Edited += CellEdited;
            _cellColumnsRender.Add(rendererText, (int)ColumnProduct.price1);
            _treeView.InsertColumn(-1, "Price1", rendererText, "text", (int)ColumnProduct.price1);

            rendererText = new CellRendererText
            {
                Editable = isEditable
            };
            rendererText.Foreground = textForground;
            rendererText.Edited += CellEdited;
            _cellColumnsRender.Add(rendererText, (int)ColumnProduct.price2);
            _treeView.InsertColumn(-1, "Price2", rendererText, "text", (int)ColumnProduct.price2);

            rendererText = new CellRendererText
            {
                Editable = isEditable
            };
            rendererText.Foreground = textForground;
            rendererText.Edited += CellEdited;
            _cellColumnsRender.Add(rendererText, (int)ColumnProduct.price3);
            _treeView.InsertColumn(-1, "Price3", rendererText, "text", (int)ColumnProduct.price3);


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
            _cellColumnsRender.Add(rendererCombo, (int)ColumnProduct.product_group_name);
            _treeView.InsertColumn(-1, "Product group", rendererCombo, "text", (int)ColumnProduct.product_group_name);

            rendererText = new CellRendererText
            {
                Editable = isEditable
            };
            rendererText.Foreground = textForground;
            rendererText.Edited += CellEdited;
            _cellColumnsRender.Add(rendererText, (int)ColumnProduct.product_name);
            _treeView.InsertColumn(-1, "Product name", rendererText, "text", (int)ColumnProduct.product_name);

        }
        private void SelectItem(object sender, EventArgs e)
        {
            if(this.mode == "dialog"){
                TreeSelection selection = _treeView.Selection;
                TreeIter iter;
                if(selection.GetSelected( out iter)){
                    Console.WriteLine("Selected Value:"+_lsModelProduct.GetValue (iter, 0).ToString()+_lsModelProduct.GetValue (iter, 1).ToString());
                    if(transaction_type==1){
                        TransactionPurchase o = (TransactionPurchase)this.parent;                    
                        o.doChildProduct(null,new clsProduct{id=_lsModelProduct.GetValue (iter, 0).ToString(), short_name=_lsModelProduct.GetValue (iter, 1).ToString(), product_name=_lsModelProduct.GetValue (iter, 1).ToString()});
                    }else if(transaction_type==2){
                        TransactionSale o = (TransactionSale)this.parent;                    
                        o.doChildProduct(null,new clsProduct{id=_lsModelProduct.GetValue (iter, 0).ToString(), short_name=_lsModelProduct.GetValue (iter, 1).ToString(), product_name=_lsModelProduct.GetValue (iter, 1).ToString()});
                    }else if(transaction_type==20){
                        WarehouseTransfer o = (WarehouseTransfer)this.parent;                    
                        o.doChildProduct(null,new clsProduct{id=_lsModelProduct.GetValue (iter, 0).ToString(), short_name=_lsModelProduct.GetValue (iter, 1).ToString(), product_name=_lsModelProduct.GetValue (iter, 1).ToString()});
                    }
                    
                }            
            }
        }

        [GLib.ConnectBefore]
        private void HandleTreeViewKeyPressEvent(object sender, KeyPressEventArgs e)
        {
            if(this.mode == "dialog"){
                if (e.Event.Key == Gdk.Key.Return || e.Event.Key == Gdk.Key.KP_Enter)  // Check if Enter key is pressed
                {
                    TreeSelection selection = _treeView.Selection;
                    TreeIter iter;
                    if(selection.GetSelected( out iter)){
                        Console.WriteLine("Selected Value:"+_lsModelProduct.GetValue (iter, 0).ToString()+_lsModelProduct.GetValue (iter, 1).ToString());
                    }            
                    if(transaction_type==1){
                        TransactionPurchase o = (TransactionPurchase)this.parent;                    
                        o.doChildProduct(null,new clsProduct{id=_lsModelProduct.GetValue (iter, 0).ToString(), short_name=_lsModelProduct.GetValue (iter, 1).ToString(), product_name=_lsModelProduct.GetValue (iter, 1).ToString()});
                    }else if(transaction_type==2){
                        TransactionSale o = (TransactionSale)this.parent;                    
                        o.doChildProduct(null,new clsProduct{id=_lsModelProduct.GetValue (iter, 0).ToString(), short_name=_lsModelProduct.GetValue (iter, 1).ToString(), product_name=_lsModelProduct.GetValue (iter, 1).ToString()});
                    }else if(transaction_type==20){
                        WarehouseTransfer o = (WarehouseTransfer)this.parent;                    
                        o.doChildProduct(null,new clsProduct{id=_lsModelProduct.GetValue (iter, 0).ToString(), short_name=_lsModelProduct.GetValue (iter, 1).ToString(), product_name=_lsModelProduct.GetValue (iter, 1).ToString()});
                    }
                }
            }            
        }

        private void AddItem(object sender, EventArgs e)
        {
            string sql = "insert into product (name,product_group) values ('',1) ";
            Console.WriteLine (sql);
            DbCl.ExecuteTrans(DbCl.getConn(), sql);
            filter.short_name=entSearch.Text.Trim();
            CreateItemsModel(true, filter);
        }

        private void RemoveItem(object sender, EventArgs e)
        {
            
        }

        private void CellEdited(object data, EditedArgs args)
        {
           TreePath path = new TreePath(args.Path);
            int column = _cellColumnsRender[(CellRenderer)data];
            _lsModelProduct.GetIter(out TreeIter iter, path);

            switch (column)
            {
                case (int)ColumnProduct.short_name:
                    {
                        int i = path.Indices[0];
                        //_clsProduct[i].short_name = int.Parse(args.NewText);
                        _clsProduct[i].short_name = args.NewText;
                        _lsModelProduct.SetValue(iter, column, _clsProduct[i].short_name);
                        string sql = "update product set short_name = '"+args.NewText+"' where id='"+_clsProduct[i].id+"' ";
                        Console.WriteLine (sql);
                        DbCl.ExecuteTrans(DbCl.getConn(), sql);
                    }
                    break;

                case (int)ColumnProduct.product_name:
                    {
                        string oldText = (string)_lsModelProduct.GetValue(iter, column);
                        int i = path.Indices[0];
                        _clsProduct[i].product_name = args.NewText;
                        _lsModelProduct.SetValue(iter, column, _clsProduct[i].product_name);
                        string sql = "update product set name = '"+args.NewText+"' where id='"+_clsProduct[i].id+"' ";
                        Console.WriteLine (sql);
                        DbCl.ExecuteTrans(DbCl.getConn(), sql);
                    }
                    break;
                case (int)ColumnProduct.barcode:
                    {
                        string oldText = (string)_lsModelProduct.GetValue(iter, column);
                        int i = path.Indices[0];
                        _clsProduct[i].barcode = args.NewText;
                        _lsModelProduct.SetValue(iter, column, _clsProduct[i].barcode);
                        string sql = "update product set barcode = '"+args.NewText+"' where id='"+_clsProduct[i].id+"' ";
                        Console.WriteLine (sql);
                        DbCl.ExecuteTrans(DbCl.getConn(), sql);
                    }
                    break;
                case (int)ColumnProduct.price1:
                    {
                        string oldText = (string)_lsModelProduct.GetValue(iter, column);
                        int i = path.Indices[0];
                        _clsProduct[i].price1 = args.NewText;
                        _lsModelProduct.SetValue(iter, column, _clsProduct[i].price1);
                        string sql = "update product set price1 = '"+args.NewText+"' where id='"+_clsProduct[i].id+"' ";
                        Console.WriteLine (sql);
                        DbCl.ExecuteTrans(DbCl.getConn(), sql);
                    }
                    break;
                case (int)ColumnProduct.price2:
                    {
                        string oldText = (string)_lsModelProduct.GetValue(iter, column);
                        int i = path.Indices[0];
                        _clsProduct[i].price2 = args.NewText;
                        _lsModelProduct.SetValue(iter, column, _clsProduct[i].price2);
                        string sql = "update product set price2 = '"+args.NewText+"' where id='"+_clsProduct[i].id+"' ";
                        Console.WriteLine (sql);
                        DbCl.ExecuteTrans(DbCl.getConn(), sql);
                    }
                    break;
                case (int)ColumnProduct.price3:
                    {
                        string oldText = (string)_lsModelProduct.GetValue(iter, column);
                        int i = path.Indices[0];
                        _clsProduct[i].price3 = args.NewText;
                        _lsModelProduct.SetValue(iter, column, _clsProduct[i].price3);
                        string sql = "update product set price3 = '"+args.NewText+"' where id='"+_clsProduct[i].id+"' ";
                        Console.WriteLine (sql);
                        DbCl.ExecuteTrans(DbCl.getConn(), sql);
                    }
                    break;
                case (int)ColumnProduct.product_group_name:
                    {
                        string oldText = (string)_lsModelProduct.GetValue(iter, column);
                        int i = path.Indices[0];                                              
                        if (args.NewText.Contains(")."))
                        {
                            String[] arr = args.NewText.Split(").");
                            _clsProduct[i].product_group_name = arr[1].Trim();
                            _lsModelProduct.SetValue(iter, column, _clsProduct[i].product_group_name );  
                            
                            string sql = "update product set product_group = '"+arr[0].Trim()+"' where id='"+_clsProduct[i].id+"' ";
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