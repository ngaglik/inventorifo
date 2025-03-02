using System;
using System.Collections.Generic;
using Gtk;
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
        //private ListStore numbers_model;
        private Dictionary<CellRenderer, int> _cellColumnsRender;
        private List<Item> _articles;

        private Entry entSearch;
        private Entry entBarcode;

        public ReferenceProduct(Window parent, string prm) : base(Orientation.Vertical, 3)
        {
            Label lbTitle = new Label();
            lbTitle.Text = "Product";
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
            _treeView.Columns[4].Visible = false;
            //_treeView.Columns[10].Visible = false;
            //_treeView.Columns[13].Visible = false;

            CreateItemsModel(true,"","");
            sw.Add(_treeView);

            /* some buttons */
            Box hbox = new Box(Orientation.Horizontal, 4)
            {
                Homogeneous = true
            };
            this.PackStart(hbox, false, false, 0);

            entSearch = new Entry();
            entSearch.PlaceholderText = "Search";
            hbox.PackStart(entSearch, true, true, 0);

            entBarcode = new Entry();
            entBarcode.PlaceholderText = "Barcode";
            hbox.PackStart(entBarcode, true, true, 0);

            Button button = new Button("Add");
            button.Clicked += AddItem;
            hbox.PackStart(button, true, true, 0);

            entSearch.Changed += HandleEntSearchChanged;
            entBarcode.Changed += HandleEntBarcodeChanged;
        }

        private class Item
        { //
            public Item(string product_id, string short_name, string product_name, string barcode, string product_group_id, string product_group_name){
                this.product_id = product_id;
                this.short_name = short_name;
                this.product_name = product_name;
                this.barcode = barcode;
                this.product_group_id = product_group_id;
                this.product_group_name = product_group_name;
            }
            public string product_id;
            public string short_name;
            public string product_name;
            public string barcode;
            public string product_group_id;
            public string product_group_name;
        }

        private enum ColumnItem
        { //
            product_id,
            short_name,
            product_name,
            barcode,           
            product_group_id,
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
            CreateItemsModel(false,entSearch.Text.Trim(),"");
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
                        
                string sql = "SELECT prod.id product_id, prod.short_name, prod.name prod_name, prod.barcode, prodgr.id product_group_id, prodgr.name product_group_name "+
                        "FROM product prod, product_group prodgr "+
                        "WHERE prod.product_group = prodgr.id "+ whrfind + whrbarcode +
                        "ORDER by prod.name asc";
                        Console.WriteLine(sql);
              
                DataTable dttv = DbCl.fillDataTable(DbCl.getConn(), sql);
                foreach (DataRow dr in dttv.Rows)
                {                    
                    string product_id=dr[0].ToString();
                    string short_name=dr[1].ToString();
                    string product_name= dr[2].ToString();
                    string barcode=dr[3].ToString();
                    string product_group_id= dr[4].ToString();
                    string product_group_name=dr[5].ToString();
                                    
                    _articles.Add(new Item(product_id, short_name, product_name, barcode, product_group_id, product_group_name ));
                }

                /* create list store */
                //
                _itemsModel = new ListStore(typeof(string), typeof(string), typeof(string), typeof(string),  typeof(string), typeof(string) );

                /* add items */
                for (int i = 0; i < _articles.Count; i++)
                {
                    iter = _itemsModel.Append();
                    _itemsModel.SetValue(iter, (int)ColumnItem.product_id, _articles[i].product_id);
                    _itemsModel.SetValue(iter, (int)ColumnItem.short_name, _articles[i].short_name);
                    _itemsModel.SetValue(iter, (int)ColumnItem.product_name, _articles[i].product_name);
                    _itemsModel.SetValue(iter, (int)ColumnItem.barcode, _articles[i].barcode);
                    _itemsModel.SetValue(iter, (int)ColumnItem.product_group_id, _articles[i].product_group_id);
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
            _cellColumnsRender.Add(rendererText, (int)ColumnItem.product_id);
            _treeView.InsertColumn(-1, "Product ID", rendererText, "text", (int)ColumnItem.product_id);

            rendererText = new CellRendererText
            {
                Editable = true
            };
            rendererText.Foreground = "green";
            rendererText.Edited += CellEdited;
            _cellColumnsRender.Add(rendererText, (int)ColumnItem.short_name);
            _treeView.InsertColumn(-1, "Short name", rendererText, "text", (int)ColumnItem.short_name);

            rendererText = new CellRendererText
            {
                Editable = true
            };
            rendererText.Foreground = "green";
            rendererText.Edited += CellEdited;
            _cellColumnsRender.Add(rendererText, (int)ColumnItem.product_name);
            _treeView.InsertColumn(-1, "Product name", rendererText, "text", (int)ColumnItem.product_name);

            rendererText = new CellRendererText
            {
                Editable = true
            };
            rendererText.Foreground = "green";
            rendererText.Edited += CellEdited;
            _cellColumnsRender.Add(rendererText, (int)ColumnItem.barcode);
            _treeView.InsertColumn(-1, "Barcode", rendererText, "text", (int)ColumnItem.barcode);

            rendererText = new CellRendererText();
            _cellColumnsRender.Add(rendererText, (int)ColumnItem.product_group_id);
            _treeView.InsertColumn(-1, "Product group id", rendererText, "text", (int)ColumnItem.product_group_id);

            rendererText = new CellRendererText();
            _cellColumnsRender.Add(rendererText, (int)ColumnItem.product_group_name);
            _treeView.InsertColumn(-1, "Product group", rendererText, "text", (int)ColumnItem.product_group_name);


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
                        //_articles[i].short_name = int.Parse(args.NewText);
                        _articles[i].short_name = args.NewText;
                        _itemsModel.SetValue(iter, column, _articles[i].short_name);
                    }
                    break;

                case (int)ColumnItem.product_name:
                    {
                        string oldText = (string)_itemsModel.GetValue(iter, column);
                        int i = path.Indices[0];
                        _articles[i].product_name = args.NewText;

                        _itemsModel.SetValue(iter, column, _articles[i].product_name);
                    }
                    break;
                case (int)ColumnItem.barcode:
                    {
                        string oldText = (string)_itemsModel.GetValue(iter, column);
                        int i = path.Indices[0];
                        _articles[i].barcode = args.NewText;

                        _itemsModel.SetValue(iter, column, _articles[i].barcode);
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