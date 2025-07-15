using System;
using System.Collections.Generic;
using Gtk;
using Gdk;
using System.Data;
using Pango;
using UI = Gtk.Builder.ObjectAttribute;
using System.Text.RegularExpressions;
using Inventorifo.Lib.Model;
using Cairo;

namespace Inventorifo.App
{
    //[Section(ContentType = typeof(EditableCellsSection), Category = Category.Widgets)]
    class TransactionSale : Gtk.Box
    {
        int transaction_type_id = 2;
        Inventorifo.Lib.LibDb DbCl = new Inventorifo.Lib.LibDb ();
        Inventorifo.Lib.LibGui GuiCl = new Inventorifo.Lib.LibGui ();
        Inventorifo.Lib.LibCore CoreCl = new Inventorifo.Lib.LibCore ();

        public MainWindow parent;
        clTransaction filterTrans;
        public TransactionSale(object parent, clTransaction filterTrans) : this(new Builder("TransactionSale.glade")) { 
            this.parent=(MainWindow)parent;
            this.filterTrans = filterTrans;
            Console.WriteLine(this.parent.user.id + " "+ this.parent.user.person_name);            
        }
        
        private TreeView _treeViewTrans;
        private ListStore _lsModelTrans;
        private Dictionary<CellRenderer, int> _cellColumnsRender;
        private List<clTransaction> _clsTrans;
        private TreeView _treeViewItems;
        private ListStore _lsModelItems;
        private Dictionary<CellRenderer, int> _cellColumnsRenderItems;
        private List<clTransactionItem1> _clsItems;
        
        Box  boxMiddle;
        Box boxItem;
        Box boxTransaction;
        Box boxCustomerDetail;
        Box boxProductDetail;
        Box boxTotalCalculation;
        Box boxPayment;

        SpinButton spnQty;
        Button btnNew;
        Button btnPreviousPayment;
        Button btnProcessCheckout;
        Button btnProcessCheckoutPrint;
        Button btnProduct;
        Button btnCustomer;
        Button btnDate;

        private Entry entSearch;
        private Entry entBarcode;
        private Entry entAmountPayment;
        private Entry entTransactionAmount;
        private Entry entTaxAmount;

        private Popover popoverCustomer ;
        private Popover popoverProduct ;
        private Popover popoverDate;
        private Popover popoverPayment;
        private TextView textViewCustomer;
        private TextView textViewProduct;

        DataTable dtTransSelected;
        DataTable dtItems;
        DataTable dtItemSelected;

        Label lbTransactionId;
        Label lbTotalItem;
        Label lbPreviousPayment;
        Label lbBillCalculated;
        Label lbChange;

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
            customer_id,
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
            purchase_price_id,
            purchase_final_price,
            item_price,
            main_discount,
            additional_discount,
            deduction_amount,
            final_price,
            subtotal,
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
            //boxItem.ModifyBg(StateType.Normal, new Gdk.Color(237, 237, 222));
            boxTransaction = (Box)builder.GetObject("BoxTransaction");
            //boxTransaction.ModifyBg(StateType.Normal, new Gdk.Color(224, 235, 235));
            boxCustomerDetail = (Box)builder.GetObject("BoxCustomerDetail");
            boxProductDetail = (Box)builder.GetObject("BoxProductDetail");
            boxTotalCalculation = (Box)builder.GetObject("BoxTotalCalculation");
            boxPayment = (Box)builder.GetObject("BoxPayment");

            spnQty = (SpinButton)builder.GetObject("SpnQty");
            spnQty.Xalign = 0.5f; 
            spnQty.KeyPressEvent += OnSpnQtyKeyPressEvent;
            spnQty.ModifyFont(FontDescription.FromString("Arial 14"));
            
            
            btnProcessCheckout = (Button)builder.GetObject("BtnProcessCheckout");
            btnProcessCheckout.Clicked += BtnProcessCheckoutClicked;
            btnProcessCheckoutPrint = (Button)builder.GetObject("BtnProcessCheckoutPrint");
            btnProcessCheckoutPrint.Clicked += btnProcessCheckoutPrintClicked;

            btnProduct = (Button)builder.GetObject("BtnProduct");
            popoverProduct = new Popover(btnProduct);    
            btnProduct.Clicked += ShowProductPopup;

            btnCustomer = (Button)builder.GetObject("BtnCustomer");
            popoverCustomer = new Popover(btnCustomer);    
            btnCustomer.Clicked += ShowCustomerPopup;

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
            textBackground = "white";
            isEditable = true;
            AddColumnsTrans(); 
            _treeViewTrans.Selection.Changed += HandleTreeVewSelectedTrans;

            _treeViewItems = (TreeView)builder.GetObject("TreeViewItem");
            _treeViewItems.Selection.Mode = SelectionMode.Single;
            _treeViewItems.ModifyFont(FontDescription.FromString("Arial 14"));
            AddColumnsItems();     
            _treeViewItems.KeyPressEvent += HandleTreeViewItemsKeyPressEvent;
            _treeViewItems.Selection.Changed += HandleTreeVewSelectedItem;
            _treeViewItems.Columns[0].Visible = false;
            _treeViewItems.Columns[1].Visible = false;
            _treeViewItems.Columns[2].Visible = false;
            _treeViewItems.Columns[4].Visible = false;
            _treeViewItems.Columns[7].Visible = false; //purchase_price
            _treeViewItems.Columns[8].Visible = false;
            // _treeViewItems.Columns[10].Visible = false; //tax
            
            textViewProduct = (TextView)builder.GetObject("TextViewProduct");
            textViewCustomer = (TextView)builder.GetObject("TextViewCustomer");
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
            
            lbChange = (Label)builder.GetObject("LbChange");
            chkTax = (CheckButton)builder.GetObject("ChkTax");
            chkTax.Toggled += HandlechkTaxChanged;

            SetTransactionModel("",entSearch.Text.Trim());
            TransactionReady();       
        }        

        private int GetTotalItem(){
            int total = 0;
            for (int i = 0; i < _clsItems.Count; i++)
            {
                total += Convert.ToInt32(_clsItems[i].quantity) ;
            } 
            return total;
        }
        private double GetTotalSalePrice(){
            double total = 0;
            for (int i = 0; i < _clsItems.Count; i++)
            {
                total += (Convert.ToDouble(_clsItems[i].quantity)*Convert.ToDouble(_clsItems[i].final_price)) ;
            } 
            return total;
        }
        private double CalculateTax(double amount){
            if(chkTax.Active) return amount*Convert.ToDouble(this.parent.conf.tax)/100;
            else return 0;
        }

        private void SetTotalCalculation(){
            if(_clsItems.Count>0){
               lbTotalItem.Text =  GetTotalItem().ToString(); 
                entTaxAmount.Text = CalculateTax(GetTotalSalePrice()).ToString();
                entTransactionAmount.Text = (GetTotalSalePrice()+CalculateTax(GetTotalSalePrice())).ToString(); 
            }
            
        }

        private void btnProcessCheckoutPrintClicked(object sender, EventArgs e)
        {
            PrintingSaleInvoice clPrinting = new PrintingSaleInvoice(this, dtTransSelected, dtItems);
            clPrinting.DoPrint(true);
        }
        
        // private void PrintButton_Clicked(object sender, EventArgs e)
        // {
        //     // Buat sebuah string untuk invoice
        //     string invoice = "Invoice No: 001\n";
        //     invoice += "Nama Pelanggan: John Doe\n";
        //     invoice += "Alamat: Jl. Contoh No. 123\n";
        //     invoice += "Total: Rp. 100.000\n";

        //     // Cetak invoice menggunakan PrintOperation
        //     Gtk.PrintOperation printOperation = new Gtk.PrintOperation();
        //     printOperation.BeginPrint += (o, args) => {
        //         args.Context.SetCairoContext(new Cairo.Context(), 0, 0);
        //     };
        //     printOperation.DrawPage += (o, args) => {
        //         Cairo.Context cr = args.Context.CairoContext;
        //         cr.SelectFontFace("Sans", Cairo.FontSlant.Normal, Cairo.FontWeight.Normal);
        //         cr.SetFontSize(12);
        //         cr.MoveTo(10, 10);
        //         cr.ShowText(invoice);
        //     };
        //     printOperation.EndPrint += (o, args) => {
        //         Console.WriteLine("Invoice telah dicetak");
        //     };
        //     printOperation.Run(Gtk.PrintOperationAction.PrintDialog, this.parent);
        // }

        [GLib.ConnectBefore]
        private void OnThisKeyPressEvent(object sender, KeyPressEventArgs e)
        {
            Console.WriteLine(e.Event.Key);
            if (e.Event.Key == Gdk.Key.F1)
            {                 
              NewTransaction(new object(),new EventArgs());
            }else if (e.Event.Key == Gdk.Key.F2)
            { 
                if(boxItem.Sensitive==true) ShowCustomerPopup(new object(),new EventArgs());
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
            textViewCustomer.Buffer.Text = "";
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
            // filterTrans.transaction_date = btnDate.Label;
            // filterTrans.id = transaction_id;
            clTransaction tran;
            clTransaction filterTrans = new clTransaction {
                    id=transaction_id.ToString(),
                    transaction_date = btnDate.Label, 
                    transaction_type_id="2"
                };
            dtTransSelected =  CoreCl.fillDtTransactionSale(filterTrans);
            foreach (DataRow dr in dtTransSelected.Rows)
            { 
                var id = transaction_id;
                var state = dr["state"].ToString();
                var customer_id = dr["customer_id"].ToString();
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
                lbBillCalculated.Text = CoreCl.GetOutstandingBalance(transaction_amount, payment_amount).ToString() ;
                
                var tag = new TextTag (null);
                textViewCustomer.Buffer.TagTable.Add (tag);
                tag.Weight = Pango.Weight.Bold;

                textViewCustomer.Buffer.Text = "\nOrganization name:\n\nOrganization address:\n\nPhone number:\n\nPerson name:\n\nPhone number:\n\n";
                var iter = textViewCustomer.Buffer.GetIterAtLine (2);
                textViewCustomer.Buffer.InsertWithTags (ref iter, organization_name, tag);

                iter = textViewCustomer.Buffer.GetIterAtLine (4);
                textViewCustomer.Buffer.InsertWithTags (ref iter, organization_address, tag);

                iter = textViewCustomer.Buffer.GetIterAtLine (6);
                textViewCustomer.Buffer.InsertWithTags (ref iter, organization_phone_number, tag);

                iter = textViewCustomer.Buffer.GetIterAtLine (8);
                textViewCustomer.Buffer.InsertWithTags (ref iter, person_name, tag);

                iter = textViewCustomer.Buffer.GetIterAtLine (10);
                textViewCustomer.Buffer.InsertWithTags (ref iter, person_phone_number, tag);
                lbTransactionId.Text = id;
                CellRendererText retrievedRenderer = GuiCl.GetCellRendererText(_treeViewItems, 2);
                

                if(Convert.ToInt32(state)==0){ 
                    if(Convert.ToInt32(payment_group_id)>5 ){   
                        if(Convert.ToDouble(transaction_amount)>Convert.ToDouble(payment_amount)){
                            GuiCl.SensitiveAllWidgets(boxItem,true);                 
                            GuiCl.SetDisableAllColumn(_treeViewItems);
                            btnCustomer.Sensitive = false;
                            btnProduct.Sensitive = false;
                            entTransactionAmount.Sensitive = false;
                            spnQty.Sensitive=false;
                            cmbPaymentMethod.Sensitive = false;
                            //boxPayment.Sensitive = true;
                            chkTax.Sensitive = false;
                            entTaxAmount.Sensitive = false;
                            cmbPaymentMethod.Sensitive = false;
                            entAmountPayment.Sensitive = true;
                            btnProcessCheckout.Sensitive = true;

                        } else{
                           // GuiCl.SensitiveAllWidgets(boxItem,false); 
                            GuiCl.SetDisableAllColumn(_treeViewItems);
                            btnCustomer.Sensitive = false;
                            btnProduct.Sensitive = false;
                            entTransactionAmount.Sensitive = false;
                            spnQty.Sensitive=false;
                            cmbPaymentMethod.Sensitive = false;
                            //boxPayment.Sensitive = false;
                            chkTax.Sensitive = false;
                            entTaxAmount.Sensitive = false;
                            cmbPaymentMethod.Sensitive = false;
                            entAmountPayment.Sensitive = false;
                            btnProcessCheckout.Sensitive = false;
                        }                        
                    }else{
                       // GuiCl.SensitiveAllWidgets(boxItem,false); 
                            //_treeViewItems.Sensitive = true;
                            GuiCl.SetDisableAllColumn(_treeViewItems);
                            btnCustomer.Sensitive = false;
                            btnProduct.Sensitive = false;
                            entTransactionAmount.Sensitive = false;
                            spnQty.Sensitive=false;
                            cmbPaymentMethod.Sensitive = false;
                            //boxPayment.Sensitive = false;
                            chkTax.Sensitive = false;
                            entTaxAmount.Sensitive = false;
                            cmbPaymentMethod.Sensitive = false;
                            entAmountPayment.Sensitive = false;
                            btnProcessCheckout.Sensitive = false;
                    }
                }else{
                 
                    GuiCl.SensitiveAllWidgets(boxItem,true);
                    GuiCl.SetEnableColumn(_treeViewItems,[10]);
                    GuiCl.SetEnableColumn(_treeViewItems,[11]);
                    GuiCl.SetEnableColumn(_treeViewItems,[12]);
                    btnCustomer.Sensitive = true;
                    btnProduct.Sensitive = true;
                    boxCustomerDetail.Sensitive = true;
                    boxProductDetail.Sensitive = true;
                    entTransactionAmount.Sensitive = true;
                    spnQty.Sensitive=true;
                    chkTax.Sensitive = true;
                    entTaxAmount.Sensitive = true;
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
        // private int GetTotalItem(){
        //     int total = 0;
        //     for (int i = 0; i < _clsItems.Count; i++)
        //     {
        //         total += Convert.ToInt32(_clsItems[i].quantity) ;
        //     } 
        //     return total;
        // }
        // private double GetTotalSalePrice(){
        //     double total = 0;
        //     for (int i = 0; i < _clsItems.Count; i++)
        //     {
        //         total += (Convert.ToDouble(_clsItems[i].quantity)*Convert.ToDouble(_clsItems[i].final_price)) ;
        //     } 
        //     return total;
        // }

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
                Console.WriteLine("method : "+cmbPaymentMethod.ActiveText);
                if(entry.Text.Trim()!="" && entTransactionAmount.Text.Trim()!=""){
                    if( Convert.ToInt32(cmbPaymentMethod.ActiveText) < 5 ){
                        if(Convert.ToDouble(entry.Text) < Convert.ToDouble(entTransactionAmount.Text)){
                            entAmountPayment.ModifyBg(StateType.Normal, new Gdk.Color(255, 237, 222));
                            entAmountPayment.ModifyFg(StateType.Normal, new Gdk.Color(0, 0, 0));
                        }else{
                            entAmountPayment.ModifyBg(StateType.Normal, new Gdk.Color(237, 255, 222));
                            entAmountPayment.ModifyFg(StateType.Normal, new Gdk.Color(0, 0, 0));
                            lbChange.Text  = CalculateChange().ToString();
                        }
                    }
                }                
            }
        }

        private double CalculateChange(){
            return Convert.ToDouble(entAmountPayment.Text)-Convert.ToDouble(entTransactionAmount.Text);
        }
        private void HandlechkTaxChanged(object sender, EventArgs e)
        {
            if(chkTax.Active){
                entTaxAmount.Text = CalculateTax(GetTotalSalePrice()).ToString();
                entTransactionAmount.Text = (GetTotalSalePrice()+CalculateTax(GetTotalSalePrice())).ToString();
                // string sql = "update transaction_order_item set tax = final_price*"+this.parent.conf.tax+" where transaction_id="+lbTransactionId.Text ;
                // Console.WriteLine (sql);
                // DbCl.ExecuteTrans(DbCl.getConn(), sql);
            }else{
                entTaxAmount.Text = "0";
                entTransactionAmount.Text = (GetTotalSalePrice()).ToString();
                // string sql = "update transaction_order_item set tax = 0 where transaction_id="+lbTransactionId.Text ;
                // Console.WriteLine (sql);
                // DbCl.ExecuteTrans(DbCl.getConn(), sql);
            }
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
            string sql = "insert into transaction (transaction_type,transaction_date,input_date,customer_id,user_id,application_id) "+
            "values (2,CURRENT_DATE,CURRENT_DATE,1,"+this.parent.user.id+",'"+this.parent.application_id+"') ";
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
        
        private void SetTransactionModel(string transaction_id, string strfind)
        {                  
                //ListStore model;
                _lsModelTrans = null;
                TreeIter iter;
                /* create array */
                _clsTrans = new List<clTransaction>();               
                // filterTrans.transaction_date = btnDate.Label;
                // filterTrans.person_name = strfind;
                // filterTrans.id = transaction_id;
                clTransaction filterTrans = new clTransaction {
                    id=transaction_id.ToString(),
                    transaction_date = btnDate.Label, 
                    transaction_type_id="2"
                };
                clTransaction tran;
                DataTable dt =  CoreCl.fillDtTransactionSale(filterTrans);
                foreach (DataRow dr in dt.Rows)
                {              
                    tran = new clTransaction{    
                        id=dr["id"].ToString(),
                        reference_id=dr["reference_id"].ToString(),
                        customer_id=dr["customer_id"].ToString(),
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
                    _lsModelTrans.SetValue(iter, (int)ColumnTrans.customer_id, _clsTrans[i].customer_id);
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
            _clsItems = new List<clTransactionItem1>();           
            clTransactionItem1 item;
            filterTrans.transaction_date = btnDate.Label;
            filterTrans.id= transaction_id.ToString();
            clTransactionItem1 filterItem = new clTransactionItem1{                    
                //product_short_name = entSearch.Text.Trim(),
            };
            dtItems = CoreCl.fillDtOrderTransactionItem(filterTrans, filterItem);
            foreach (DataRow dr in dtItems.Rows)
            {   
                //double final_price = CoreCl.CalculateFinalPrice(dr["item_price"].ToString(), dr["main_discount"].ToString(), dr["additionl_discount"].ToString(), dr["deduction_amount"].ToString());
                item = new clTransactionItem1{
                    id=dr["id"].ToString(),
                    transaction_id=dr["transaction_id"].ToString(),
                    product_id=dr["product_id"].ToString(),
                    product_short_name=dr["product_short_name"].ToString(),
                    product_name=dr["product_name"].ToString(),
                    stock_id=dr["stock_id"].ToString(),
                    quantity=dr["quantity"].ToString(),
                    unit=dr["unit"].ToString(),
                    unit_name=dr["unit_name"].ToString(),
                    purchase_price_id=dr["purchase_price_id"].ToString(),
                    purchase_subtotal= (Convert.ToDouble(dr["purchase_final_price"].ToString())*Convert.ToDouble(dr["quantity"].ToString())).ToString(),
                    item_price=dr["item_price"].ToString(),
                    main_discount=dr["main_discount"].ToString(),
                    additional_discount=dr["additional_discount"].ToString(),
                    deduction_amount=dr["deduction_amount"].ToString(),
                    final_price= dr["final_price"].ToString(),
                    subtotal= (Convert.ToDouble(dr["final_price"].ToString())*Convert.ToDouble(dr["quantity"].ToString())).ToString(),
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

            _lsModelItems = new ListStore(typeof(string),typeof(string),typeof(string),typeof(string),typeof(string),typeof(string),typeof(string),typeof(string),typeof(string),typeof(string),typeof(string),typeof(string),typeof(string),typeof(string),typeof(string),typeof(string),typeof(string),typeof(string),typeof(string),typeof(string),typeof(string),typeof(string),typeof(string),typeof(string));

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
                _lsModelItems.SetValue(iter, (int)ColumnItems.purchase_price_id, _clsItems[i].purchase_price_id);
                _lsModelItems.SetValue(iter, (int)ColumnItems.purchase_final_price, _clsItems[i].purchase_final_price);
                _lsModelItems.SetValue(iter, (int)ColumnItems.item_price, _clsItems[i].item_price);
                _lsModelItems.SetValue(iter, (int)ColumnItems.main_discount, _clsItems[i].main_discount);
                _lsModelItems.SetValue(iter, (int)ColumnItems.additional_discount, _clsItems[i].additional_discount);
                _lsModelItems.SetValue(iter, (int)ColumnItems.deduction_amount, _clsItems[i].deduction_amount);
                _lsModelItems.SetValue(iter, (int)ColumnItems.final_price, _clsItems[i].final_price);
                _lsModelItems.SetValue(iter, (int)ColumnItems.subtotal, _clsItems[i].subtotal);
                _lsModelItems.SetValue(iter, (int)ColumnItems.tax, _clsItems[i].tax);
                _lsModelItems.SetValue(iter, (int)ColumnItems.state, _clsItems[i].state);
                _lsModelItems.SetValue(iter, (int)ColumnItems.state_name, _clsItems[i].state_name);
                _lsModelItems.SetValue(iter, (int)ColumnItems.location, _clsItems[i].location);
                _lsModelItems.SetValue(iter, (int)ColumnItems.location_name, _clsItems[i].location_name);
                _lsModelItems.SetValue(iter, (int)ColumnItems.condition, _clsItems[i].condition);
                _lsModelItems.SetValue(iter, (int)ColumnItems.condition_name, _clsItems[i].condition_name);
            }
            _treeViewItems.Model = _lsModelItems;   
              
            // lbTotalItem.Text =  GetTotalItem().ToString(); 
            // entTaxAmount.Text = CalculateTax(GetTotalSalePrice()).ToString();
            // entTransactionAmount.Text = (GetTotalSalePrice()+CalculateTax(GetTotalSalePrice())).ToString();
            SetTotalCalculation();
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
                if(_lsModelItems.GetValue (iter, 18).ToString()!="0"){ 
                    string sql = "delete from transaction_order_item where id="+_lsModelItems.GetValue (iter, 0).ToString();
                    Console.WriteLine(sql);
                    DbCl.ExecuteScalar(DbCl.getConn(), sql);
                    SetItemModel(Convert.ToDouble(lbTransactionId.Text));
                }
            }
                      
        }

        private void HandleTreeVewSelectedItem(object sender, EventArgs e)
        {
            if (!_treeViewItems.Selection.GetSelected(out TreeIter it))
                 return;
            TreePath path = _lsModelItems.GetPath(it);
            var id = (string)_lsModelItems.GetValue(it, (int)ColumnItems.product_id);            
            SelectedItem(id);
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
            var iter = textViewProduct.Buffer.GetIterAtLine (2);
            textViewProduct.Buffer.InsertWithTags (ref iter, dtItemSelected.Rows[0].ItemArray[1].ToString(), tag);

            iter = textViewProduct.Buffer.GetIterAtLine (4);
            textViewProduct.Buffer.InsertWithTags (ref iter, dtItemSelected.Rows[0].ItemArray[2].ToString(), tag);

            iter = textViewProduct.Buffer.GetIterAtLine (6);
            textViewProduct.Buffer.InsertWithTags (ref iter, dtItemSelected.Rows[0].ItemArray[3].ToString(), tag);
            
            iter = textViewProduct.Buffer.GetIterAtLine (8);
            textViewProduct.Buffer.InsertWithTags (ref iter, dtItemSelected.Rows[0].ItemArray[5].ToString(), tag);
        }
       
        public Response AddItem(string product_id, string quantity){
            Console.WriteLine("========AddItem======1===="+ product_id); 
            string sql="";
            Response resp = new Response();
            Boolean valid=false; 
            //double store_quantity = 0;
            double global_quantity = 0;
            //check available product
            DataTable dts = CoreCl.fillDtProduct(transaction_type_id,new clStock{product_id=product_id});
            foreach(DataRow drs in dts.Rows){
                global_quantity = Convert.ToDouble(drs["global_quantity"].ToString());
                if(Convert.ToDouble(spnQty.Text) > global_quantity ){
                    resp = new Response{ code = "22",description = "Failed, "+drs["short_name"].ToString()+" out of stock on store"} ;
                }else{
                    resp = new Response{ code = "20",description = "Success"} ;
                    valid = true;
                }
            } 

            Console.WriteLine("==============3====" + valid.ToString()); 
            //insert transaksi item disik
            
            if(valid){
                //FIFO show all stock in store location
                sql = "select stock.id stock_id,stock.quantity,stock.product_id,stock.price_id,stock.location, stock.condition "+
                "from stock,location loc,location_group locgr "+
                "where stock.quantity>0 and state=0 and locgr.id=2 and product_id="+ product_id + " "+
                "and stock.location=loc.id and loc.location_group=locgr.id "+
                "order by stock.id asc";
                Console.WriteLine(sql);   
                double balance = 0;
                dts = DbCl.fillDataTable(DbCl.getConn(), sql);
                balance = Convert.ToDouble(quantity);  
                foreach(DataRow drs in dts.Rows){                    
                    if(Convert.ToDouble(balance) > Convert.ToDouble(drs["quantity"].ToString()) ){                        
                        sql = "insert into transaction_item (transaction_id,product_id,quantity,stock_id,price_id,state, location, condition) "+
                        "values("+lbTransactionId.Text+ ","+product_id + ","+drs["quantity"].ToString()+","+ drs["stock_id"].ToString() + ","+drs["price_id"].ToString()+",1,"+ drs["location"].ToString() + ","+drs["condition"].ToString()+")" ;
                        Console.WriteLine (sql); 
                        DbCl.ExecuteTrans(DbCl.getConn(), sql);  
                        CoreCl.InsertStockHistory(lbTransactionId.Text,transaction_type_id.ToString(), product_id, drs["stock_id"].ToString(),drs["location"].ToString(),drs["condition"].ToString(),"-"+drs["quantity"].ToString());
                        sql = "update stock set quantity=0 where id="+drs["stock_id"].ToString() ;
                        Console.WriteLine (sql);
                        DbCl.ExecuteTrans(DbCl.getConn(), sql);
                        balance = balance-Convert.ToDouble(drs["quantity"].ToString());
                    }else{                        
                        sql = "insert into transaction_item (transaction_id,product_id,quantity,stock_id,price_id,state, location, condition) "+
                        "values("+lbTransactionId.Text+ ","+product_id + ","+balance.ToString()+","+ drs["stock_id"].ToString() + ","+drs["price_id"].ToString()+",1,"+ drs["location"].ToString() + ","+drs["condition"].ToString()+")" ;
                        Console.WriteLine (sql); 
                        DbCl.ExecuteTrans(DbCl.getConn(), sql); 
                        CoreCl.InsertStockHistory(lbTransactionId.Text,transaction_type_id.ToString(), product_id, drs["stock_id"].ToString(),drs["location"].ToString(),drs["condition"].ToString(),"-"+balance.ToString());
                        sql = "update stock set quantity=quantity-"+balance.ToString()+" where id="+drs["stock_id"].ToString() ;
                        Console.WriteLine (sql);
                        DbCl.ExecuteTrans(DbCl.getConn(), sql);
                        balance = 0;
                    }
                    if (balance==0) break;
                }
                 
                sql = "update transaction set state=2 where id="+lbTransactionId.Text;
                Console.WriteLine(sql);
                DbCl.ExecuteTrans(DbCl.getConn(), sql);
                resp = new Response{ code = "20",description = "Success"} ;
            }            
            return resp;            
        }

        [GLib.ConnectBefore]
        private void OnSpnQtyKeyPressEvent(object sender, KeyPressEventArgs e)
        {
            if(dtItemSelected is not null){
               // Console.WriteLine(e.Event.Key);
                if (e.Event.Key == Gdk.Key.Return)
                {    
                    Console.WriteLine("=OnSpnQtyKeyPressEvent=======");
                    string sql = "select stock.id stock_id,stock.quantity,stock.product_id,stock.price_id,stock.location,TO_CHAR(stock.expired_date,'yyyy-MM-dd') expired_date, stock.unit, stock.condition "+
                    "from stock,location loc,location_group locgr "+
                    "where stock.quantity>0 and state=0 and locgr.id=2 and product_id="+ dtItemSelected.Rows[0].ItemArray[0].ToString() + " "+
                    "and stock.location=loc.id and loc.location_group=locgr.id "+
                    "order by stock.id asc limit 1";
                    Console.WriteLine(sql);   
                    DataTable dts = DbCl.fillDataTable(DbCl.getConn(), sql);
                    foreach(DataRow drs in dts.Rows){ 
                        sql = "insert into transaction_order_item (transaction_id,product_id,quantity,stock_id,purchase_price_id,main_discount, additional_discount,deduction_amount, state) "+
                        "values("+lbTransactionId.Text+ ","+dtItemSelected.Rows[0].ItemArray[0].ToString() + ","+spnQty.Text+","+ drs["stock_id"].ToString() + ","+drs["price_id"].ToString()+",0,0,0,1)" ;
                        Console.WriteLine (sql); 
                        DbCl.ExecuteTrans(DbCl.getConn(), sql); 
                    }
                    SetItemModel(Convert.ToDouble(lbTransactionId.Text));
                    ItemTransactionReady(true);
                    SetTotalCalculation();
                    // Response resp = AddItem();
                    // if(resp.code=="20"){
                    //     SetItemModel(Convert.ToDouble(lbTransactionId.Text));
                    //     ItemTransactionReady(true);
                    // }else{
                    //     string message = resp.description;
                    //     MessageDialog md = new MessageDialog(null, DialogFlags.DestroyWithParent, MessageType.Error, ButtonsType.Close, message);
                    //     md.Run();
                    //     md.Destroy();
                    // }
                } 
            }         
        }

        private Response DoCheckout()
        {
            Response resp = new Response();
            Boolean valid = false;           

            if( Convert.ToInt32(cmbPaymentMethod.ActiveText) < 5){
                if(Convert.ToDouble(entAmountPayment.Text) < Convert.ToDouble(entTransactionAmount.Text)){
                    resp = new Response{ code = "23",description = "Oh sorry, payment amount less than total Sale price"};
                    valid=false;
                }else{
                    valid = true;
                }
            }else{
                valid=true;
            }
            if(valid){
                clTransactionItem1 filterItem = new clTransactionItem1{                    
                    //product_short_name = entSearch.Text.Trim(),
                };
                clTransaction filterTrans = new clTransaction {
                    id=lbTransactionId.Text,
                    transaction_date = DateTime.Now.ToString("yyyy-MM-dd"), 
                    transaction_type_id="1"
                };
                DataTable dts = CoreCl.fillDtOrderTransactionItem(filterTrans, filterItem);
                string sql;
                foreach(DataRow drs in dts.Rows){
                    resp  = AddItem(drs["product_id"].ToString(), drs["quantity"].ToString());                    
                    sql = "update transaction_order_item set state=0 where id="+drs["id"].ToString();
                    Console.WriteLine(sql);
                    DbCl.ExecuteTrans(DbCl.getConn(), sql);
                    double final_price= CoreCl.CalculateFinalPrice(drs["item_price"].ToString(),drs["main_discount"].ToString(),drs["additional_discount"].ToString(),drs["deduction_amount"].ToString());
                    double tax = CalculateTax(final_price);
                    sql = "insert into transaction_item (transaction_id,product_id,stock_id,quantity,unit,purchase_price_id,item_price,main_discount,additional_discount,deduction_amount,final_price,tax,state,location,condition) "+
                    "values ('"+drs["transaction_id"].ToString()+"','"+drs["product_id"].ToString()+"','"+drs["stock_id"].ToString()+"','"+drs["quantity"].ToString()+"','"+drs["unit"].ToString()+"','"+drs["purchase_price_id"].ToString()+"','"+drs["item_price"].ToString()+"','"+drs["main_discount"].ToString()+"','"+drs["additional_discount"].ToString()+"','"+drs["deduction_amount"].ToString()+"','"+final_price.ToString()+"','"+tax.ToString()+"',0,'"+drs["location"].ToString()+"','"+drs["condition"].ToString()+"') ";
                    Console.WriteLine(sql);
                    DbCl.ExecuteTrans(DbCl.getConn(), sql);
                }
               
                sql = "update transaction_item set state=0 where transaction_id="+lbTransactionId.Text;
                Console.WriteLine(sql);
                DbCl.ExecuteTrans(DbCl.getConn(), sql);

                double paymentAmount = Convert.ToDouble(entAmountPayment.Text.Trim());
                if(paymentAmount>Convert.ToDouble(entTransactionAmount.Text)) paymentAmount = Convert.ToDouble(entTransactionAmount.Text);
                sql = "insert into payment (transaction_id,payment_date,amount,user_id) values ("+lbTransactionId.Text+",CURRENT_TIMESTAMP,"+paymentAmount.ToString()+","+this.parent.user.id+")";
                Console.WriteLine(sql);
                DbCl.ExecuteTrans(DbCl.getConn(), sql);
                sql = "update transaction set is_tax = '"+ chkTax.Active.ToString() +"', tax_amount='"+entTaxAmount.Text.Trim()+"', transaction_amount="+entTransactionAmount.Text.Trim()+", payment_amount=" + CoreCl.GetPaymentAmount(lbTransactionId.Text)+", payment_group_id="+cmbPaymentMethod.ActiveText+", state=0 where id="+lbTransactionId.Text;
                Console.WriteLine(sql);
                DbCl.ExecuteTrans(DbCl.getConn(), sql);   
                            
                //insert journal sale                
                if(Convert.ToInt32(cmbPaymentMethod.ActiveText)>5 ) // account rechievable
                    if(lbBillCalculated.Text=="0") { 
                         // sale rechievable 
                        CoreCl.InsertJournal(5, Convert.ToDouble(entTransactionAmount.Text.Trim())- Convert.ToDouble(entTaxAmount.Text.Trim()), Convert.ToDouble(entTaxAmount.Text.Trim()), lbTransactionId.Text, "Transaction sale installments paid", this.parent.user.id, this.parent.application_id );
                         // install payment 
                        CoreCl.InsertJournal(6,paymentAmount, 0, lbTransactionId.Text, "Payment sale", this.parent.user.id, this.parent.application_id );
                    }else{ // install payment 
                        CoreCl.InsertJournal(6,paymentAmount, 0, lbTransactionId.Text, "Payment sale", this.parent.user.id, this.parent.application_id );
                    }                    
                else{ //cash payment
                    CoreCl.InsertJournal(4, Convert.ToDouble(entTransactionAmount.Text.Trim())- Convert.ToDouble(entTaxAmount.Text.Trim()), Convert.ToDouble(entTaxAmount.Text.Trim()), lbTransactionId.Text, "Transaction sale rechieved payment", this.parent.user.id, this.parent.application_id );
                }

                
                SetTransactionModel("",entSearch.Text.Trim());  
                SelectedTrans(lbTransactionId.Text);
                SetItemModel(Convert.ToDouble(lbTransactionId.Text));                 

                TransactionReady();
                resp = new Response{ code = "20",description = "Success"} ;
            }  
            return resp;
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
            _cellColumnsRender.Add(rendererText, (int)ColumnTrans.customer_id);
            _treeViewTrans.InsertColumn(-1, "Customer ID", rendererText, "text", (int)ColumnTrans.customer_id);            

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
            
            CellRendererText rendererText = new CellRendererText(); //0
            _cellColumnsRenderItems.Add(rendererText, (int)ColumnItems.id);
            _treeViewItems.InsertColumn(-1, "ID", rendererText, "text", (int)ColumnItems.id);

            rendererText = new CellRendererText(); //1
            _cellColumnsRenderItems.Add(rendererText, (int)ColumnItems.transaction_id);
            _treeViewItems.InsertColumn(-1, "Transaction ID", rendererText, "text", (int)ColumnItems.transaction_id);            

            rendererText = new CellRendererText(); //2
            _cellColumnsRenderItems.Add(rendererText, (int)ColumnItems.product_id);
            _treeViewItems.InsertColumn(-1, "Product ID", rendererText, "text", (int)ColumnItems.product_id); 

            rendererText = new CellRendererText(); //3
            _cellColumnsRenderItems.Add(rendererText, (int)ColumnItems.product_short_name);
            _treeViewItems.InsertColumn(-1, "Short name", rendererText, "text", (int)ColumnItems.product_short_name);

            rendererText = new CellRendererText(); //4
            _cellColumnsRenderItems.Add(rendererText, (int)ColumnItems.stock_id);
            _treeViewItems.InsertColumn(-1, "Stock ID", rendererText, "text", (int)ColumnItems.stock_id);

            rendererText = new CellRendererText(); //5
            _cellColumnsRenderItems.Add(rendererText, (int)ColumnItems.quantity);
            _treeViewItems.InsertColumn(-1, "Quantity", rendererText, "text", (int)ColumnItems.quantity);

            rendererText = new CellRendererText();      //6
            _cellColumnsRenderItems.Add(rendererText, (int)ColumnItems.unit_name);
            _treeViewItems.InsertColumn(-1, "Unit", rendererText, "text", (int)ColumnItems.unit_name);

            rendererText = new CellRendererText(); //7
            _cellColumnsRenderItems.Add(rendererText, (int)ColumnItems.purchase_price_id);
            _treeViewItems.InsertColumn(-1, "Purchase Price ID", rendererText, "text", (int)ColumnItems.purchase_price_id);

            rendererText = new CellRendererText(); //8
            _cellColumnsRenderItems.Add(rendererText, (int)ColumnItems.purchase_final_price);
            _treeViewItems.InsertColumn(-1, "Purchase final price", rendererText, "text", (int)ColumnItems.purchase_final_price);
            
            rendererText = new CellRendererText();   //9
            _cellColumnsRenderItems.Add(rendererText, (int)ColumnItems.item_price);
            _treeViewItems.InsertColumn(-1, "Item Price", rendererText, "text", (int)ColumnItems.item_price);

            ListStore lstModelCombo = new ListStore(typeof(string), typeof(string));
            String sql = "Select id,discount from discount_group order by discount asc";
            DataTable dt = DbCl.fillDataTable(DbCl.getConn(), sql);
            foreach (DataRow dr in dt.Rows)
            {
                lstModelCombo.AppendValues(dr[0].ToString(), dr[1].ToString());
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
            rendererCombo.EditingStarted += EditingStarted;       //10  
            _cellColumnsRenderItems.Add(rendererCombo, (int)ColumnItems.main_discount);
            _treeViewItems.InsertColumn(-1, "Main discount", rendererCombo, "text", (int)ColumnItems.main_discount);

     
            lstModelCombo = new ListStore(typeof(string), typeof(string));
            sql = "Select id,discount from discount_group order by discount asc";
            dt = DbCl.fillDataTable(DbCl.getConn(), sql);
            foreach (DataRow dr in dt.Rows)
            {
                lstModelCombo.AppendValues(dr[0].ToString(), dr[1].ToString());
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
            rendererCombo.EditingStarted += EditingStarted;       //11   
            _cellColumnsRenderItems.Add(rendererCombo, (int)ColumnItems.additional_discount);
            _treeViewItems.InsertColumn(-1, "Additional discount", rendererCombo, "text", (int)ColumnItems.additional_discount);

        
            rendererText = new CellRendererText //12
            {
                Editable = isEditable
            };
            rendererText.Foreground = textForeground;
            rendererText.Edited += CellEditedItem;  
            _cellColumnsRenderItems.Add(rendererText, (int)ColumnItems.deduction_amount);
            _treeViewItems.InsertColumn(-1, "Deduction price", rendererText, "text", (int)ColumnItems.deduction_amount);

            rendererText = new CellRendererText();   //13
            _cellColumnsRenderItems.Add(rendererText, (int)ColumnItems.final_price);
            _treeViewItems.InsertColumn(-1, "Final Price", rendererText, "text", (int)ColumnItems.final_price);

            rendererText = new CellRendererText();   //14
            _cellColumnsRenderItems.Add(rendererText, (int)ColumnItems.subtotal);
            _treeViewItems.InsertColumn(-1, "Subtotal", rendererText, "text", (int)ColumnItems.subtotal);

            rendererText = new CellRendererText(); //15
            _cellColumnsRenderItems.Add(rendererText, (int)ColumnItems.tax);
            _treeViewItems.InsertColumn(-1, "Tax", rendererText, "text", (int)ColumnItems.tax);
            
            rendererText = new CellRendererText();       //16     
            _cellColumnsRenderItems.Add(rendererText, (int)ColumnItems.location_name);
            _treeViewItems.InsertColumn(-1, "Location", rendererText, "text", (int)ColumnItems.location_name);

            rendererText = new CellRendererText();          //17 
            _cellColumnsRenderItems.Add(rendererText, (int)ColumnItems.condition_name);
            _treeViewItems.InsertColumn(-1, "Condition", rendererText, "text", (int)ColumnItems.condition_name);

            rendererText = new CellRendererText(); //18
            _cellColumnsRenderItems.Add(rendererText, (int)ColumnItems.state_name);
            _treeViewItems.InsertColumn(-1, "State", rendererText, "text", (int)ColumnItems.state_name);
           
            rendererText = new CellRendererText(); //19
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
                case (int)ColumnItems.main_discount:
                {
                    int i = path.Indices[0];
                    _clsItems[i].main_discount = args.NewText;
                    _lsModelItems.SetValue(iter, column, _clsItems[i].main_discount);

                    //calculate final price
                    double final_price = CoreCl.CalculateFinalPrice(_clsItems[i].item_price, _clsItems[i].main_discount, _clsItems[i].additional_discount, _clsItems[i].deduction_amount);
                    _clsItems[i].final_price = final_price.ToString();
                    _lsModelItems.SetValue(iter, (int)ColumnItems.final_price, _clsItems[i].final_price);

                    _clsItems[i].subtotal = (Convert.ToDouble(_clsItems[i].quantity)* Convert.ToDouble(_clsItems[i].final_price)).ToString();
                    _lsModelItems.SetValue(iter, (int)ColumnItems.subtotal, _clsItems[i].subtotal);

                    string sql = "update transaction_order_item set main_discount = '"+args.NewText+"' where id='"+_clsItems[i].id+"' ";
                    Console.WriteLine (sql);
                    DbCl.ExecuteTrans(DbCl.getConn(), sql);                    
                    chkTax.Active = false;
                    SetTotalCalculation();
                }
                break;
                case (int)ColumnItems.additional_discount:
                {
                    int i = path.Indices[0];
                    _clsItems[i].additional_discount = args.NewText;
                    _lsModelItems.SetValue(iter, column, _clsItems[i].additional_discount);

                    //calculate final price
                    double final_price = CoreCl.CalculateFinalPrice(_clsItems[i].item_price, _clsItems[i].main_discount, _clsItems[i].additional_discount, _clsItems[i].deduction_amount);
                    _clsItems[i].final_price = final_price.ToString();
                    _lsModelItems.SetValue(iter, (int)ColumnItems.final_price, _clsItems[i].final_price);

                    _clsItems[i].subtotal = (Convert.ToDouble(_clsItems[i].quantity)* Convert.ToDouble(_clsItems[i].final_price)).ToString();
                    _lsModelItems.SetValue(iter, (int)ColumnItems.subtotal, _clsItems[i].subtotal);

                    string sql = "update transaction_order_item set additional_discount = '"+args.NewText+"'  where id='"+_clsItems[i].id+"' ";
                    Console.WriteLine (sql);
                    DbCl.ExecuteTrans(DbCl.getConn(), sql);                    
                    chkTax.Active = false;
                    SetTotalCalculation();
                }
                break;
                case (int)ColumnItems.deduction_amount:
                {
                    int i = path.Indices[0];
                    _clsItems[i].deduction_amount = args.NewText;
                    _lsModelItems.SetValue(iter, column, _clsItems[i].deduction_amount);

                    //calculate final price
                    double final_price = CoreCl.CalculateFinalPrice(_clsItems[i].item_price, _clsItems[i].main_discount, _clsItems[i].additional_discount, _clsItems[i].deduction_amount);
                    _clsItems[i].final_price = final_price.ToString();
                    _lsModelItems.SetValue(iter, (int)ColumnItems.final_price, _clsItems[i].final_price);

                    _clsItems[i].subtotal = (Convert.ToDouble(_clsItems[i].quantity) *Convert.ToDouble(_clsItems[i].final_price)).ToString();
                    _lsModelItems.SetValue(iter, (int)ColumnItems.subtotal, _clsItems[i].subtotal);

                    string sql = "update transaction_order_item set deduction_amount = '"+args.NewText+"' where id='"+_clsItems[i].id+"' ";
                    Console.WriteLine (sql);
                    DbCl.ExecuteTrans(DbCl.getConn(), sql);                
                    chkTax.Active = false;
                    SetTotalCalculation();
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

        public void doChildProduct(object o,clsProduct prm){
            GLib.Timeout.Add(5, () =>
            {
                GuiCl.RemoveAllWidgets(popoverProduct);
                Label popLabel = new Label(prm.short_name);
                popoverProduct.Add(popLabel);
                popoverProduct.SetSizeRequest(200, 20);
                popLabel.Show();
                SelectedItem(prm.id.ToString());
                spnQty.GrabFocus();
                return false;
            });
        }
        
        private void ShowProductPopup(object sender, EventArgs e)
        {   GLib.Timeout.Add(5, () =>
            {     
                GuiCl.RemoveAllWidgets(popoverProduct);        
                ReferenceProduct refWidget = new ReferenceProduct(this,"dialog",2,new clStock{is_active="true"});
                popoverProduct.Add(refWidget);
                popoverProduct.SetSizeRequest(600, 300);
                refWidget.Show();          
                popoverProduct.ShowAll();
                return false;
            });
        }

        public void doChildCustomer(object o,string prm){
            GLib.Timeout.Add(0, () =>
            {
                GuiCl.RemoveAllWidgets(popoverCustomer);
                Label popLabel = new Label((string)o);
                popoverCustomer.Add(popLabel);
                popoverCustomer.SetSizeRequest(200, 20);
                popLabel.Show();
                string sql = "update transaction set customer_id="+prm+" where id= "+lbTransactionId.Text;
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
        private void ShowCustomerPopup(object sender, EventArgs e)
        {
            GLib.Timeout.Add(0, () =>
            {                   
                GuiCl.RemoveAllWidgets(popoverCustomer);        
                ReferenceCustomer refWidget = new ReferenceCustomer(this,"dialog","Sale");
                popoverCustomer.Add(refWidget);
                popoverCustomer.SetSizeRequest(200, 400);
                refWidget.Show();          
                popoverCustomer.ShowAll();
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
            if (e.Event.Key == Gdk.Key.Return)
            {            
                Response resp = DoCheckout();
                if(resp.code=="20"){
                    SetItemModel(Convert.ToDouble(lbTransactionId.Text));
                    ItemTransactionReady(true);
                }else{
                    string message = resp.description;
                    MessageDialog md = new MessageDialog(null, DialogFlags.DestroyWithParent, MessageType.Error, ButtonsType.Close, message);
                    md.Run();
                    md.Destroy();
                }                   
            }       
        }
        private void BtnProcessCheckoutClicked(object sender, EventArgs e){
            Response resp = DoCheckout();
            if(resp.code=="20"){
                SetItemModel(Convert.ToDouble(lbTransactionId.Text));
                ItemTransactionReady(true);
            }else{
                string message = resp.description;
                MessageDialog md = new MessageDialog(null, DialogFlags.DestroyWithParent, MessageType.Error, ButtonsType.Close, message);
                md.Run();
                md.Destroy();
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