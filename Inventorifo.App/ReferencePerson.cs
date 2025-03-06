using System;
using System.Collections.Generic;
using Gtk;
using System.Data;
using Pango;

namespace Inventorifo.App
{
    //[Section(ContentType = typeof(EditableCellsSection), Category = Category.Widgets)]
    class ReferencePerson : Gtk.Box
    {
        Inventorifo.Lib.LibDb DbCl = new Inventorifo.Lib.LibDb ();
        private TreeView _treeView;
        private ListStore _itemsModel;
        //private ListStore numbers_model;
        private Dictionary<CellRenderer, int> _cellColumnsRender;
        private List<Item> _articles;
        public object parent;
        private Entry entSearch;
        
        Boolean isEditable;
        string textForground;
        string prm;

        public ReferencePerson(object parent, string mode, string prm) : base(Orientation.Vertical, 3)
        {
            this.parent=parent;
            this.prm = prm;
            Label lbTitle = new Label();
            lbTitle.Text = "Person";
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

            AddColumns();
            //_treeView.Columns[4].Visible = false;

            CreateItemsModel(true,"");
            sw.Add(_treeView); 
            this.PackStart(hbox, false, false, 0);
                        
            entSearch.Changed += HandleEntSearchChanged;
            entSearch.GrabFocus();
        }

        private class Item
        { //
            public Item(string id, string name, string address, string phone_number, string is_active, string national_id_number, string tax_id_number, string health_insurance_id_number){
                this.id = id;
                this.name = name;
                this.address = address;
                this.phone_number = phone_number;
                this.is_active = is_active;
                this.national_id_number = national_id_number;
                this.tax_id_number = tax_id_number;
                this.health_insurance_id_number = health_insurance_id_number;
            }
            public string id;
            public string name;
            public string address;
            public string phone_number;
            public string is_active;
            public string national_id_number;
            public string tax_id_number;
            public string health_insurance_id_number;
        }

        private enum ColumnItem
        { //
            id,
            name,
            address,
            phone_number,
            is_active,
            national_id_number,
            tax_id_number,
            health_insurance_id_number,
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
                _articles = new List<Item>();

                string whrfind = "";
                if(strfind!="") whrfind = "and upper(pers.name) like upper('" + strfind + "%')  ";

                string sql = "SELECT pers.id , pers.name, pers.address,  pers.phone_number,  pers.is_active, pers.national_id_number, pers.tax_id_number,pers.health_insurance_id_number "+
                        "FROM person pers "+
                        "WHERE 1=1 "+ whrfind +
                        "ORDER by pers.name asc";
                        Console.WriteLine(sql);
              
                DataTable dttv = DbCl.fillDataTable(DbCl.getConn(), sql);
                foreach (DataRow dr in dttv.Rows)
                {                    
                    string id=dr[0].ToString();
                    string name=dr[1].ToString();
                    string address=dr[2].ToString();
                    string phone_number=dr[3].ToString();
                    string is_active=dr[4].ToString();
                    string national_id_number=dr[5].ToString();
                    string tax_id_number=dr[6].ToString();
                    string health_insurance_id_number=dr[7].ToString();
                                    
                    _articles.Add(new Item(id, name, address, phone_number,is_active, national_id_number, tax_id_number, health_insurance_id_number));
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
                    _itemsModel.SetValue(iter, (int)ColumnItem.national_id_number, _articles[i].national_id_number);
                    _itemsModel.SetValue(iter, (int)ColumnItem.tax_id_number, _articles[i].tax_id_number);
                    _itemsModel.SetValue(iter, (int)ColumnItem.health_insurance_id_number, _articles[i].health_insurance_id_number);
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
            _cellColumnsRender.Add(rendererText, (int)ColumnItem.national_id_number);
            _treeView.InsertColumn(-1, "National id number", rendererText, "text", (int)ColumnItem.national_id_number);

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

            rendererText = new CellRendererText
            {
                Editable = isEditable
            };
            rendererText.Foreground = textForground;
            rendererText.Edited += CellEdited;
            _cellColumnsRender.Add(rendererText, (int)ColumnItem.health_insurance_id_number);
            _treeView.InsertColumn(-1, "Health insurance id number", rendererText, "text", (int)ColumnItem.health_insurance_id_number);
        }

        private void AddItem(object sender, EventArgs e)
        {
            string sql = "insert into person (name) values ('') ";
            Console.WriteLine (sql);
            DbCl.ExecuteTrans(DbCl.getConn(), sql);
            CreateItemsModel(true,entSearch.Text.Trim());
        }
        private void SelectItem(object sender, EventArgs e)
        {
            
            //lstItem = (Gtk.ListStore) treeItemTransaksi.Model;
            TreeSelection selection = _treeView.Selection;
    
            TreeIter iter;
            if(selection.GetSelected( out iter)){
                Console.WriteLine("Selected Value:"+_itemsModel.GetValue (iter, 0).ToString()+_itemsModel.GetValue (iter, 1).ToString());
            }
            if(this.prm=="customer"){
                ReferenceCustomer o = (ReferenceCustomer)this.parent;
                o.doChild("Okay! "+ _itemsModel.GetValue (iter, 1).ToString() +" selected",_itemsModel.GetValue (iter, 0).ToString());
            }else if(this.prm=="supplier"){
                ReferenceSupplier o = (ReferenceSupplier)this.parent;
                o.doChild("Okay! "+ _itemsModel.GetValue (iter, 1).ToString() +" selected, please continue filling out the form Organization",_itemsModel.GetValue (iter, 0).ToString());
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
                        string sql = "update person set name = '"+args.NewText+"' where id='"+_articles[i].id+"' ";
                        Console.WriteLine (sql);
                        DbCl.ExecuteTrans(DbCl.getConn(), sql);
                    }
                    break;
                case (int)ColumnItem.address:
                    {
                        int i = path.Indices[0];
                        _articles[i].address = args.NewText;
                        _itemsModel.SetValue(iter, column, _articles[i].address);
                        string sql = "update person set address = '"+args.NewText+"' where id='"+_articles[i].id+"' ";
                        Console.WriteLine (sql);
                        DbCl.ExecuteTrans(DbCl.getConn(), sql);
                    }
                    break;
                case (int)ColumnItem.phone_number:
                    {
                        int i = path.Indices[0];
                        _articles[i].phone_number = args.NewText;
                        _itemsModel.SetValue(iter, column, _articles[i].phone_number);
                        string sql = "update person set phone_number = '"+args.NewText+"' where id='"+_articles[i].id+"' ";
                        Console.WriteLine (sql);
                        DbCl.ExecuteTrans(DbCl.getConn(), sql);
                    }
                    break;
                case (int)ColumnItem.national_id_number:
                    {
                        int i = path.Indices[0];
                        _articles[i].national_id_number = args.NewText;
                        _itemsModel.SetValue(iter, column, _articles[i].national_id_number);
                        string sql = "update person set national_id_number = '"+args.NewText+"' where id='"+_articles[i].id+"' ";
                        Console.WriteLine (sql);
                        DbCl.ExecuteTrans(DbCl.getConn(), sql);
                    }
                    break;
                case (int)ColumnItem.tax_id_number:
                    {
                        int i = path.Indices[0];
                        _articles[i].tax_id_number = args.NewText;
                        _itemsModel.SetValue(iter, column, _articles[i].tax_id_number);
                        string sql = "update person set tax_id_number = '"+args.NewText+"' where id='"+_articles[i].id+"' ";
                        Console.WriteLine (sql);
                        DbCl.ExecuteTrans(DbCl.getConn(), sql);
                    }
                    break;
                case (int)ColumnItem.health_insurance_id_number:
                    {
                        int i = path.Indices[0];
                        _articles[i].health_insurance_id_number = args.NewText;
                        _itemsModel.SetValue(iter, column, _articles[i].health_insurance_id_number);
                        string sql = "update person set health_insurance_id_number = '"+args.NewText+"' where id='"+_articles[i].id+"' ";
                        Console.WriteLine (sql);
                        DbCl.ExecuteTrans(DbCl.getConn(), sql);
                    }
                    break;
                case (int)ColumnItem.is_active:
                    {
                        int i = path.Indices[0];
                        _articles[i].is_active = args.NewText;
                        _itemsModel.SetValue(iter, column, _articles[i].is_active);
                        string sql = "update person set is_active = '"+args.NewText+"' where id='"+_articles[i].id+"' ";
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