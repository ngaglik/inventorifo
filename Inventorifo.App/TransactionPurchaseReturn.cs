using System;
using System.Collections.Generic;
using Gtk;
using Gdk;
using System.Data;
using Pango;
using UI = Gtk.Builder.ObjectAttribute;
using System.Text.RegularExpressions;
using Inventorifo.Lib.Model;

namespace Inventorifo.App
{
    //[Section(ContentType = typeof(EditableCellsSection), Category = Category.Widgets)]
    class TransactionPurchaseReturn : Gtk.Box
    {
        int transaction_type_id = 3;
        Inventorifo.Lib.LibDb DbCl = new Inventorifo.Lib.LibDb ();
        Inventorifo.Lib.LibGui GuiCl = new Inventorifo.Lib.LibGui ();
        Inventorifo.Lib.LibCore CoreCl = new Inventorifo.Lib.LibCore ();

        public MainWindow parent;
        public string prm;
        public TransactionPurchaseReturn(object parent, string prm) : this(new Builder("TransactionSaleReturn.glade")) { 
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
        Button btnProcessCheckout;
        Button btnProduct;
        Button btnDate;

        private Entry entSearch;
        private Entry entBarcode;
        private Entry entAmountPayment;
        private Entry entTransactionAmount;
        private Entry entTaxAmount;

        private Popover popoverReference ;
        private Popover popoverProduct ;
        private Popover popoverDate;
        private Popover popoverPayment;
        private TextView textViewCustomer;
        private TextView textViewProduct;

        DataTable dtTransSelected;
        DataTable dtItemSelected;
        DataTable dtAddItemSelected;

        Label lbTransactionId;
        Label lbTotalItem;
        Label lbReferenceId;

        string textForeground;
        string textBackground;
        Boolean isEditable;

        ComboBoxText cmbDestinationLocation;
        ComboBoxText cmbDestinationCondition;
        
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
            purchase_price_id,
            purchase_item_price,
            purchase_main_discount,
            purchase_additional_discount,
            purchase_deduction_amount,
            purchase_final_price,
            purchase_tax,
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

        public TransactionPurchaseReturn(Builder builder) : base(builder.GetRawOwnedObject("TransactionSaleReturn"))
        {
            builder.Autoconnect(this);

            Label lbTitle = (Label)builder.GetObject("LbTitle");
            lbTitle.Text = "Purchase Return";
            lbTitle.ModifyFont(FontDescription.FromString("Arial 18"));
            

            Box  boxMiddle = (Box)builder.GetObject("BoxMiddle");
            boxMiddle.SetSizeRequest(-1, -1); // Allow dynamic resizing
            boxMiddle.Expand = true;
            boxItem = (Box)builder.GetObject("BoxItem");
            //boxItem.ModifyBg(StateType.Normal, new Gdk.Color(237, 237, 222));
            boxTransaction = (Box)builder.GetObject("BoxTransaction");
            //boxTransaction.ModifyBg(StateType.Normal, new Gdk.Color(224, 235, 235));
            boxProductDetail = (Box)builder.GetObject("BoxProductDetail");
            boxTotalCalculation = (Box)builder.GetObject("BoxTotalCalculation");
            spnQty = (SpinButton)builder.GetObject("SpnQty");
            spnQty.Xalign = 0.5f; 
            spnQty.KeyPressEvent += OnSpnQtyKeyPressEvent;
            spnQty.ModifyFont(FontDescription.FromString("Arial 14"));
            
            
            btnProcessCheckout = (Button)builder.GetObject("BtnProcessCheckout");
            btnProcessCheckout.Clicked += BtnProcessCheckoutClicked;

            btnProduct = (Button)builder.GetObject("BtnProduct");
            popoverReference = new Popover(btnProduct);     
            btnProduct.Clicked += ShowReferencePopup;

            btnDate = (Button)builder.GetObject("BtnDate");
            btnDate.Label = calendar.Date.ToString("yyyy-MM-dd");
            popoverDate = new Popover(btnDate);    
            btnDate.Clicked += ShowDatePopup;
                        
            btnNew = (Button)builder.GetObject("BtnNew");
            btnNew.Clicked += NewTransaction;
            
            entSearch = (Entry)builder.GetObject("EntSearch");
            entSearch.Changed += HandleEntSearchChanged;
          
            _treeViewTrans = (TreeView)builder.GetObject("TreeViewTrans");
            _treeViewTrans.Selection.Mode = SelectionMode.Single;
            textForeground = "green";
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
            _treeViewItems.Columns[10].Visible = false; //tax
            
            textViewProduct = (TextView)builder.GetObject("TextViewProduct");          

            lbTransactionId = (Label)builder.GetObject("LbTransactionId");
            lbTransactionId.ModifyFont(FontDescription.FromString("Arial 14"));
            lbReferenceId = (Label)builder.GetObject("LbReferenceId");
            lbReferenceId.ModifyFont(FontDescription.FromString("Arial 14"));
            lbTotalItem = (Label)builder.GetObject("LbTotalItem");
            lbTotalItem.ModifyFont(FontDescription.FromString("Arial 14"));
            this.KeyPressEvent += OnThisKeyPressEvent;
            

            cmbDestinationLocation = (ComboBoxText)builder.GetObject("CmbDestinationLocation");
            GuiCl.FillComboBoxText(cmbDestinationLocation, "Select id,name from location order by id asc",0);
            cmbDestinationCondition = (ComboBoxText)builder.GetObject("CmbDestinationCondition");
            GuiCl.FillComboBoxText(cmbDestinationCondition, "Select id,name from condition order by id asc",0);

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
                if(boxItem.Sensitive==true) ShowReferencePopup(new object(),new EventArgs());
            } else if (e.Event.Key == Gdk.Key.F3)
            {                 
                if(boxItem.Sensitive==true) ItemTransactionReady(true);
            }       
        }

        private void TransactionReady(){
            GuiCl.SensitiveAllWidgets(boxItem,false);      
            btnNew.Sensitive = true;      
            textViewProduct.Buffer.Text = "";
            spnQty.Text = "1";
            boxTransaction.Sensitive = true;
            btnNew.Sensitive = true; 
        }

        public void SelectFirstRow(ListStore ts, TreeView tv){
            TreeIter iter;
            if (ts.GetIterFirst(out iter)) // Get the first row
            {
                tv.Selection.SelectIter(iter);
            }
        }
        private void ItemTransactionReady(Boolean showpopup){
            if(showpopup) ShowReferencePopup(new object(),new EventArgs());                       
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
            DataTable dtTransSelected = CoreCl.fillDtTransactionPurchaseReturn(transaction_id, btnDate.Label, "");
            foreach (DataRow dr in dtTransSelected.Rows)
            { 
                var id = transaction_id;
                var reference_id = dr["reference_id"].ToString();
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

                lbTransactionId.Text = id;
                lbReferenceId.Text = reference_id;
                CellRendererText retrievedRenderer = GuiCl.GetCellRendererText(_treeViewItems, 2);
                
                if(Convert.ToInt32(state)==0){ 
                    if(Convert.ToInt32(payment_group_id)>5 ){   
                        if(Convert.ToDouble(transaction_amount)>Convert.ToDouble(payment_amount)){
                            GuiCl.SensitiveAllWidgets(boxItem,true);                 
                            GuiCl.SetDisableAllColumn(_treeViewItems);
                            btnProduct.Sensitive = false;
                            entTransactionAmount.Sensitive = false;
                            spnQty.Sensitive=false;
                            chkTax.Sensitive = false;
                            entTaxAmount.Sensitive = false;
                            btnProcessCheckout.Sensitive = true;

                        } else{
                           // GuiCl.SensitiveAllWidgets(boxItem,false); 
                            GuiCl.SetDisableAllColumn(_treeViewItems);
                            btnProduct.Sensitive = false;
                            entTransactionAmount.Sensitive = false;
                            spnQty.Sensitive=false;
                            chkTax.Sensitive = false;
                            entTaxAmount.Sensitive = false;
                            btnProcessCheckout.Sensitive = false;
                        }                        
                    }else{
                            GuiCl.SetDisableAllColumn(_treeViewItems);
                            btnProduct.Sensitive = false;
                            spnQty.Sensitive=false;
                            btnProcessCheckout.Sensitive = false;
                    }
                }else{
                 
                    GuiCl.SensitiveAllWidgets(boxItem,true);
                    GuiCl.SetEnableColumn(_treeViewItems,[5]);
                    btnProduct.Sensitive = true;
                    boxProductDetail.Sensitive = true;
                    spnQty.Sensitive=true;
                    btnProcessCheckout.Sensitive = true;
                }
                ItemTransactionReady(false);
                SetItemModel(Convert.ToDouble(lbTransactionId.Text));  
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
                total += (Convert.ToDouble(_clsItems[i].quantity)*Convert.ToDouble(_clsItems[i].purchase_final_price)) ;
            } 
            return total;
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
            "values (3,CURRENT_DATE,CURRENT_DATE,1,"+this.parent.user.id+",'"+this.parent.application_id+"') ";
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
                clTransaction tran;
                dtTransSelected =  CoreCl.fillDtTransactionPurchaseReturn(transaction_id, btnDate.Label, strfind);
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
            _clsItems = new List<clTransactionItem1>();           
            clTransactionItem1 item; 
            DataTable dt = CoreCl.fillDtTransactionItem(transaction_id.ToString(),"");
            foreach (DataRow dr in dt.Rows)
            {   
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
                    purchase_item_price=dr["purchase_item_price"].ToString(),
                   // price=dr["price"].ToString(),
                    tax=dr["tax"].ToString(),
                    state=dr["state"].ToString(), 
                    state_name=dr["state_name"].ToString(), 
                    location=dr["location"].ToString(), 
                    location_name=dr["location_name"].ToString(), 
                    condition=dr["condition"].ToString(),
                    condition_name=dr["condition_name"].ToString(), 
                };                    
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
                _lsModelItems.SetValue(iter, (int)ColumnItems.purchase_item_price, _clsItems[i].purchase_item_price);
                _lsModelItems.SetValue(iter, (int)ColumnItems.purchase_price_id, _clsItems[i].purchase_price_id);
                _lsModelItems.SetValue(iter, (int)ColumnItems.purchase_final_price, _clsItems[i].purchase_final_price);
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
        }

        
        [GLib.ConnectBefore]
        private void HandleTreeViewItemsKeyPressEvent(object sender, KeyPressEventArgs e)
        {
            if (e.Event.Key == Gdk.Key.Delete || e.Event.Key == Gdk.Key.KP_Delete)  // Check if Enter key is pressed
            {
                // TreeSelection selection = _treeViewItems.Selection;
                // TreeIter iter;
                // if(selection.GetSelected( out iter)){
                //     Console.WriteLine("Selected Value:"+_lsModelItems.GetValue (iter, 0).ToString()+_lsModelItems.GetValue (iter, 1).ToString());
                // }            
                // Console.WriteLine("state: "+_lsModelItems.GetValue (iter, 13).ToString());
                // if(_lsModelItems.GetValue (iter, 13).ToString()!="0"){
                //     string sql = "delete from stock where id="+_lsModelItems.GetValue (iter, 5).ToString();
                //     Console.WriteLine(sql);
                //     DbCl.ExecuteScalar(DbCl.getConn(), sql);
                //     sql = "delete from transaction_item where id="+_lsModelItems.GetValue (iter, 0).ToString();
                //     Console.WriteLine(sql);
                //     DbCl.ExecuteScalar(DbCl.getConn(), sql);
                //     SetItemModel(Convert.ToDouble(lbTransactionId.Text));
                //     if(GetTotalItem()==0) {
                //         sql = "update transaction set state=1 where id="+lbTransactionId.Text;
                //         Console.WriteLine (sql);
                //         DbCl.ExecuteTrans(DbCl.getConn(), sql);                     
                //     }
                //     ItemTransactionReady(true);
                // }
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
       
        public Response AddItem(){
            Console.WriteLine("==============1===="+ dtItemSelected.Rows[0].ItemArray[0].ToString()); 
            string sql="";
            Response resp = new Response();
            Boolean valid=false; 
            double store_quantity = 0;
            double global_quantity = 0;
            //check available product
            DataTable dts = CoreCl.fillDtProduct(1,new clStock{product_id=dtItemSelected.Rows[0].ItemArray[0].ToString()});
            foreach(DataRow drs in dts.Rows){
                Console.WriteLine(drs["short_name"].ToString()+"======2======== "+drs["store_quantity"].ToString()+"===="); 
                store_quantity =  Convert.ToDouble(drs["store_quantity"].ToString()); 
                global_quantity = Convert.ToDouble(drs["global_quantity"].ToString());
                if(Convert.ToDouble(spnQty.Text) > store_quantity  && Convert.ToDouble(spnQty.Text) < global_quantity ){
                    resp = new Response{ code = "21",description = "Failed "+drs["short_name"].ToString()+" out of stock, but available on other location"} ;
                }else if(Convert.ToDouble(spnQty.Text) > store_quantity ){
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
                "from transaction_item ti, stock,location loc,location_group locgr "+
                "where ti.transaction_id='"+lbReferenceId.Text+"' and ti.stock_id=stock.id and ti.product_id="+ dtItemSelected.Rows[0].ItemArray[0].ToString() + " "+
                "and stock.location=loc.id and loc.location_group=locgr.id "+
                "order by stock.id asc";
                Console.WriteLine(sql);   
                double balance = 0;
                dts = DbCl.fillDataTable(DbCl.getConn(), sql);
                balance = Convert.ToDouble(spnQty.Text);  
                foreach(DataRow drs in dts.Rows){                    
                    if(Convert.ToDouble(balance) > Convert.ToDouble(drs["quantity"].ToString()) ){
                        
                        sql = "insert into transaction_item (transaction_id,product_id,quantity,stock_id,price_id,state, location, condition) "+
                        "values("+lbTransactionId.Text+ ","+dtItemSelected.Rows[0].ItemArray[0].ToString() + ","+drs["quantity"].ToString()+","+ drs["stock_id"].ToString() + ","+drs["price_id"].ToString()+",1,"+ drs["location"].ToString() + ","+drs["condition"].ToString()+")" ;
                        Console.WriteLine (sql); 
                        DbCl.ExecuteTrans(DbCl.getConn(), sql);  
                        CoreCl.InsertStockHistory(lbTransactionId.Text,transaction_type_id.ToString(), dtItemSelected.Rows[0].ItemArray[0].ToString(), drs["stock_id"].ToString(),drs["location"].ToString(),drs["condition"].ToString(),"-"+drs["quantity"].ToString());
                        sql = "update stock set quantity=0 where id="+drs["stock_id"].ToString() ;
                        Console.WriteLine (sql);
                        DbCl.ExecuteTrans(DbCl.getConn(), sql);
                        balance = balance-Convert.ToDouble(drs["quantity"].ToString());
                    }else{
                        
                        sql = "insert into transaction_item (transaction_id,product_id,quantity,stock_id,price_id,state, location, condition) "+
                        "values("+lbTransactionId.Text+ ","+dtItemSelected.Rows[0].ItemArray[0].ToString() + ","+balance.ToString()+","+ drs["stock_id"].ToString() + ","+drs["price_id"].ToString()+",1,"+ drs["location"].ToString() + ","+drs["condition"].ToString()+")" ;
                        Console.WriteLine (sql); 
                        DbCl.ExecuteTrans(DbCl.getConn(), sql); 
                        CoreCl.InsertStockHistory(lbTransactionId.Text,transaction_type_id.ToString(), dtItemSelected.Rows[0].ItemArray[0].ToString(), drs["stock_id"].ToString(),drs["location"].ToString(),drs["condition"].ToString(),"-"+balance.ToString());
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
                    Response resp = AddItem();
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
            
            rendererText = new CellRendererText(); //1
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

            rendererText = new CellRendererText();
            _cellColumnsRenderItems.Add(rendererText, (int)ColumnItems.quantity);
            _treeViewItems.InsertColumn(-1, "Quantity", rendererText, "text", (int)ColumnItems.quantity);

            rendererText = new CellRendererText();      
            _cellColumnsRenderItems.Add(rendererText, (int)ColumnItems.unit_name);
            _treeViewItems.InsertColumn(-1, "Unit", rendererText, "text", (int)ColumnItems.unit_name);

            rendererText = new CellRendererText();
            _cellColumnsRenderItems.Add(rendererText, (int)ColumnItems.purchase_item_price);
            _treeViewItems.InsertColumn(-1, "Purchase item price", rendererText, "text", (int)ColumnItems.purchase_item_price);

            rendererText = new CellRendererText();
            _cellColumnsRenderItems.Add(rendererText, (int)ColumnItems.purchase_price_id);
            _treeViewItems.InsertColumn(-1, "Price ID", rendererText, "text", (int)ColumnItems.purchase_price_id);

            rendererText = new CellRendererText();   
            _cellColumnsRenderItems.Add(rendererText, (int)ColumnItems.purchase_final_price);
            _treeViewItems.InsertColumn(-1, "Final Price", rendererText, "text", (int)ColumnItems.purchase_final_price);

            rendererText = new CellRendererText();
            _cellColumnsRenderItems.Add(rendererText, (int)ColumnItems.tax);
            _treeViewItems.InsertColumn(-1, "Tax", rendererText, "text", (int)ColumnItems.tax);
            
            rendererText = new CellRendererText();            
            _cellColumnsRenderItems.Add(rendererText, (int)ColumnItems.location_name);
            _treeViewItems.InsertColumn(-1, "Location", rendererText, "text", (int)ColumnItems.location_name);

            rendererText = new CellRendererText();           
            _cellColumnsRenderItems.Add(rendererText, (int)ColumnItems.condition_name);
            _treeViewItems.InsertColumn(-1, "Condition", rendererText, "text", (int)ColumnItems.condition_name);

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
                    // int i = path.Indices[0];
                    // _clsItems[i].quantity = args.NewText;
                    // _lsModelItems.SetValue(iter, column, _clsItems[i].quantity);
                    // string sql = "update stock set quantity = '"+args.NewText+"' where id='"+_clsItems[i].stock_id+"' ";
                    // Console.WriteLine (sql);
                    // DbCl.ExecuteTrans(DbCl.getConn(), sql);
                    // sql = "update transaction_item set quantity = '"+args.NewText+"' where id='"+_clsItems[i].id+"' ";
                    // Console.WriteLine (sql);
                    // DbCl.ExecuteTrans(DbCl.getConn(), sql);
                    // entTaxAmount.Text = CalculateTax(GetTotalSalePrice()).ToString();
                    // entTransactionAmount.Text = (GetTotalSalePrice()+CalculateTax(GetTotalSalePrice())).ToString();
                }
                break;
                case (int)ColumnItems.purchase_final_price:
                {
                    // int i = path.Indices[0];
                    // _clsItems[i].price = args.NewText;
                    // _lsModelItems.SetValue(iter, column, _clsItems[i].price);
                    // string sql = "update transaction_item set price = '"+args.NewText+"' where price_id='"+_clsItems[i].price_id+"' ";
                    // Console.WriteLine (sql);
                    // DbCl.ExecuteTrans(DbCl.getConn(), sql);
                    // sql = "update price set price = '"+args.NewText+"' where id='"+_clsItems[i].price_id+"' ";
                    // Console.WriteLine (sql);
                    // DbCl.ExecuteTrans(DbCl.getConn(), sql);
                    // entTaxAmount.Text = CalculateTax(GetTotalSalePrice()).ToString();
                    // entTransactionAmount.Text = (GetTotalSalePrice()+CalculateTax(GetTotalSalePrice())).ToString();
                }
                break;
            }
        }

        private void RemoveItem(object sender, EventArgs e)
        {
            
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
        private void ShowReferencePopup(object sender, EventArgs e)
        {
            GLib.Timeout.Add(0, () =>
            {                   
                GuiCl.RemoveAllWidgets(popoverReference);   
                clTransaction filterTrans = new clTransaction{
                    transaction_type_id = "1",
                    transaction_date = btnDate.Label,
                };           
                ReportTransactionPurchase refWidget = new ReportTransactionPurchase(this,"dialog",filterTrans);
                popoverReference.Add(refWidget);
                popoverReference.SetSizeRequest(900, 500);
                refWidget.Show();          
                popoverReference.ShowAll();
                return false;
            });
        }
        public void doChildProduct(object o,clReportTransaction prm){
            GLib.Timeout.Add(5, () =>
            {
                GuiCl.RemoveAllWidgets(popoverReference);
                Label popLabel = new Label(prm.product_short_name);
                popoverReference.Add(popLabel);
                popoverReference.SetSizeRequest(200, 20);
                popLabel.Show();
                lbReferenceId.Text = prm.transaction_id;
                string sql = "update transaction set reference_id='"+lbReferenceId.Text.Trim()+"', state=2 where id="+lbTransactionId.Text;
                Console.WriteLine(sql);
                DbCl.ExecuteTrans(DbCl.getConn(), sql);
                SelectedItem(prm.product_id.ToString());
                spnQty.GrabFocus();
                return false;
            });
        }       
        private void BtnProcessCheckoutClicked(object sender, EventArgs e){
            Response resp = DoCheckout();
            if(resp.code=="20"){
                SetItemModel(Convert.ToDouble(lbTransactionId.Text));
                ItemTransactionReady(false);
            }else{
                string message = resp.description;
                MessageDialog md = new MessageDialog(null, DialogFlags.DestroyWithParent, MessageType.Error, ButtonsType.Close, message);
                md.Run();
                md.Destroy();
            }
        }

      
        private Response DoCheckout()
        {
            Response resp = new Response();
            string sql = "update transaction_item set state=0 where transaction_id="+lbTransactionId.Text;
            Console.WriteLine(sql);
            DbCl.ExecuteTrans(DbCl.getConn(), sql);
            sql = "update transaction set return_amount=return_amount+" + GetTotalPurchasePrice().ToString()+", state=0 where id="+lbTransactionId.Text;
            Console.WriteLine(sql);
            DbCl.ExecuteTrans(DbCl.getConn(), sql);
            SetTransactionModel("",entSearch.Text.Trim());  
            SelectedTrans(lbTransactionId.Text);
            SetItemModel(Convert.ToDouble(lbTransactionId.Text));
            //ItemTransactionReady(false);
            TransactionReady();
            resp = new Response{ code = "20",description = "Success"} ;
            return resp;
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