using System;
using System.Collections.Generic;
using Gtk;
using System.Data;
using Pango;
using Inventorifo.Lib.Model;

namespace Inventorifo.App
{
    //[Section(ContentType = typeof(EditableCellsSection), Category = Category.Widgets)]
    class WarehouseStockHistory : Gtk.Box
    {
        Inventorifo.Lib.LibDb DbCl = new Inventorifo.Lib.LibDb ();
        Inventorifo.Lib.LibGui GuiCl = new Inventorifo.Lib.LibGui ();
        Inventorifo.Lib.LibCore CoreCl = new Inventorifo.Lib.LibCore ();
        
        private TreeView _treeView;
        private ListStore _itemsModel;
        //private ListStore numbers_model;
        private Dictionary<CellRenderer, int> _cellColumnsRender;
        private List<clStockHistory> _clsStock;

        private Entry entSearch;
        private Entry entBarcode;

        private ComboBoxText cmbLocation;
        private ComboBoxText cmbCondition;


        public WarehouseStockHistory(Window parent, string prm) : base(Orientation.Vertical, 3)
        {
            Label lbTitle = new Label();
            lbTitle.Text = "Stock history";
            lbTitle.ModifyFont(FontDescription.FromString("Arial 18"));
            this.PackStart(lbTitle, false, true, 0);
            
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
            _treeView.Columns[0].Visible = false; //id
            _treeView.Columns[3].Visible = false; //type 
            _treeView.Columns[5].Visible = false; //product d
            _treeView.Columns[7].Visible = false; //product name
            _treeView.Columns[8].Visible = false; //barcode
            _treeView.Columns[12].Visible = false; //unit id
            _treeView.Columns[14].Visible = false; //product group id
            _treeView.Columns[16].Visible = false; //location id
            _treeView.Columns[18].Visible = false; //location  id
            _treeView.Columns[20].Visible = false; //cond  id
            sw.Add(_treeView);

            /* some buttons */
            Box hbox = new Box(Orientation.Horizontal, 4)
            {
                Homogeneous = true
            };
            this.PackStart(hbox, false, false, 0);

            cmbLocation = new ComboBoxText();
            GuiCl.FillComboBoxText(cmbLocation, "Select id,name from location order by id asc",0);
            hbox.PackStart(cmbLocation, true, true, 0);

            cmbCondition = new ComboBoxText();
            GuiCl.FillComboBoxText(cmbCondition, "Select id,name from condition order by id asc",0);
            hbox.PackStart(cmbCondition, true, true, 0);

            entBarcode = new Entry();
            entBarcode.PlaceholderText = "Barcode";
            hbox.PackStart(entBarcode, true, true, 0);

            entSearch = new Entry();
            entSearch.PlaceholderText = "Search";
            hbox.PackStart(entSearch, true, true, 0);

            // Button button = new Button("Add item");
            // button.Clicked += AddItem;
            // hbox.PackStart(button, true, true, 0);

            entSearch.Changed += HandleEntSearchChanged;
            entBarcode.Changed += HandleEntBarcodeChanged;
            //CreateItemsModel("","");
        }
        
        private enum ColumnItem
        { 
            id,
            transaction_id,
            input_date,
            transaction_type,
            transaction_type_name,
            product_id,
            short_name,
            product_name,
            barcode,
            quantity_before,
            quantity,
            quantity_after,
            unit,
            unit_name,            
            product_group_id,
            product_group_name,
            location,
            location_name,
            location_group,
            location_group_name,
            condition,
            condition_name,
            Num
        };

        private enum ColumnNumber
        {
            Text,
            Num
        };
        
        private void HandleEntSearchChanged(object sender, EventArgs e)
        {
            CreateItemsModel(entSearch.Text.Trim(),"");
        }
        private void HandleEntBarcodeChanged(object sender, EventArgs e)
        {
            CreateItemsModel("",entBarcode.Text.Trim());
        }
        private void CreateItemsModel(string strfind,string strbarcode)
        {      
            if(strfind=="" && strbarcode==""){          
                _treeView.Model = null;
            }else{
                _itemsModel = null;
                TreeIter iter;
                /* create array */
                _clsStock = new List<clStockHistory>();

                string whrfind = "";
                if(strfind!="") whrfind = "and (upper(prod.name) like upper('" + strfind + "%') or upper(prod.short_name) like upper('" + strfind + "%')) ";
                string whrbarcode = "";
                if(strbarcode!="") whrbarcode = "and prod.barcode =  '" + strbarcode + "' ";
                 
                string sql = "SELECT hist.id, hist.transaction_id, TO_CHAR(hist.input_date,'yyyy-MM-dd') input_date, hist.transaction_type, typ.name transaction_type_name, prod.id product_id, prod.short_name, prod.name product_name, prod.barcode, hist.quantity_before, hist.quantity, hist.quantity_after, hist.unit, unit.name unit_name, prodgr.id product_group_id, prodgr.name product_group_name, hist.location, loc.name location_name, loc.location_group, locgr.name location_group_name, hist.condition, cond.name condition_name "+
                "FROM transaction_type typ, product prod, product_group prodgr, stock_history hist, unit, location loc, location_group locgr, condition cond "+
                "WHERE hist.location="+cmbLocation.ActiveText+" and hist.condition="+cmbCondition.ActiveText+" "+
                "and typ.id=hist.transaction_type and loc.id=hist.location "+whrfind + whrbarcode +
                "and loc.location_group = locgr.id and hist.product_id = prod.id and prod.product_group = prodgr.id and unit.id=hist.unit and hist.condition = cond.id "+
                "ORDER by prod.id desc, hist.id desc ";
                        Console.WriteLine(sql);
                clStockHistory sto;
                DataTable dttv = DbCl.fillDataTable(DbCl.getConn(), sql);
                foreach (DataRow dr in dttv.Rows)
                {   
                    sto = new clStockHistory{ 
                        id=dr["id"].ToString(),
                        transaction_id=dr["transaction_id"].ToString(),
                        transaction_type=dr["transaction_type"].ToString(),
                        transaction_type_name=dr["transaction_type_name"].ToString(),
                        input_date=dr["input_date"].ToString(),
                        product_id=dr["product_id"].ToString(),
                        short_name=dr["short_name"].ToString(),
                        product_name=dr["product_name"].ToString(),
                        barcode=dr["barcode"].ToString(),
                        quantity_before=dr["quantity_before"].ToString(),
                        quantity=dr["quantity"].ToString(),
                        quantity_after=dr["quantity_after"].ToString(),
                        unit=dr["unit"].ToString(),
                        unit_name=dr["unit_name"].ToString(),                        
                        product_group_id=dr["product_group_id"].ToString(),
                        product_group_name=dr["product_group_name"].ToString(),
                        location=dr["location"].ToString(),
                        location_name=dr["location_name"].ToString(),
                        location_group=dr["location_group"].ToString(),
                        location_group_name=dr["location_group_name"].ToString(),
                        condition=dr["condition"].ToString(),
                        condition_name=dr["condition_name"].ToString(),
                    };
                    _clsStock.Add(sto);   
                }

                /* create list store */
                //
                _itemsModel = new ListStore(typeof(string),typeof(string),typeof(string),typeof(string),typeof(string),typeof(string),typeof(string),typeof(string),typeof(string),typeof(string),typeof(string),typeof(string),typeof(string),typeof(string),typeof(string),typeof(string),typeof(string),typeof(string),typeof(string),typeof(string),typeof(string),typeof(string));

                /* add items */
                for (int i = 0; i < _clsStock.Count; i++)
                {
                    iter = _itemsModel.Append();
                    _itemsModel.SetValue(iter, (int)ColumnItem.id, _clsStock[i].id);
                    _itemsModel.SetValue(iter, (int)ColumnItem.transaction_id, _clsStock[i].transaction_id);
                    _itemsModel.SetValue(iter, (int)ColumnItem.transaction_type, _clsStock[i].transaction_type);
                    _itemsModel.SetValue(iter, (int)ColumnItem.transaction_type_name, _clsStock[i].transaction_type_name);
                    _itemsModel.SetValue(iter, (int)ColumnItem.input_date, _clsStock[i].input_date);
                    _itemsModel.SetValue(iter, (int)ColumnItem.product_id, _clsStock[i].product_id);
                    _itemsModel.SetValue(iter, (int)ColumnItem.short_name, _clsStock[i].short_name);
                    _itemsModel.SetValue(iter, (int)ColumnItem.product_name, _clsStock[i].product_name);
                    _itemsModel.SetValue(iter, (int)ColumnItem.barcode, _clsStock[i].barcode);
                    _itemsModel.SetValue(iter, (int)ColumnItem.quantity_before, _clsStock[i].quantity_before);
                    _itemsModel.SetValue(iter, (int)ColumnItem.quantity, _clsStock[i].quantity);
                    _itemsModel.SetValue(iter, (int)ColumnItem.quantity_after, _clsStock[i].quantity_after);
                    _itemsModel.SetValue(iter, (int)ColumnItem.unit, _clsStock[i].unit);
                    _itemsModel.SetValue(iter, (int)ColumnItem.unit_name, _clsStock[i].unit_name);
                    _itemsModel.SetValue(iter, (int)ColumnItem.product_group_id, _clsStock[i].product_group_id);
                    _itemsModel.SetValue(iter, (int)ColumnItem.product_group_name, _clsStock[i].product_group_name);
                    _itemsModel.SetValue(iter, (int)ColumnItem.location, _clsStock[i].location);
                    _itemsModel.SetValue(iter, (int)ColumnItem.location_name, _clsStock[i].location_name);
                    _itemsModel.SetValue(iter, (int)ColumnItem.location_group, _clsStock[i].location_group);
                    _itemsModel.SetValue(iter, (int)ColumnItem.location_group_name, _clsStock[i].location_group_name);
                    _itemsModel.SetValue(iter, (int)ColumnItem.condition, _clsStock[i].condition);
                    _itemsModel.SetValue(iter, (int)ColumnItem.condition_name, _clsStock[i].condition_name);
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
            _cellColumnsRender.Add(rendererText, (int)ColumnItem.id); //0
            _treeView.InsertColumn(-1, "id", rendererText, "text", (int)ColumnItem.id);

            rendererText = new CellRendererText();
            _cellColumnsRender.Add(rendererText, (int)ColumnItem.input_date); //0
            _treeView.InsertColumn(-1, "Date", rendererText, "text", (int)ColumnItem.input_date);

            rendererText = new CellRendererText();
            _cellColumnsRender.Add(rendererText, (int)ColumnItem.transaction_id); //1
            _treeView.InsertColumn(-1, "Transaction ID", rendererText, "text", (int)ColumnItem.transaction_id);

            rendererText = new CellRendererText();
            _cellColumnsRender.Add(rendererText, (int)ColumnItem.transaction_type); //2
            _treeView.InsertColumn(-1, "Transaction type", rendererText, "text", (int)ColumnItem.transaction_type);

            rendererText = new CellRendererText();
            _cellColumnsRender.Add(rendererText, (int)ColumnItem.transaction_type_name); //3
            _treeView.InsertColumn(-1, "Transaction type name", rendererText, "text", (int)ColumnItem.transaction_type_name);

            rendererText = new CellRendererText();
            _cellColumnsRender.Add(rendererText, (int)ColumnItem.product_id); //4
            _treeView.InsertColumn(-1, "Product ID", rendererText, "text", (int)ColumnItem.product_id);

            rendererText = new CellRendererText();
            _cellColumnsRender.Add(rendererText, (int)ColumnItem.short_name); //5
            _treeView.InsertColumn(-1, "Short name", rendererText, "text", (int)ColumnItem.short_name);

            rendererText = new CellRendererText();
            _cellColumnsRender.Add(rendererText, (int)ColumnItem.product_name); //6
            _treeView.InsertColumn(-1, "Product name", rendererText, "text", (int)ColumnItem.product_name);

            rendererText = new CellRendererText();
            _cellColumnsRender.Add(rendererText, (int)ColumnItem.barcode); //7
            _treeView.InsertColumn(-1, "Barcode", rendererText, "text", (int)ColumnItem.barcode);

            rendererText = new CellRendererText();
            _cellColumnsRender.Add(rendererText, (int)ColumnItem.quantity_before); //8
            _treeView.InsertColumn(-1, "Quantity before", rendererText, "text", (int)ColumnItem.quantity_before);

            rendererText = new CellRendererText();
            _cellColumnsRender.Add(rendererText, (int)ColumnItem.quantity); //9
            _treeView.InsertColumn(-1, "Quantity", rendererText, "text", (int)ColumnItem.quantity);

            rendererText = new CellRendererText();
            _cellColumnsRender.Add(rendererText, (int)ColumnItem.quantity_after); //10
            _treeView.InsertColumn(-1, "Quantity after", rendererText, "text", (int)ColumnItem.quantity_after);

            rendererText = new CellRendererText();
            _cellColumnsRender.Add(rendererText, (int)ColumnItem.unit); //11
            _treeView.InsertColumn(-1, "Unit id", rendererText, "text", (int)ColumnItem.unit);

            rendererText = new CellRendererText();
            _cellColumnsRender.Add(rendererText, (int)ColumnItem.unit_name); //12
            _treeView.InsertColumn(-1, "Unit", rendererText, "text", (int)ColumnItem.unit_name);
            
            rendererText = new CellRendererText();
            _cellColumnsRender.Add(rendererText, (int)ColumnItem.product_group_id); //13
            _treeView.InsertColumn(-1, "Product group id", rendererText, "text", (int)ColumnItem.product_group_id);

            rendererText = new CellRendererText();
            _cellColumnsRender.Add(rendererText, (int)ColumnItem.product_group_name); //14
            _treeView.InsertColumn(-1, "Product group", rendererText, "text", (int)ColumnItem.product_group_name);

            rendererText = new CellRendererText();
            _cellColumnsRender.Add(rendererText, (int)ColumnItem.location); //15
            _treeView.InsertColumn(-1, "Location id", rendererText, "text", (int)ColumnItem.location);

            rendererText = new CellRendererText();
            _cellColumnsRender.Add(rendererText, (int)ColumnItem.location_name); //16
            _treeView.InsertColumn(-1, "Location", rendererText, "text", (int)ColumnItem.location_name);

            rendererText = new CellRendererText();
            _cellColumnsRender.Add(rendererText, (int)ColumnItem.location_group); //17
            _treeView.InsertColumn(-1, "location group id", rendererText, "text", (int)ColumnItem.location_group);

            rendererText = new CellRendererText();
            _cellColumnsRender.Add(rendererText, (int)ColumnItem.location_group_name); //18
            _treeView.InsertColumn(-1, "Location group", rendererText, "text", (int)ColumnItem.location_group_name);

            rendererText = new CellRendererText();
            _cellColumnsRender.Add(rendererText, (int)ColumnItem.condition); //119
            _treeView.InsertColumn(-1, "Condition id", rendererText, "text", (int)ColumnItem.condition);

            rendererText = new CellRendererText();
            _cellColumnsRender.Add(rendererText, (int)ColumnItem.condition_name); //20
            _treeView.InsertColumn(-1, "Condition", rendererText, "text", (int)ColumnItem.condition_name);
        }

        private void AddItem(object sender, EventArgs e)
        {
            
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
                        //_clsStock[i].short_name = int.Parse(args.NewText);
                        _clsStock[i].short_name = args.NewText;
                        _itemsModel.SetValue(iter, column, _clsStock[i].short_name);
                    }
                    break;

                case (int)ColumnItem.product_name:
                    {
                        string oldText = (string)_itemsModel.GetValue(iter, column);
                        int i = path.Indices[0];
                        _clsStock[i].product_name = args.NewText;

                        _itemsModel.SetValue(iter, column, _clsStock[i].product_name);
                    }
                    break;
                case (int)ColumnItem.barcode:
                    {
                        string oldText = (string)_itemsModel.GetValue(iter, column);
                        int i = path.Indices[0];
                        _clsStock[i].barcode = args.NewText;

                        _itemsModel.SetValue(iter, column, _clsStock[i].barcode);
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