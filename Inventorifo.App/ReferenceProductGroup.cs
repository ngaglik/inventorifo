using System;
using System.Collections.Generic;
using Gtk;
using System.Data;
using Pango;
using Inventorifo.Lib.Model;

namespace Inventorifo.App
{
    //[Section(ContentType = typeof(EditableCellsSection), Category = Category.Widgets)]
    class ReferenceProductGroup : Gtk.Box
    {
        Inventorifo.Lib.LibDb DbCl = new Inventorifo.Lib.LibDb ();
        Inventorifo.Lib.LibGui GuiCl = new Inventorifo.Lib.LibGui ();
        Inventorifo.Lib.LibCore CoreCl = new Inventorifo.Lib.LibCore ();
        
        private TreeView _treeView;
        private ListStore _itemsModel;
        //private ListStore numbers_model;
        private Dictionary<CellRenderer, int> _cellColumnsRender;
        private List<clsProductGroup> _articles;

        private Entry entSearch;
        private Entry entBarcode;

        public ReferenceProductGroup(Window parent, string prm) : base(Orientation.Vertical, 3)
        {
            Label lbTitle = new Label();
            lbTitle.Text = "Product Group";
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
                _itemsModel = null;
                TreeIter iter;
                /* create array */
                _articles = new List<clsProductGroup>();
                clsProductGroup pers;

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
                    pers = new clsProductGroup{
                        id=dr[0].ToString(),
                        name=dr[1].ToString(),
                    } ; 
                    _articles.Add(pers);  
                }

                /* create list store */
                //
                _itemsModel = new ListStore(typeof(string), typeof(string));

                /* add items */
                for (int i = 0; i < _articles.Count; i++)
                {
                    iter = _itemsModel.Append();
                    _itemsModel.SetValue(iter, (int)ColumnItem.id, _articles[i].id);
                    _itemsModel.SetValue(iter, (int)ColumnItem.name, _articles[i].name);
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
                case (int)ColumnItem.name:
                    {
                        int i = path.Indices[0];
                        _articles[i].name = args.NewText;
                        _itemsModel.SetValue(iter, column, _articles[i].name);
                        string sql = "update product_group set name = '"+args.NewText+"' where id='"+_articles[i].id+"' ";
                        Console.WriteLine (sql);
                        DbCl.ExecuteTrans(DbCl.getConn(), sql);
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