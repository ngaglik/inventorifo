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
    class TransactionSale : Gtk.Box
    {
        Inventorifo.Lib.LibDb DbCl = new Inventorifo.Lib.LibDb ();
        Inventorifo.Lib.LibGui GuiCl = new Inventorifo.Lib.LibGui ();
        public MainWindow parent;
        public string prm;
        public TransactionSale(object parent, string prm) : this(new Builder("TransactionSale.glade")) { 
            this.parent=(MainWindow)parent;
            this.prm = prm;
            Console.WriteLine(this.parent.user.id + " "+ this.parent.user.person_name);
            
        }
        private TreeView _treeView;
        private ListStore _transModel;
        private Dictionary<CellRenderer, int> _cellColumnsRender;
        private List<TransactionHistory> _articles;

        private TreeView _treeViewItems;
        private ListStore _itemsModel;
        private Dictionary<CellRenderer, int> _cellColumnsRenderItems;
        private List<TransactionHistory> _articlesItems;
        
        Box  boxMiddle;
        Box boxItem;
        SpinButton spnQty;

        private Entry entSearch;
        private Entry entBarcode;
        private Popover popoverSupplier ;
        private Popover popoverProduct ;
        private TextView textViewSupplier;
        private TextView textViewProduct;

        DataTable dtTransSelected;
        DataTable dtItemSelected;

        public TransactionSale(Builder builder) : base(builder.GetRawOwnedObject("TransactionSale"))
        {
            builder.Autoconnect(this);

            Label lbTitle = (Label)builder.GetObject("LbTitle");
            lbTitle.Text = "Sale";
            lbTitle.ModifyFont(FontDescription.FromString("Arial 18"));

            Box  boxMiddle = (Box)builder.GetObject("BoxMiddle");
            boxMiddle.SetSizeRequest(-1, -1); // Allow dynamic resizing
            boxMiddle.Expand = true;
            boxItem = (Box)builder.GetObject("BoxItem");
            
            spnQty = (SpinButton)builder.GetObject("SpnQty");
            
            Button btnProcessCheckout = (Button)builder.GetObject("BtnProcessCheckout");
            btnProcessCheckout.Clicked += DoCheckout;

            Button  btnProduct = (Button)builder.GetObject("BtnProduct");
            popoverProduct = new Popover(btnProduct);    
            btnProduct.Clicked += ShowProductPopup;

            Button  btnSupplier = (Button)builder.GetObject("BtnSupplier");
            popoverSupplier = new Popover(btnSupplier);    
            btnSupplier.Clicked += ShowSupplierPopup;

            Button  btnAdd = (Button)builder.GetObject("BtnAdd");
            btnAdd.Clicked += AddTransaction;
            
            entSearch = (Entry)builder.GetObject("EntSearch");
            entSearch.Changed += HandleEntSearchChanged;

            _treeView = (TreeView)builder.GetObject("TreeViewTrans");
            _treeView.Selection.Mode = SelectionMode.Single;
            _cellColumnsRender = new Dictionary<CellRenderer, int>();
            AddColumns();
            _treeView.Selection.Changed += OnTreeSelectionChanged;

            _treeViewItems = (TreeView)builder.GetObject("TreeViewItem");
            _treeViewItems.Selection.Mode = SelectionMode.Single;
            _cellColumnsRenderItems = new Dictionary<CellRenderer, int>();
    
            
            textViewProduct = (TextView)builder.GetObject("TextViewProduct");
            textViewSupplier = (TextView)builder.GetObject("TextViewSupplier");
            SetTransactionModel(true,entSearch.Text.Trim());
            GuiCl.SensitiveAllWidgets(boxItem,false);
        }

        private class TransactionHistory
        {                             //  id, supplier_id,organization_name, organization_phone_number, person_name, phone_number, transaction_date, state, user_id, application_id
            public TransactionHistory(string id, string supplier_id, string organization_name,string organization_address,string organization_phone_number,string person_name,string person_phone_number, string transaction_date, string state, string user_id, string application_id ){
                this.id = id;
                this.supplier_id = supplier_id;
                this.organization_name = organization_name;
                this.organization_address = organization_address;
                this.organization_phone_number = organization_phone_number;
                this.person_name = person_name;
                this.person_phone_number = person_phone_number;
                this.transaction_date = transaction_date;
                this.state = state;
                this.user_id = user_id;
                this.application_id = application_id;
            }
            public string id;
            public string supplier_id;
            public string organization_name;
            public string organization_address;
            public string organization_phone_number;
            public string person_name;
            public string person_phone_number;
            public string transaction_date;
            public string state;
            public string user_id;
            public string application_id;
        }

        private enum ColumnItem
        { //
            id,
            supplier_id,
            organization_name,
            organization_address,
            organization_phone_number,
            person_name,
            person_phone_number,
            transaction_date,
            state,
            user_id,
            application_id,
            Num
        };

        private enum ColumnNumber
        {
            Text,
            Num
        };
        private void TransactionReady(){
            GuiCl.SensitiveAllWidgets(boxItem,false);     
            textViewSupplier.Buffer.Text = "";
            textViewProduct.Buffer.Text = "";
            spnQty.Text = "1";

        }
        private void ItemTransactionReady(){
            GuiCl.SensitiveAllWidgets(boxItem,true);  
            //ShowProductPopup(new object(),new EventArgs());    
            spnQty.Text = "1";           
        }
       private void OnTreeSelectionChanged(object sender, EventArgs e)
        {
            if (!_treeView.Selection.GetSelected(out TreeIter it))
                 return;
            TreePath path = _transModel.GetPath(it);
            var organization_name = (string)_transModel.GetValue(it, (int)ColumnItem.organization_name);
            var organization_address = (string)_transModel.GetValue(it, (int)ColumnItem.organization_address);
            var organization_phone_number = (string)_transModel.GetValue(it, (int)ColumnItem.organization_phone_number);
            var person_name = (string)_transModel.GetValue(it, (int)ColumnItem.person_name);
            var person_phone_number = (string)_transModel.GetValue(it, (int)ColumnItem.person_phone_number);

            var tag = new TextTag (null);
            textViewSupplier.Buffer.TagTable.Add (tag);
            tag.Weight = Pango.Weight.Bold;

            textViewSupplier.Buffer.Text = "\nOrganization name:\n\nOrganization address:\n\nPhone number:\n\nPerson name:\n\nPhone number:\n\n";
            var iter = textViewSupplier.Buffer.GetIterAtLine (2);
            textViewSupplier.Buffer.InsertWithTags (ref iter, organization_name, tag);

            iter = textViewSupplier.Buffer.GetIterAtLine (4);
            textViewSupplier.Buffer.InsertWithTags (ref iter, organization_address, tag);

            iter = textViewSupplier.Buffer.GetIterAtLine (6);
            textViewSupplier.Buffer.InsertWithTags (ref iter, organization_phone_number, tag);

            iter = textViewSupplier.Buffer.GetIterAtLine (8);
            textViewSupplier.Buffer.InsertWithTags (ref iter, person_name, tag);

            iter = textViewSupplier.Buffer.GetIterAtLine (10);
            textViewSupplier.Buffer.InsertWithTags (ref iter, person_phone_number, tag);
            ItemTransactionReady();

        }
        private void HandleEntSearchChanged(object sender, EventArgs e)
        {
            Entry entry = sender as Entry;
            if (entry != null)
            {
                SetTransactionModel(true,entry.Text.Trim());
            }
        }
        private void AddTransaction(object sender, EventArgs e)
        {
            string sql = "insert into transaction (transaction_type,transaction_date,input_date,supplier_id,user_id,application_id) "+
            "values (2,CURRENT_DATE,CURRENT_DATE,1,"+this.parent.user.id+",'"+this.parent.application_id+"') ";
            Console.WriteLine (sql);
            DbCl.ExecuteTrans(DbCl.getConn(), sql);
            SetTransactionModel(true,entSearch.Text.Trim());            
            GuiCl.SensitiveAllWidgets(boxItem,true);
        }
        private void SetTransactionModel(Boolean showAll,string strfind)
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
                string sql = "select tr.id,tr.supplier_id,sup.organization_name,sup.organization_address,sup.organization_phone_number,pers.name person_name,pers.phone_number,TO_CHAR(tr.transaction_date,'yyyy-mm-dd') transaction_date,tr.state,tr.user_id,tr.application_id "+
                "from transaction tr left outer join supplier sup on tr.supplier_id=sup.id "+
                "left outer join person pers on sup.person_id=pers.id,(select usr.id, pers.name person_name, pers.phone_number from person pers,userlogin usr where usr.person_id=pers.id) uspers "+
                "where tr.transaction_type=1 and tr.transaction_date = CURRENT_date and uspers.id=tr.user_id "+
                "ORDER by tr.id desc";
                Console.WriteLine(sql);
                dtTransSelected =  DbCl.fillDataTable(DbCl.getConn(), sql);
                foreach (DataRow dr in dtTransSelected.Rows)
                {                    
                    string id=dr[0].ToString(); 
                    string supplier_id=dr[1].ToString(); 
                    string organization_name=dr[2].ToString(); 
                    string organization_address=dr[3].ToString(); 
                    string organization_phone_number=dr[4].ToString(); 
                    string person_name=dr[5].ToString(); 
                    string phone_number=dr[6].ToString(); 
                    string transaction_date=dr[7].ToString();   
                    string state=dr[8].ToString(); 
                    string user_id=dr[9].ToString(); 
                    string application_id=dr[10].ToString();                                    
                    _articles.Add(new TransactionHistory(id, supplier_id,organization_name,organization_address, organization_phone_number, person_name, phone_number, transaction_date, state, user_id, application_id));
                }
                //Console.WriteLine(_articles[0].transaction_date);

                /* create list store */
                //
                _transModel = new ListStore(typeof(string),typeof(string),typeof(string),typeof(string),typeof(string),typeof(string),typeof(string),typeof(string),typeof(string));

                /* add items */
                for (int i = 0; i < _articles.Count; i++)
                {
                    iter = _transModel.Append();
                    _transModel.SetValue(iter, (int)ColumnItem.id, _articles[i].id);
                    _transModel.SetValue(iter, (int)ColumnItem.supplier_id, _articles[i].supplier_id);
                    _transModel.SetValue(iter, (int)ColumnItem.organization_name, _articles[i].organization_name);
                    _transModel.SetValue(iter, (int)ColumnItem.organization_address, _articles[i].organization_address);
                    _transModel.SetValue(iter, (int)ColumnItem.organization_phone_number, _articles[i].organization_phone_number);
                    _transModel.SetValue(iter, (int)ColumnItem.person_name, _articles[i].person_name);
                    _transModel.SetValue(iter, (int)ColumnItem.person_phone_number, _articles[i].person_phone_number);
                    _transModel.SetValue(iter, (int)ColumnItem.transaction_date, _articles[i].transaction_date);
                    _transModel.SetValue(iter, (int)ColumnItem.state, _articles[i].state);
                    _transModel.SetValue(iter, (int)ColumnItem.user_id, _articles[i].user_id);
                    _transModel.SetValue(iter, (int)ColumnItem.application_id, _articles[i].application_id);
                }
                _treeView.Model = _transModel;           
            }
        }

        private void setDtItem(string prm)
        {                          
            dtItemSelected = new DataTable();
            string sql = "SELECT prod.id, prod.short_name, prod.name prod_name, prod.barcode, prod.product_group, prodgr.name product_group_name "+
                    "FROM product prod, product_group prodgr "+
                    "WHERE prod.product_group = prodgr.id and prod.id= "+prm;
                    Console.WriteLine(sql);          
            dtItemSelected = DbCl.fillDataTable(DbCl.getConn(), sql);   

            var tag = new TextTag (null);
            textViewProduct.Buffer.TagTable.Add (tag);
            tag.Weight = Pango.Weight.Bold;

            textViewProduct.Buffer.Text = "\nShort:\n\nName:\n\nBarcode:\n\nGroup:\n\n";
            var iter = textViewSupplier.Buffer.GetIterAtLine (2);
            textViewProduct.Buffer.InsertWithTags (ref iter, dtItemSelected.Rows[0].ItemArray[1].ToString(), tag);

            iter = textViewProduct.Buffer.GetIterAtLine (4);
            textViewProduct.Buffer.InsertWithTags (ref iter, dtItemSelected.Rows[0].ItemArray[2].ToString(), tag);

            iter = textViewProduct.Buffer.GetIterAtLine (6);
            textViewProduct.Buffer.InsertWithTags (ref iter, dtItemSelected.Rows[0].ItemArray[3].ToString(), tag);
            
            iter = textViewProduct.Buffer.GetIterAtLine (8);
            textViewProduct.Buffer.InsertWithTags (ref iter, dtItemSelected.Rows[0].ItemArray[5].ToString(), tag);
        }
        public void InsertStock(){
            string sql = "insert into stock (product_id,quantity,input_date,expired_date,purchase_price, unit, condition, location, price_id)"+
            "values ("+dtItemSelected.Rows[0].ItemArray[0].ToString()+ ","+spnQty.Text+",CURRENT_DATE,)";
        }
        private void AddSelectedItem()
        {
            string sql = "insert into transaction_item (transaction_id,product_id,stock_id,purchase_price,state) "+
            "values("+dtTransSelected.Rows[0].ItemArray[0].ToString()+ ","+dtItemSelected.Rows[0].ItemArray[0].ToString()+ ")" ;
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
            _cellColumnsRender.Add(rendererText, (int)ColumnItem.supplier_id);
            _treeView.InsertColumn(-1, "Supplier ID", rendererText, "text", (int)ColumnItem.supplier_id);            

            rendererText = new CellRendererText();
            _cellColumnsRender.Add(rendererText, (int)ColumnItem.organization_name);
            _treeView.InsertColumn(-1, "Organization Name", rendererText, "text", (int)ColumnItem.organization_name); 

            rendererText = new CellRendererText();
            _cellColumnsRender.Add(rendererText, (int)ColumnItem.organization_phone_number);
            _treeView.InsertColumn(-1, "Organization Phone Number", rendererText, "text", (int)ColumnItem.organization_phone_number);

            rendererText = new CellRendererText();
            _cellColumnsRender.Add(rendererText, (int)ColumnItem.person_name);
            _treeView.InsertColumn(-1, "Person Name", rendererText, "text", (int)ColumnItem.person_name);

            rendererText = new CellRendererText();
            _cellColumnsRender.Add(rendererText, (int)ColumnItem.transaction_date);
            _treeView.InsertColumn(-1, "Transaction Date", rendererText, "text", (int)ColumnItem.transaction_date);

            rendererText = new CellRendererText();
            _cellColumnsRender.Add(rendererText, (int)ColumnItem.state);
            _treeView.InsertColumn(-1, "State", rendererText, "text", (int)ColumnItem.state);

            rendererText = new CellRendererText();
            _cellColumnsRender.Add(rendererText, (int)ColumnItem.user_id);
            _treeView.InsertColumn(-1, "User ID", rendererText, "text", (int)ColumnItem.user_id);

            rendererText = new CellRendererText();
            _cellColumnsRender.Add(rendererText, (int)ColumnItem.application_id);
            _treeView.InsertColumn(-1, "Application ID", rendererText, "text", (int)ColumnItem.application_id);

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
            GLib.Timeout.Add(5, () =>
            {
                GuiCl.RemoveAllWidgets(popoverProduct);
                Label popLabel = new Label((string)o);
                popoverProduct.Add(popLabel);
                popoverProduct.SetSizeRequest(200, 20);
                popLabel.Show();
                setDtItem(prm);
                spnQty.GrabFocus();
                return false;
            });
        }
        
        private void ShowProductPopup(object sender, EventArgs e)
        {   GLib.Timeout.Add(5, () =>
            {     
                GuiCl.RemoveAllWidgets(popoverProduct);        
                ReferenceProduct refWidget = new ReferenceProduct(this,"dialog","purchase");
                popoverProduct.Add(refWidget);
                popoverProduct.SetSizeRequest(500, 300);
                refWidget.Show();          
                popoverProduct.ShowAll();
                return false;
            });
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
           // SetTransactionModel(true,entSearch.Text.Trim());
        }
        
        private void ShowSupplierPopup(object sender, EventArgs e)
        {
            //GuiCl.RemoveAllWidgets(popover);        
            //                  
            GuiCl.RemoveAllWidgets(popoverSupplier);        
            ReferenceSupplier refWidget = new ReferenceSupplier(this,"dialog","purchase");
            popoverSupplier.Add(refWidget);
            popoverSupplier.SetSizeRequest(400, 300);
            refWidget.Show();          
            popoverSupplier.ShowAll();
        }
        private void DoCheckout(object sender, EventArgs e)
        {
            TransactionReady();
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