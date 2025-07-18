using System;
using System.Collections.Generic;
using Gtk;
using System.Data;
using Pango;
using Inventorifo.Lib.Model;

namespace Inventorifo.App
{
    //[Section(ContentType = typeof(EditableCellsSection), Category = Category.Widgets)]
    class ReportTransactionSaleReturn : Gtk.Box
    {
        Inventorifo.Lib.LibDb DbCl = new Inventorifo.Lib.LibDb ();
        Inventorifo.Lib.LibGui GuiCl = new Inventorifo.Lib.LibGui ();
        Inventorifo.Lib.LibCore CoreCl = new Inventorifo.Lib.LibCore ();
        
        private TreeView _treeView;
        private ListStore _itemsModel;
        //private ListStore numbers_model;
        private Dictionary<CellRenderer, int> _cellColumnsRender;
        private List<clReportTransaction> _articles;
        public object parent;
        private Entry entSearch;
        Button btnDate = new Button();

        private Popover popoverDate;
        Calendar calendar = new Calendar();
        
        Boolean isEditable;
        string textForground;
        string prm;
        string mode;

        public ReportTransactionSaleReturn(object parent, string mode, string prm) : base(Orientation.Vertical, 3)
        {
            this.parent=parent;
            this.prm = prm;
            this.mode=mode;
            
            Label lbTitle = new Label();
            lbTitle.Text = "Report Sale Return";
            lbTitle.ModifyFont(FontDescription.FromString("Arial 18"));
            this.PackStart(lbTitle, false, true, 0);
            Box hbox = new Box(Orientation.Horizontal, 4)
            {
                Homogeneous = true
            };
            entSearch = new Entry();
            entSearch.PlaceholderText = "Search";
            hbox.PackStart(entSearch, true, true, 0);
            entSearch.Changed += HandleEntSearchChanged;
            entSearch.GrabFocus();

            btnDate.Label = calendar.Date.ToString("yyyy-MM-dd");
            popoverDate = new Popover(btnDate);    
            btnDate.Clicked += ShowDatePopup;
            hbox.PackStart(btnDate, true, true, 0);

            //Console.WriteLine("PackType changed to " + mode);

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
            _treeView.KeyPressEvent += HandleTreeViewKeyPressEvent;

            AddColumns();
            //_treeView.Columns[4].Visible = false;

            CreateItemsModel(true,"");
            sw.Add(_treeView); 
            this.PackStart(hbox, false, false, 0);
                        
            
        }

        private enum ColumnItems
        { //
            id,
            input_date,
            transaction_id,
            product_id,
            product_short_name,
            product_name,
            stock_id,
            quantity,
            unit,
            unit_name,
            purchase_price_id,
            purchase_item_price,
            price,
            tax,
            state,
            state_name,
            location,
            location_name,
            condition,
            condition_name,
            expired_date,
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
        public void CreateItemsModel(Boolean showAll,string strfind)
        {      
            if(strfind=="" && !showAll) {          
                _treeView.Model = null;
            }else{
                //ListStore model;
                _itemsModel = null;
                TreeIter iter;
                /* create array */
                clTransaction filterTrans = new clTransaction{
                    transaction_type_id = "4",
                    transaction_date = btnDate.Label,
                };
                clTransactionItem1 filterItem = new clTransactionItem1{                    
                    product_short_name = entSearch.Text.Trim(),
                };
                _articles = new List<clReportTransaction>();
                clReportTransaction rpt;
                DataTable dttv = CoreCl.fillDtReportTransaction(filterTrans, filterItem);
                foreach (DataRow dr in dttv.Rows)
                {            
                    rpt = new clReportTransaction{ 
                        id=dr["id"].ToString(), 
                        transaction_id=dr["transaction_id"].ToString(),        
                        product_id=dr["product_id"].ToString(),
                        product_short_name=dr["product_short_name"].ToString(),
                        product_name=dr["product_name"].ToString(),
                        stock_id=dr["stock_id"].ToString(),
                        unit=dr["unit"].ToString(),
                        unit_name=dr["unit_name"].ToString(),
                        purchase_price_id=dr["purchase_price_id"].ToString(),
                        purchase_item_price=dr["purchase_item_price"].ToString(),
                        tax=dr["tax"].ToString(),
                        state=dr["state"].ToString(),
                        state_name=dr["state_name"].ToString(),
                        location=dr["location"].ToString(),
                        location_name=dr["location_name"].ToString(),
                        condition=dr["condition"].ToString(),
                        condition_name=dr["condition_name"].ToString(),
                    } ; 
                    _articles.Add(rpt);    
                }

                /* create list store */
                //
                _itemsModel = new ListStore(typeof(string),typeof(string),typeof(string),typeof(string),typeof(string),typeof(string),typeof(string),typeof(string),typeof(string),typeof(string),typeof(string),typeof(string),typeof(string),typeof(string),typeof(string),typeof(string),typeof(string),typeof(string),typeof(string));

                /* add items */
                for (int i = 0; i < _articles.Count; i++)
                {
                    iter = _itemsModel.Append();
                    _itemsModel.SetValue(iter, (int)ColumnItems.id, _articles[i].id);
                    _itemsModel.SetValue(iter, (int)ColumnItems.transaction_id, _articles[i].transaction_id);
                    _itemsModel.SetValue(iter, (int)ColumnItems.product_id, _articles[i].product_id);
                    _itemsModel.SetValue(iter, (int)ColumnItems.product_short_name, _articles[i].product_short_name);
                    _itemsModel.SetValue(iter, (int)ColumnItems.product_name, _articles[i].product_name);
                    _itemsModel.SetValue(iter, (int)ColumnItems.stock_id, _articles[i].stock_id);
                    _itemsModel.SetValue(iter, (int)ColumnItems.quantity, _articles[i].quantity);
                    _itemsModel.SetValue(iter, (int)ColumnItems.unit, _articles[i].unit);
                    _itemsModel.SetValue(iter, (int)ColumnItems.unit_name, _articles[i].unit_name);
                    _itemsModel.SetValue(iter, (int)ColumnItems.purchase_price_id, _articles[i].purchase_price_id);
                    _itemsModel.SetValue(iter, (int)ColumnItems.purchase_item_price, _articles[i].purchase_item_price);
                    _itemsModel.SetValue(iter, (int)ColumnItems.price, _articles[i].price);
                    _itemsModel.SetValue(iter, (int)ColumnItems.tax, _articles[i].tax);
                    _itemsModel.SetValue(iter, (int)ColumnItems.state, _articles[i].state);
                    _itemsModel.SetValue(iter, (int)ColumnItems.state_name, _articles[i].state_name);
                    _itemsModel.SetValue(iter, (int)ColumnItems.location, _articles[i].location);
                    _itemsModel.SetValue(iter, (int)ColumnItems.location_name, _articles[i].location_name);
                    _itemsModel.SetValue(iter, (int)ColumnItems.condition, _articles[i].condition);
                    _itemsModel.SetValue(iter, (int)ColumnItems.condition_name, _articles[i].condition_name);
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
            _cellColumnsRender.Add(rendererText, (int)ColumnItems.id);
            _treeView.InsertColumn(-1, "ID", rendererText, "text", (int)ColumnItems.id);

            rendererText = new CellRendererText();
            rendererText.Foreground = textForground;
            rendererText.Edited += CellEdited;
            _cellColumnsRender.Add(rendererText, (int)ColumnItems.transaction_id);
            _treeView.InsertColumn(-1, "transaction_id", rendererText, "text", (int)ColumnItems.transaction_id);

            rendererText = new CellRendererText();
            rendererText.Foreground = textForground;
            _cellColumnsRender.Add(rendererText, (int)ColumnItems.product_id);
            _treeView.InsertColumn(-1, "product_id", rendererText, "text", (int)ColumnItems.product_id);

            
            rendererText = new CellRendererText();
            rendererText.Foreground = textForground;
            _cellColumnsRender.Add(rendererText, (int)ColumnItems.product_short_name);
            _treeView.InsertColumn(-1, "product_short_name", rendererText, "text", (int)ColumnItems.product_short_name);

            rendererText = new CellRendererText();
            rendererText.Foreground = textForground;
            _cellColumnsRender.Add(rendererText, (int)ColumnItems.product_name);
            _treeView.InsertColumn(-1, "product_name", rendererText, "text", (int)ColumnItems.product_name);

            rendererText = new CellRendererText();
            rendererText.Foreground = textForground;
            _cellColumnsRender.Add(rendererText, (int)ColumnItems.stock_id);
            _treeView.InsertColumn(-1, "stock_id", rendererText, "text", (int)ColumnItems.stock_id);

            
            rendererText = new CellRendererText();
            rendererText.Foreground = textForground;
            _cellColumnsRender.Add(rendererText, (int)ColumnItems.quantity);
            _treeView.InsertColumn(-1, "quantity", rendererText, "text", (int)ColumnItems.quantity);

            
            rendererText = new CellRendererText();
            rendererText.Foreground = textForground;
            _cellColumnsRender.Add(rendererText, (int)ColumnItems.unit);
            _treeView.InsertColumn(-1, "unit", rendererText, "text", (int)ColumnItems.unit);

            rendererText = new CellRendererText();
            rendererText.Foreground = textForground;
            _cellColumnsRender.Add(rendererText, (int)ColumnItems.unit_name);
            _treeView.InsertColumn(-1, "unit_name", rendererText, "text", (int)ColumnItems.unit_name);

            rendererText = new CellRendererText();
            rendererText.Foreground = textForground;
            _cellColumnsRender.Add(rendererText, (int)ColumnItems.purchase_price_id);
            _treeView.InsertColumn(-1, "purchase_price_id", rendererText, "text", (int)ColumnItems.purchase_price_id);

            
            rendererText = new CellRendererText();
            rendererText.Foreground = textForground;
            _cellColumnsRender.Add(rendererText, (int)ColumnItems.purchase_item_price);
            _treeView.InsertColumn(-1, "purchase_item_price", rendererText, "text", (int)ColumnItems.purchase_item_price);
            
            rendererText = new CellRendererText();
            rendererText.Foreground = textForground;
            _cellColumnsRender.Add(rendererText, (int)ColumnItems.price);
            _treeView.InsertColumn(-1, "price", rendererText, "text", (int)ColumnItems.price);

            rendererText = new CellRendererText();
            rendererText.Foreground = textForground;
            _cellColumnsRender.Add(rendererText, (int)ColumnItems.tax);
            _treeView.InsertColumn(-1, "tax", rendererText, "text", (int)ColumnItems.tax);

            rendererText = new CellRendererText();
            rendererText.Foreground = textForground;
            _cellColumnsRender.Add(rendererText, (int)ColumnItems.state);
            _treeView.InsertColumn(-1, "state", rendererText, "text", (int)ColumnItems.state);
            
            rendererText = new CellRendererText();
            rendererText.Foreground = textForground;
            _cellColumnsRender.Add(rendererText, (int)ColumnItems.state_name);
            _treeView.InsertColumn(-1, "state_name", rendererText, "text", (int)ColumnItems.state_name);
            
            rendererText = new CellRendererText();
            rendererText.Foreground = textForground;
            _cellColumnsRender.Add(rendererText, (int)ColumnItems.location);
            _treeView.InsertColumn(-1, "location", rendererText, "text", (int)ColumnItems.location);

            rendererText = new CellRendererText();
            rendererText.Foreground = textForground;
            _cellColumnsRender.Add(rendererText, (int)ColumnItems.location_name);
            _treeView.InsertColumn(-1, "location_name", rendererText, "text", (int)ColumnItems.location_name);

            rendererText = new CellRendererText();
            rendererText.Foreground = textForground;
            _cellColumnsRender.Add(rendererText, (int)ColumnItems.condition);
            _treeView.InsertColumn(-1, "condition", rendererText, "text", (int)ColumnItems.condition);
            
            rendererText = new CellRendererText();
            rendererText.Foreground = textForground;
            _cellColumnsRender.Add(rendererText, (int)ColumnItems.condition_name);
            _treeView.InsertColumn(-1, "condition_name", rendererText, "text", (int)ColumnItems.condition_name);
            
        }
        private void ShowDatePopup(object sender, EventArgs e)
        {   
            GLib.Timeout.Add(0, () =>
            { 
                GuiCl.RemoveAllWidgets(popoverDate);
                calendar = new Calendar();
                calendar.DaySelected += HandleCalendarDaySelected;
                popoverDate.Add(calendar);
                popoverDate.SetSizeRequest(300, 150);
               // calendar.Show();          
                popoverDate.ShowAll();
                return false;
            });
        }
        private void HandleCalendarDaySelected(object sender, EventArgs e){
            btnDate.Label = calendar.Date.ToString("yyyy-MM-dd");
            CreateItemsModel(true,entSearch.Text.Trim()); 
            popoverDate.Hide();
        }
        private void AddItem(object sender, EventArgs e)
        {
        }
        private void SelectItem(object sender, EventArgs e)
        {            
                    
        }
        [GLib.ConnectBefore]
        private void HandleTreeViewKeyPressEvent(object sender, KeyPressEventArgs e)
        {
            
        }

        private void RemoveItem(object sender, EventArgs e)
        {            
        }

        private void CellEdited(object data, EditedArgs args)
        {          
        }

        private void EditingStarted(object o, EditingStartedArgs args)
        {
        }

        private bool SeparatorRow(ITreeModel model, TreeIter iter)
        {
            return false;
        }
    }
}