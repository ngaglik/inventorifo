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
        private TreeView _treeViewTrans;
        private ListStore _lsModelTrans;
        private Dictionary<CellRenderer, int> _cellColumnsRender;
        private List<clTransaction> _clsTrans;

        private TreeView _treeViewItems;
        private ListStore _lsModelItems;
        private Dictionary<CellRenderer, int> _cellColumnsRenderItems;
        private List<clTransItem> _clsItems;
        
        Box  boxMiddle;
        Box boxItem;
        Box boxTransaction;
        SpinButton spnQty;
        Button  btnNew;
        private Entry entSearch;
        private Entry entBarcode;
        private Entry entAmountPayment;
        private Popover popoverSupplier ;
        private Popover popoverProduct ;
        private TextView textViewSupplier;
        private TextView textViewProduct;

        DataTable dtTransSelected;
        DataTable dtItemSelected;

        Label lbTransactionId;
        Label lbTotalItem;
        Label lbTotalPrice;

        string textForground;
        Boolean isEditable;

        ComboBoxText cmbPaymentMethod;
        Dictionary<int, string> dictPaymentMethod = new Dictionary<int, string>();
        Gtk.ListStore lsPaymentMethod = new ListStore(typeof(string), typeof(string));
        CellRendererCombo cellComboPaymentMethod = new Gtk.CellRendererCombo();

        private enum ColumnTrans
        { 
            id,
            reference_id,
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

        private enum ColNumberTrans
        {
            Text,
            Num
        };

        private enum ColumnItems
        { 
            id,
            transaction_id,
            product_id,
            product_name,
            stock_id,
            quantity,
            unit,
            unit_name,
            purchase_price,
            price_id,
            price,
            tax,
            state,
            location,
            location_name,
            condition,
            condition_name,
            Num
        };
       
        private enum ColNumberItems
        {
            Text,
            Num
        };


        public TransactionPurchase(Builder builder) : base(builder.GetRawOwnedObject("TransactionPurchase"))
        {
            builder.Autoconnect(this);

            Label lbTitle = (Label)builder.GetObject("LbTitle");
            lbTitle.Text = "Purchase";
            lbTitle.ModifyFont(FontDescription.FromString("Arial 18"));
            

            Box  boxMiddle = (Box)builder.GetObject("BoxMiddle");
            boxMiddle.SetSizeRequest(-1, -1); // Allow dynamic resizing
            boxMiddle.Expand = true;
            boxItem = (Box)builder.GetObject("BoxItem");
            boxItem.ModifyBg(StateType.Normal, new Gdk.Color(237, 237, 222));
            boxTransaction = (Box)builder.GetObject("BoxTransaction");
            boxTransaction.ModifyBg(StateType.Normal, new Gdk.Color(224, 235, 235));

            spnQty = (SpinButton)builder.GetObject("SpnQty");
            spnQty.KeyPressEvent += OnSpnQtyKeyPressEvent;
            
            
            Button btnProcessCheckout = (Button)builder.GetObject("BtnProcessCheckout");
            btnProcessCheckout.Clicked += DoCheckout;

            Button  btnProduct = (Button)builder.GetObject("BtnProduct");
            popoverProduct = new Popover(btnProduct);    
            btnProduct.Clicked += ShowProductPopup;

            Button  btnSupplier = (Button)builder.GetObject("BtnSupplier");
            popoverSupplier = new Popover(btnSupplier);    
            btnSupplier.Clicked += ShowSupplierPopup;

            
            btnNew = (Button)builder.GetObject("BtnNew");
            btnNew.Clicked += NewTransaction;
            
            entSearch = (Entry)builder.GetObject("EntSearch");
            entSearch.Changed += HandleEntSearchChanged;

            entAmountPayment = (Entry)builder.GetObject("EntAmountPayment");

            _treeViewTrans = (TreeView)builder.GetObject("TreeViewTrans");
            _treeViewTrans.Selection.Mode = SelectionMode.Single;
            textForground = "green";
            isEditable = true;
            AddColumnsTrans(); 
            _treeViewTrans.Selection.Changed += SelectedTrans;

            _treeViewItems = (TreeView)builder.GetObject("TreeViewItem");
            _treeViewItems.Selection.Mode = SelectionMode.Single;
             AddColumnsItems();     
            
            
            textViewProduct = (TextView)builder.GetObject("TextViewProduct");
            textViewSupplier = (TextView)builder.GetObject("TextViewSupplier");
            cmbPaymentMethod = (ComboBoxText)builder.GetObject("CmbPaymentMethod");
            FillCmbPaymentMethod(0);
            

            lbTransactionId = (Label)builder.GetObject("LbTransactionId");
            lbTransactionId.ModifyFont(FontDescription.FromString("Arial 14"));
            lbTotalItem = (Label)builder.GetObject("LbTotalItem");
            lbTotalItem.ModifyFont(FontDescription.FromString("Arial 14"));
            lbTotalPrice = (Label)builder.GetObject("LbTotalPrice");
            lbTotalPrice.ModifyFont(FontDescription.FromString("Arial 14"));
            this.KeyPressEvent += OnThisKeyPressEvent;

            
            SetTransactionModel(true,entSearch.Text.Trim());
            TransactionReady();       
        }        

        [GLib.ConnectBefore]
        private void OnThisKeyPressEvent(object sender, KeyPressEventArgs e)
        {
            Console.WriteLine(e.Event.Key);
            if (e.Event.Key == Gdk.Key.F1)
            {                 
              NewTransaction(new object(),new EventArgs());
            }else if (e.Event.Key == Gdk.Key.F2)
            { 
                if(boxItem.Sensitive==true) ShowSupplierPopup(new object(),new EventArgs());
            } else if (e.Event.Key == Gdk.Key.F3)
            {                 
                if(boxItem.Sensitive==true) ItemTransactionReady(true);
            }       
        }

        public void FillCmbPaymentMethod(int paymentMethodId)
        {
            Gtk.Application.Invoke(delegate
            {
                Gtk.ListStore ls = new ListStore(typeof(string), typeof(string));
                String sql = "Select id,name from payment_group " +
                        "order by id asc";
                DataTable dt = DbCl.fillDataTable(DbCl.getConn(), sql);
                foreach (DataRow dr in dt.Rows)
                {
                    ls.AppendValues(dr[0].ToString(), dr[1].ToString());
                }
                cmbPaymentMethod.Clear();
                Gtk.CellRendererText text = new Gtk.CellRendererText();
                cmbPaymentMethod.Model = ls;
                cmbPaymentMethod.PackStart(text, false);
                cmbPaymentMethod.AddAttribute(text, "text", 1);
                cmbPaymentMethod.Active = 0;
            });
        }
        private void TransactionReady(){
            GuiCl.SensitiveAllWidgets(boxItem,false);      
            btnNew.Sensitive = true;      
            textViewSupplier.Buffer.Text = "";
            textViewProduct.Buffer.Text = "";
            spnQty.Text = "1";
            boxTransaction.Sensitive = true;
            btnNew.Sensitive = true; 
            entAmountPayment.Text = "0";               
        }
        public void SelectFirstRow(ListStore ts, TreeView tv){
            TreeIter iter;
            if (ts.GetIterFirst(out iter)) // Get the first row
            {
                tv.Selection.SelectIter(iter);
            }
        }
        private void ItemTransactionReady(Boolean showpopup){
            if(showpopup) ShowProductPopup(new object(),new EventArgs());
            GuiCl.SensitiveAllWidgets(boxItem,true);                  
            spnQty.Text = "1";        
            textViewProduct.Buffer.Text = "";

        }
        private void SelectedTrans(object sender, EventArgs e)
        {
            if (!_treeViewTrans.Selection.GetSelected(out TreeIter it))
                 return;
            TreePath path = _lsModelTrans.GetPath(it);
            var id = (string)_lsModelTrans.GetValue(it, (int)ColumnTrans.id);
            var supplier_id = (string)_lsModelTrans.GetValue(it, (int)ColumnTrans.supplier_id);
            var organization_name = (string)_lsModelTrans.GetValue(it, (int)ColumnTrans.organization_name);
            var organization_address = (string)_lsModelTrans.GetValue(it, (int)ColumnTrans.organization_address);
            var organization_phone_number = (string)_lsModelTrans.GetValue(it, (int)ColumnTrans.organization_phone_number);
            var person_name = (string)_lsModelTrans.GetValue(it, (int)ColumnTrans.person_name);
            var person_phone_number = (string)_lsModelTrans.GetValue(it, (int)ColumnTrans.person_phone_number);

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
            lbTransactionId.Text = id;
            ItemTransactionReady(false);
            SetItemModel(Convert.ToDouble(id));
            lbTotalItem.Text =  GetTotalItem().ToString();
            lbTotalPrice.Text = GetTotalPurchasePrice().ToString();
        }
        private int GetTotalItem(){
            int total = 0;
            for (int i = 0; i < _clsItems.Count; i++)
            {
                total += Convert.ToInt32(_clsItems[i].quantity) ;
            } 
            return total;
        }
        private double GetTotalPurchasePrice(){
            double total = 0;
            for (int i = 0; i < _clsItems.Count; i++)
            {
                total += (Convert.ToDouble(_clsItems[i].quantity)*Convert.ToDouble(_clsItems[i].purchase_price)) ;
            } 
            return total;
        }
        private void HandleEntSearchChanged(object sender, EventArgs e)
        {
            Entry entry = sender as Entry;
            if (entry != null)
            {
                SetTransactionModel(true,entry.Text.Trim()); 
            }
        }
        private void NewTransaction(object sender, EventArgs e)
        {
            TransactionReady();
            string sql = "insert into transaction (transaction_type,transaction_date,input_date,supplier_id,user_id,application_id) "+
            "values (1,CURRENT_DATE,CURRENT_DATE,1,"+this.parent.user.id+",'"+this.parent.application_id+"') ";
            Console.WriteLine (sql);
            DbCl.ExecuteTrans(DbCl.getConn(), sql);
            SetTransactionModel(true,entSearch.Text.Trim());  
            SelectFirstRow(_lsModelTrans,_treeViewTrans);          
            ItemTransactionReady(false);
        }
        private void SetTransactionModel(Boolean showAll,string strfind)
        {      
            if(strfind=="" && !showAll) {          
                _treeViewTrans.Model = null;
            }else{
                
                //ListStore model;
                _lsModelTrans = null;
                TreeIter iter;
                /* create array */
                _clsTrans = new List<clTransaction>();
                string whrfind = "";
                if(strfind!="") whrfind = "and (upper(sup.organization_name) like upper('" + strfind + "%') or upper(pers.name) like upper('" + strfind + "%') )";
                string sql = "select tr.id,tr.reference_id, tr.supplier_id,sup.organization_name,sup.organization_address,sup.organization_phone_number,pers.name person_name,pers.phone_number,TO_CHAR(tr.transaction_date,'yyyy-mm-dd') transaction_date,tr.state,tr.user_id,tr.application_id "+
                "from transaction tr left outer join supplier sup on tr.supplier_id=sup.id "+
                "left outer join person pers on sup.person_id=pers.id,(select usr.id, pers.name person_name, pers.phone_number from person pers,userlogin usr where usr.person_id=pers.id) uspers "+
                "where tr.transaction_type=1 and tr.transaction_date = CURRENT_date and uspers.id=tr.user_id "+
                "ORDER by tr.id desc";
                Console.WriteLine(sql);
                clTransaction tran;
                dtTransSelected =  DbCl.fillDataTable(DbCl.getConn(), sql);
                foreach (DataRow dr in dtTransSelected.Rows)
                {              
                    tran = new clTransaction{    
                        id=dr["id"].ToString(),
                        reference_id=dr["reference_id"].ToString(),
                        supplier_id=dr["supplier_id"].ToString(),
                        organization_name=dr["organization_name"].ToString(),
                        organization_address=dr["organization_address"].ToString(), 
                        organization_phone_number=dr["organization_phone_number"].ToString(),
                        person_name=dr["person_name"].ToString(),
                        person_phone_number=dr["phone_number"].ToString(),
                        transaction_date=dr["transaction_date"].ToString(),  
                        state=dr["state"].ToString(),
                        user_id=dr["user_id"].ToString(),
                        application_id=dr["application_id"].ToString()
                    } ;                                 
                    _clsTrans.Add(tran);
                } 

                _lsModelTrans = new ListStore(typeof(string),typeof(string),typeof(string),typeof(string),typeof(string),typeof(string),typeof(string),typeof(string),typeof(string),typeof(string));

                /* add items */
                for (int i = 0; i < _clsTrans.Count; i++)
                {
                    iter = _lsModelTrans.Append();
                    _lsModelTrans.SetValue(iter, (int)ColumnTrans.id, _clsTrans[i].id);
                    _lsModelTrans.SetValue(iter, (int)ColumnTrans.reference_id, _clsTrans[i].reference_id);
                    _lsModelTrans.SetValue(iter, (int)ColumnTrans.supplier_id, _clsTrans[i].supplier_id);
                    _lsModelTrans.SetValue(iter, (int)ColumnTrans.organization_name, _clsTrans[i].organization_name);
                    _lsModelTrans.SetValue(iter, (int)ColumnTrans.organization_address, _clsTrans[i].organization_address);
                    _lsModelTrans.SetValue(iter, (int)ColumnTrans.organization_phone_number, _clsTrans[i].organization_phone_number);
                    _lsModelTrans.SetValue(iter, (int)ColumnTrans.person_name, _clsTrans[i].person_name);
                    _lsModelTrans.SetValue(iter, (int)ColumnTrans.person_phone_number, _clsTrans[i].person_phone_number);
                    _lsModelTrans.SetValue(iter, (int)ColumnTrans.transaction_date, _clsTrans[i].transaction_date);
                    _lsModelTrans.SetValue(iter, (int)ColumnTrans.state, _clsTrans[i].state);
                    _lsModelTrans.SetValue(iter, (int)ColumnTrans.user_id, _clsTrans[i].user_id);
                    _lsModelTrans.SetValue(iter, (int)ColumnTrans.application_id, _clsTrans[i].application_id);
                }
                _treeViewTrans.Model = _lsModelTrans;    
                     
            }
        }
        private void SetItemModel(Double transaction_id)
        {          
            //ListStore model;
            _lsModelItems = null;
            TreeIter iter;
            /* create array */
            _clsItems = new List<clTransItem>();
             string sql = "select ti.id, ti.transaction_id, ti.product_id, pr.name product_name,ti.stock_id, "+
            "st.quantity, st.unit,un.name unit_name, st.purchase_price, ti.price_id, ti.price, ti.tax, ti.state, "+
            " st.location, lo.name location_name, st.condition, co.name condition_name "+
            "from transaction_item ti, product pr, stock st left outer join unit un on un.id=st.unit left outer join condition co on st.condition=co.id left outer join location lo on st.location=lo.id "+
            "where ti.product_id=pr.id and ti.stock_id=st.id and ti.transaction_id="+transaction_id.ToString()+ " "+
            "ORDER by ti.id desc";
            //tekan kene
            Console.WriteLine(sql);
            clTransItem item;
            dtTransSelected =  DbCl.fillDataTable(DbCl.getConn(), sql);
            foreach (DataRow dr in dtTransSelected.Rows)
            {   
                item = new clTransItem{
                    id=dr["id"].ToString(),
                    transaction_id=dr["transaction_id"].ToString(),
                    product_id=dr["product_id"].ToString(),
                    product_name=dr["product_name"].ToString(),
                    stock_id=dr["stock_id"].ToString(),
                    quantity=dr["quantity"].ToString(),
                    unit=dr["unit"].ToString(),
                    unit_name=dr["unit_name"].ToString(),
                    purchase_price=dr["purchase_price"].ToString(),
                    price_id=dr["price_id"].ToString(),
                    price=dr["price"].ToString(),
                    tax=dr["tax"].ToString(),
                    state=dr["state"].ToString(), 
                    location=dr["location"].ToString(), 
                    location_name=dr["location_name"].ToString(), 
                    condition=dr["condition"].ToString(),
                    condition_name=dr["condition_name"].ToString(), 
                };
                _clsItems.Add(item);                    
            }

            _lsModelItems = new ListStore(typeof(string),typeof(string),typeof(string),typeof(string),typeof(string),typeof(string),typeof(string),typeof(string),typeof(string),typeof(string),typeof(string),typeof(string),typeof(string),typeof(string),typeof(string),typeof(string),typeof(string));

            /* add items */
            for (int i = 0; i < _clsItems.Count; i++)
            {
                iter = _lsModelItems.Append();
                _lsModelItems.SetValue(iter, (int)ColumnItems.id, _clsItems[i].id);
                _lsModelItems.SetValue(iter, (int)ColumnItems.transaction_id, _clsItems[i].transaction_id);
                _lsModelItems.SetValue(iter, (int)ColumnItems.product_id, _clsItems[i].product_id);
                _lsModelItems.SetValue(iter, (int)ColumnItems.product_name, _clsItems[i].product_name);
                _lsModelItems.SetValue(iter, (int)ColumnItems.stock_id, _clsItems[i].stock_id);
                _lsModelItems.SetValue(iter, (int)ColumnItems.quantity, _clsItems[i].quantity);
                _lsModelItems.SetValue(iter, (int)ColumnItems.unit, _clsItems[i].unit);
                _lsModelItems.SetValue(iter, (int)ColumnItems.unit_name, _clsItems[i].unit_name);
                _lsModelItems.SetValue(iter, (int)ColumnItems.purchase_price, _clsItems[i].purchase_price);
                _lsModelItems.SetValue(iter, (int)ColumnItems.price_id, _clsItems[i].price_id);
                _lsModelItems.SetValue(iter, (int)ColumnItems.price, _clsItems[i].price);
                _lsModelItems.SetValue(iter, (int)ColumnItems.tax, _clsItems[i].tax);
                _lsModelItems.SetValue(iter, (int)ColumnItems.state, _clsItems[i].state);
                _lsModelItems.SetValue(iter, (int)ColumnItems.location, _clsItems[i].location);
                _lsModelItems.SetValue(iter, (int)ColumnItems.location_name, _clsItems[i].location_name);
                _lsModelItems.SetValue(iter, (int)ColumnItems.condition, _clsItems[i].condition);
                _lsModelItems.SetValue(iter, (int)ColumnItems.condition_name, _clsItems[i].condition_name);
            }
            _treeViewItems.Model = _lsModelItems;   
              
            
        }
        private void SelectedItem(string prm)
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
        public Int64 InsertStock(){
            string sql = "insert into stock (product_id,quantity,input_date,expired_date,purchase_price, unit, condition, location)"+
            "values ("+dtItemSelected.Rows[0].ItemArray[0].ToString()+ ","+spnQty.Text+",CURRENT_DATE,CURRENT_DATE,0,1,1,1) returning id";
            Console.WriteLine (sql);
            return DbCl.ExecuteScalar(DbCl.getConn(), sql);
        }

        [GLib.ConnectBefore]
        private void OnSpnQtyKeyPressEvent(object sender, KeyPressEventArgs e)
        {
            if(dtItemSelected is not null){
               // Console.WriteLine(e.Event.Key);
                if (e.Event.Key == Gdk.Key.Return)
                {                 
                    Int64 stock_id = InsertStock();
                  //  tekan kene
                    string sql = "insert into transaction_item (transaction_id,product_id,stock_id,purchase_price,state) "+
                    "values("+lbTransactionId.Text+ ","+dtItemSelected.Rows[0].ItemArray[0].ToString() + ","+ stock_id.ToString() + ",0,1)" ;
                    Console.WriteLine (sql);
                    DbCl.ExecuteTrans(DbCl.getConn(), sql); 
                    SetItemModel(Convert.ToDouble(lbTransactionId.Text));
                    ItemTransactionReady(true);
                } 
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
                model.SetValue(iter, (int)ColNumberTrans.Text, i.ToString());
            }
            return model;
        }
       
        private void AddColumnsTrans()
        {
            _cellColumnsRender = new Dictionary<CellRenderer, int>();

            CellRendererText rendererText = new CellRendererText();
            _cellColumnsRender.Add(rendererText, (int)ColumnTrans.id);
            _treeViewTrans.InsertColumn(-1, "ID", rendererText, "text", (int)ColumnTrans.id);
            
            rendererText = new CellRendererText
            {
                Editable = isEditable
            };
            rendererText.Foreground = textForground;
            rendererText.Edited += CellEditedTrans;
            _cellColumnsRender.Add(rendererText, (int)ColumnTrans.reference_id);
            _treeViewTrans.InsertColumn(-1, "Reference ID", rendererText, "text", (int)ColumnTrans.reference_id);            

            rendererText = new CellRendererText();
            _cellColumnsRender.Add(rendererText, (int)ColumnTrans.supplier_id);
            _treeViewTrans.InsertColumn(-1, "Supplier ID", rendererText, "text", (int)ColumnTrans.supplier_id);            

            rendererText = new CellRendererText();
            _cellColumnsRender.Add(rendererText, (int)ColumnTrans.organization_name);
            _treeViewTrans.InsertColumn(-1, "Organization Name", rendererText, "text", (int)ColumnTrans.organization_name); 

            rendererText = new CellRendererText();
            _cellColumnsRender.Add(rendererText, (int)ColumnTrans.organization_phone_number);
            _treeViewTrans.InsertColumn(-1, "Organization Phone Number", rendererText, "text", (int)ColumnTrans.organization_phone_number);

            rendererText = new CellRendererText();
            _cellColumnsRender.Add(rendererText, (int)ColumnTrans.person_name);
            _treeViewTrans.InsertColumn(-1, "Person Name", rendererText, "text", (int)ColumnTrans.person_name);

            rendererText = new CellRendererText();
            _cellColumnsRender.Add(rendererText, (int)ColumnTrans.transaction_date);
            _treeViewTrans.InsertColumn(-1, "Transaction Date", rendererText, "text", (int)ColumnTrans.transaction_date);

            rendererText = new CellRendererText();
            _cellColumnsRender.Add(rendererText, (int)ColumnTrans.state);
            _treeViewTrans.InsertColumn(-1, "State", rendererText, "text", (int)ColumnTrans.state);

            rendererText = new CellRendererText();
            _cellColumnsRender.Add(rendererText, (int)ColumnTrans.user_id);
            _treeViewTrans.InsertColumn(-1, "User ID", rendererText, "text", (int)ColumnTrans.user_id);

            rendererText = new CellRendererText();
            _cellColumnsRender.Add(rendererText, (int)ColumnTrans.application_id);
            _treeViewTrans.InsertColumn(-1, "Application ID", rendererText, "text", (int)ColumnTrans.application_id);

        }
          
        private void AddColumnsItems()
        {

            _cellColumnsRenderItems = new Dictionary<CellRenderer, int>();
            
            CellRendererText rendererText = new CellRendererText();
            _cellColumnsRenderItems.Add(rendererText, (int)ColumnItems.id);
            _treeViewItems.InsertColumn(-1, "ID", rendererText, "text", (int)ColumnItems.id);

            rendererText = new CellRendererText();
            _cellColumnsRenderItems.Add(rendererText, (int)ColumnItems.transaction_id);
            _treeViewItems.InsertColumn(-1, "Transaction ID", rendererText, "text", (int)ColumnItems.transaction_id);            

            rendererText = new CellRendererText();
            _cellColumnsRenderItems.Add(rendererText, (int)ColumnItems.product_id);
            _treeViewItems.InsertColumn(-1, "Product ID", rendererText, "text", (int)ColumnItems.product_id); 

            rendererText = new CellRendererText();
            _cellColumnsRenderItems.Add(rendererText, (int)ColumnItems.product_name);
            _treeViewItems.InsertColumn(-1, "Product name", rendererText, "text", (int)ColumnItems.product_name);

            rendererText = new CellRendererText();
            _cellColumnsRenderItems.Add(rendererText, (int)ColumnItems.stock_id);
            _treeViewItems.InsertColumn(-1, "Stock ID", rendererText, "text", (int)ColumnItems.stock_id);

            rendererText = new CellRendererText();
            _cellColumnsRenderItems.Add(rendererText, (int)ColumnItems.quantity);
            _treeViewItems.InsertColumn(-1, "Quantity", rendererText, "text", (int)ColumnItems.quantity);

            rendererText = new CellRendererText();
            _cellColumnsRenderItems.Add(rendererText, (int)ColumnItems.unit);
            _treeViewItems.InsertColumn(-1, "Unit", rendererText, "text", (int)ColumnItems.unit);

            rendererText = new CellRendererText();
            _cellColumnsRenderItems.Add(rendererText, (int)ColumnItems.unit_name);
            _treeViewItems.InsertColumn(-1, "Unit name", rendererText, "text", (int)ColumnItems.unit_name);

            rendererText = new CellRendererText
            {
                Editable = true
            };
            rendererText.Foreground = "green";
            rendererText.Edited += CellEditedItem;
            _cellColumnsRenderItems.Add(rendererText, (int)ColumnItems.purchase_price);
            _treeViewItems.InsertColumn(-1, "Purchase price", rendererText, "text", (int)ColumnItems.purchase_price);

            rendererText = new CellRendererText();
            _cellColumnsRenderItems.Add(rendererText, (int)ColumnItems.price_id);
            _treeViewItems.InsertColumn(-1, "Price ID", rendererText, "text", (int)ColumnItems.price_id);

            rendererText = new CellRendererText();
            _cellColumnsRenderItems.Add(rendererText, (int)ColumnItems.price);
            _treeViewItems.InsertColumn(-1, "Price", rendererText, "text", (int)ColumnItems.price);

            rendererText = new CellRendererText();
            _cellColumnsRenderItems.Add(rendererText, (int)ColumnItems.tax);
            _treeViewItems.InsertColumn(-1, "Tax", rendererText, "text", (int)ColumnItems.tax);

            rendererText = new CellRendererText();
            _cellColumnsRenderItems.Add(rendererText, (int)ColumnItems.state);
            _treeViewItems.InsertColumn(-1, "State", rendererText, "text", (int)ColumnItems.state);

            rendererText = new CellRendererText();
            _cellColumnsRenderItems.Add(rendererText, (int)ColumnItems.location);
            _treeViewItems.InsertColumn(-1, "Location", rendererText, "text", (int)ColumnItems.location);

            rendererText = new CellRendererText();
            _cellColumnsRenderItems.Add(rendererText, (int)ColumnItems.location_name);
            _treeViewItems.InsertColumn(-1, "Location name", rendererText, "text", (int)ColumnItems.location_name);

            rendererText = new CellRendererText();
            _cellColumnsRenderItems.Add(rendererText, (int)ColumnItems.condition);
            _treeViewItems.InsertColumn(-1, "Condition", rendererText, "text", (int)ColumnItems.condition);

            rendererText = new CellRendererText();
            _cellColumnsRenderItems.Add(rendererText, (int)ColumnItems.condition_name);
            _treeViewItems.InsertColumn(-1, "Condition name", rendererText, "text", (int)ColumnItems.condition_name);
        }
        private void CellEditedItem(object data, EditedArgs args)
        {
           TreePath path = new TreePath(args.Path);
            int column = _cellColumnsRenderItems[(CellRenderer)data];
            _lsModelItems.GetIter(out TreeIter iter, path);

            switch (column)
            {
                case (int)ColumnItems.purchase_price:
                {
                    int i = path.Indices[0];
                    _clsItems[i].purchase_price = args.NewText;
                    _lsModelItems.SetValue(iter, column, _clsItems[i].purchase_price);
                    string sql = "update transaction_item set purchase_price = '"+args.NewText+"' where id='"+_clsItems[i].id+"' ";
                    Console.WriteLine (sql);
                    DbCl.ExecuteTrans(DbCl.getConn(), sql);
                    sql = "update stock set purchase_price = '"+args.NewText+"' where id='"+_clsItems[i].stock_id+"' ";
                    Console.WriteLine (sql);
                    DbCl.ExecuteTrans(DbCl.getConn(), sql);
                    lbTotalPrice.Text = GetTotalPurchasePrice().ToString();
                }
                break;
            }
        }

        private void RemoveItem(object sender, EventArgs e)
        {
            
        }

        private void CellEditedTrans(object data, EditedArgs args)
        {
           TreePath path = new TreePath(args.Path);
            int column = _cellColumnsRender[(CellRenderer)data];
            _lsModelTrans.GetIter(out TreeIter iter, path);
            switch (column)
            {
                case (int)ColumnTrans.reference_id:
                {
                    int i = path.Indices[0];
                    _clsTrans[i].reference_id = args.NewText;
                    _lsModelTrans.SetValue(iter, column, _clsTrans[i].reference_id);
                    string sql = "update transaction set reference_id = '"+args.NewText+"' where id='"+_clsTrans[i].id+"' ";
                    Console.WriteLine (sql);
                    DbCl.ExecuteTrans(DbCl.getConn(), sql);                  
                }
                break;
            };
        }

        public void doChildProduct(object o,string prm){
            GLib.Timeout.Add(5, () =>
            {
                GuiCl.RemoveAllWidgets(popoverProduct);
                Label popLabel = new Label((string)o);
                popoverProduct.Add(popLabel);
                popoverProduct.SetSizeRequest(200, 20);
                popLabel.Show();
                SelectedItem(prm);
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
            if( Convert.ToInt32(cmbPaymentMethod.ActiveText) < 5){
                if(Convert.ToInt32(entAmountPayment.Text) < GetTotalPurchasePrice()){
                    string message = "Oh sorry, payment amount less than total purchase price";
                    MessageDialog md = new MessageDialog(null, DialogFlags.DestroyWithParent, MessageType.Error, ButtonsType.Close, message);
                    md.Run();
                    md.Destroy();
                }else{

                }
            }
            
           // TransactionReady();
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