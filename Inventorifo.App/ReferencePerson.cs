using System;
using System.Collections.Generic;
using Gtk;
using System.Data;

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

        private Entry entSearch;

        public ReferencePerson() : base(Orientation.Vertical, 3)
        {
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
                Editable = true
            };
            rendererText.Foreground = "green";
            rendererText.Edited += CellEdited;
            _cellColumnsRender.Add(rendererText, (int)ColumnItem.name);
            _treeView.InsertColumn(-1, "Name", rendererText, "text", (int)ColumnItem.name);

            rendererText = new CellRendererText
            {
                Editable = true
            };
            rendererText.Foreground = "green";
            rendererText.Edited += CellEdited;
            _cellColumnsRender.Add(rendererText, (int)ColumnItem.address);
            _treeView.InsertColumn(-1, "Address", rendererText, "text", (int)ColumnItem.address);

            rendererText = new CellRendererText
            {
                Editable = true
            };
            rendererText.Foreground = "green";
            rendererText.Edited += CellEdited;
            _cellColumnsRender.Add(rendererText, (int)ColumnItem.is_active);
            _treeView.InsertColumn(-1, "Is_active", rendererText, "text", (int)ColumnItem.is_active);

            rendererText = new CellRendererText
            {
                Editable = true
            };
            rendererText.Foreground = "green";
            rendererText.Edited += CellEdited;
            _cellColumnsRender.Add(rendererText, (int)ColumnItem.national_id_number);
            _treeView.InsertColumn(-1, "National_id_number", rendererText, "text", (int)ColumnItem.national_id_number);

            rendererText = new CellRendererText
            {
                Editable = true
            };
            rendererText.Foreground = "green";
            rendererText.Edited += CellEdited;
            _cellColumnsRender.Add(rendererText, (int)ColumnItem.tax_id_number);
            _treeView.InsertColumn(-1, "Tax_id_number", rendererText, "text", (int)ColumnItem.tax_id_number);

            rendererText = new CellRendererText
            {
                Editable = true
            };
            rendererText.Foreground = "green";
            rendererText.Edited += CellEdited;
            _cellColumnsRender.Add(rendererText, (int)ColumnItem.health_insurance_id_number);
            _treeView.InsertColumn(-1, "Health_insurance_id_number", rendererText, "text", (int)ColumnItem.health_insurance_id_number);
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
                case (int)ColumnItem.name:
                    {
                        int i = path.Indices[0];
                        //_articles[i].short_name = int.Parse(args.NewText);
                        _articles[i].name = args.NewText;
                        _itemsModel.SetValue(iter, column, _articles[i].name);
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