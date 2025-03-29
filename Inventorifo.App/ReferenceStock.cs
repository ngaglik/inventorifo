using System;
using System.Collections.Generic;
using Gtk;
using System.Data;
using Pango;

namespace Inventorifo.App
{
    //[Section(ContentType = typeof(EditableCellsSection), Category = Category.Widgets)]
    class ReferenceStock : Gtk.Box
    {
        Inventorifo.Lib.LibDb DbCl = new Inventorifo.Lib.LibDb ();
        Inventorifo.Lib.LibGui GuiCl = new Inventorifo.Lib.LibGui ();
        Inventorifo.Lib.LibCore CoreCl = new Inventorifo.Lib.LibCore ();
        
        private TreeView _treeView;
        private ListStore _itemsModel;
        //private ListStore numbers_model;
        private Dictionary<CellRenderer, int> _cellColumnsRender;
        private List<clStock> _clsStock;

        private Entry entSearch;
        private Entry entBarcode;

        public ReferenceStock(Window parent, string prm) : base(Orientation.Vertical, 3)
        {
            Label lbTitle = new Label();
            lbTitle.Text = "Stock";
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
            // _treeView.Columns[5].Visible = false; //unit_id
            // _treeView.Columns[10].Visible = false; //product_group_id
            // _treeView.Columns[12].Visible = false; //stock_id
            // _treeView.Columns[13].Visible = false; //price_id

            sw.Add(_treeView);

            /* some buttons */
            Box hbox = new Box(Orientation.Horizontal, 4)
            {
                Homogeneous = true
            };
            this.PackStart(hbox, false, false, 0);

            entBarcode = new Entry();
            entBarcode.PlaceholderText = "Barcode";
            hbox.PackStart(entBarcode, true, true, 0);

            entSearch = new Entry();
            entSearch.PlaceholderText = "Search";
            hbox.PackStart(entSearch, true, true, 0);

            Button button = new Button("Add item");
            button.Clicked += AddItem;
            hbox.PackStart(button, true, true, 0);

            entSearch.Changed += HandleEntSearchChanged;
            entBarcode.Changed += HandleEntBarcodeChanged;
            //CreateItemsModel("","");
        }
        
        private enum ColumnItem
        { 
            id,
            product_id,
            short_name,
            product_name,
            barcode,
            quantity,
            unit,
            unit_name,
            expired_date,
            price_id,
            purchase_price,
            price,
            product_group_id,
            product_group_name,
            location,
            location_name,
            location_group,
            location_group_name,
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
                _clsStock = new List<clStock>();

                string whrfind = "";
                if(strfind!="") whrfind = "and (upper(prod.name) like upper('" + strfind + "%') or upper(prod.short_name) like upper('" + strfind + "%')) ";
                string whrbarcode = "";
                if(strbarcode!="") whrbarcode = "and prod.barcode =  '" + strbarcode + "' ";
                        
                string sql = "SELECT stock.id, prod.id product_id, prod.short_name, prod.name product_name, prod.barcode, "+
                        "stock.quantity, stock.unit, unit.name unit_name, TO_CHAR(stock.expired_date, 'yyyy-mm-dd') expired_date, "+
                        "price.id price_id, price.purchase_price, price.price, "+
                        "prodgr.id product_group_id, prodgr.name product_group_name, "+
                        "stock.location, loc.name location_name, "+
                        "loc.location_group, locgr.name location_group_name "+
                        "FROM product prod "+
                        "LEFT OUTER JOIN stock on prod.id = stock.product_id "+
                        "LEFT OUTER JOIN location loc on stock.location = loc.id "+
                        "LEFT OUTER JOIN location_group locgr on loc.location_group = locgr.id "+
                        "LEFT OUTER JOIN price on price.id = stock.price_id "+
                        "left outer join unit on stock.unit = unit.id, product_group prodgr "+
                        "WHERE stock.state=0 and prod.product_group = prodgr.id "+ whrfind +whrbarcode +
                        "ORDER by prod.name asc, stock.id asc ";
                        Console.WriteLine(sql);
                clStock sto;
                DataTable dttv = DbCl.fillDataTable(DbCl.getConn(), sql);
                foreach (DataRow dr in dttv.Rows)
                {   
                    sto = new clStock{ 
                        id=dr["id"].ToString(),
                        product_id=dr["product_id"].ToString(),
                        short_name=dr["short_name"].ToString(),
                        product_name=dr["product_name"].ToString(),
                        barcode=dr["barcode"].ToString(),
                        quantity=dr["quantity"].ToString(),
                        unit=dr["unit"].ToString(),
                        unit_name=dr["unit_name"].ToString(),
                        expired_date=dr["expired_date"].ToString(),
                        price_id=dr["price_id"].ToString(),
                        purchase_price=dr["purchase_price"].ToString(),
                        price=dr["price"].ToString(),
                        product_group_id=dr["product_group_id"].ToString(),
                        product_group_name=dr["product_group_name"].ToString(),
                        location=dr["location"].ToString(),
                        location_name=dr["location_name"].ToString(),
                        location_group=dr["location_group"].ToString(),
                        location_group_name=dr["location_group_name"].ToString(),
                    };
                    _clsStock.Add(sto);   
                }

                /* create list store */
                //
                _itemsModel = new ListStore(typeof(string), typeof(string), typeof(string), typeof(string), typeof(string), typeof(string), typeof(string), typeof(string),  typeof(string), typeof(string), typeof(string), typeof(string), typeof(string), typeof(string), typeof(string), typeof(string), typeof(string), typeof(string) );

                /* add items */
                for (int i = 0; i < _clsStock.Count; i++)
                {
                    iter = _itemsModel.Append();
                    _itemsModel.SetValue(iter, (int)ColumnItem.id, _clsStock[i].id);
                    _itemsModel.SetValue(iter, (int)ColumnItem.product_id, _clsStock[i].product_id);
                    _itemsModel.SetValue(iter, (int)ColumnItem.short_name, _clsStock[i].short_name);
                    _itemsModel.SetValue(iter, (int)ColumnItem.product_name, _clsStock[i].product_name);
                    _itemsModel.SetValue(iter, (int)ColumnItem.barcode, _clsStock[i].barcode);
                    _itemsModel.SetValue(iter, (int)ColumnItem.quantity, _clsStock[i].quantity);
                    _itemsModel.SetValue(iter, (int)ColumnItem.unit, _clsStock[i].unit);
                    _itemsModel.SetValue(iter, (int)ColumnItem.unit_name, _clsStock[i].unit_name);
                    _itemsModel.SetValue(iter, (int)ColumnItem.expired_date, _clsStock[i].expired_date);
                    _itemsModel.SetValue(iter, (int)ColumnItem.price_id, _clsStock[i].price_id);
                    _itemsModel.SetValue(iter, (int)ColumnItem.purchase_price, _clsStock[i].purchase_price);
                    _itemsModel.SetValue(iter, (int)ColumnItem.price, _clsStock[i].price);
                    _itemsModel.SetValue(iter, (int)ColumnItem.product_group_id, _clsStock[i].product_group_id);
                    _itemsModel.SetValue(iter, (int)ColumnItem.product_group_name, _clsStock[i].product_group_name);
                    _itemsModel.SetValue(iter, (int)ColumnItem.location, _clsStock[i].location);
                    _itemsModel.SetValue(iter, (int)ColumnItem.location_name, _clsStock[i].location_name);
                    _itemsModel.SetValue(iter, (int)ColumnItem.location_group, _clsStock[i].location_group);
                    _itemsModel.SetValue(iter, (int)ColumnItem.location_group_name, _clsStock[i].location_group_name);
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
            _treeView.InsertColumn(-1, "Stock id", rendererText, "text", (int)ColumnItem.id);

            rendererText = new CellRendererText();
            _cellColumnsRender.Add(rendererText, (int)ColumnItem.product_id);
            _treeView.InsertColumn(-1, "Product ID", rendererText, "text", (int)ColumnItem.product_id);

            rendererText = new CellRendererText();
            _cellColumnsRender.Add(rendererText, (int)ColumnItem.short_name);
            _treeView.InsertColumn(-1, "Short name", rendererText, "text", (int)ColumnItem.short_name);

            rendererText = new CellRendererText();
            _cellColumnsRender.Add(rendererText, (int)ColumnItem.product_name);
            _treeView.InsertColumn(-1, "Product name", rendererText, "text", (int)ColumnItem.product_name);

            rendererText = new CellRendererText();
            _cellColumnsRender.Add(rendererText, (int)ColumnItem.barcode);
            _treeView.InsertColumn(-1, "Barcode", rendererText, "text", (int)ColumnItem.barcode);

            rendererText = new CellRendererText();
            _cellColumnsRender.Add(rendererText, (int)ColumnItem.quantity);
            _treeView.InsertColumn(-1, "Quantity", rendererText, "text", (int)ColumnItem.quantity);

            rendererText = new CellRendererText();
            _cellColumnsRender.Add(rendererText, (int)ColumnItem.unit);
            _treeView.InsertColumn(-1, "Unit id", rendererText, "text", (int)ColumnItem.unit);

            rendererText = new CellRendererText();
            _cellColumnsRender.Add(rendererText, (int)ColumnItem.unit_name);
            _treeView.InsertColumn(-1, "Unit", rendererText, "text", (int)ColumnItem.unit_name);

            rendererText = new CellRendererText();
            _cellColumnsRender.Add(rendererText, (int)ColumnItem.price_id);
            _treeView.InsertColumn(-1, "Price id", rendererText, "text", (int)ColumnItem.price_id);

            rendererText = new CellRendererText();
            _cellColumnsRender.Add(rendererText, (int)ColumnItem.purchase_price);
            _treeView.InsertColumn(-1, "Purchase Price", rendererText, "text", (int)ColumnItem.purchase_price);

            rendererText = new CellRendererText();
            _cellColumnsRender.Add(rendererText, (int)ColumnItem.price);
            _treeView.InsertColumn(-1, "Price", rendererText, "text", (int)ColumnItem.price);

            rendererText = new CellRendererText
            {
                Editable = true
            };
            rendererText.Foreground = "green";
            rendererText.Edited += CellEdited;
            _cellColumnsRender.Add(rendererText, (int)ColumnItem.expired_date);
            _treeView.InsertColumn(-1, "Expired date", rendererText, "text", (int)ColumnItem.expired_date);

            rendererText = new CellRendererText();
            _cellColumnsRender.Add(rendererText, (int)ColumnItem.product_group_id);
            _treeView.InsertColumn(-1, "Product group id", rendererText, "text", (int)ColumnItem.product_group_id);

            rendererText = new CellRendererText();
            _cellColumnsRender.Add(rendererText, (int)ColumnItem.product_group_name);
            _treeView.InsertColumn(-1, "Product group", rendererText, "text", (int)ColumnItem.product_group_name);

            rendererText = new CellRendererText();
            _cellColumnsRender.Add(rendererText, (int)ColumnItem.location);
            _treeView.InsertColumn(-1, "Product group id", rendererText, "text", (int)ColumnItem.location);

            rendererText = new CellRendererText();
            _cellColumnsRender.Add(rendererText, (int)ColumnItem.location_name);
            _treeView.InsertColumn(-1, "Product group", rendererText, "text", (int)ColumnItem.location_name);

            rendererText = new CellRendererText();
            _cellColumnsRender.Add(rendererText, (int)ColumnItem.location_group);
            _treeView.InsertColumn(-1, "Product group id", rendererText, "text", (int)ColumnItem.location_group);

            rendererText = new CellRendererText();
            _cellColumnsRender.Add(rendererText, (int)ColumnItem.location_group_name);
            _treeView.InsertColumn(-1, "Product group", rendererText, "text", (int)ColumnItem.location_group_name);

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