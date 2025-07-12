using System;
using System.Collections.Generic;
using Gtk;
using System.Data;
using Pango;
using Inventorifo.Lib.Model;
using Cairo;

namespace Inventorifo.App
{
    //[Section(ContentType = typeof(EditableCellsSection), Category = Category.Widgets)]
    class CashDrawer : Gtk.Box
    {
        Inventorifo.Lib.LibDb DbCl = new Inventorifo.Lib.LibDb ();
        Inventorifo.Lib.LibGui GuiCl = new Inventorifo.Lib.LibGui ();
        Inventorifo.Lib.LibCore CoreCl = new Inventorifo.Lib.LibCore ();
        
        private TreeView _treeView;
        private ListStore _itemsModel;
        //private ListStore numbers_model;
        private Dictionary<CellRenderer, int> _cellColumnsRender;
        private List<clJournal> _articles;

        private Entry entSearch;
        private Entry entBarcode;

        public CashDrawer(Window parent) : base(Orientation.Vertical, 3)
        {
            Label lbTitle = new Label();
            lbTitle.Text = "Cash Drawer";
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
        { //
            id,
            account_id,
            account_name,
            transaction_date,
            reference_id,
            description,
            debet_amount,
            credit_amount,
            user_id,
            person_name,
            application_id
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
                _articles = new List<clJournal>();
                clJournal item;
                string sql = "select jl.id, account_id, acc.name account_name, account_type, acct.name account_type_name, reference_id, description, TO_CHAR(transaction_date, 'yyyy-mm-dd') transaction_date, debet_amount, credit_amount, user_id, pers.name person_name, application_id "+
                "from journal jl, account acc, account_type acct, userlogin us, person pers "+
                "where jl.account_id = acc.id and acc.account_type = acct.id and jl.user_id=us.id and us.person_id = pers.id ";
                Console.WriteLine(sql);              
                DataTable dttv = DbCl.fillDataTable(DbCl.getConn(), sql);
                foreach (DataRow dr in dttv.Rows)
                {                    
                    item = new clJournal{
                        id=dr["id"].ToString(),
                        account_id=dr["account_id"].ToString(),
                        account_name=dr["account_name"].ToString(),
                        transaction_date=dr["transaction_date"].ToString(),
                        reference_id=dr["reference_id"].ToString(),
                        description=dr["description"].ToString(),
                        debet_amount=dr["debet_amount"].ToString(),
                        credit_amount=dr["credit_amount"].ToString(),
                        user_id=dr["user_id"].ToString(),
                        person_name=dr["person_name"].ToString()
                    };                                    
                    _articles.Add(item);
                }

                /* create list store */
                //
                _itemsModel = new ListStore(typeof(string), typeof(string), typeof(string), typeof(string), typeof(string), typeof(string), typeof(string), typeof(string), typeof(string), typeof(string) );

                /* add items */
                for (int i = 0; i < _articles.Count; i++)
                {
                    iter = _itemsModel.Append();
                    _itemsModel.SetValue(iter, (int)ColumnItem.id, _articles[i].id);
                    _itemsModel.SetValue(iter, (int)ColumnItem.account_id, _articles[i].account_id);
                    _itemsModel.SetValue(iter, (int)ColumnItem.account_name, _articles[i].account_name);
                    _itemsModel.SetValue(iter, (int)ColumnItem.transaction_date, _articles[i].transaction_date);
                    _itemsModel.SetValue(iter, (int)ColumnItem.reference_id, _articles[i].reference_id);
                    _itemsModel.SetValue(iter, (int)ColumnItem.description, _articles[i].description);
                    _itemsModel.SetValue(iter, (int)ColumnItem.debet_amount, _articles[i].debet_amount);
                    _itemsModel.SetValue(iter, (int)ColumnItem.credit_amount, _articles[i].credit_amount);
                    _itemsModel.SetValue(iter, (int)ColumnItem.user_id, _articles[i].user_id);
                    _itemsModel.SetValue(iter, (int)ColumnItem.person_name, _articles[i].person_name);
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
            _cellColumnsRender.Add(rendererText, (int)ColumnItem.account_id);
            _treeView.InsertColumn(-1, "Account ID", rendererText, "text", (int)ColumnItem.account_id);            

            rendererText = new CellRendererText();
            _cellColumnsRender.Add(rendererText, (int)ColumnItem.account_name);
            _treeView.InsertColumn(-1, "Account Name", rendererText, "text", (int)ColumnItem.account_name);

            rendererText = new CellRendererText();
            _cellColumnsRender.Add(rendererText, (int)ColumnItem.transaction_date);
            _treeView.InsertColumn(-1, "Date", rendererText, "text", (int)ColumnItem.transaction_date);

            rendererText = new CellRendererText();
            _cellColumnsRender.Add(rendererText, (int)ColumnItem.reference_id);
            _treeView.InsertColumn(-1, "Reference ID", rendererText, "text", (int)ColumnItem.reference_id);

            rendererText = new CellRendererText();
            _cellColumnsRender.Add(rendererText, (int)ColumnItem.description);
            _treeView.InsertColumn(-1, "Description", rendererText, "text", (int)ColumnItem.description);

            rendererText = new CellRendererText();
            _cellColumnsRender.Add(rendererText, (int)ColumnItem.debet_amount);
            _treeView.InsertColumn(-1, "Debet", rendererText, "text", (int)ColumnItem.debet_amount);

            rendererText = new CellRendererText();
            _cellColumnsRender.Add(rendererText, (int)ColumnItem.credit_amount);
            _treeView.InsertColumn(-1, "Credit", rendererText, "text", (int)ColumnItem.credit_amount);
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