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
        int prm;
        string mode;
        private Boolean showAll;
        ComboBoxText CmbIsActive;

        public ReferenceProduct(object parent, string mode, int prm) : base(Orientation.Vertical, 3)
        {
            this.parent=parent;
            this.prm = prm;
            this.mode = mode;

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

            CmbIsActive = new ComboBoxText();
            _lsModelIsActive = new ListStore(typeof(string));
            // Tambahkan beberapa item ke ListStore
            _lsModelIsActive.AppendValues("Show All");
            _lsModelIsActive.AppendValues("Active");
            _lsModelIsActive.AppendValues("Inactive");
            CmbIsActive.Model = _lsModelIsActive;
            Gtk.CellRendererText cellRendererText = new Gtk.CellRendererText();
            CmbIsActive.PackStart(cellRendererText, true);

            hbox.PackStart(CmbIsActive, true, true, 0);  
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
                        if(prm==1) CmbIsActive.Active = 0;
                        else CmbIsActive.Active = 1;
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
                        CmbIsActive.Active = 0;
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
            store_quantity,
            global_quantity,
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
            CreateItemsModel(this.showAll,entSearch.Text.Trim(),"");
        }
        private void HandleEntBarcodeChanged(object sender, EventArgs e)
        {
            CreateItemsModel(this.showAll,"",entBarcode.Text.Trim());
        }
        private void CreateItemsModel(Boolean showAll,string strfind,string strbarcode)
        {      
            if((strfind=="" && strbarcode=="") && !this.showAll) {          
                _treeView.Model = null;
            }else{
                _lsModelProduct = null;
                TreeIter iter;
                /* create array */
                _clsProduct = new List<clsProduct>();
                clsProduct prod;
                DataTable dttv = CoreCl.fillDtProduct(CmbIsActive.ActiveText, "",strbarcode,strfind,prm.ToString());
                foreach (DataRow dr in dttv.Rows)
                {            
                    prod = new clsProduct{        
                        id=dr["id"].ToString(),
                        short_name=dr["short_name"].ToString(),
                        product_name= dr["product_name"].ToString(),
                        store_quantity=dr["store_quantity"].ToString(),
                        global_quantity=dr["global_quantity"].ToString(),
                        barcode=dr["barcode"].ToString(),
                        product_group= dr["product_group"].ToString(),
                        product_group_name= dr["product_group_name"].ToString(),
                    } ;                                 
                    _clsProduct.Add(prod);      
                }

                /* create list store */
                //
                _lsModelProduct = new ListStore(typeof(string), typeof(string),typeof(string), typeof(string), typeof(string), typeof(string),  typeof(string),  typeof(string));

                /* add items */
                for (int i = 0; i < _clsProduct.Count; i++)
                {
                    iter = _lsModelProduct.Append();
                    _lsModelProduct.SetValue(iter, (int)ColumnProduct.id, _clsProduct[i].id);
                    _lsModelProduct.SetValue(iter, (int)ColumnProduct.short_name, _clsProduct[i].short_name);
                    _lsModelProduct.SetValue(iter, (int)ColumnProduct.product_name, _clsProduct[i].product_name);
                    _lsModelProduct.SetValue(iter, (int)ColumnProduct.store_quantity, _clsProduct[i].store_quantity);
                    _lsModelProduct.SetValue(iter, (int)ColumnProduct.global_quantity, _clsProduct[i].global_quantity);
                    _lsModelProduct.SetValue(iter, (int)ColumnProduct.barcode, _clsProduct[i].barcode);
                    _lsModelProduct.SetValue(iter, (int)ColumnProduct.product_group, _clsProduct[i].product_group);
                    _lsModelProduct.SetValue(iter, (int)ColumnProduct.product_group_name, _clsProduct[i].product_group_name);
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

            
            rendererText = new CellRendererText(); //3
            _cellColumnsRender.Add(rendererText, (int)ColumnProduct.store_quantity);
            _treeView.InsertColumn(-1, "Store quantity", rendererText, "text", (int)ColumnProduct.store_quantity);


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
                    if(prm==1){
                        TransactionPurchase o = (TransactionPurchase)this.parent;                    
                        o.doChildProduct("Product "+ _lsModelProduct.GetValue (iter, 1).ToString() +" selected",_lsModelProduct.GetValue (iter, 0).ToString());
                    }else if(prm==2){
                        TransactionSale o = (TransactionSale)this.parent;                    
                        o.doChildProduct("Product "+ _lsModelProduct.GetValue (iter, 1).ToString() +" selected",_lsModelProduct.GetValue (iter, 0).ToString());
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
                    if(prm==1){
                        TransactionPurchase o = (TransactionPurchase)this.parent;                    
                        o.doChildProduct("Product "+ _lsModelProduct.GetValue (iter, 1).ToString() +" selected",_lsModelProduct.GetValue (iter, 0).ToString());
                    }else if(prm==2){
                        TransactionSale o = (TransactionSale)this.parent;                    
                        o.doChildProduct("Product "+ _lsModelProduct.GetValue (iter, 1).ToString() +" selected",_lsModelProduct.GetValue (iter, 0).ToString());
                    }
                }
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