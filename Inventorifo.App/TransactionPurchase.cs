using System;
using System.Collections.Generic;
using Gtk;
using Gdk;
using System.Data;
using Pango;
using UI = Gtk.Builder.ObjectAttribute;
using System.Text.RegularExpressions;

namespace Inventorifo.App
{
    //[Section(ContentType = typeof(EditableCellsSection), Category = Category.Widgets)]
    class TransactionPurchase : Gtk.Box
    {
        Inventorifo.Lib.LibDb DbCl = new Inventorifo.Lib.LibDb ();
        Inventorifo.Lib.LibGui GuiCl = new Inventorifo.Lib.LibGui ();
        Inventorifo.Lib.LibCore CoreCl = new Inventorifo.Lib.LibCore ();

        public MainWindow parent;
        public string prm;
        public TransactionPurchase(object parent, string prm) : this(new Builder("TransactionPurchase.glade")) { 
            this.parent=(MainWindow)parent;
            this.prm = prm;
            //Console.WriteLine(this.parent.user.id + " "+ this.parent.user.person_name);            
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
        Box boxSupplierDetail;
        Box boxProductDetail;
        Box boxTotalCalculation;
        Box boxPayment;

        SpinButton spnQty;
        Button  btnNew;
        Button  btnPreviousPayment;
        Button btnProcessCheckout;
        Button btnProduct;
        Button btnSupplier;
        Button btnDate;

        private Entry entSearch;
        private Entry entBarcode;
        private Entry entAmountPayment;
        private Entry entTransactionAmount;
        private Entry entTaxAmount;

        private Popover popoverSupplier ;
        private Popover popoverProduct ;
        private Popover popoverDate;
        private Popover popoverPayment;
        private TextView textViewSupplier;
        private TextView textViewProduct;

        DataTable dtTransSelected;
        DataTable dtItemSelected;

        Label lbTransactionId;
        Label lbTotalItem;
        Label lbPreviousPayment;
        Label lbBillCalculated;

        string textForeground;
        string textBackground;
        Boolean isEditable;

        ComboBoxText cmbPaymentMethod;
        Dictionary<int, string> dictPaymentMethod = new Dictionary<int, string>();
        Gtk.ListStore lsPaymentMethod = new ListStore(typeof(string), typeof(string));
        CellRendererCombo cellComboPaymentMethod = new Gtk.CellRendererCombo();
        
        Calendar calendar = new Calendar();
        CheckButton chkTax;

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
            transaction_type_id,
            transaction_type_name,
            transaction_date,
            transaction_amount,
            return_amount,
            payment_group_id,
            payment_group_name,
            payment_amount,
            user_id,
            user_name,
            state,
            state_name,
            state_fgcolor,
            state_bgcolor,
            application_id,
            tax_amount,
            is_tax,
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
            product_short_name,
            product_name,
            stock_id,
            quantity,
            unit,
            unit_name,
            price_id,
            purchase_price,
            price,
            tax,
            state,
            state_name,
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
            //boxItem.ModifyBg(StateType.Normal, new Gdk.Color(237, 237, 222));
            boxTransaction = (Box)builder.GetObject("BoxTransaction");
            //boxTransaction.ModifyBg(StateType.Normal, new Gdk.Color(224, 235, 235));
            boxSupplierDetail = (Box)builder.GetObject("BoxSupplierDetail");
            boxProductDetail = (Box)builder.GetObject("BoxProductDetail");
            boxTotalCalculation = (Box)builder.GetObject("BoxTotalCalculation");
            boxPayment = (Box)builder.GetObject("BoxPayment");

            spnQty = (SpinButton)builder.GetObject("SpnQty");
            spnQty.Xalign = 0.5f; 
            spnQty.KeyPressEvent += OnSpnQtyKeyPressEvent;
            spnQty.ModifyFont(FontDescription.FromString("Arial 14"));
            
            
            btnProcessCheckout = (Button)builder.GetObject("BtnProcessCheckout");
            btnProcessCheckout.Clicked += DoCheckout;

            btnProduct = (Button)builder.GetObject("BtnProduct");
            popoverProduct = new Popover(btnProduct);    
            btnProduct.Clicked += ShowProductPopup;

            btnSupplier = (Button)builder.GetObject("BtnSupplier");
            popoverSupplier = new Popover(btnSupplier);    
            btnSupplier.Clicked += ShowSupplierPopup;

            btnDate = (Button)builder.GetObject("BtnDate");
            btnDate.Label = calendar.Date.ToString("yyyy-MM-dd");
            popoverDate = new Popover(btnDate);    
            btnDate.Clicked += ShowDatePopup;
            
            
            btnNew = (Button)builder.GetObject("BtnNew");
            btnNew.Clicked += NewTransaction;
            btnPreviousPayment = (Button)builder.GetObject("BtnPreviousPayment");
            popoverPayment = new Popover(btnPreviousPayment); 
            btnPreviousPayment.ModifyFont(FontDescription.FromString("Arial 14"));
            btnPreviousPayment.Clicked += ShowPaymentPopup;
            
            entSearch = (Entry)builder.GetObject("EntSearch");
            entSearch.Changed += HandleEntSearchChanged;

            entAmountPayment = (Entry)builder.GetObject("EntAmountPayment");
            entAmountPayment.Xalign = 0.5f; 
            entAmountPayment.ModifyFont(FontDescription.FromString("Arial 14"));
            entAmountPayment.Changed += HandleEntAmountPaymentChanged;
            entAmountPayment.KeyPressEvent += OnEntAmountPaymentKeyPressEvent;

            //entAmountPayment.ModifyBg(StateType.Normal, new Gdk.Color(255, 237, 222));
            entTransactionAmount = (Entry)builder.GetObject("EntTransactionAmount");
            entTransactionAmount.Xalign = 0.5f; 
            entTransactionAmount.ModifyFont(FontDescription.FromString("Arial 14"));
            //entTransactionAmount.ModifyBg(StateType.Normal, new Gdk.Color(255, 237, 222));
            entTaxAmount = (Entry)builder.GetObject("EntTaxAmount");
            entTaxAmount.Xalign = 0.5f; 
            entTaxAmount.ModifyFont(FontDescription.FromString("Arial 14"));

            _treeViewTrans = (TreeView)builder.GetObject("TreeViewTrans");
            _treeViewTrans.Selection.Mode = SelectionMode.Single;
            textForeground = "green";
            textBackground = "lightred";
            isEditable = true;
            AddColumnsTrans(); 
            _treeViewTrans.Selection.Changed += HandleTreeVewSelectedTrans;

            _treeViewItems = (TreeView)builder.GetObject("TreeViewItem");
            _treeViewItems.Selection.Mode = SelectionMode.Single;
            _treeViewItems.ModifyFont(FontDescription.FromString("Arial 14"));
             AddColumnsItems();     
             _treeViewItems.KeyPressEvent += HandleTreeViewItemsKeyPressEvent;
            _treeViewItems.Columns[0].Visible = false;
            _treeViewItems.Columns[1].Visible = false;
            _treeViewItems.Columns[2].Visible = false;
            _treeViewItems.Columns[4].Visible = false;
            _treeViewItems.Columns[8].Visible = false;
            _treeViewItems.Columns[10].Visible = false; //tax
            
            textViewProduct = (TextView)builder.GetObject("TextViewProduct");
            textViewSupplier = (TextView)builder.GetObject("TextViewSupplier");
            cmbPaymentMethod = (ComboBoxText)builder.GetObject("CmbPaymentMethod");
            FillCmbPaymentMethod(0);
            

            lbTransactionId = (Label)builder.GetObject("LbTransactionId");
            lbTransactionId.ModifyFont(FontDescription.FromString("Arial 14"));
            lbTotalItem = (Label)builder.GetObject("LbTotalItem");
            lbTotalItem.ModifyFont(FontDescription.FromString("Arial 14"));
            lbPreviousPayment = (Label)builder.GetObject("LbPreviousPayment");
            lbBillCalculated = (Label)builder.GetObject("LbBillCalculated");
            lbBillCalculated.ModifyFont(FontDescription.FromString("Arial 14"));
            this.KeyPressEvent += OnThisKeyPressEvent;

            chkTax = (CheckButton)builder.GetObject("ChkTax");
            chkTax.Toggled += HandlechkTaxChanged;

            SetTransactionModel("",entSearch.Text.Trim());
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
            entAmountPayment.ModifyBg(StateType.Normal, new Gdk.Color(255, 255, 255));
            entAmountPayment.ModifyFg(StateType.Normal, new Gdk.Color(0, 0, 0));
            entTransactionAmount.ModifyBg(StateType.Normal, new Gdk.Color(255, 255, 255));
            entTransactionAmount.ModifyFg(StateType.Normal, new Gdk.Color(0, 0, 0));
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
            spnQty.Text = "1";        
            textViewProduct.Buffer.Text = "";
        }
        private void HandleTreeVewSelectedTrans(object sender, EventArgs e)
        {
            if (!_treeViewTrans.Selection.GetSelected(out TreeIter it))
                 return;
            TreePath path = _lsModelTrans.GetPath(it);
            var id = (string)_lsModelTrans.GetValue(it, (int)ColumnTrans.id);            
            SelectedTrans(id);
        }
        private void SelectedTrans(string transaction_id)
        {
            DataTable dtTransSelected = fillDtTransaction(transaction_id,"");
            foreach (DataRow dr in dtTransSelected.Rows)
            { 
                var id = transaction_id;
                var state = dr["state"].ToString();
                var supplier_id = dr["supplier_id"].ToString();
                var organization_name = dr["organization_name"].ToString();
                var organization_address = dr["organization_address"].ToString();
                var organization_phone_number = dr["organization_phone_number"].ToString();
                var person_name = dr["person_name"].ToString();
                var person_phone_number = dr["person_phone_number"].ToString();
                var payment_group_id = dr["payment_group_id"].ToString();
                var payment_group_name = dr["payment_group_name"].ToString();
                var payment_amount = dr["payment_amount"].ToString();
                var transaction_amount = dr["transaction_amount"].ToString();
                var transaction_date = dr["transaction_date"].ToString();
                var tax_amount = dr["tax_amount"].ToString();
                bool is_tax;
                bool succes = bool.TryParse(dr["is_tax"].ToString(), out is_tax);
                setActivePaymentMethod(payment_group_id);

                entAmountPayment.Text = "0";
                btnPreviousPayment.Label = payment_amount;
                entTaxAmount.Text = tax_amount;
                entTransactionAmount.Text = transaction_amount;
                lbBillCalculated.Text = (Convert.ToDouble(transaction_amount)+Convert.ToDouble(tax_amount)-Convert.ToDouble(payment_amount)).ToString() ;
                
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
                

                if(Convert.ToInt32(state)==0){ 
                    if(Convert.ToInt32(payment_group_id)>5 ){   
                        if(Convert.ToDouble(transaction_amount)>Convert.ToDouble(payment_amount)){
                            GuiCl.SensitiveAllWidgets(boxItem,true);                 
                            GuiCl.SetDisableAllColumn(_treeViewItems);
                            btnSupplier.Sensitive = false;
                            btnProduct.Sensitive = false;
                            entTransactionAmount.Sensitive = false;
                            spnQty.Sensitive=false;
                            cmbPaymentMethod.Sensitive = false;
                            //boxPayment.Sensitive = true;
                            cmbPaymentMethod.Sensitive = false;
                            entAmountPayment.Sensitive = true;
                            btnProcessCheckout.Sensitive = true;

                        } else{
                            GuiCl.SetDisableAllColumn(_treeViewItems);
                            btnSupplier.Sensitive = false;
                            btnProduct.Sensitive = false;
                            entTransactionAmount.Sensitive = false;
                            spnQty.Sensitive=false;
                            cmbPaymentMethod.Sensitive = false;
                            //boxPayment.Sensitive = false;
                            cmbPaymentMethod.Sensitive = false;
                            entAmountPayment.Sensitive = false;
                            btnProcessCheckout.Sensitive = false;
                        }                        
                    }else{
                            GuiCl.SetDisableAllColumn(_treeViewItems);
                            btnSupplier.Sensitive = false;
                            btnProduct.Sensitive = false;
                            entTransactionAmount.Sensitive = false;
                            spnQty.Sensitive=false;
                            cmbPaymentMethod.Sensitive = false;
                            //boxPayment.Sensitive = false;
                            cmbPaymentMethod.Sensitive = false;
                            entAmountPayment.Sensitive = false;
                            btnProcessCheckout.Sensitive = false;
                    }
                }else{                 
                    GuiCl.SensitiveAllWidgets(boxItem,true);  
                    GuiCl.SetEnableColumn(_treeViewItems,[5,6,7,11,12]);
                    btnSupplier.Sensitive = true;
                    btnProduct.Sensitive = true;
                    boxSupplierDetail.Sensitive = true;
                    boxProductDetail.Sensitive = true;
                    entTransactionAmount.Sensitive = true;
                    spnQty.Sensitive=true;
                    cmbPaymentMethod.Sensitive = true;
                    entAmountPayment.Sensitive = true;
                    btnProcessCheckout.Sensitive = true;
                }

                ItemTransactionReady(false);
                SetItemModel(Convert.ToDouble(lbTransactionId.Text));  
                chkTax.Active = is_tax;
            }                              
            
        }
              
        public void setActivePaymentMethod(string pattern){
            var store = (ListStore)cmbPaymentMethod.Model;
                    int index = 0;
                    foreach (object[] row in store)
                    {
                        if (pattern == row[0].ToString())
                        {
                            cmbPaymentMethod.Active = index;
                            break;
                        }
                        index++;
                    }
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

        private void HandleEntAmountPaymentChanged(object sender, EventArgs e){
            Entry entry = sender as Entry;
            if (entry != null)
            {
                //number validation
                if (!Regex.IsMatch(entry.Text, @"^\d*$")) // Only digits allowed
                {
                    entry.Text = Regex.Replace(entry.Text, @"\D", ""); // Remove non-numeric characters
                }
                //change background
                if(entry.Text.Trim()!="" && entTransactionAmount.Text.Trim()!=""){
                    if( Convert.ToInt32(cmbPaymentMethod.ActiveText) < 5 ){
                        if(Convert.ToDouble(entry.Text) < Convert.ToDouble(entTransactionAmount.Text)){
                            entAmountPayment.ModifyBg(StateType.Normal, new Gdk.Color(255, 237, 222));
                            entAmountPayment.ModifyFg(StateType.Normal, new Gdk.Color(0, 0, 0));
                        }else{
                            entAmountPayment.ModifyBg(StateType.Normal, new Gdk.Color(237, 255, 222));
                            entAmountPayment.ModifyFg(StateType.Normal, new Gdk.Color(0, 0, 0));
                        }
                    }
                }                
            }
        }

        private void HandlechkTaxChanged(object sender, EventArgs e)
        {
            if(chkTax.Active){
                entTaxAmount.Text = CalculateTax(GetTotalPurchasePrice()).ToString();
                entTransactionAmount.Text = (GetTotalPurchasePrice()+CalculateTax(GetTotalPurchasePrice())).ToString();
            }else{
                entTaxAmount.Text = "0";
                entTransactionAmount.Text = (GetTotalPurchasePrice()).ToString();
            }
        }
        private double CalculateTax(double amount){
            if(chkTax.Active) return amount*Convert.ToDouble(this.parent.conf.tax)/100;
            else return 0;
        }
        private void HandleEntSearchChanged(object sender, EventArgs e)
        {
            Entry entry = sender as Entry;
            if (entry != null)
            {
                SetTransactionModel("",entry.Text.Trim()); 
            }
        }
        private void NewTransaction(object sender, EventArgs e)
        {
            TransactionReady();
            string sql = "insert into transaction (transaction_type,transaction_date,input_date,supplier_id,user_id,application_id) "+
            "values (1,CURRENT_DATE,CURRENT_DATE,1,"+this.parent.user.id+",'"+this.parent.application_id+"') ";
            Console.WriteLine (sql);
            DbCl.ExecuteTrans(DbCl.getConn(), sql);
            SetTransactionModel("",entSearch.Text.Trim());  
            SelectFirstRow(_lsModelTrans,_treeViewTrans);          
            ItemTransactionReady(false);
        }
        private void HandleCalendarDaySelected(object sender, EventArgs e){
            btnDate.Label = calendar.Date.ToString("yyyy-MM-dd");
            SetTransactionModel("",entSearch.Text.Trim()); 
            SelectFirstRow(_lsModelTrans,_treeViewTrans);
            popoverDate.Hide();
        }
        private DataTable fillDtTransaction(string transaction_id, string strfind){
            string whrfind = "",whrid="";
            if(transaction_id!="") whrid = "and tr.id="+transaction_id+ " ";
            if(strfind!="") whrfind = "and (upper(sup.organization_name) like upper('" + strfind + "%') or upper(pers.name) like upper('" + strfind + "%') )";
            
            string sql = "select tr.id,tr.reference_id, "+
                "tr.supplier_id,sup.organization_name,sup.organization_address,sup.organization_phone_number,pers.name person_name,pers.phone_number person_phone_number, "+
                "tr.transaction_type transaction_type_id, ty.name transaction_type_name, TO_CHAR(tr.transaction_date,'yyyy-mm-dd') transaction_date, tr.transaction_amount, tr.return_amount, "+
                "tr.payment_group_id, py.name payment_group_name, tr.payment_amount, "+
                "tr.user_id, usr.name user_name,  "+
                "tr.state, st.name state_name, st.fgcolor state_fgcolor, st.bgcolor state_bgcolor,  "+
                "tr.application_id, tr.tax_amount, tr.is_tax "+
                "from transaction tr left outer join supplier sup on tr.supplier_id=sup.id "+
                "left outer join payment_group py on tr.payment_group_id = py.id "+
                "left outer join person pers on sup.person_id=pers.id, "+
                "transaction_state st, transaction_type ty, "+
                "(select usr.id, pers.name, pers.phone_number from person pers,userlogin usr where usr.person_id=pers.id) usr "+
                "where tr.transaction_type=1 and tr.state=st.id and tr.transaction_type=ty.id "+
                "and tr.transaction_date::date = '"+btnDate.Label+"'::date and usr.id=tr.user_id "+
                whrid+ whrfind+ 
                "ORDER by tr.id desc";
                Console.WriteLine(sql);
                return DbCl.fillDataTable(DbCl.getConn(), sql);
        }
        private void SetTransactionModel(string transaction_id, string strfind)
        {                  
                //ListStore model;
                _lsModelTrans = null;
                TreeIter iter;
                /* create array */
                _clsTrans = new List<clTransaction>();
               
                clTransaction tran;
                dtTransSelected =  fillDtTransaction(transaction_id, strfind);
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
                        person_phone_number=dr["person_phone_number"].ToString(),
                        transaction_type_id=dr["transaction_type_id"].ToString(), 
                        transaction_type_name=dr["transaction_type_name"].ToString(), 
                        transaction_date=dr["transaction_date"].ToString(),  
                        transaction_amount=dr["transaction_amount"].ToString(),  
                        return_amount=dr["return_amount"].ToString(),  
                        payment_group_id=dr["payment_group_id"].ToString(),  
                        payment_group_name=dr["payment_group_name"].ToString(),
                        payment_amount=dr["payment_amount"].ToString(),
                        user_id=dr["user_id"].ToString(),
                        user_name=dr["user_name"].ToString(),
                        state=dr["state"].ToString(),
                        state_name=dr["state_name"].ToString(),
                        state_fgcolor=dr["state_fgcolor"].ToString(),
                        state_bgcolor=dr["state_bgcolor"].ToString(),
                        application_id=dr["application_id"].ToString(),
                    } ;                                 
                    _clsTrans.Add(tran);
                } 

                _lsModelTrans = new ListStore(typeof(string),typeof(string),typeof(string),typeof(string),typeof(string),typeof(string),typeof(string),typeof(string),typeof(string),typeof(string),typeof(string),typeof(string),typeof(string),typeof(string),typeof(string),typeof(string),typeof(string),typeof(string),typeof(string),typeof(string),typeof(string),typeof(string),typeof(string));

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
                    _lsModelTrans.SetValue(iter, (int)ColumnTrans.transaction_type_id, _clsTrans[i].transaction_type_id);
                    _lsModelTrans.SetValue(iter, (int)ColumnTrans.transaction_type_name, _clsTrans[i].transaction_type_name);
                    _lsModelTrans.SetValue(iter, (int)ColumnTrans.transaction_date, _clsTrans[i].transaction_date);
                    _lsModelTrans.SetValue(iter, (int)ColumnTrans.transaction_amount, _clsTrans[i].transaction_amount);
                    _lsModelTrans.SetValue(iter, (int)ColumnTrans.return_amount, _clsTrans[i].return_amount);
                    _lsModelTrans.SetValue(iter, (int)ColumnTrans.payment_group_id, _clsTrans[i].payment_group_id);
                    _lsModelTrans.SetValue(iter, (int)ColumnTrans.payment_group_name, _clsTrans[i].payment_group_name);
                    _lsModelTrans.SetValue(iter, (int)ColumnTrans.payment_amount, _clsTrans[i].payment_amount);
                    _lsModelTrans.SetValue(iter, (int)ColumnTrans.user_id, _clsTrans[i].user_id);
                    _lsModelTrans.SetValue(iter, (int)ColumnTrans.user_name, _clsTrans[i].user_name);
                    _lsModelTrans.SetValue(iter, (int)ColumnTrans.state, _clsTrans[i].state);
                    _lsModelTrans.SetValue(iter, (int)ColumnTrans.state_name, _clsTrans[i].state_name);
                    _lsModelTrans.SetValue(iter, (int)ColumnTrans.state_fgcolor, _clsTrans[i].state_fgcolor);
                    _lsModelTrans.SetValue(iter, (int)ColumnTrans.state_bgcolor, _clsTrans[i].state_bgcolor);
                    _lsModelTrans.SetValue(iter, (int)ColumnTrans.application_id, _clsTrans[i].application_id);
                }
                _treeViewTrans.Model = _lsModelTrans;
        }
       
       
        private void SetItemModel(Double transaction_id)
        {          
            //ListStore model;
            _lsModelItems = null;
            TreeIter iter;
            /* create array */
            _clsItems = new List<clTransItem>();
             string sql = "select ti.id, ti.transaction_id, ti.product_id, pr.short_name product_short_name, pr.name product_name,ti.stock_id, "+
            "case when ti.tax is null then 0 else ti.tax end tax, ti.state, state.name state_name, "+
            "st.quantity, st.unit,un.name unit_name, "+
            "price.id price_id, case when price.purchase_price is null then 0 else price.purchase_price end purchase_price, case when price.price is null then 0 else price.price end price, "+
            "st.location, lo.name location_name, st.condition, co.name condition_name "+
            "from transaction_item_state state, transaction_item ti, product pr, stock st "+
            "LEFT OUTER JOIN price on price.id = st.price_id "+
            "left outer join unit un on un.id=st.unit "+
            "left outer join condition co on st.condition=co.id "+
            "left outer join location lo on st.location=lo.id "+
            "where ti.product_id=pr.id and ti.stock_id=st.id and state.id=ti.state "+
            "and ti.transaction_id="+transaction_id.ToString()+ " "+
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
                    product_short_name=dr["product_short_name"].ToString(),
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
                    state_name=dr["state_name"].ToString(), 
                    location=dr["location"].ToString(), 
                    location_name=dr["location_name"].ToString(), 
                    condition=dr["condition"].ToString(),
                    condition_name=dr["condition_name"].ToString(), 
                };
                _clsItems.Add(item);                    
            }

            _lsModelItems = new ListStore(typeof(string),typeof(string),typeof(string),typeof(string),typeof(string),typeof(string),typeof(string),typeof(string),typeof(string),typeof(string),typeof(string),typeof(string),typeof(string),typeof(string),typeof(string),typeof(string),typeof(string),typeof(string),typeof(string));

            /* add items */
            for (int i = 0; i < _clsItems.Count; i++)
            {
                iter = _lsModelItems.Append();
                _lsModelItems.SetValue(iter, (int)ColumnItems.id, _clsItems[i].id);
                _lsModelItems.SetValue(iter, (int)ColumnItems.transaction_id, _clsItems[i].transaction_id);
                _lsModelItems.SetValue(iter, (int)ColumnItems.product_id, _clsItems[i].product_id);
                _lsModelItems.SetValue(iter, (int)ColumnItems.product_short_name, _clsItems[i].product_short_name);
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
                _lsModelItems.SetValue(iter, (int)ColumnItems.state_name, _clsItems[i].state_name);
                _lsModelItems.SetValue(iter, (int)ColumnItems.location, _clsItems[i].location);
                _lsModelItems.SetValue(iter, (int)ColumnItems.location_name, _clsItems[i].location_name);
                _lsModelItems.SetValue(iter, (int)ColumnItems.condition, _clsItems[i].condition);
                _lsModelItems.SetValue(iter, (int)ColumnItems.condition_name, _clsItems[i].condition_name);
            }
            _treeViewItems.Model = _lsModelItems;   
              
            lbTotalItem.Text =  GetTotalItem().ToString(); 
            entTaxAmount.Text = CalculateTax(GetTotalPurchasePrice()).ToString();
            entTransactionAmount.Text = (GetTotalPurchasePrice()+CalculateTax(GetTotalPurchasePrice())).ToString();
        }

        
        [GLib.ConnectBefore]
        private void HandleTreeViewItemsKeyPressEvent(object sender, KeyPressEventArgs e)
        {
            if (e.Event.Key == Gdk.Key.Delete || e.Event.Key == Gdk.Key.KP_Delete)  // Check if Enter key is pressed
            {
                TreeSelection selection = _treeViewItems.Selection;
                TreeIter iter;
                if(selection.GetSelected( out iter)){
                    Console.WriteLine("Selected Value:"+_lsModelItems.GetValue (iter, 0).ToString()+_lsModelItems.GetValue (iter, 1).ToString());
                }            
                string sql = "delete from stock where id="+_lsModelItems.GetValue (iter, 5).ToString();
                Console.WriteLine(sql);
                DbCl.ExecuteScalar(DbCl.getConn(), sql);
                sql = "delete from transaction_item where id="+_lsModelItems.GetValue (iter, 0).ToString();
                Console.WriteLine(sql);
                DbCl.ExecuteScalar(DbCl.getConn(), sql);
                SetItemModel(Convert.ToDouble(lbTransactionId.Text));
                if(GetTotalItem()==0) {
                    sql = "update transaction set state=1 where id="+lbTransactionId.Text;
                    Console.WriteLine (sql);
                    DbCl.ExecuteTrans(DbCl.getConn(), sql);                     
                }
                ItemTransactionReady(true);
            }
                      
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
        public Int64 InsertPrice(){
            string sql = "insert into price (input_date,purchase_price,price) values (CURRENT_TIMESTAMP,"+ CoreCl.GetLastPurchasePrice(dtItemSelected.Rows[0].ItemArray[0].ToString()).ToString() +","+ CoreCl.GetLastSalePrice(dtItemSelected.Rows[0].ItemArray[0].ToString()).ToString()+") returning id";
            Console.WriteLine (sql);
            return DbCl.ExecuteScalar(DbCl.getConn(), sql);
        }
        public Int64 InsertStock(string price_id){
            string sql = "insert into stock (product_id,quantity,input_date,expired_date, price_id, unit, condition, location)"+
            "values ("+dtItemSelected.Rows[0].ItemArray[0].ToString()+ ","+spnQty.Text+",CURRENT_DATE,CURRENT_DATE,"+price_id+",1,1,1) returning id";
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

                    Int64 price_id = InsertPrice();
                    Int64 stock_id = InsertStock(price_id.ToString());
                  //  tekan kene
                    string sql = "insert into transaction_item (transaction_id,product_id,stock_id,price_id,state) "+
                    "values("+lbTransactionId.Text+ ","+dtItemSelected.Rows[0].ItemArray[0].ToString() + ","+ stock_id.ToString() + ","+price_id.ToString()+",1)" ;
                    Console.WriteLine (sql);
                    DbCl.ExecuteTrans(DbCl.getConn(), sql); 
                    sql = "update transaction set state=2 where id="+lbTransactionId.Text;
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

            CellRendererText rendererText = new CellRendererText(); // 0
            //rendererText.Foreground = _lsModelTrans.GetValue((int)ColumnTrans.state_fgcolor) ;
            //rendererText.Background = ColumnTrans.state_bgcolor;
            _cellColumnsRender.Add(rendererText, (int)ColumnTrans.id);
            _treeViewTrans.InsertColumn(-1, "ID", rendererText, "text", (int)ColumnTrans.id);
            
            rendererText = new CellRendererText //1
            {
                Editable = isEditable
            };
            rendererText.Foreground = textForeground;
            rendererText.Background = textBackground;
            rendererText.Edited += CellEditedTrans;
            _cellColumnsRender.Add(rendererText, (int)ColumnTrans.reference_id);
            _treeViewTrans.InsertColumn(-1, "Reference ID", rendererText, "text", (int)ColumnTrans.reference_id);            

            rendererText = new CellRendererText(); //2
            _cellColumnsRender.Add(rendererText, (int)ColumnTrans.supplier_id);
            _treeViewTrans.InsertColumn(-1, "Supplier ID", rendererText, "text", (int)ColumnTrans.supplier_id);            

            rendererText = new CellRendererText(); //3
            _cellColumnsRender.Add(rendererText, (int)ColumnTrans.organization_name);
            _treeViewTrans.InsertColumn(-1, "Organization name", rendererText, "text", (int)ColumnTrans.organization_name); 

            rendererText = new CellRendererText(); //4
            _cellColumnsRender.Add(rendererText, (int)ColumnTrans.organization_phone_number);
            _treeViewTrans.InsertColumn(-1, "Organization phone number", rendererText, "text", (int)ColumnTrans.organization_phone_number);

            rendererText = new CellRendererText(); //5
            _cellColumnsRender.Add(rendererText, (int)ColumnTrans.person_name);
            _treeViewTrans.InsertColumn(-1, "Person name", rendererText, "text", (int)ColumnTrans.person_name);

            rendererText = new CellRendererText(); //5
            _cellColumnsRender.Add(rendererText, (int)ColumnTrans.person_phone_number);
            _treeViewTrans.InsertColumn(-1, "Person phone number", rendererText, "text", (int)ColumnTrans.person_phone_number);

            rendererText = new CellRendererText(); //6
            _cellColumnsRender.Add(rendererText, (int)ColumnTrans.transaction_type_id);
            _treeViewTrans.InsertColumn(-1, "Transaction type", rendererText, "text", (int)ColumnTrans.transaction_type_id);

            rendererText = new CellRendererText(); //6
            _cellColumnsRender.Add(rendererText, (int)ColumnTrans.transaction_type_name);
            _treeViewTrans.InsertColumn(-1, "Transaction type name", rendererText, "text", (int)ColumnTrans.transaction_type_name);

            rendererText = new CellRendererText(); //6
            _cellColumnsRender.Add(rendererText, (int)ColumnTrans.transaction_date);
            _treeViewTrans.InsertColumn(-1, "Transaction date", rendererText, "text", (int)ColumnTrans.transaction_date);

            rendererText = new CellRendererText();//13
            _cellColumnsRender.Add(rendererText, (int)ColumnTrans.transaction_amount);
            _treeViewTrans.InsertColumn(-1, "Transaction amount", rendererText, "text", (int)ColumnTrans.transaction_amount);

            rendererText = new CellRendererText();//13
            _cellColumnsRender.Add(rendererText, (int)ColumnTrans.return_amount);
            _treeViewTrans.InsertColumn(-1, "Transaction amount", rendererText, "text", (int)ColumnTrans.return_amount);

             rendererText = new CellRendererText(); //10
            _cellColumnsRender.Add(rendererText, (int)ColumnTrans.payment_group_id);
            _treeViewTrans.InsertColumn(-1, "Payment group id", rendererText, "text", (int)ColumnTrans.payment_group_id);

            rendererText = new CellRendererText();//11
            _cellColumnsRender.Add(rendererText, (int)ColumnTrans.payment_group_name);
            _treeViewTrans.InsertColumn(-1, "Payment group name", rendererText, "text", (int)ColumnTrans.payment_group_name);

            rendererText = new CellRendererText();//12
            _cellColumnsRender.Add(rendererText, (int)ColumnTrans.payment_amount);
            _treeViewTrans.InsertColumn(-1, "Payment amount", rendererText, "text", (int)ColumnTrans.payment_amount);

            rendererText = new CellRendererText(); //8
            _cellColumnsRender.Add(rendererText, (int)ColumnTrans.user_id);
            _treeViewTrans.InsertColumn(-1, "User ID", rendererText, "text", (int)ColumnTrans.user_id);

            rendererText = new CellRendererText(); //8
            _cellColumnsRender.Add(rendererText, (int)ColumnTrans.user_name);
            _treeViewTrans.InsertColumn(-1, "User name", rendererText, "text", (int)ColumnTrans.user_name);

            rendererText = new CellRendererText(); //7
            _cellColumnsRender.Add(rendererText, (int)ColumnTrans.state);
            _treeViewTrans.InsertColumn(-1, "State", rendererText, "text", (int)ColumnTrans.state);

            rendererText = new CellRendererText(); //7
            _cellColumnsRender.Add(rendererText, (int)ColumnTrans.state_name);
            _treeViewTrans.InsertColumn(-1, "State name", rendererText, "text", (int)ColumnTrans.state_name);

            rendererText = new CellRendererText();//15
            _cellColumnsRender.Add(rendererText, (int)ColumnTrans.state_fgcolor);
            _treeViewTrans.InsertColumn(-1, "State fgcolor", rendererText, "text", (int)ColumnTrans.state_fgcolor);

            rendererText = new CellRendererText();//16
            _cellColumnsRender.Add(rendererText, (int)ColumnTrans.state_bgcolor);            
            _treeViewTrans.InsertColumn(-1, "State bgcolor", rendererText, "text", (int)ColumnTrans.state_bgcolor);

            rendererText = new CellRendererText(); //9
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
            _cellColumnsRenderItems.Add(rendererText, (int)ColumnItems.product_short_name);
            _treeViewItems.InsertColumn(-1, "Short name", rendererText, "text", (int)ColumnItems.product_short_name);

            rendererText = new CellRendererText();
            _cellColumnsRenderItems.Add(rendererText, (int)ColumnItems.stock_id);
            _treeViewItems.InsertColumn(-1, "Stock ID", rendererText, "text", (int)ColumnItems.stock_id);

            rendererText = new CellRendererText
            {
                Editable = isEditable
            };
            rendererText.Foreground = textForeground;
            rendererText.Edited += CellEditedItem;
            _cellColumnsRenderItems.Add(rendererText, (int)ColumnItems.quantity);
            _treeViewItems.InsertColumn(-1, "Quantity", rendererText, "text", (int)ColumnItems.quantity);

            ListStore lstModelCombo = new ListStore(typeof(string), typeof(string));
            String sql = "Select id,name from unit order by name asc";
            DataTable dt = DbCl.fillDataTable(DbCl.getConn(), sql);
            foreach (DataRow dr in dt.Rows)
            {
                lstModelCombo.AppendValues(dr[0].ToString(), dr[0].ToString() + ").  " + dr[1].ToString());
            }
            CellRendererCombo rendererCombo = new CellRendererCombo
            { 
                Model = lstModelCombo,
                TextColumn = 1,
                HasEntry = false,
                Editable = isEditable
            };
            rendererCombo.Foreground = textForeground;
            rendererCombo.Edited += CellEditedItem;
            rendererCombo.EditingStarted += EditingStarted;           
            _cellColumnsRenderItems.Add(rendererCombo, (int)ColumnItems.unit_name);
            _treeViewItems.InsertColumn(-1, "Unit", rendererCombo, "text", (int)ColumnItems.unit_name);


            rendererText = new CellRendererText
            {
                Editable = isEditable
            };
            rendererText.Foreground = textForeground;
            rendererText.Edited += CellEditedItem;
            _cellColumnsRenderItems.Add(rendererText, (int)ColumnItems.purchase_price);
            _treeViewItems.InsertColumn(-1, "Purchase price", rendererText, "text", (int)ColumnItems.purchase_price);

            rendererText = new CellRendererText();
            _cellColumnsRenderItems.Add(rendererText, (int)ColumnItems.price_id);
            _treeViewItems.InsertColumn(-1, "Price ID", rendererText, "text", (int)ColumnItems.price_id);

            rendererText = new CellRendererText();
            _cellColumnsRenderItems.Add(rendererText, (int)ColumnItems.price);
            _treeViewItems.InsertColumn(-1, "Sale Price", rendererText, "text", (int)ColumnItems.price);

            rendererText = new CellRendererText();
            _cellColumnsRenderItems.Add(rendererText, (int)ColumnItems.tax);
            _treeViewItems.InsertColumn(-1, "Tax", rendererText, "text", (int)ColumnItems.tax);

            
            lstModelCombo = new ListStore(typeof(string), typeof(string));
            sql = "Select id,name from location order by id asc";
            dt = DbCl.fillDataTable(DbCl.getConn(), sql);
            foreach (DataRow dr in dt.Rows)
            {
                lstModelCombo.AppendValues(dr[0].ToString(), dr[0].ToString() + ").  " + dr[1].ToString());
            }
            rendererCombo = new CellRendererCombo
            { 
                Model = lstModelCombo,
                TextColumn = 1,
                HasEntry = false,
                Editable = isEditable
            };
            rendererCombo.Foreground = textForeground;
            rendererCombo.Edited += CellEditedItem;
            rendererCombo.EditingStarted += EditingStarted;           
            _cellColumnsRenderItems.Add(rendererCombo, (int)ColumnItems.location_name);
            _treeViewItems.InsertColumn(-1, "Location", rendererCombo, "text", (int)ColumnItems.location_name);


            lstModelCombo = new ListStore(typeof(string), typeof(string));
            sql = "Select id,name from condition order by id asc";
            dt = DbCl.fillDataTable(DbCl.getConn(), sql);
            foreach (DataRow dr in dt.Rows)
            {
                lstModelCombo.AppendValues(dr[0].ToString(), dr[0].ToString() + ").  " + dr[1].ToString());
            }
            rendererCombo = new CellRendererCombo
            { 
                Model = lstModelCombo,
                TextColumn = 1,
                HasEntry = false,
                Editable = isEditable
            };
            rendererCombo.Foreground = textForeground;
            rendererCombo.Edited += CellEditedItem;
            rendererCombo.EditingStarted += EditingStarted;           
            _cellColumnsRenderItems.Add(rendererCombo, (int)ColumnItems.condition_name);
            _treeViewItems.InsertColumn(-1, "Condition", rendererCombo, "text", (int)ColumnItems.condition_name);

            rendererText = new CellRendererText();
            _cellColumnsRenderItems.Add(rendererText, (int)ColumnItems.state_name);
            _treeViewItems.InsertColumn(-1, "State", rendererText, "text", (int)ColumnItems.state_name);
           
           rendererText = new CellRendererText();
            _cellColumnsRenderItems.Add(rendererText, (int)ColumnItems.product_name);
            _treeViewItems.InsertColumn(-1, "Product name", rendererText, "text", (int)ColumnItems.product_name);

        }
        private void CellEditedItem(object data, EditedArgs args)
        {
           TreePath path = new TreePath(args.Path);
            int column = _cellColumnsRenderItems[(CellRenderer)data];
            _lsModelItems.GetIter(out TreeIter iter, path);

            switch (column)
            {
                case (int)ColumnItems.quantity:
                {
                    int i = path.Indices[0];
                    _clsItems[i].quantity = args.NewText;
                    _lsModelItems.SetValue(iter, column, _clsItems[i].quantity);
                    string sql = "update stock set quantity = '"+args.NewText+"' where id='"+_clsItems[i].stock_id+"' ";
                    Console.WriteLine (sql);
                    DbCl.ExecuteTrans(DbCl.getConn(), sql);
                    entTaxAmount.Text = CalculateTax(GetTotalPurchasePrice()).ToString();
                    entTransactionAmount.Text = (GetTotalPurchasePrice()+CalculateTax(GetTotalPurchasePrice())).ToString();
                }
                break;
                case (int)ColumnItems.purchase_price:
                {   
                    // upadte harga beli pada tabel harga saja
                    // transaction_item tidak perlu diupdate 
                    // harga beli perlakuannya referensi bukan transaksi untuk mengakomodir perubahan setelah barang datang

                    int i = path.Indices[0];
                    _clsItems[i].purchase_price = args.NewText;
                    _lsModelItems.SetValue(iter, column, _clsItems[i].purchase_price);
                    string sql = "update price set purchase_price = '"+args.NewText+"' where id='"+_clsItems[i].price_id+"' ";
                    Console.WriteLine (sql);
                    DbCl.ExecuteTrans(DbCl.getConn(), sql);
                    entTaxAmount.Text = CalculateTax(GetTotalPurchasePrice()).ToString();
                    entTransactionAmount.Text = (GetTotalPurchasePrice()+CalculateTax(GetTotalPurchasePrice())).ToString();
                }
                break;
                case (int)ColumnItems.location_name:
                    {
                        string oldText = (string)_lsModelItems.GetValue(iter, column);
                        int i = path.Indices[0];                                              
                        if (args.NewText.Contains(")."))
                        {
                            String[] arr = args.NewText.Split(").");
                            _clsItems[i].location_name = arr[1].Trim();
                            _lsModelItems.SetValue(iter, column, _clsItems[i].location_name );  
                            
                            string sql = "update stock set location = '"+arr[0].Trim()+"' where id='"+_clsItems[i].stock_id+"' ";
                            Console.WriteLine (sql);
                            DbCl.ExecuteTrans(DbCl.getConn(), sql);
                        }
                    }
                    break;
                case (int)ColumnItems.condition_name:
                    {
                        string oldText = (string)_lsModelItems.GetValue(iter, column);
                        int i = path.Indices[0];                                              
                        if (args.NewText.Contains(")."))
                        {
                            String[] arr = args.NewText.Split(").");
                            _clsItems[i].condition_name = arr[1].Trim();
                            _lsModelItems.SetValue(iter, column, _clsItems[i].condition_name );  
                            
                            string sql = "update stock set condition = '"+arr[0].Trim()+"' where id='"+_clsItems[i].stock_id+"' ";
                            Console.WriteLine (sql);
                            DbCl.ExecuteTrans(DbCl.getConn(), sql);
                        }
                    }
                    break;
                case (int)ColumnItems.unit_name:
                    {
                        string oldText = (string)_lsModelItems.GetValue(iter, column);
                        int i = path.Indices[0];                                              
                        if (args.NewText.Contains(")."))
                        {
                            String[] arr = args.NewText.Split(").");
                            _clsItems[i].unit_name = arr[1].Trim();
                            _lsModelItems.SetValue(iter, column, _clsItems[i].unit_name );  
                            
                            string sql = "update stock set unit = '"+arr[0].Trim()+"' where id='"+_clsItems[i].stock_id+"' ";
                            Console.WriteLine (sql);
                            DbCl.ExecuteTrans(DbCl.getConn(), sql);
                        }
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
                ReferenceProduct refWidget = new ReferenceProduct(this,"dialog",1);
                popoverProduct.Add(refWidget);
                popoverProduct.SetSizeRequest(600, 300);
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
                string sql = "update transaction set supplier_id="+prm+" where id= "+lbTransactionId.Text;
                Console.WriteLine (sql);
                DbCl.ExecuteTrans(DbCl.getConn(), sql);
                SetTransactionModel("",entSearch.Text.Trim());
                SelectedTrans(lbTransactionId.Text);
                ItemTransactionReady(false);
                return false;
            });
           // SetTransactionModel(true,entSearch.Text.Trim());
        }
        private void ShowDatePopup(object sender, EventArgs e)
        {   
            GLib.Timeout.Add(0, () =>
            { 
                GuiCl.RemoveAllWidgets(popoverDate);
                calendar = new Calendar();
                calendar.DaySelected += HandleCalendarDaySelected;
                popoverDate.Add(calendar);
                popoverDate.SetSizeRequest(300, 150);
               // calendar.Show();          
                popoverDate.ShowAll();
                return false;
            });
        }
        private void ShowSupplierPopup(object sender, EventArgs e)
        {
            GLib.Timeout.Add(0, () =>
            {                   
                GuiCl.RemoveAllWidgets(popoverSupplier);        
                ReferenceSupplier refWidget = new ReferenceSupplier(this,"dialog","purchase");
                popoverSupplier.Add(refWidget);
                popoverSupplier.SetSizeRequest(200, 400);
                refWidget.Show();          
                popoverSupplier.ShowAll();
                return false;
            });
        }
        private void ShowPaymentPopup(object sender, EventArgs e)
        {
            GLib.Timeout.Add(0, () =>
            {                
                GuiCl.RemoveAllWidgets(popoverPayment);        
                InstallmentsPaid refWidget = new InstallmentsPaid(this,lbTransactionId.Text);
                popoverPayment.Add(refWidget);
                popoverPayment.SetSizeRequest(400, 300);
                refWidget.Show();          
                popoverPayment.ShowAll();
                return false;
            });
        }
        [GLib.ConnectBefore]
        private void OnEntAmountPaymentKeyPressEvent(object sender, KeyPressEventArgs e)
        {
            //if(dtItemSelected is not null){
               // Console.WriteLine(e.Event.Key);
                if (e.Event.Key == Gdk.Key.Return)
                {      
                    Console.WriteLine("aaaaaaaaaalllllllllllllllla");           
                    DoCheckout(sender,e);                    
                } 
            //}         
        }
        private void DoCheckout(object sender, EventArgs e)
        {
            Boolean valid = false;
            TreeSelection selection = _treeViewTrans.Selection;
            TreeIter iter;
            if(selection.GetSelected( out iter)){
                Console.WriteLine("Selected Value:"+_lsModelTrans.GetValue (iter, 0).ToString()+_lsModelTrans.GetValue (iter, 1).ToString());
            }  

            if( Convert.ToInt32(cmbPaymentMethod.ActiveText) < 5){
                if(Convert.ToInt32(entAmountPayment.Text) < Convert.ToInt32(entTransactionAmount.Text)){
                    string message = "Oh sorry, payment amount less than total purchase price";
                    MessageDialog md = new MessageDialog(null, DialogFlags.DestroyWithParent, MessageType.Error, ButtonsType.Close, message);
                    md.Run();
                    md.Destroy();
                    valid=false;
                }else{
                    valid = true;
                }
            }else{
                valid=true;
            }
            if(valid){
                string sql = "select * from transaction_item where transaction_id="+_lsModelTrans.GetValue (iter, 0).ToString();
                Console.WriteLine(sql);
                DataTable dt =  DbCl.fillDataTable(DbCl.getConn(), sql);
                foreach (DataRow dr in dt.Rows)
                { 
                    sql = "update stock set state=0 where id="+dr["stock_id"].ToString();
                    Console.WriteLine(sql);
                    DbCl.ExecuteTrans(DbCl.getConn(), sql);
                }
                sql = "update transaction_item set state=0 where transaction_id="+_lsModelTrans.GetValue (iter, 0).ToString();
                Console.WriteLine(sql);
                DbCl.ExecuteTrans(DbCl.getConn(), sql);
                sql = "insert into payment (transaction_id,payment_date,amount,user_id) values ("+_lsModelTrans.GetValue (iter, 0).ToString()+",CURRENT_TIMESTAMP,"+entAmountPayment.Text.Trim()+","+this.parent.user.id+")";
                Console.WriteLine(sql);
                DbCl.ExecuteTrans(DbCl.getConn(), sql);
                sql = "update transaction set is_tax="+chkTax.Active.ToString()+", transaction_amount="+entTransactionAmount.Text.Trim()+", payment_amount=" + CoreCl.GetPaymentAmount(_lsModelTrans.GetValue (iter, 0).ToString()).ToString()+", payment_group_id="+cmbPaymentMethod.ActiveText+", state=0 where id="+_lsModelTrans.GetValue (iter, 0).ToString();
                Console.WriteLine(sql);
                DbCl.ExecuteTrans(DbCl.getConn(), sql);
                SetTransactionModel("",entSearch.Text.Trim());  
                SelectedTrans(lbTransactionId.Text);
                SetItemModel(Convert.ToDouble(lbTransactionId.Text));
                //ItemTransactionReady(false);
                TransactionReady();
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