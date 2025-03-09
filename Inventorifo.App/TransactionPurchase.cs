using System;
using System.Collections.Generic;
using Gtk;
using Gdk;
using System.Data;
using Pango;
using UI = Gtk.Builder.ObjectAttribute;

namespace Inventorifo.App
{
    //[Section(ContentType = typeof(EditableCellsSection), Category = Category.Widgets)]
    class TransactionPurchase : Gtk.Box
    {
        Inventorifo.Lib.LibDb DbCl = new Inventorifo.Lib.LibDb ();
        Inventorifo.Lib.LibGui GuiCl = new Inventorifo.Lib.LibGui ();
        public MainWindow parent;
        public string prm;
        public TransactionPurchase(object parent, string prm) : this(new Builder("TransactionPurchase.glade")) { 
            this.parent=(MainWindow)parent;
            this.prm = prm;
            Console.WriteLine(this.parent.user.id + " "+ this.parent.user.person_name);
            
        }
        private TreeView _treeView;
        private ListStore _transModel;
        private TreeView _treeViewItems;
        private ListStore _itemsModel;
        //private ListStore numbers_model;
        private Dictionary<CellRenderer, int> _cellColumnsRender;
        private List<TransactionHistory> _articles;

        private Entry entSearch;
        private Entry entBarcode;
        private Popover popoverSupplier ;
        private Popover popoverProduct ;

        public TransactionPurchase(Builder builder) : base(builder.GetRawOwnedObject("TransactionPurchase"))
        {
            builder.Autoconnect(this);

            Label lbTitle = (Label)builder.GetObject("LbTitle");
            lbTitle.Text = "Purchase";
            lbTitle.ModifyFont(FontDescription.FromString("Arial 18"));

            Box  boxMiddle = (Box)builder.GetObject("BoxMiddle");
            boxMiddle.SetSizeRequest(-1, -1); // Allow dynamic resizing
            boxMiddle.Expand = true;

            Button  btnProduct = (Button)builder.GetObject("BtnProduct");
            popoverProduct = new Popover(btnProduct);    
            btnProduct.Clicked += AddProduct;

            Button  btnSupplier = (Button)builder.GetObject("BtnSupplier");
            popoverSupplier = new Popover(btnSupplier);    
            btnSupplier.Clicked += AddSupplier;

            Button  btnAdd = (Button)builder.GetObject("BtnAdd");
            btnAdd.Clicked += AddItem;
            
            Entry entSearch = (Entry)builder.GetObject("EntSearch");
            entSearch.Changed += HandleEntSearchChanged;

            _treeView = (TreeView)builder.GetObject("TreeViewTrans");
            _treeView.Selection.Mode = SelectionMode.Single;

            _treeViewItems = (TreeView)builder.GetObject("TreeViewItem");
            _treeViewItems.Selection.Mode = SelectionMode.Single;
            _cellColumnsRender = new Dictionary<CellRenderer, int>();
            AddColumns();

            CreateItemsModel(true,entSearch.Text.Trim());
        }

        private class TransactionHistory
        { //
            public TransactionHistory(string id, string transaction_date){
                this.id = id;
                this.transaction_date = transaction_date;
            }
            public string id;
            public string transaction_date;
        }

        private enum ColumnItem
        { //
            id,
            transaction_date,
            Num
        };

        private enum ColumnNumber
        {
            Text,
            Num
        };
        
        private void HandleEntSearchChanged(object sender, EventArgs e)
        {
            Entry entry = sender as Entry;
            if (entry != null)
            {
                CreateItemsModel(true,entry.Text.Trim());
            }
        }
        private void CreateItemsModel(Boolean showAll,string strfind)
        {      
            if(strfind=="" && !showAll) {          
                _treeView.Model = null;
            }else{
                //ListStore model;
                _transModel = null;
                TreeIter iter;
                /* create array */
                _articles = new List<TransactionHistory>();

                string whrfind = "";
                if(strfind!="") whrfind = "and (upper(sup.organization_name) like upper('" + strfind + "%') or upper(pers.name) like upper('" + strfind + "%') )";
                string sql = "select tr.id,TO_CHAR(tr.transaction_date,'yyyy-mm-dd') transaction_date,tr.state,tr.supplier_id,sup.organization_name,sup.organization_phone_number,pers.name person_name,pers.phone_number,tr.user_id,tr.application_id "+
                "from transaction tr left outer join supplier sup on tr.supplier_id=sup.id "+
                "left outer join person pers on sup.person_id=pers.id,(select usr.id, pers.name person_name, pers.phone_number from person pers,userlogin usr where usr.person_id=pers.id) uspers "+
                "where tr.transaction_type=1 and tr.transaction_date = CURRENT_date and uspers.id=tr.user_id "+
                "ORDER by tr.id desc";
                Console.WriteLine(sql);
              
                DataTable dttv = DbCl.fillDataTable(DbCl.getConn(), sql);
                foreach (DataRow dr in dttv.Rows)
                {                    
                    string id=dr[0].ToString(); 
                    string transaction_date=dr[1].ToString();                                    
                    _articles.Add(new TransactionHistory(id, transaction_date));
                }
                //Console.WriteLine(_articles[0].transaction_date);

                /* create list store */
                //
                _transModel = new ListStore(typeof(string), typeof(string));

                /* add items */
                for (int i = 0; i < _articles.Count; i++)
                {
                    iter = _transModel.Append();
                    _transModel.SetValue(iter, (int)ColumnItem.id, _articles[i].id);
                    _transModel.SetValue(iter, (int)ColumnItem.transaction_date, _articles[i].transaction_date);
                }
                _treeView.Model = _transModel;           

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
            _cellColumnsRender.Add(rendererText, (int)ColumnItem.transaction_date);
            _treeView.InsertColumn(-1, "Name", rendererText, "text", (int)ColumnItem.transaction_date);            

        }

        private void AddItem(object sender, EventArgs e)
        {

            string sql = "insert into transaction (transaction_type,transaction_date,input_date,supplier_id,user_id,application_id) "+
            "values (1,CURRENT_DATE,CURRENT_DATE,1,"+this.parent.user.id+",'"+this.parent.application_id+"') ";
            Console.WriteLine (sql);
            DbCl.ExecuteTrans(DbCl.getConn(), sql);
           // CreateItemsModel(true,entSearch.Text.Trim());
        }

        private void RemoveItem(object sender, EventArgs e)
        {
            
        }

        private void CellEdited(object data, EditedArgs args)
        {
           TreePath path = new TreePath(args.Path);
            int column = _cellColumnsRender[(CellRenderer)data];
            _transModel.GetIter(out TreeIter iter, path);

        }

        public void doChildProduct(object o,string prm){
            GLib.Timeout.Add(0, () =>
            {
                GuiCl.RemoveAllWidgets(popoverProduct);
                Label popLabel = new Label((string)o);
                popoverProduct.Add(popLabel);
                popoverProduct.SetSizeRequest(200, 20);
                popLabel.Show();
                Console.WriteLine("Your'e select "+prm);
                return false;
            });
        }
        
        private void AddProduct(object sender, EventArgs e)
        {
            //GuiCl.RemoveAllWidgets(popover);        
            //                  
            GuiCl.RemoveAllWidgets(popoverProduct);        
            ReferenceProduct refWidget = new ReferenceProduct(this,"dialog","purchase");
            popoverProduct.Add(refWidget);
            popoverProduct.SetSizeRequest(800, 300);
            refWidget.Show();          
            popoverProduct.ShowAll();
        }

        public void doChildSupplier(object o,string prm){
            GLib.Timeout.Add(0, () =>
            {
                GuiCl.RemoveAllWidgets(popoverSupplier);
                Label popLabel = new Label((string)o);
                popoverSupplier.Add(popLabel);
                popoverSupplier.SetSizeRequest(200, 20);
                popLabel.Show();
                string sql = "update transaction set supplier_id='"+prm+"' where id= ";
                Console.WriteLine (sql);
                DbCl.ExecuteTrans(DbCl.getConn(), sql);
                
                return false;
            });
           // CreateItemsModel(true,entSearch.Text.Trim());
        }
        
        private void AddSupplier(object sender, EventArgs e)
        {
            //GuiCl.RemoveAllWidgets(popover);        
            //                  
            GuiCl.RemoveAllWidgets(popoverSupplier);        
            ReferenceSupplier refWidget = new ReferenceSupplier(this,"dialog","purchase");
            popoverSupplier.Add(refWidget);
            popoverSupplier.SetSizeRequest(800, 300);
            refWidget.Show();          
            popoverSupplier.ShowAll();
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