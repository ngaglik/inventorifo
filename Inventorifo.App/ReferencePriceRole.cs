using System;
using System.Collections.Generic;
using Gtk;
using System.Data;
using Pango;
using Inventorifo.Lib.Model;

namespace Inventorifo.App
{
    //[Section(ContentType = typeof(EditableCellsSection), Category = Category.Widgets)]
    class ReferencePriceRole : Gtk.Box
    {
        Inventorifo.Lib.LibDb DbCl = new Inventorifo.Lib.LibDb ();
        Inventorifo.Lib.LibGui GuiCl = new Inventorifo.Lib.LibGui ();
        Inventorifo.Lib.LibCore CoreCl = new Inventorifo.Lib.LibCore ();
        
        private TreeView _treeView;
        private ListStore _itemsModel;
        //private ListStore numbers_model;
        private Dictionary<CellRenderer, int> _cellColumnsRender;
        private List<clsPriceRole> _articles;

        private Entry entSearch;
        private Entry entBarcode;
        Boolean isEditable = true;
        string textForground = "green";        
        private ListStore _taxComboModel;
        private ListStore _isActiveComboModel;
        private ListStore _discountComboModel;
        private ListStore _priceComboModel;

        public ReferencePriceRole(Window parent, string prm) : base(Orientation.Vertical, 3)
        {
            Label lbTitle = new Label();
            lbTitle.Text = "Price Role";
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
            //_treeView.Columns[4].Visible = false;

            CreateItemsModel(true,"");
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


            Button button = new Button("Add");
            button.Clicked += AddItem;
            hbox.PackStart(button, true, true, 0);

            entSearch.Changed += HandleEntSearchChanged;
        }

        
        private enum ColumnItem
        { 
            id,
            customer_group_name,
            price,
            tax_group_id,
            tax_group_name,
            discount_group_id,
            discount_group_name,
            payment_group_id,
            is_active,
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
                _itemsModel = null;
                TreeIter iter;
                /* create array */
                _articles = new List<clsPriceRole>();
                clsPriceRole pers;

                string whrfind = "";
                if(strfind!="") whrfind = "and upper(custgr.name) like upper('" + strfind + "%')  ";

                string sql = "SELECT custgr.id , custgr.name customer_group_name, pricerl.price, pricerl.tax tax_group_id,taxgr.name tax_group_name, pricerl.discount discount_group_id, pricerl.payment payment_group_id, pricerl.is_active "+
                        ", discgr.name discount_group_name "+
                        "FROM customer_group custgr "+
                        "LEFT OUTER JOIN pricing_role pricerl on pricerl.customer_group=custgr.id " +
                        "LEFT OUTER JOIN tax_group taxgr on pricerl.tax=taxgr.id " +
                        "LEFT OUTER JOIN discount_group discgr on pricerl.discount=discgr.id " +
                        "WHERE 1=1 "+ whrfind +
                        "ORDER by custgr.id asc";
                        Console.WriteLine(sql);
              
                DataTable dttv = DbCl.fillDataTable(DbCl.getConn(), sql);
                foreach (DataRow dr in dttv.Rows)
                {                    
                    pers = new clsPriceRole{
                        id=dr["id"].ToString(),
                        customer_group_name=dr["customer_group_name"].ToString(),
                        price=dr["price"].ToString(),
                        tax_group_id=dr["tax_group_id"].ToString(),
                        tax_group_name=dr["tax_group_name"].ToString(),
                        discount_group_id=dr["discount_group_id"].ToString(),
                        discount_group_name=dr["discount_group_name"].ToString(),
                        payment_group_id=dr["payment_group_id"].ToString(),
                        is_active=dr["is_active"].ToString(),
                    } ; 
                    _articles.Add(pers);  
                }

                /* create list store */
                //
                _itemsModel = new ListStore(typeof(string),typeof(string),typeof(string), typeof(string),typeof(string),  typeof(string),typeof(string), typeof(string),typeof(string), typeof(string));

                /* add items */
                for (int i = 0; i < _articles.Count; i++)
                {
                    iter = _itemsModel.Append();
                    _itemsModel.SetValue(iter, (int)ColumnItem.id, _articles[i].id);
                    _itemsModel.SetValue(iter, (int)ColumnItem.customer_group_name, _articles[i].customer_group_name);
                    _itemsModel.SetValue(iter, (int)ColumnItem.price, _articles[i].price);
                    _itemsModel.SetValue(iter, (int)ColumnItem.tax_group_id, _articles[i].tax_group_id);
                    _itemsModel.SetValue(iter, (int)ColumnItem.tax_group_name, _articles[i].tax_group_name);
                    _itemsModel.SetValue(iter, (int)ColumnItem.discount_group_id, _articles[i].discount_group_id);
                    _itemsModel.SetValue(iter, (int)ColumnItem.discount_group_name, _articles[i].discount_group_name);
                    _itemsModel.SetValue(iter, (int)ColumnItem.payment_group_id, _articles[i].payment_group_id);
                    _itemsModel.SetValue(iter, (int)ColumnItem.is_active, _articles[i].is_active);
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
            _treeView.InsertColumn(-1, "ID", rendererText, "text", (int)ColumnItem.id);

            rendererText = new CellRendererText();
            _cellColumnsRender.Add(rendererText, (int)ColumnItem.customer_group_name);
            _treeView.InsertColumn(-1, "customer group name", rendererText, "text", (int)ColumnItem.customer_group_name);            

            _priceComboModel = new ListStore(typeof(string), typeof(string));
            _priceComboModel.AppendValues("1","price 1");
            _priceComboModel.AppendValues("2","price 2");
            _priceComboModel.AppendValues("3","price 3");
            CellRendererCombo rendererCombo = new CellRendererCombo
            { 
                Model = _priceComboModel,
                TextColumn = 1,
                HasEntry = false,
                Editable = isEditable
            };
            rendererCombo.Foreground = textForground;
            rendererCombo.Edited += CellEdited;
            rendererCombo.EditingStarted += EditingStarted;
            _cellColumnsRender.Add(rendererCombo, (int)ColumnItem.price);
            _treeView.InsertColumn(-1, "price", rendererCombo, "text", (int)ColumnItem.price);

            // rendererText = new CellRendererText();
            // _cellColumnsRender.Add(rendererText, (int)ColumnItem.price);
            // _treeView.InsertColumn(-1, "price", rendererText, "text", (int)ColumnItem.price);

            _taxComboModel = new ListStore(typeof(string), typeof(string));
            String sql = "Select id,name from tax_group order by id asc";
            DataTable dt = DbCl.fillDataTable(DbCl.getConn(), sql);
            foreach (DataRow dr in dt.Rows)
            {
                _taxComboModel.AppendValues(dr[0].ToString(), dr[1].ToString());
            }
             rendererCombo = new CellRendererCombo
            {
                Model = _taxComboModel,
                TextColumn = 1,
                HasEntry = false,
                Editable = isEditable
            };

            rendererCombo.Foreground = textForground;
            rendererCombo.Edited += CellEdited;
            rendererCombo.EditingStarted += EditingStarted;           
            _cellColumnsRender.Add(rendererCombo, (int)ColumnItem.tax_group_name);
            _treeView.InsertColumn(-1, "tax", rendererCombo, "text", (int)ColumnItem.tax_group_name);

            _discountComboModel = new ListStore(typeof(string), typeof(string));
            sql = "Select id,name from discount_group order by id asc";
            dt = DbCl.fillDataTable(DbCl.getConn(), sql);
            foreach (DataRow dr in dt.Rows)
            {
                _discountComboModel.AppendValues(dr[0].ToString(), dr[1].ToString());
            }
            rendererCombo = new CellRendererCombo
            {
                Model = _discountComboModel,
                TextColumn = 1,
                HasEntry = false,
                Editable = isEditable
            };

            rendererCombo.Foreground = textForground;
            rendererCombo.Edited += CellEdited;
            rendererCombo.EditingStarted += EditingStarted;           
            _cellColumnsRender.Add(rendererCombo, (int)ColumnItem.discount_group_name);
            _treeView.InsertColumn(-1, "discount", rendererCombo, "text", (int)ColumnItem.discount_group_name);

            // rendererText = new CellRendererText();
            // _cellColumnsRender.Add(rendererText, (int)ColumnItem.payment_group_id);
            // _treeView.InsertColumn(-1, "payment group id", rendererText, "text", (int)ColumnItem.payment_group_id);

            _isActiveComboModel = new ListStore(typeof(string), typeof(string));
            _isActiveComboModel.AppendValues("True","True");
            _isActiveComboModel.AppendValues("False","False");
             rendererCombo = new CellRendererCombo
            { 
                Model = _isActiveComboModel,
                TextColumn = 1,
                HasEntry = false,
                Editable = isEditable
            };
            rendererCombo.Foreground = textForground;
            rendererCombo.Edited += CellEdited;
            rendererCombo.EditingStarted += EditingStarted;
            _cellColumnsRender.Add(rendererCombo, (int)ColumnItem.is_active);
            _treeView.InsertColumn(-1, "Is Active", rendererCombo, "text", (int)ColumnItem.is_active);

        }

        private void AddItem(object sender, EventArgs e)
        {
            string sql = "insert into product_group (name) values ('') ";
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
            _itemsModel.GetIter(out TreeIter iter, path);

            switch (column)
            {
                case (int)ColumnItem.price:
                {
                    int i = path.Indices[0];

                    // Cari ID berdasarkan nama yang dipilih
                    TreeIter comboIter;
                    string selectedId = "";
                    if (_priceComboModel.GetIterFirst(out comboIter))
                    {
                        do
                        {
                            string comboName = (string)_priceComboModel.GetValue(comboIter, 1);
                            if (comboName == args.NewText)
                            {
                                selectedId = (string)_priceComboModel.GetValue(comboIter, 0);
                                break;
                            }
                        } while (_priceComboModel.IterNext(ref comboIter));
                    }

                    Console.WriteLine("selectedId "+ selectedId);
                    if (!string.IsNullOrEmpty(selectedId))
                    {
                        _articles[i].price = selectedId;
                        _itemsModel.SetValue(iter, column, args.NewText);

                        string sql = $"update pricing_role set price = '{selectedId}' where id='{_articles[i].id}'";
                        Console.WriteLine(sql);
                        DbCl.ExecuteTrans(DbCl.getConn(), sql);
                    }
                }
                break;
                case (int)ColumnItem.tax_group_name:
                {
                    int i = path.Indices[0];

                    // Cari ID berdasarkan nama yang dipilih
                    TreeIter comboIter;
                    string selectedId = "";
                    if (_taxComboModel.GetIterFirst(out comboIter))
                    {
                        do
                        {
                            string comboName = (string)_taxComboModel.GetValue(comboIter, 1);
                            if (comboName == args.NewText)
                            {
                                selectedId = (string)_taxComboModel.GetValue(comboIter, 0);
                                break;
                            }
                        } while (_taxComboModel.IterNext(ref comboIter));
                    }

                    Console.WriteLine("selectedId "+ selectedId);
                    if (!string.IsNullOrEmpty(selectedId))
                    {
                        _articles[i].tax_group_id = selectedId;
                        _articles[i].tax_group_name = args.NewText;
                        _itemsModel.SetValue(iter, column, args.NewText);

                        string sql = $"update pricing_role set tax = '{selectedId}' where id='{_articles[i].id}'";
                        Console.WriteLine(sql);
                        DbCl.ExecuteTrans(DbCl.getConn(), sql);
                    }
                }
                break;
                case (int)ColumnItem.discount_group_name:
                {
                    int i = path.Indices[0];

                    // Cari ID berdasarkan nama yang dipilih
                    TreeIter comboIter;
                    string selectedId = "";
                    if (_discountComboModel.GetIterFirst(out comboIter))
                    {
                        do
                        {
                            string comboName = (string)_discountComboModel.GetValue(comboIter, 1);
                            if (comboName == args.NewText)
                            {
                                selectedId = (string)_discountComboModel.GetValue(comboIter, 0);
                                break;
                            }
                        } while (_discountComboModel.IterNext(ref comboIter));
                    }

                    Console.WriteLine("selectedId "+ selectedId);
                    if (!string.IsNullOrEmpty(selectedId))
                    {
                        _articles[i].discount_group_id = selectedId;
                        _articles[i].discount_group_name = args.NewText;
                        _itemsModel.SetValue(iter, column, args.NewText);

                        string sql = $"update pricing_role set discount = '{selectedId}' where id='{_articles[i].id}'";
                        Console.WriteLine(sql);
                        DbCl.ExecuteTrans(DbCl.getConn(), sql);
                    }
                }
                break;
                case (int)ColumnItem.is_active:
                {
                    int i = path.Indices[0];

                    // Cari ID berdasarkan nama yang dipilih
                    TreeIter comboIter;
                    string selectedId = "";
                    if (_isActiveComboModel.GetIterFirst(out comboIter))
                    {
                        do
                        {
                            string comboName = (string)_isActiveComboModel.GetValue(comboIter, 1);
                            if (comboName == args.NewText)
                            {
                                selectedId = (string)_isActiveComboModel.GetValue(comboIter, 0);
                                break;
                            }
                        } while (_isActiveComboModel.IterNext(ref comboIter));
                    }

                    Console.WriteLine("selectedId "+ selectedId);
                    if (!string.IsNullOrEmpty(selectedId))
                    {
                        _articles[i].is_active = selectedId;
                        //_articles[i].tax_group_name = args.NewText;
                        _itemsModel.SetValue(iter, column, args.NewText);

                        string sql = $"update pricing_role set is_active = '{selectedId}' where id='{_articles[i].id}'";
                        Console.WriteLine(sql);
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