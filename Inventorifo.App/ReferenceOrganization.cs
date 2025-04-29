using System;
using System.Collections.Generic;
using Gtk;
using System.Data;
using Pango;
using Inventorifo.Lib.Model;

namespace Inventorifo.App
{
    //[Section(ContentType = typeof(EditableCellsSection), Category = Category.Widgets)]
    class ReferenceOrganization : Gtk.Box
    {
        Inventorifo.Lib.LibDb DbCl = new Inventorifo.Lib.LibDb ();
        Inventorifo.Lib.LibGui GuiCl = new Inventorifo.Lib.LibGui ();
        Inventorifo.Lib.LibCore CoreCl = new Inventorifo.Lib.LibCore ();
        
        private TreeView _treeView;
        private ListStore _itemsModel;
        //private ListStore numbers_model;
        private Dictionary<CellRenderer, int> _cellColumnsRender;
        private List<clsPerson> _articles;
        public object parent;
        private Entry entSearch;
        
        Boolean isEditable;
        string textForground;
        string prm;
        string mode;

        public ReferenceOrganization(object parent, string mode, string prm) : base(Orientation.Vertical, 3)
        {
            this.parent=parent;
            this.prm = prm;
            this.mode=mode;
            
            Label lbTitle = new Label();
            lbTitle.Text = "Organization";
            lbTitle.ModifyFont(FontDescription.FromString("Arial 18"));
            this.PackStart(lbTitle, false, true, 0);
            Box hbox = new Box(Orientation.Horizontal, 4)
            {
                Homogeneous = true
            };
            entSearch = new Entry();
            entSearch.PlaceholderText = "Search";
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
                    } 
                    break;
            }
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
                        
            entSearch.Changed += HandleEntSearchChanged;
            entSearch.GrabFocus();
        }

        private enum ColumnItem
        { //
            id,
            name,
            address,
            phone_number,
            is_active,
            tax_id_number,
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
                _articles = new List<clsPerson>();
                clsPerson pers;
                DataTable dttv = CoreCl.fillDtOrganization(strfind);
                foreach (DataRow dr in dttv.Rows)
                {            
                    pers = new clsPerson{         
                        id=dr["id"].ToString(),
                        name=dr["name"].ToString(),
                        address=dr["address"].ToString(),
                        phone_number=dr["phone_number"].ToString(),
                        is_active=dr["is_active"].ToString(),
                        tax_id_number=dr["tax_id_number"].ToString(),
                    } ; 
                    _articles.Add(pers);    
                }

                /* create list store */
                //
                _itemsModel = new ListStore(typeof(string), typeof(string), typeof(string), typeof(string), typeof(string), typeof(string), typeof(string), typeof(string));

                /* add items */
                for (int i = 0; i < _articles.Count; i++)
                {
                    iter = _itemsModel.Append();
                    _itemsModel.SetValue(iter, (int)ColumnItem.id, _articles[i].id);
                    _itemsModel.SetValue(iter, (int)ColumnItem.name, _articles[i].name);
                    _itemsModel.SetValue(iter, (int)ColumnItem.address, _articles[i].address);
                    _itemsModel.SetValue(iter, (int)ColumnItem.is_active, _articles[i].is_active);
                    _itemsModel.SetValue(iter, (int)ColumnItem.tax_id_number, _articles[i].tax_id_number);
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
                Editable = isEditable
            };
            rendererText.Foreground = textForground;
            rendererText.Edited += CellEdited;
            _cellColumnsRender.Add(rendererText, (int)ColumnItem.name);
            _treeView.InsertColumn(-1, "Name", rendererText, "text", (int)ColumnItem.name);

            rendererText = new CellRendererText
            {
                Editable = isEditable
            };
            rendererText.Foreground = textForground;
            rendererText.Edited += CellEdited;
            _cellColumnsRender.Add(rendererText, (int)ColumnItem.address);
            _treeView.InsertColumn(-1, "Address", rendererText, "text", (int)ColumnItem.address);

            
            rendererText = new CellRendererText
            {
                Editable = isEditable
            };
            rendererText.Foreground = textForground;
            rendererText.Edited += CellEdited;
            _cellColumnsRender.Add(rendererText, (int)ColumnItem.tax_id_number);
            _treeView.InsertColumn(-1, "Tax id number", rendererText, "text", (int)ColumnItem.tax_id_number);

            ListStore lstModelCombo = new ListStore(typeof(string), typeof(string));
            lstModelCombo.AppendValues("true","true");
            lstModelCombo.AppendValues("false","false");
            CellRendererCombo rendererCombo = new CellRendererCombo
            { 
                Model = lstModelCombo,
                TextColumn = (int)ColumnNumber.Text,
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
            string sql = "insert into organization (name) values ('') ";
            Console.WriteLine (sql);
            DbCl.ExecuteTrans(DbCl.getConn(), sql);
            CreateItemsModel(true,entSearch.Text.Trim());
        }
        private void SelectItem(object sender, EventArgs e)
        {            
            TreeSelection selection = _treeView.Selection;    
            TreeIter iter;
            if(selection.GetSelected( out iter)){
                Console.WriteLine("Selected Value:"+_itemsModel.GetValue (iter, 0).ToString()+_itemsModel.GetValue (iter, 1).ToString());
            }
            if(this.prm=="source"){
                WarehouseTransfer o = (WarehouseTransfer)this.parent;
                o.doChildSourceOrganization(null,new  clsOrganization{id=_itemsModel.GetValue (iter, 0).ToString(), name=_itemsModel.GetValue (iter, 1).ToString()});
            }else if(this.prm=="destination"){
                WarehouseTransfer o = (WarehouseTransfer)this.parent;
                o.doChildDestinationOrganization(null,new clsOrganization{id=_itemsModel.GetValue (iter, 0).ToString(), name=_itemsModel.GetValue (iter, 1).ToString()});
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
                        Console.WriteLine("Selected Value:"+_itemsModel.GetValue (iter, 0).ToString()+_itemsModel.GetValue (iter, 1).ToString());
                    }
                    if(this.prm=="source"){
                        WarehouseTransfer o = (WarehouseTransfer)this.parent;
                        o.doChildSourceOrganization(null,new clsOrganization{id=_itemsModel.GetValue (iter, 0).ToString(), name=_itemsModel.GetValue (iter, 1).ToString()});
                    }else if(this.prm=="destination"){
                        WarehouseTransfer o = (WarehouseTransfer)this.parent;
                        o.doChildDestinationOrganization(null,new clsOrganization{id=_itemsModel.GetValue (iter, 0).ToString(), name=_itemsModel.GetValue (iter, 1).ToString()});
                    }  
                }
            }

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
                        string sql = "update organization set name = '"+args.NewText+"' where id='"+_articles[i].id+"' ";
                        Console.WriteLine (sql);
                        DbCl.ExecuteTrans(DbCl.getConn(), sql);
                    }
                    break;
                case (int)ColumnItem.address:
                    {
                        int i = path.Indices[0];
                        _articles[i].address = args.NewText;
                        _itemsModel.SetValue(iter, column, _articles[i].address);
                        string sql = "update organization set address = '"+args.NewText+"' where id='"+_articles[i].id+"' ";
                        Console.WriteLine (sql);
                        DbCl.ExecuteTrans(DbCl.getConn(), sql);
                    }
                    break;
                case (int)ColumnItem.phone_number:
                    {
                        int i = path.Indices[0];
                        _articles[i].phone_number = args.NewText;
                        _itemsModel.SetValue(iter, column, _articles[i].phone_number);
                        string sql = "update organization set phone_number = '"+args.NewText+"' where id='"+_articles[i].id+"' ";
                        Console.WriteLine (sql);
                        DbCl.ExecuteTrans(DbCl.getConn(), sql);
                    }
                    break;
                case (int)ColumnItem.tax_id_number:
                    {
                        int i = path.Indices[0];
                        _articles[i].tax_id_number = args.NewText;
                        _itemsModel.SetValue(iter, column, _articles[i].tax_id_number);
                        string sql = "update organization set tax_id_number = '"+args.NewText+"' where id='"+_articles[i].id+"' ";
                        Console.WriteLine (sql);
                        DbCl.ExecuteTrans(DbCl.getConn(), sql);
                    }
                    break;
                case (int)ColumnItem.is_active:
                    {
                        int i = path.Indices[0];
                        _articles[i].is_active = args.NewText;
                        _itemsModel.SetValue(iter, column, _articles[i].is_active);
                        string sql = "update organization set is_active = '"+args.NewText+"' where id='"+_articles[i].id+"' ";
                        Console.WriteLine (sql);
                        DbCl.ExecuteTrans(DbCl.getConn(), sql);
                    }
                    break;
            }
        }

        private void EditingStarted(object o, EditingStartedArgs args)
        {
           ((ComboBox)args.Editable).RowSeparatorFunc += SeparatorRow;
        }

        private bool SeparatorRow(ITreeModel model, TreeIter iter)
        {
            TreePath path = model.GetPath(iter);
            int idx = path.Indices[0];

            return idx == 5;
        }
    }
}