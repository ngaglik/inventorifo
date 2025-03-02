using System;
using System.Collections.Generic;
using Gtk;
using Gdk;
using System.Data;
using Pango;

namespace Inventorifo.App
{
    //[Section(ContentType = typeof(EditableCellsSection), Category = Category.Widgets)]
    class ReferenceCustomer : Gtk.Box
    {
        Inventorifo.Lib.LibDb DbCl = new Inventorifo.Lib.LibDb ();
        private TreeView _treeView;
        private ListStore _itemsModel;
        //private ListStore numbers_model;
        private Dictionary<CellRenderer, int> _cellColumnsRender;
        private List<Item> _articles;
        public MainWindow parent;
        private Entry entSearch;

        private Popover popover ;
        

        public ReferenceCustomer(object parent, string prm) : base(Orientation.Vertical, 6)
        {
            //this.parent = parent;
            Label lbTitle = new Label();
            lbTitle.Text = "Customer";
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
            _treeView.Columns[1].Visible = false;

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

            popover = new Popover(button);    
            

            entSearch.Changed += HandleEntSearchChanged;
        }

        private class Item
        { //
            public Item(string id, string person_id,string person_name, string person_address,string person_phone_number, string organization_name, string organization_address, string organization_phone_number, string is_active, string organization_tax_id_number){
                this.id = id;
                this.person_id = person_id;
                this.person_name = person_name;
                this.person_address = person_address;
                this.person_phone_number = person_phone_number;
                this.organization_name = organization_name;
                this.organization_address = organization_address;
                this.organization_phone_number = organization_phone_number;
                this.is_active = is_active;
                this.organization_tax_id_number = organization_tax_id_number;
            }
            public string id;
            public string person_id;
            public string person_name;
            public string person_address;
            public string person_phone_number;
            public string organization_name;
            public string organization_address;
            public string organization_phone_number;
            public string is_active;
            public string organization_tax_id_number;
        }

        private enum ColumnItem
        { //
            id,
            person_id,
            person_name,
            person_address,
            person_phone_number,
            organization_name,
            organization_address,
            organization_phone_number,
            is_active,
            organization_tax_id_number,
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
                _articles = new List<Item>();

                string whrfind = "";
                if(strfind!="") whrfind = "and (upper(pers.name) like upper('" + strfind + "%') OR upper(cust.organization_name) like upper('" + strfind + "%')   )";

                string sql = "SELECT cust.id ,pers.id person_id, pers.name person_name, pers.address person_address,  pers.phone_number person_phone_number, cust.organization_name, cust.organization_address, cust.organization_phone_number, cust.is_active, cust.organization_tax_id_number "+
                        "FROM customer cust left outer join person pers on cust.person_id=pers.id "+
                        "WHERE 1=1 "+ whrfind +
                        "ORDER by pers.name asc";
                        Console.WriteLine(sql);
              
                DataTable dttv = DbCl.fillDataTable(DbCl.getConn(), sql);
                foreach (DataRow dr in dttv.Rows)
                {                    
                    string id=dr[0].ToString();
                    string person_id=dr[1].ToString();
                    string person_name=dr[2].ToString();
                    string person_address=dr[3].ToString();
                    string person_phone_number=dr[4].ToString();
                    string organization_name=dr[5].ToString();
                    string organization_address=dr[6].ToString();
                    string organization_phone_number=dr[7].ToString();
                    string is_active=dr[8].ToString();
                    string organization_tax_id_number=dr[9].ToString();
                                    
                    _articles.Add(new Item(id, person_id, person_name, person_address,person_phone_number, organization_name, organization_address, organization_phone_number, is_active, organization_tax_id_number ));
                }

                /* create list store */
                //
                _itemsModel = new ListStore(typeof(string), typeof(string), typeof(string), typeof(string), typeof(string), typeof(string), typeof(string), typeof(string), typeof(string), typeof(string));

                /* add items */
                for (int i = 0; i < _articles.Count; i++)
                {
                    iter = _itemsModel.Append();
                    _itemsModel.SetValue(iter, (int)ColumnItem.id, _articles[i].id);
                    _itemsModel.SetValue(iter, (int)ColumnItem.person_id, _articles[i].person_id);
                    _itemsModel.SetValue(iter, (int)ColumnItem.person_name, _articles[i].person_name);
                    _itemsModel.SetValue(iter, (int)ColumnItem.person_address, _articles[i].person_address);
                    _itemsModel.SetValue(iter, (int)ColumnItem.person_phone_number, _articles[i].person_phone_number);
                    _itemsModel.SetValue(iter, (int)ColumnItem.organization_name, _articles[i].organization_name);
                    _itemsModel.SetValue(iter, (int)ColumnItem.organization_address, _articles[i].organization_address);
                    _itemsModel.SetValue(iter, (int)ColumnItem.organization_phone_number, _articles[i].organization_phone_number);                    
                    _itemsModel.SetValue(iter, (int)ColumnItem.is_active, _articles[i].is_active);
                    _itemsModel.SetValue(iter, (int)ColumnItem.organization_tax_id_number, _articles[i].organization_tax_id_number);
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
            _cellColumnsRender.Add(rendererText, (int)ColumnItem.id);
            _treeView.InsertColumn(-1, "PersonID", rendererText, "text", (int)ColumnItem.id);

            rendererText = new CellRendererText
            {
                Editable = true
            };
            rendererText.Foreground = "green";
            rendererText.Edited += CellEdited;
            _cellColumnsRender.Add(rendererText, (int)ColumnItem.person_name);
            _treeView.InsertColumn(-1, "Person Name", rendererText, "text", (int)ColumnItem.person_name);

            rendererText = new CellRendererText
            {
                Editable = true
            };
            rendererText.Foreground = "green";
            rendererText.Edited += CellEdited;
            _cellColumnsRender.Add(rendererText, (int)ColumnItem.person_address);
            _treeView.InsertColumn(-1, "Person Address", rendererText, "text", (int)ColumnItem.person_address);

            rendererText = new CellRendererText
            {
                Editable = true
            };
            rendererText.Foreground = "green";
            rendererText.Edited += CellEdited;
            _cellColumnsRender.Add(rendererText, (int)ColumnItem.person_phone_number);
            _treeView.InsertColumn(-1, "Person Phone Number", rendererText, "text", (int)ColumnItem.person_phone_number);

            rendererText = new CellRendererText
            {
                Editable = true
            };
            rendererText.Foreground = "green";
            rendererText.Edited += CellEdited;
            _cellColumnsRender.Add(rendererText, (int)ColumnItem.organization_name);
            _treeView.InsertColumn(-1, "Organization Name", rendererText, "text", (int)ColumnItem.organization_name);

            rendererText = new CellRendererText
            {
                Editable = true
            };
            rendererText.Foreground = "green";
            rendererText.Edited += CellEdited;
            _cellColumnsRender.Add(rendererText, (int)ColumnItem.organization_address);
            _treeView.InsertColumn(-1, "Organization Address", rendererText, "text", (int)ColumnItem.organization_address);

            rendererText = new CellRendererText
            {
                Editable = true
            };
            rendererText.Foreground = "green";
            rendererText.Edited += CellEdited;
            _cellColumnsRender.Add(rendererText, (int)ColumnItem.organization_phone_number);
            _treeView.InsertColumn(-1, "Organization Phone Number", rendererText, "text", (int)ColumnItem.organization_phone_number);
        

            ListStore lstModelCombo = new ListStore(typeof(string), typeof(string));
            lstModelCombo.AppendValues("true","true");
            lstModelCombo.AppendValues("false","false");
            CellRendererCombo rendererCombo = new CellRendererCombo
            { 
                Model = lstModelCombo,
                TextColumn = (int)ColumnNumber.Text,
                HasEntry = false,
                Editable = true
            };
            rendererCombo.Foreground = "green";
            rendererCombo.Edited += CellEdited;
            rendererCombo.EditingStarted += EditingStarted;
            _cellColumnsRender.Add(rendererCombo, (int)ColumnItem.is_active);
            _treeView.InsertColumn(-1, "Is Active", rendererCombo, "text", (int)ColumnItem.is_active);

            rendererText = new CellRendererText
            {
                Editable = true
            };
            rendererText.Foreground = "green";
            rendererText.Edited += CellEdited;
            _cellColumnsRender.Add(rendererText, (int)ColumnItem.organization_tax_id_number);
            _treeView.InsertColumn(-1, "Organization Tax ID Number", rendererText, "text", (int)ColumnItem.organization_tax_id_number);
        }

        static void RemoveAllWidgets(Container container)
        {
            foreach (Widget child in container.Children)
            {
                container.Remove(child);
                child.Destroy(); // Ensure the widget is completely removed
            }
        }
        public void doChild(object o){
            GLib.Timeout.Add(0, () =>
            {
                RemoveAllWidgets(popover);
                Label popoverLabel = new Label((string)o);
                popover.Add(popoverLabel);
                popover.SetSizeRequest(200, 20);
                popoverLabel.Show();
                return false;
            });
        }
        
        private void AddItem(object sender, EventArgs e)
        {
            RemoveAllWidgets(popover);        
            ReferencePerson widgetPerson = new ReferencePerson(this,"dialog",null);
            popover.Add(widgetPerson);
            popover.SetSizeRequest(800, 400);
            widgetPerson.Show();          
            popover.ShowAll();
           //  string sql = "insert into customer (name) values ('') ";
           //  Console.WriteLine (sql);
           //  DbCl.ExecuteTrans(DbCl.getConn(), sql);
           //  CreateItemsModel(true,entSearch.Text.Trim());
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
                case (int)ColumnItem.person_name:
                    {
                        int i = path.Indices[0];
                        _articles[i].person_name = args.NewText;
                        _itemsModel.SetValue(iter, column, _articles[i].person_name);
                        string sql = "update person set name = '"+args.NewText+"' where id='"+_articles[i].person_id+"' ";
                        Console.WriteLine (sql);
                        DbCl.ExecuteTrans(DbCl.getConn(), sql);
                    }
                    break;
                case (int)ColumnItem.person_address:
                    {
                        int i = path.Indices[0];
                        _articles[i].person_address = args.NewText;
                        _itemsModel.SetValue(iter, column, _articles[i].person_address);
                        string sql = "update person set address = '"+args.NewText+"' where id='"+_articles[i].person_id+"' ";
                        Console.WriteLine (sql);
                        DbCl.ExecuteTrans(DbCl.getConn(), sql);
                    }
                    break;
                case (int)ColumnItem.person_phone_number:
                    {
                        int i = path.Indices[0];
                        _articles[i].person_phone_number = args.NewText;
                        _itemsModel.SetValue(iter, column, _articles[i].person_phone_number);
                        string sql = "update person set phone_number = '"+args.NewText+"' where id='"+_articles[i].person_id+"' ";
                        Console.WriteLine (sql);
                        DbCl.ExecuteTrans(DbCl.getConn(), sql);
                    }
                    break;
                case (int)ColumnItem.organization_name:
                    {
                        int i = path.Indices[0];
                        _articles[i].organization_name = args.NewText;
                        _itemsModel.SetValue(iter, column, _articles[i].organization_name);
                        string sql = "update customer set organization_name = '"+args.NewText+"' where id='"+_articles[i].id+"' ";
                        Console.WriteLine (sql);
                        DbCl.ExecuteTrans(DbCl.getConn(), sql);
                    }
                    break;
                case (int)ColumnItem.organization_address:
                    {
                        int i = path.Indices[0];
                        _articles[i].organization_address = args.NewText;
                        _itemsModel.SetValue(iter, column, _articles[i].organization_address);
                        string sql = "update customer set organization_address = '"+args.NewText+"' where id='"+_articles[i].id+"' ";
                        Console.WriteLine (sql);
                        DbCl.ExecuteTrans(DbCl.getConn(), sql);
                    }
                    break;
                case (int)ColumnItem.organization_phone_number:
                    {
                        int i = path.Indices[0];
                        _articles[i].organization_phone_number = args.NewText;
                        _itemsModel.SetValue(iter, column, _articles[i].organization_phone_number);
                        string sql = "update customer set organization_phone_number = '"+args.NewText+"' where id='"+_articles[i].id+"' ";
                        Console.WriteLine (sql);
                        DbCl.ExecuteTrans(DbCl.getConn(), sql);
                    }
                    break;
                case (int)ColumnItem.organization_tax_id_number:
                    {
                        int i = path.Indices[0];
                        _articles[i].organization_tax_id_number = args.NewText;
                        _itemsModel.SetValue(iter, column, _articles[i].organization_tax_id_number);
                        string sql = "update customer set organization_tax_id_number = '"+args.NewText+"' where id='"+_articles[i].id+"' ";
                        Console.WriteLine (sql);
                        DbCl.ExecuteTrans(DbCl.getConn(), sql);
                    }
                    break;
                case (int)ColumnItem.is_active:
                    {
                        int i = path.Indices[0];
                        _articles[i].is_active = args.NewText;
                        _itemsModel.SetValue(iter, column, _articles[i].is_active);
                        string sql = "update customer set is_active = '"+args.NewText+"' where id='"+_articles[i].id+"' ";
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