using System;
using System.Collections.Generic;
using Gtk;
using Gdk;
using System.Data;
using Pango;
using Inventorifo.Lib.Model;

namespace Inventorifo.App
{
    //[Section(ContentType = typeof(EditableCellsSection), Category = Category.Widgets)]
    class ReferenceSupplier : Gtk.Box
    {
        Inventorifo.Lib.LibDb DbCl = new Inventorifo.Lib.LibDb ();
        Inventorifo.Lib.LibGui GuiCl = new Inventorifo.Lib.LibGui ();
        Inventorifo.Lib.LibCore CoreCl = new Inventorifo.Lib.LibCore ();

        private TreeView _treeView;
        private ListStore _itemsModel;
        //private ListStore numbers_model;
        private Dictionary<CellRenderer, int> _cellColumnsRender;
        private List<clsSupplier> _articles;
        private Popover popover ;
        private Entry entSearch;

        public object parent;
        Boolean isEditable;
        string textForground;
        string prm;
        string mode;

        public ReferenceSupplier(object parent, string mode,string prm) : base(Orientation.Vertical, 3)
        {
            this.parent=parent;
            this.prm = prm;
            this.mode=mode;

            Label lbTitle = new Label();
            lbTitle.Text = "Supplier";
            lbTitle.ModifyFont(FontDescription.FromString("Arial 18"));
            this.PackStart(lbTitle, false, true, 0);
             /* some buttons */
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
                        popover = new Popover(button);  
                    } 
                    break;
            } 
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
            //_treeView.Columns[1].Visible = false;

            CreateItemsModel(true,"");
            sw.Add(_treeView);            

            this.PackStart(hbox, false, false, 0);

            entSearch.Changed += HandleEntSearchChanged;
            entSearch.GrabFocus();
        }
      
        private enum ColumnItem
        { //
            id,
            organization_id,            
            organization_name,
            organization_address,
            organization_phone_number,
            organization_tax_id_number,
            person_id,
            person_name,
            person_address,
            person_phone_number,
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
                _articles = new List<clsSupplier>();

                string whrfind = "";
                if(strfind!="") whrfind = "and (upper(pers.name) like upper('" + strfind + "%') OR upper(supp.organization_name) like upper('" + strfind + "%')   )";

                string sql = "SELECT supp.id ,pers.id person_id, pers.name person_name, pers.address person_address,  pers.phone_number person_phone_number, org.id organization_id, org.name organization_name, org.address organization_address, org.phone_number organization_phone_number, org.is_active, org.tax_id_number organization_tax_id_number "+
                        "FROM supplier supp left outer join person pers on supp.person_id=pers.id left outer join organization org on supp.organization_id=org.id "+
                        "WHERE 1=1 "+ whrfind +
                        "ORDER by pers.name asc";
                        Console.WriteLine(sql);
                clsSupplier sto;
                DataTable dttv = DbCl.fillDataTable(DbCl.getConn(), sql);
                foreach (DataRow dr in dttv.Rows)
                {           
                    sto = new clsSupplier{          
                        id=dr["id"].ToString(),
                        organization_id=dr["organization_id"].ToString(),
                        organization_name=dr["organization_name"].ToString(),
                        organization_address=dr["organization_address"].ToString(),
                        organization_phone_number=dr["organization_phone_number"].ToString(),
                        organization_tax_id_number=dr["organization_tax_id_number"].ToString(),
                        person_id=dr["person_id"].ToString(),
                        person_name=dr["person_name"].ToString(),
                        person_address=dr["person_address"].ToString(),
                        person_phone_number=dr["person_phone_number"].ToString(),
                        is_active=dr["is_active"].ToString(),
                    };
                    _articles.Add(sto);             
                }

                /* create list store */
                //
                _itemsModel = new ListStore(typeof(string), typeof(string), typeof(string), typeof(string), typeof(string), typeof(string), typeof(string), typeof(string), typeof(string), typeof(string), typeof(string));

                /* add items */
                for (int i = 0; i < _articles.Count; i++)
                {
                    iter = _itemsModel.Append();
                    _itemsModel.SetValue(iter, (int)ColumnItem.id, _articles[i].id);
                    _itemsModel.SetValue(iter, (int)ColumnItem.organization_id, _articles[i].organization_id);
                    _itemsModel.SetValue(iter, (int)ColumnItem.organization_name, _articles[i].organization_name);
                    _itemsModel.SetValue(iter, (int)ColumnItem.organization_address, _articles[i].organization_address);
                    _itemsModel.SetValue(iter, (int)ColumnItem.organization_phone_number, _articles[i].organization_phone_number); 
                    _itemsModel.SetValue(iter, (int)ColumnItem.organization_tax_id_number, _articles[i].organization_tax_id_number);      
                    _itemsModel.SetValue(iter, (int)ColumnItem.person_id, _articles[i].person_id);
                    _itemsModel.SetValue(iter, (int)ColumnItem.person_name, _articles[i].person_name);
                    _itemsModel.SetValue(iter, (int)ColumnItem.person_address, _articles[i].person_address);
                    _itemsModel.SetValue(iter, (int)ColumnItem.person_phone_number, _articles[i].person_phone_number);             
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
            _cellColumnsRender.Add(rendererText, (int)ColumnItem.organization_id);
            _treeView.InsertColumn(-1, "Organization ID", rendererText, "text", (int)ColumnItem.organization_id);
            
            rendererText = new CellRendererText
            {
                Editable = isEditable
            };
            rendererText.Foreground = textForground;
            rendererText.Edited += CellEdited;
            _cellColumnsRender.Add(rendererText, (int)ColumnItem.organization_name);
            _treeView.InsertColumn(-1, "Organization Name", rendererText, "text", (int)ColumnItem.organization_name);

            rendererText = new CellRendererText
            {
                Editable = isEditable
            };
            rendererText.Foreground = textForground;
            rendererText.Edited += CellEdited;
            _cellColumnsRender.Add(rendererText, (int)ColumnItem.organization_address);
            _treeView.InsertColumn(-1, "Organization Address", rendererText, "text", (int)ColumnItem.organization_address);

            rendererText = new CellRendererText
            {
                Editable = isEditable
            };
            rendererText.Foreground = textForground;
            rendererText.Edited += CellEdited;
            _cellColumnsRender.Add(rendererText, (int)ColumnItem.organization_phone_number);
            _treeView.InsertColumn(-1, "Organization Phone Number", rendererText, "text", (int)ColumnItem.organization_phone_number);
            
             rendererText = new CellRendererText();
            _cellColumnsRender.Add(rendererText, (int)ColumnItem.person_id);
            _treeView.InsertColumn(-1, "PersonID", rendererText, "text", (int)ColumnItem.person_id);

            rendererText = new CellRendererText
            {
                Editable = isEditable
            };
            rendererText.Foreground = textForground;
            rendererText.Edited += CellEdited;
            _cellColumnsRender.Add(rendererText, (int)ColumnItem.person_name);
            _treeView.InsertColumn(-1, "Person Name", rendererText, "text", (int)ColumnItem.person_name);

            rendererText = new CellRendererText
            {
                Editable = isEditable
            };
            rendererText.Foreground = textForground;
            rendererText.Edited += CellEdited;
            _cellColumnsRender.Add(rendererText, (int)ColumnItem.person_address);
            _treeView.InsertColumn(-1, "Person Address", rendererText, "text", (int)ColumnItem.person_address);

            rendererText = new CellRendererText
            {
                Editable = isEditable
            };
            rendererText.Foreground = textForground;
            rendererText.Edited += CellEdited;
            _cellColumnsRender.Add(rendererText, (int)ColumnItem.person_phone_number);
            _treeView.InsertColumn(-1, "Person Phone Number", rendererText, "text", (int)ColumnItem.person_phone_number);


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
            rendererCombo.Foreground = "green";
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
            _cellColumnsRender.Add(rendererText, (int)ColumnItem.organization_tax_id_number);
            _treeView.InsertColumn(-1, "Organization Tax ID Number", rendererText, "text", (int)ColumnItem.organization_tax_id_number);

        }
        private void SelectItem(object sender, EventArgs e)
        {
            TreeSelection selection = _treeView.Selection;
            TreeIter iter;
            if(selection.GetSelected( out iter)){
                Console.WriteLine("Selected Value:"+_itemsModel.GetValue (iter, 0).ToString()+_itemsModel.GetValue (iter, 1).ToString());
            }            
            TransactionPurchase o = (TransactionPurchase)this.parent;
            o.doChildSupplier("Yeay! "+ _itemsModel.GetValue (iter, 2).ToString() +" selected",_itemsModel.GetValue (iter, 0).ToString());
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
                    TransactionPurchase o = (TransactionPurchase)this.parent;
                    o.doChildSupplier("Yeay! "+ _itemsModel.GetValue (iter, 2).ToString() +" selected",_itemsModel.GetValue (iter, 0).ToString());
                }
            }
        }
        public void doChild(object o,string prm){
            GLib.Timeout.Add(0, () =>
            {
                GuiCl.RemoveAllWidgets(popover);
                Label popoverLabel = new Label((string)o);
                popover.Add(popoverLabel);
                popover.SetSizeRequest(400, 20);
                popoverLabel.Show();

                string sql = "insert into supplier (person_id) values ('"+prm+"') ";
                Console.WriteLine (sql);
                DbCl.ExecuteTrans(DbCl.getConn(), sql);
                CreateItemsModel(true,entSearch.Text.Trim());
                return false;
            });
        }

        public void doChildOrganization(object o, clsOrganization prm){
            GLib.Timeout.Add(0, () =>
            {
                GuiCl.RemoveAllWidgets(popover);
                Label popoverLabel = new Label(prm.name);
                popover.Add(popoverLabel);
                popover.SetSizeRequest(400, 20);
                popoverLabel.Show();

                string sql = "insert into supplier (organization_id) values ('"+prm.id+"') ";
                Console.WriteLine (sql);
                DbCl.ExecuteTrans(DbCl.getConn(), sql);
                CreateItemsModel(true,entSearch.Text.Trim());
                return false;
            });
        }

        private void AddItem(object sender, EventArgs e)
        {
            GuiCl.RemoveAllWidgets(popover);        
            ReferenceOrganization widgetPerson = new ReferenceOrganization(this,"dialog","supplier");
            popover.Add(widgetPerson);
            popover.SetSizeRequest(800, 400);
            widgetPerson.Show();          
            popover.ShowAll();
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
                case (int)ColumnItem.organization_name:
                    {
                        int i = path.Indices[0];
                        _articles[i].organization_name = args.NewText;
                        _itemsModel.SetValue(iter, column, _articles[i].organization_name);
                        string sql = "update organization set name = '"+args.NewText+"' where id='"+_articles[i].organization_id+"' ";
                        Console.WriteLine (sql);
                        DbCl.ExecuteTrans(DbCl.getConn(), sql);
                    }
                    break;
                case (int)ColumnItem.organization_address:
                    {
                        int i = path.Indices[0];
                        _articles[i].organization_address = args.NewText;
                        _itemsModel.SetValue(iter, column, _articles[i].organization_address);
                        string sql = "update organization set address = '"+args.NewText+"' where id='"+_articles[i].organization_id+"' ";
                        Console.WriteLine (sql);
                        DbCl.ExecuteTrans(DbCl.getConn(), sql);
                    }
                    break;
                case (int)ColumnItem.organization_phone_number:
                    {
                        int i = path.Indices[0];
                        _articles[i].organization_phone_number = args.NewText;
                        _itemsModel.SetValue(iter, column, _articles[i].organization_phone_number);
                        string sql = "update organization set phone_number = '"+args.NewText+"' where id='"+_articles[i].organization_id+"' ";
                        Console.WriteLine (sql);
                        DbCl.ExecuteTrans(DbCl.getConn(), sql);
                    }
                    break;
                case (int)ColumnItem.organization_tax_id_number:
                    {
                        int i = path.Indices[0];
                        _articles[i].organization_tax_id_number = args.NewText;
                        _itemsModel.SetValue(iter, column, _articles[i].organization_tax_id_number);
                        string sql = "update organization set tax_id_number = '"+args.NewText+"' where id='"+_articles[i].organization_id+"' ";
                        Console.WriteLine (sql);
                        DbCl.ExecuteTrans(DbCl.getConn(), sql);
                    }
                    break;
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
                case (int)ColumnItem.is_active:
                    {
                        int i = path.Indices[0];
                        _articles[i].is_active = args.NewText;
                        _itemsModel.SetValue(iter, column, _articles[i].is_active);
                        string sql = "update supplier set is_active = '"+args.NewText+"' where id='"+_articles[i].id+"' ";
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