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
    class WarehouseTransfer : Gtk.Box
    {
        Inventorifo.Lib.LibDb DbCl = new Inventorifo.Lib.LibDb ();
        Inventorifo.Lib.LibGui GuiCl = new Inventorifo.Lib.LibGui ();
        Inventorifo.Lib.LibCore CoreCl = new Inventorifo.Lib.LibCore ();

        public MainWindow parent;
        clTransfer filterTrans;
        public WarehouseTransfer(object parent, clTransfer filterTrans) : this(new Builder("WarehouseTransfer.glade")) { 
            this.parent=(MainWindow)parent;
            this.filterTrans = filterTrans;
            Console.WriteLine(this.parent.user.id + " "+ this.parent.user.person_name);
            
        }
        private TreeView _treeViewTrans;
        private ListStore _lsModelTrans;
        private Dictionary<CellRenderer, int> _cellColumnsRender;
        private List<clTransfer> _clsTrans;

        private TreeView _treeViewItems;
        private ListStore _lsModelItems;
        private Dictionary<CellRenderer, int> _cellColumnsRenderItems;
        private List<clTransactionItem1> _clsItems;
        
        Box  boxMiddle;
        Box boxItem;
        Box boxTransaction;
        Box boxSource;
        Box boxProductDetail;
        Box boxDestination;
        //Box boxPayment;

        SpinButton spnQty;
        Button  btnNew;
        //Button  btnPreviousPayment;
        Button btnSave;
        Button btnProduct;
        Button btnSourceOrganization;
        Button btnDestinationOrganization;
        Button btnDate;

        private Entry entSearch;
        private Entry entBarcode;
        //private Entry entAmountPayment;
        //private Entry entTransactionAmount;
        //private Entry entTaxAmount;

        private Popover popoverProduct ;
        private Popover popoverSourceOrganization ;
        private Popover popoverDestinationOrganization ;
        private Popover popoverDate;
        //private Popover popoverPayment;
        private TextView textViewSourceOrganization;
        private TextView textViewDestinationOrganization;
        private TextView textViewProduct;

        DataTable dtTransSelected;
        DataTable dtItemSelected;

        Label lbTransactionId;
        Label lbTotalItem;

        string sourceOrganizationId;
        string destinationOrganizationId;
        string textForeground;
        string textBackground;
        Boolean isEditable;

        ComboBoxText cmbTransferType;
        ComboBoxText cmbSourceLocation;
        ComboBoxText cmbSourceCondition;
        ComboBoxText cmbDestinationLocation;
        ComboBoxText cmbDestinationCondition;

        Dictionary<int, string> dictTransferType = new Dictionary<int, string>();
        Gtk.ListStore lsTransferType = new ListStore(typeof(string), typeof(string));
        CellRendererCombo cellComboTransferType = new Gtk.CellRendererCombo();
        
        Calendar calendar = new Calendar();
        //CheckButton chkTax;

        private enum ColumnTrans
        { 
            id,
            reference_id,
            source_organization_id,
            source_organization_name,
            source_location_id,
            source_location_name,
            source_condition_id,
            source_condition_name,
            destination_organization_id,
            destination_organization_name,
            destination_location_id,
            destination_location_name,
            destination_condition_id,
            destination_condition_name,
            user_id,
            user_name,
            state,
            state_name,
            state_fgcolor,
            state_bgcolor,
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
            purchase_subtotal,
            purchase_tax,
            // item_price,
            // main_discount,
            // additional_discount,
            // deduction_amount,
            // final_price,
            // subtotal,
            // tax,
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


        public WarehouseTransfer(Builder builder) : base(builder.GetRawOwnedObject("WarehouseTransfer"))
        {
            builder.Autoconnect(this);

            Label lbTitle = (Label)builder.GetObject("LbTitle");
            lbTitle.Text = "Transfer Stock";
            lbTitle.ModifyFont(FontDescription.FromString("Arial 18"));
            

            Box  boxMiddle = (Box)builder.GetObject("BoxMiddle");
            boxMiddle.SetSizeRequest(-1, -1); // Allow dynamic resizing
            boxMiddle.Expand = true;
            boxItem = (Box)builder.GetObject("BoxItem");
            //boxItem.ModifyBg(StateType.Normal, new Gdk.Color(237, 237, 222));
            boxTransaction = (Box)builder.GetObject("BoxTransaction");
            //boxTransaction.ModifyBg(StateType.Normal, new Gdk.Color(224, 235, 235));
            //boxSource = (Box)builder.GetObject("boxSource");
            boxProductDetail = (Box)builder.GetObject("BoxProductDetail");
            boxDestination = (Box)builder.GetObject("boxDestination");
            //boxPayment = (Box)builder.GetObject("BoxPayment");

            spnQty = (SpinButton)builder.GetObject("SpnQty");
            spnQty.Xalign = 0.5f; 
            spnQty.KeyPressEvent += OnSpnQtyKeyPressEvent;
            spnQty.ModifyFont(FontDescription.FromString("Arial 14"));
            
            
            btnSave = (Button)builder.GetObject("BtnSave");
            btnSave.Clicked += BtnProcessCheckoutClicked;

            btnProduct = (Button)builder.GetObject("BtnProduct");
            popoverProduct = new Popover(btnProduct);    
            btnProduct.Clicked += ShowProductPopup;

            btnSourceOrganization = (Button)builder.GetObject("BtnSourceOrganization");
            popoverSourceOrganization = new Popover(btnSourceOrganization);    
            btnSourceOrganization.Clicked += ShowSourceOrganizationPopup;

            btnDestinationOrganization = (Button)builder.GetObject("BtnDestinationOrganization");
            popoverDestinationOrganization = new Popover(btnDestinationOrganization);    
            btnDestinationOrganization.Clicked += ShowDestinationOrganizationPopup;

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
            textBackground = "lightred";
            isEditable = true;
            AddColumnsTrans(); 
            _treeViewTrans.Selection.Changed += HandleTreeVewSelectedTrans;
            _treeViewTrans.Columns[2].Visible = false;
            _treeViewTrans.Columns[4].Visible = false;
            _treeViewTrans.Columns[6].Visible = false;
            _treeViewTrans.Columns[8].Visible = false;
            _treeViewTrans.Columns[10].Visible = false;
            _treeViewTrans.Columns[12].Visible = false;
            _treeViewTrans.Columns[14].Visible = false;
            _treeViewTrans.Columns[16].Visible = false;

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
            textViewSourceOrganization = (TextView)builder.GetObject("TextViewSourceOrganization");
            textViewDestinationOrganization = (TextView)builder.GetObject("TextViewDestinationOrganization");

            cmbTransferType = (ComboBoxText)builder.GetObject("CmbTransferType");
            GuiCl.FillComboBoxText(cmbTransferType, "Select id,name from transfer_type order by id asc",1);

            cmbSourceLocation = (ComboBoxText)builder.GetObject("CmbSourceLocation");
            GuiCl.FillComboBoxText(cmbSourceLocation, "Select id,name from location order by id asc",0);
            cmbSourceLocation.Changed += HandleCmbSourceLocationChanged;
            cmbSourceCondition = (ComboBoxText)builder.GetObject("CmbSourceCondition");
            GuiCl.FillComboBoxText(cmbSourceCondition, "Select id,name from condition order by id asc",0);
            cmbSourceCondition.Changed += HandleCmbSourceConditionChanged;
            cmbDestinationLocation = (ComboBoxText)builder.GetObject("CmbDestinationLocation");
            GuiCl.FillComboBoxText(cmbDestinationLocation, "Select id,name from location order by id asc",0);
            cmbDestinationLocation.Changed += HandleCmbDestinationLocationChanged;
            cmbDestinationCondition = (ComboBoxText)builder.GetObject("CmbDestinationCondition");
            GuiCl.FillComboBoxText(cmbDestinationCondition, "Select id,name from condition order by id asc",0);
            cmbDestinationCondition.Changed += HandleCmbDestinationConditionChanged;

            lbTransactionId = (Label)builder.GetObject("LbTransactionId");
            lbTransactionId.ModifyFont(FontDescription.FromString("Arial 14"));
            lbTotalItem = (Label)builder.GetObject("LbTotalItem");
            lbTotalItem.ModifyFont(FontDescription.FromString("Arial 14"));
            // lbPreviousPayment = (Label)builder.GetObject("LbPreviousPayment");
            // lbBillCalculated = (Label)builder.GetObject("LbBillCalculated");
            // lbBillCalculated.ModifyFont(FontDescription.FromString("Arial 14"));
            this.KeyPressEvent += OnThisKeyPressEvent;
            
            // lbChange = (Label)builder.GetObject("LbChange");
            // chkTax = (CheckButton)builder.GetObject("ChkTax");
            // chkTax.Toggled += HandlechkTaxChanged;

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
            }else if (e.Event.Key == Gdk.Key.F3)
            {                 
                if(boxItem.Sensitive==true) ItemTransactionReady(true);
            }       
        }

        private void HandleCmbSourceLocationChanged(object sender, EventArgs e)
        {
            ComboBoxText widget = sender as ComboBoxText;
            if (widget != null && widget.HasFocus)
            {
                string sql = "update transfer set source_location_id="+widget.ActiveText+" where id="+lbTransactionId.Text;
                Console.WriteLine (sql);
                DbCl.ExecuteTrans(DbCl.getConn(), sql);  
            }
        }
        private void HandleCmbSourceConditionChanged(object sender, EventArgs e)
        {
            ComboBoxText widget = sender as ComboBoxText;
            if (widget != null && widget.HasFocus)
            {
                string sql = "update transfer set source_condition_id="+widget.ActiveText+" where id="+lbTransactionId.Text;
                Console.WriteLine (sql);
                DbCl.ExecuteTrans(DbCl.getConn(), sql);  
            }
        }
        private void HandleCmbDestinationLocationChanged(object sender, EventArgs e)
        {
            ComboBoxText widget = sender as ComboBoxText;
            if (widget != null && widget.HasFocus)
            {
                string sql = "update transfer set destination_location_id="+widget.ActiveText+" where id="+lbTransactionId.Text;
                Console.WriteLine (sql);
                DbCl.ExecuteTrans(DbCl.getConn(), sql);  
            }
        }
        private void HandleCmbDestinationConditionChanged(object sender, EventArgs e)
        {
            ComboBoxText widget = sender as ComboBoxText;
            if (widget != null && widget.HasFocus)
            {
                string sql = "update transfer set destination_condition_id="+widget.ActiveText+" where id="+lbTransactionId.Text;
                Console.WriteLine (sql);
                DbCl.ExecuteTrans(DbCl.getConn(), sql);  
            }
        }

        private void TransactionReady(){
            GuiCl.SensitiveAllWidgets(boxItem,false);      
            btnNew.Sensitive = true;      
            textViewProduct.Buffer.Text = "";
            textViewSourceOrganization.Buffer.Text="";
            textViewDestinationOrganization.Buffer.Text="";
            spnQty.Text = "1";
            boxTransaction.Sensitive = true;
            btnNew.Sensitive = true; 
            // entAmountPayment.Text = "0";               
            // entAmountPayment.ModifyBg(StateType.Normal, new Gdk.Color(255, 255, 255));
            // entAmountPayment.ModifyFg(StateType.Normal, new Gdk.Color(0, 0, 0));
            // entTransactionAmount.ModifyBg(StateType.Normal, new Gdk.Color(255, 255, 255));
            // entTransactionAmount.ModifyFg(StateType.Normal, new Gdk.Color(0, 0, 0));
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
            DataTable dtTransSelected = CoreCl.fillDtTransfer(transaction_id, btnDate.Label, "");
            foreach (DataRow dr in dtTransSelected.Rows)
            { 
                var id = transaction_id;
                var state = dr["state"].ToString();
                var transfer_type_id = dr["transfer_type_id"].ToString();
                var transfer_type_name = dr["transfer_type_name"].ToString();
                var source_organization_id = dr["source_organization_id"].ToString();
                var source_organization_name = dr["source_organization_name"].ToString();
                var source_location_id = dr["source_location_id"].ToString();
                var source_location_name = dr["source_location_name"].ToString();
                var source_condition_id = dr["source_condition_id"].ToString();
                var source_condition_name = dr["source_condition_name"].ToString();
                var destination_organization_id = dr["destination_organization_id"].ToString();
                var destination_organization_name = dr["destination_organization_name"].ToString();
                var destination_location_id = dr["destination_location_id"].ToString();
                var destination_location_name = dr["destination_location_name"].ToString();
                var destination_condition_id = dr["destination_condition_id"].ToString();
                var destination_condition_name = dr["destination_condition_name"].ToString();
                var transfer_date = dr["transfer_date"].ToString();
                var user_id = dr["user_id"].ToString();
                var user_name = dr["user_name"].ToString();
                //setActivePaymentMethod(payment_group_id);

                //lbBillCalculated.Text = (Convert.ToDouble(transaction_amount)+Convert.ToDouble(tax_amount)-Convert.ToDouble(payment_amount)).ToString() ;
                                
                lbTransactionId.Text = id;
                GuiCl.SetActiveComboBoxText(cmbTransferType,transfer_type_id);
                
                sourceOrganizationId = source_organization_id;
                textViewSourceOrganization.Buffer.Text = source_organization_name;
                GuiCl.SetActiveComboBoxText(cmbSourceLocation,source_location_id);
                GuiCl.SetActiveComboBoxText(cmbSourceCondition,source_condition_id);

                destinationOrganizationId = destination_organization_id;
                textViewDestinationOrganization.Buffer.Text = destination_organization_name;
                GuiCl.SetActiveComboBoxText(cmbDestinationLocation,destination_location_id);
                GuiCl.SetActiveComboBoxText(cmbDestinationCondition,destination_condition_id);
                
                CellRendererText retrievedRenderer = GuiCl.GetCellRendererText(_treeViewItems, 2);
                

                if(Convert.ToInt32(state)==0){ 

                   // GuiCl.SensitiveAllWidgets(boxItem,false); 
                    //_treeViewItems.Sensitive = true;
                    textViewDestinationOrganization.Sensitive=false;
                    textViewSourceOrganization.Sensitive=false;
                    GuiCl.SetDisableAllColumn(_treeViewItems);
                    btnSourceOrganization.Sensitive = false;
                    btnDestinationOrganization.Sensitive = false;
                    btnProduct.Sensitive = false;
                    //entTransactionAmount.Sensitive = false;
                    spnQty.Sensitive=false;
                    cmbTransferType.Sensitive = false;
                    //boxPayment.Sensitive = false;
                    //chkTax.Sensitive = false;
                    //entTaxAmount.Sensitive = false;
                    //cmbPaymentMethod.Sensitive = false;
                    //entAmountPayment.Sensitive = false;
                    btnSave.Sensitive = false;
                    
                }else{
                 
                    GuiCl.SensitiveAllWidgets(boxItem,true);
                    //GuiCl.SetEnableColumn(_treeViewItems,[5]);
                    textViewDestinationOrganization.Sensitive=true;
                    textViewSourceOrganization.Sensitive=true;
                    btnSourceOrganization.Sensitive = true;
                    btnDestinationOrganization.Sensitive = true;
                    btnProduct.Sensitive = true;
                   // boxSource.Sensitive = true;
                    boxProductDetail.Sensitive = true;
                    //entTransactionAmount.Sensitive = true;
                    spnQty.Sensitive=true;
                    //chkTax.Sensitive = true;
                    //entTaxAmount.Sensitive = true;
                    cmbTransferType.Sensitive = true;
                    //entAmountPayment.Sensitive = true;
                    btnSave.Sensitive = true;
                }
                ItemTransactionReady(false); 
                SetItemModel(Convert.ToDouble(transaction_id)); 
                
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
            string source_organization_id="1",source_location_id = "1",source_condition_id="1",destination_organization_id="1",destination_location_id="1",destination_condition_id="1";
            switch (cmbTransferType.ActiveText){
                case "1":
                {
                    source_organization_id = "0";
                    destination_organization_id = this.parent.conf.organization_id;
                }
                break;
                case "2":
                {
                    source_organization_id = this.parent.conf.organization_id;
                    destination_organization_id = this.parent.conf.organization_id;
                }
                break;
                case "3":
                {
                    source_organization_id = this.parent.conf.organization_id;
                    destination_organization_id = "0";
                }
                break;
            }
            
            string sql = "insert into transfer (transfer_type_id,transfer_date,input_date,source_organization_id,source_location_id,source_condition_id,destination_organization_id,destination_location_id,destination_condition_id, user_id,application_id, reference_id) "+
            "values ("+cmbTransferType.ActiveText+",CURRENT_DATE,CURRENT_DATE,"+source_organization_id+",1,1,"+destination_organization_id+",1,1,"+this.parent.user.id+",'"+this.parent.application_id+"','') ";
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
                _clsTrans = new List<clTransfer>();               
                clTransfer tran;
                dtTransSelected =  CoreCl.fillDtTransfer(transaction_id, btnDate.Label, "");
                foreach (DataRow dr in dtTransSelected.Rows)
                {              
                    tran = new clTransfer{    
                        id=dr["id"].ToString(),
                        reference_id=dr["reference_id"].ToString(),
                        source_organization_id=dr["source_organization_id"].ToString(),
                        source_organization_name=dr["source_organization_name"].ToString(),
                        source_location_id=dr["source_location_id"].ToString(),
                        source_location_name=dr["source_location_name"].ToString(),
                        source_condition_id=dr["source_condition_id"].ToString(),
                        source_condition_name=dr["source_condition_name"].ToString(),
                        destination_organization_id=dr["destination_organization_id"].ToString(),
                        destination_organization_name=dr["destination_organization_name"].ToString(),
                        destination_location_id=dr["destination_location_id"].ToString(),
                        destination_location_name=dr["destination_location_name"].ToString(),
                        destination_condition_id=dr["destination_condition_id"].ToString(),
                        destination_condition_name=dr["destination_condition_name"].ToString(),
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

                _lsModelTrans = new ListStore(typeof(string),typeof(string),typeof(string),typeof(string),typeof(string),typeof(string),typeof(string),typeof(string),typeof(string),typeof(string),typeof(string),typeof(string),typeof(string),typeof(string),typeof(string),typeof(string),typeof(string),typeof(string),typeof(string),typeof(string),typeof(string));

                /* add items */
                for (int i = 0; i < _clsTrans.Count; i++)
                {
                    iter = _lsModelTrans.Append();
                    _lsModelTrans.SetValue(iter, (int)ColumnTrans.id, _clsTrans[i].id);
                    _lsModelTrans.SetValue(iter, (int)ColumnTrans.reference_id, _clsTrans[i].reference_id);
                    _lsModelTrans.SetValue(iter, (int)ColumnTrans.source_organization_id, _clsTrans[i].source_organization_id);
                    _lsModelTrans.SetValue(iter, (int)ColumnTrans.source_organization_name, _clsTrans[i].source_organization_name);
                    _lsModelTrans.SetValue(iter, (int)ColumnTrans.source_location_id, _clsTrans[i].source_location_id);
                    _lsModelTrans.SetValue(iter, (int)ColumnTrans.source_location_name, _clsTrans[i].source_location_name);
                    _lsModelTrans.SetValue(iter, (int)ColumnTrans.source_condition_id, _clsTrans[i].source_condition_id);
                    _lsModelTrans.SetValue(iter, (int)ColumnTrans.source_condition_name, _clsTrans[i].source_condition_name);
                    _lsModelTrans.SetValue(iter, (int)ColumnTrans.destination_organization_id, _clsTrans[i].destination_organization_id);
                    _lsModelTrans.SetValue(iter, (int)ColumnTrans.destination_organization_name, _clsTrans[i].destination_organization_name);
                    _lsModelTrans.SetValue(iter, (int)ColumnTrans.destination_location_id, _clsTrans[i].destination_location_id);
                    _lsModelTrans.SetValue(iter, (int)ColumnTrans.destination_location_name, _clsTrans[i].destination_location_name);
                    _lsModelTrans.SetValue(iter, (int)ColumnTrans.destination_condition_id, _clsTrans[i].destination_condition_id);
                    _lsModelTrans.SetValue(iter, (int)ColumnTrans.destination_condition_name, _clsTrans[i].destination_condition_name);
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
            Gtk.Application.Invoke(delegate
            {    
                //ListStore model;
                _lsModelItems = null;
                TreeIter iter;
                /* create array */
                _clsItems = new List<clTransactionItem1>();
                clTransactionItem1 item;
                
                clTransactionItem1 filterItem = new clTransactionItem1{                    
                    //product_short_name = entSearch.Text.Trim(),
                };
                clTransfer filterTrans = new clTransfer {
                    id=transaction_id.ToString(),
                    transaction_date = DateTime.Now.ToString("yyyy-MM-dd"), 
                    transaction_type_id="1"
                };
                DataTable dt = CoreCl.fillDtTransferItem(filterTrans, filterItem);
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
                        purchase_main_discount=dr["purchase_main_discount"].ToString(),
                        purchase_additional_discount=dr["purchase_additional_discount"].ToString(),
                        purchase_deduction_amount=dr["purchase_deduction_amount"].ToString(),
                        purchase_final_price= dr["purchase_final_price"].ToString(),
                        purchase_subtotal= (Convert.ToDouble(dr["purchase_final_price"].ToString())*Convert.ToDouble(dr["quantity"].ToString())).ToString(),
                        purchase_tax=dr["purchase_tax"].ToString(),
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
                    _lsModelItems.SetValue(iter, (int)ColumnItems.purchase_item_price, _clsItems[i].purchase_item_price);
                    _lsModelItems.SetValue(iter, (int)ColumnItems.purchase_main_discount, _clsItems[i].purchase_main_discount);
                    _lsModelItems.SetValue(iter, (int)ColumnItems.purchase_additional_discount, _clsItems[i].purchase_additional_discount);
                    _lsModelItems.SetValue(iter, (int)ColumnItems.purchase_deduction_amount, _clsItems[i].purchase_deduction_amount);
                    _lsModelItems.SetValue(iter, (int)ColumnItems.purchase_final_price, _clsItems[i].purchase_final_price);
                    _lsModelItems.SetValue(iter, (int)ColumnItems.purchase_subtotal, _clsItems[i].purchase_subtotal);
                    _lsModelItems.SetValue(iter, (int)ColumnItems.purchase_tax, _clsItems[i].purchase_tax);
                    _lsModelItems.SetValue(iter, (int)ColumnItems.state, _clsItems[i].state);
                    _lsModelItems.SetValue(iter, (int)ColumnItems.state_name, _clsItems[i].state_name);
                    _lsModelItems.SetValue(iter, (int)ColumnItems.location, _clsItems[i].location);
                    _lsModelItems.SetValue(iter, (int)ColumnItems.location_name, _clsItems[i].location_name);
                    _lsModelItems.SetValue(iter, (int)ColumnItems.condition, _clsItems[i].condition);
                    _lsModelItems.SetValue(iter, (int)ColumnItems.condition_name, _clsItems[i].condition_name);
                }
                _treeViewItems.Model = _lsModelItems;   
                  
                lbTotalItem.Text =  GetTotalItem().ToString(); 
                // entTaxAmount.Text = CalculateTax(GetTotalPurchasePrice()).ToString();
                // entTransactionAmount.Text = (GetTotalPurchasePrice()+CalculateTax(GetTotalPurchasePrice())).ToString();
            });
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
                Console.WriteLine("state: "+_lsModelItems.GetValue (iter, 13).ToString());
                if(_lsModelItems.GetValue (iter, 13).ToString()!="0"){
                    string sql = "delete from stock where id="+_lsModelItems.GetValue (iter, 5).ToString();
                    Console.WriteLine(sql);
                    DbCl.ExecuteScalar(DbCl.getConn(), sql);
                    sql = "delete from transfer_item where id="+_lsModelItems.GetValue (iter, 0).ToString();
                    Console.WriteLine(sql);
                    DbCl.ExecuteScalar(DbCl.getConn(), sql);
                    SetItemModel(Convert.ToDouble(lbTransactionId.Text));
                    if(GetTotalItem()==0) {
                        sql = "update transfer set state=1 where id="+lbTransactionId.Text;
                        Console.WriteLine (sql);
                        DbCl.ExecuteTrans(DbCl.getConn(), sql);                     
                    }
                    ItemTransactionReady(true);
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
            // dtItemSelected = new DataTable();
            // string sql = "SELECT prod.id, prod.short_name, prod.name prod_name, prod.barcode, prod.product_group, prodgr.name product_group_name, prod.price1, prod.price2, prod.price3, prod.last_purchase_price "+
            //         "FROM product prod, product_group prodgr "+
            //         "WHERE prod.product_group = prodgr.id and prod.id= "+prm;
            //         Console.WriteLine(sql);          
            // dtItemSelected = DbCl.fillDataTable(DbCl.getConn(), sql);   

            clStock filterStock = new clStock{product_id=prm};
            dtItemSelected = CoreCl.fillDtProduct(0,filterStock);

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
            textViewProduct.Buffer.InsertWithTags (ref iter, dtItemSelected.Rows[0].ItemArray[10].ToString(), tag);
        }
        
        public Response AddItemTransferIn(){
            Console.WriteLine("======AddItemTransferIn========1===="+ dtItemSelected.Rows[0].ItemArray[0].ToString()); 
            int transaction_type_id = 20+Convert.ToInt32(cmbTransferType.ActiveText);
            Response resp = new Response();
            Int64 price_id = InsertPrice();
            Int64 stock_id = InsertStockIn(price_id.ToString());
            string sql = "insert into transfer_item (transaction_id,product_id,quantity,stock_id,purchase_price_id,state,location,condition) "+
            "values("+lbTransactionId.Text+ ","+dtItemSelected.Rows[0].ItemArray[0].ToString() + ","+spnQty.Text+","+ stock_id.ToString() + ","+price_id.ToString()+",1,"+cmbDestinationLocation.ActiveText+","+cmbDestinationCondition.ActiveText+")" ;
            Console.WriteLine (sql);
            DbCl.ExecuteTrans(DbCl.getConn(), sql); 
            
            CoreCl.InsertStockHistory(lbTransactionId.Text,transaction_type_id.ToString(), dtItemSelected.Rows[0].ItemArray[0].ToString(),stock_id.ToString(),cmbDestinationLocation.ActiveText,cmbDestinationCondition.ActiveText,spnQty.Text);
            sql = "update transaction set state=2 where id="+lbTransactionId.Text;
            Console.WriteLine (sql);
            DbCl.ExecuteTrans(DbCl.getConn(), sql); 
            resp = new Response{ code = "20",description = "Success"} ;                     
            return resp;   
        }

        public Int64 InsertPrice(){
            string sql = "insert into purchase_price (input_date,item_price,final_price) "+
            "values (CURRENT_TIMESTAMP,"+ dtItemSelected.Rows[0].ItemArray[7].ToString() +","+ dtItemSelected.Rows[0].ItemArray[7].ToString()+") returning id";
            Console.WriteLine (sql);
            return DbCl.ExecuteScalar(DbCl.getConn(), sql);
        }
        public Int64 InsertStockIn(string price_id){
            string sql = "insert into stock (product_id,quantity,input_date,expired_date, price_id, unit, location, condition)"+
            "values ("+dtItemSelected.Rows[0].ItemArray[0].ToString()+ ","+spnQty.Text+",CURRENT_DATE,CURRENT_DATE,"+price_id+",1,"+cmbDestinationLocation.ActiveText+","+cmbDestinationCondition.ActiveText+") returning id";
            Console.WriteLine (sql);
            return DbCl.ExecuteScalar(DbCl.getConn(), sql);
            //tekan kene
        }
        
        public Response AddItemInternal(){
            Console.WriteLine("======AddItemInternal========1===="+ dtItemSelected.Rows[0].ItemArray[0].ToString()); 
            int transaction_type_id = 20+Convert.ToInt32(cmbTransferType.ActiveText);
            string sql="";
            Response resp = new Response();
            Boolean valid=false; 
            double store_quantity = 0;
            double global_quantity = 0;
            //check available product
            DataTable dts = CoreCl.fillDtProduct(transaction_type_id,new clStock{product_id=dtItemSelected.Rows[0].ItemArray[0].ToString(),location=cmbSourceLocation.ActiveText,condition=cmbSourceCondition.ActiveText});
            foreach(DataRow drs in dts.Rows){
                global_quantity = Convert.ToDouble(drs["global_quantity"].ToString());
                if(Convert.ToDouble(spnQty.Text) > global_quantity ){
                    resp = new Response{ code = "22",description = "Failed, "+drs["short_name"].ToString()+" out of stock on store"} ;
                }else{
                    resp = new Response{ code = "20",description = "Success"} ;
                    valid = true;
                }
            } 
            Console.WriteLine("======internal========3====" + valid.ToString());             
            if(valid){
                //FIFO show all stock in store location
                //int transaction_type_id = 20+Convert.ToInt32(cmbTransferType.ActiveText);
                double balance = 0;
                balance = Convert.ToDouble(spnQty.Text);  
                sql = "select stock.id stock_id,stock.quantity,stock.product_id,stock.price_id,stock.location,TO_CHAR(stock.expired_date,'yyyy-MM-dd') expired_date, stock.unit, stock.condition "+
                "from stock,location loc,location_group locgr "+
                "where stock.quantity>0 and state=0 and product_id="+ dtItemSelected.Rows[0].ItemArray[0].ToString() + " "+
                "and stock.location=loc.id and loc.location_group=locgr.id "+
                "order by stock.id asc";
                Console.WriteLine(sql);   
                dts = DbCl.fillDataTable(DbCl.getConn(), sql);
                foreach(DataRow drs in dts.Rows){             
                   
                    if(Convert.ToDouble(balance) > Convert.ToDouble(drs["quantity"].ToString()) ){
                        //source
                        sql = "update stock set quantity=0 where id="+drs["stock_id"].ToString() ;
                        Console.WriteLine (sql);
                        DbCl.ExecuteTrans(DbCl.getConn(), sql);    
                        sql = sql = "insert into transfer_item (transaction_id,product_id,quantity,stock_id,purchase_price_id,state) "+
                        "values("+lbTransactionId.Text+ ","+dtItemSelected.Rows[0].ItemArray[0].ToString() + ","+drs["quantity"].ToString()+","+ drs["stock_id"].ToString() + ","+drs["price_id"].ToString()+",1)" ;
                        Console.WriteLine (sql); 
                        DbCl.ExecuteTrans(DbCl.getConn(), sql);                          
                        CoreCl.InsertStockHistory(lbTransactionId.Text,transaction_type_id.ToString(), dtItemSelected.Rows[0].ItemArray[0].ToString(),drs["stock_id"].ToString(),cmbSourceLocation.ActiveText,cmbSourceCondition.ActiveText,"-"+drs["quantity"].ToString());
                    
                        //destination
                        clStock stodest = new clStock{
                            product_id = drs["product_id"].ToString(),
                            quantity = drs["quantity"].ToString(),
                            expired_date = drs["expired_date"].ToString(),
                            price_id = drs["price_id"].ToString(),
                            unit = drs["unit"].ToString(),
                            condition = cmbDestinationCondition.ActiveText,
                            location = cmbDestinationLocation.ActiveText,
                        };
                        Int64 stock_id_dest = InsertStock(stodest);
                        CoreCl.InsertStockHistory(lbTransactionId.Text,transaction_type_id.ToString(), dtItemSelected.Rows[0].ItemArray[0].ToString(),stock_id_dest.ToString(),cmbDestinationLocation.ActiveText,cmbDestinationCondition.ActiveText,drs["quantity"].ToString());
                        
                        balance = balance-Convert.ToDouble(drs["quantity"].ToString());
                    }else{
                        //source
                        sql = "update stock set quantity=quantity-"+balance.ToString()+" where id="+drs["stock_id"].ToString() ;
                        Console.WriteLine (sql);
                        DbCl.ExecuteTrans(DbCl.getConn(), sql); 
                        sql = sql = "insert into transfer_item (transaction_id,product_id,quantity,stock_id,purchase_price_id,state) "+
                        "values("+lbTransactionId.Text+ ","+dtItemSelected.Rows[0].ItemArray[0].ToString() + ","+balance.ToString()+","+ drs["stock_id"].ToString() + ","+drs["price_id"].ToString()+",1)" ;
                        Console.WriteLine (sql); 
                        DbCl.ExecuteTrans(DbCl.getConn(), sql); 
                        CoreCl.InsertStockHistory(lbTransactionId.Text,transaction_type_id.ToString(), dtItemSelected.Rows[0].ItemArray[0].ToString(),drs["stock_id"].ToString(),cmbSourceLocation.ActiveText,cmbSourceCondition.ActiveText,"-"+balance.ToString());                      
                        
                        //destination
                        clStock stodest = new clStock{
                            product_id = drs["product_id"].ToString(),
                            quantity = balance.ToString(),
                            expired_date = drs["expired_date"].ToString(),
                            price_id = drs["price_id"].ToString(),
                            unit = drs["unit"].ToString(),
                            condition = cmbDestinationCondition.ActiveText,
                            location = cmbDestinationLocation.ActiveText,
                        };
                        Int64 stock_id_dest = InsertStock(stodest);                        
                        CoreCl.InsertStockHistory(lbTransactionId.Text,transaction_type_id.ToString(), dtItemSelected.Rows[0].ItemArray[0].ToString(),stock_id_dest.ToString(),cmbDestinationLocation.ActiveText,cmbDestinationCondition.ActiveText,balance.ToString());

                        balance = 0;
                    }
                    if (balance==0) break;
                }
                Console.WriteLine("=======internal =======4====");
                sql = "update transaction set state=2 where id="+lbTransactionId.Text;
                Console.WriteLine(sql);
                DbCl.ExecuteTrans(DbCl.getConn(), sql);
                resp = new Response{ code = "20",description = "Success"} ;
            }            
            return resp;            
        }

        public Int64 InsertStock(clStock sto){
            string sql = "insert into stock (product_id,quantity,input_date,expired_date, price_id, unit, condition, location)"+
            "values ("+sto.product_id+ ","+ sto.quantity+",CURRENT_DATE,'"+sto.expired_date+"',"+sto.price_id+","+sto.unit+","+sto.condition+","+sto.location+") returning id";
            Console.WriteLine (sql);
            return DbCl.ExecuteScalar(DbCl.getConn(), sql);
        }

        public Response AddItemTransferOut(){
            Console.WriteLine("======AddItemTransferOut========1===="+ dtItemSelected.Rows[0].ItemArray[0].ToString()); 
            int transaction_type_id = 20+Convert.ToInt32(cmbTransferType.ActiveText);
            string sql="";
            Response resp = new Response();
            Boolean valid=false; 
            double store_quantity = 0;
            double global_quantity = 0;
            //check available product
            DataTable dts = CoreCl.fillDtProduct(transaction_type_id,new clStock{product_id=dtItemSelected.Rows[0].ItemArray[0].ToString(),location=cmbSourceLocation.ActiveText,condition=cmbSourceCondition.ActiveText});
            foreach(DataRow drs in dts.Rows){
                global_quantity = Convert.ToDouble(drs["global_quantity"].ToString());
                if(Convert.ToDouble(spnQty.Text) > global_quantity ){
                    resp = new Response{ code = "22",description = "Failed, "+drs["short_name"].ToString()+" out of stock on store"} ;
                }else{
                    resp = new Response{ code = "20",description = "Success"} ;
                    valid = true;
                }
            } 
            Console.WriteLine("======AddItemTransferOut========3====" + valid.ToString());             
            if(valid){
                //FIFO show all stock in store location
                //int transaction_type_id = 20+Convert.ToInt32(cmbTransferType.ActiveText);
                double balance = 0;
                balance = Convert.ToDouble(spnQty.Text);  
                sql = "select stock.id stock_id,stock.quantity,stock.product_id,stock.price_id,stock.location,TO_CHAR(stock.expired_date,'yyyy-MM-dd') expired_date, stock.unit, stock.condition "+
                "from stock,location loc,location_group locgr "+
                "where stock.quantity>0 and state=0 and product_id="+ dtItemSelected.Rows[0].ItemArray[0].ToString() + " "+
                "and stock.location=loc.id and loc.location_group=locgr.id "+
                "order by stock.id asc";
                Console.WriteLine(sql);   
                dts = DbCl.fillDataTable(DbCl.getConn(), sql);
                foreach(DataRow drs in dts.Rows){             
                   
                    if(Convert.ToDouble(balance) > Convert.ToDouble(drs["quantity"].ToString()) ){
                        //source
                        sql = "update stock set quantity=0 where id="+drs["stock_id"].ToString() ;
                        Console.WriteLine (sql);
                        DbCl.ExecuteTrans(DbCl.getConn(), sql);    
                        sql = sql = "insert into transfer_item (transaction_id,product_id,quantity,stock_id,purchase_price_id,state) "+
                        "values("+lbTransactionId.Text+ ","+dtItemSelected.Rows[0].ItemArray[0].ToString() + ","+drs["quantity"].ToString()+","+ drs["stock_id"].ToString() + ","+drs["price_id"].ToString()+",1)" ;
                        Console.WriteLine (sql); 
                        DbCl.ExecuteTrans(DbCl.getConn(), sql);                          
                        CoreCl.InsertStockHistory(lbTransactionId.Text,transaction_type_id.ToString(), dtItemSelected.Rows[0].ItemArray[0].ToString(),drs["stock_id"].ToString(),cmbSourceLocation.ActiveText,cmbSourceCondition.ActiveText,"-"+drs["quantity"].ToString());
                    
                        balance = balance-Convert.ToDouble(drs["quantity"].ToString());
                    }else{
                        //source
                        sql = "update stock set quantity=quantity-"+balance.ToString()+" where id="+drs["stock_id"].ToString() ;
                        Console.WriteLine (sql);
                        DbCl.ExecuteTrans(DbCl.getConn(), sql); 
                        sql = sql = "insert into transfer_item (transaction_id,product_id,quantity,stock_id,purchase_price_id,state) "+
                        "values("+lbTransactionId.Text+ ","+dtItemSelected.Rows[0].ItemArray[0].ToString() + ","+balance.ToString()+","+ drs["stock_id"].ToString() + ","+drs["price_id"].ToString()+",1)" ;
                        Console.WriteLine (sql); 
                        DbCl.ExecuteTrans(DbCl.getConn(), sql); 
                        CoreCl.InsertStockHistory(lbTransactionId.Text,transaction_type_id.ToString(), dtItemSelected.Rows[0].ItemArray[0].ToString(),drs["stock_id"].ToString(),cmbSourceLocation.ActiveText,cmbSourceCondition.ActiveText,"-"+balance.ToString());                      
                       
                        balance = 0;
                    }
                    if (balance==0) break;
                }
                Console.WriteLine("=======AddItemTransferOut =======4====");
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
                    Console.WriteLine("cmbTransferType.ActiveText "+cmbTransferType.ActiveText);
                    Response resp = new Response();
                    if(cmbTransferType.ActiveText=="1"){
                        resp  = AddItemTransferIn();
                    }else if(cmbTransferType.ActiveText=="2"){
                        resp =  AddItemInternal();
                    }else if(cmbTransferType.ActiveText=="3"){
                        resp  = AddItemTransferOut();
                    }
                    
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
            
            rendererText = new CellRendererText(); //2
            _cellColumnsRender.Add(rendererText, (int)ColumnTrans.reference_id);
            _treeViewTrans.InsertColumn(-1, "Reference ID", rendererText, "text", (int)ColumnTrans.reference_id);            

            rendererText = new CellRendererText(); //2
            _cellColumnsRender.Add(rendererText, (int)ColumnTrans.source_organization_id);
            _treeViewTrans.InsertColumn(-1, "Source Organization", rendererText, "text", (int)ColumnTrans.source_organization_id);            

            rendererText = new CellRendererText(); //3
            _cellColumnsRender.Add(rendererText, (int)ColumnTrans.source_organization_name);
            _treeViewTrans.InsertColumn(-1, "Source Organization", rendererText, "text", (int)ColumnTrans.source_organization_name); 

            rendererText = new CellRendererText(); //4
            _cellColumnsRender.Add(rendererText, (int)ColumnTrans.source_location_id);
            _treeViewTrans.InsertColumn(-1, "Source Location", rendererText, "text", (int)ColumnTrans.source_location_id);

            rendererText = new CellRendererText(); //5
            _cellColumnsRender.Add(rendererText, (int)ColumnTrans.source_location_name);
            _treeViewTrans.InsertColumn(-1, "Source Location", rendererText, "text", (int)ColumnTrans.source_location_name);

            rendererText = new CellRendererText(); //5
            _cellColumnsRender.Add(rendererText, (int)ColumnTrans.source_condition_id);
            _treeViewTrans.InsertColumn(-1, "Source Condition", rendererText, "text", (int)ColumnTrans.source_condition_id);

            rendererText = new CellRendererText(); //6
            _cellColumnsRender.Add(rendererText, (int)ColumnTrans.source_condition_name);
            _treeViewTrans.InsertColumn(-1, "Source Condition", rendererText, "text", (int)ColumnTrans.source_condition_name);

            rendererText = new CellRendererText(); //6
            _cellColumnsRender.Add(rendererText, (int)ColumnTrans.destination_organization_id);
            _treeViewTrans.InsertColumn(-1, "Destination Organization", rendererText, "text", (int)ColumnTrans.destination_organization_id);

            rendererText = new CellRendererText(); //6
            _cellColumnsRender.Add(rendererText, (int)ColumnTrans.destination_organization_name);
            _treeViewTrans.InsertColumn(-1, "Destination Organization", rendererText, "text", (int)ColumnTrans.destination_organization_name);

            rendererText = new CellRendererText();//13
            _cellColumnsRender.Add(rendererText, (int)ColumnTrans.destination_location_id);
            _treeViewTrans.InsertColumn(-1, "Destination Location", rendererText, "text", (int)ColumnTrans.destination_location_id);

            rendererText = new CellRendererText();//13
            _cellColumnsRender.Add(rendererText, (int)ColumnTrans.destination_location_name);
            _treeViewTrans.InsertColumn(-1, "Destination Location", rendererText, "text", (int)ColumnTrans.destination_location_name);

             rendererText = new CellRendererText(); //10
            _cellColumnsRender.Add(rendererText, (int)ColumnTrans.destination_condition_id);
            _treeViewTrans.InsertColumn(-1, "Destination Condition", rendererText, "text", (int)ColumnTrans.destination_condition_id);

            rendererText = new CellRendererText();//11
            _cellColumnsRender.Add(rendererText, (int)ColumnTrans.destination_condition_name);
            _treeViewTrans.InsertColumn(-1, "Destination Condition", rendererText, "text", (int)ColumnTrans.destination_condition_name);

            rendererText = new CellRendererText();//12
            _cellColumnsRender.Add(rendererText, (int)ColumnTrans.user_id);
            _treeViewTrans.InsertColumn(-1, "User id", rendererText, "text", (int)ColumnTrans.user_id);

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
            //5,6,7,11,12
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

            rendererText = new CellRendererText();//3
            _cellColumnsRenderItems.Add(rendererText, (int)ColumnItems.product_short_name);
            _treeViewItems.InsertColumn(-1, "Short name", rendererText, "text", (int)ColumnItems.product_short_name);

            rendererText = new CellRendererText();//4
            _cellColumnsRenderItems.Add(rendererText, (int)ColumnItems.stock_id);
            _treeViewItems.InsertColumn(-1, "Stock ID", rendererText, "text", (int)ColumnItems.stock_id);

            rendererText = new CellRendererText(); //5
            _cellColumnsRenderItems.Add(rendererText, (int)ColumnItems.quantity);
            _treeViewItems.InsertColumn(-1, "Quantity", rendererText, "text", (int)ColumnItems.quantity);

            rendererText = new CellRendererText();     //6    
            _cellColumnsRenderItems.Add(rendererText, (int)ColumnItems.unit_name);
            _treeViewItems.InsertColumn(-1, "Unit", rendererText, "text", (int)ColumnItems.unit_name);

            rendererText = new CellRendererText(); //7
            _cellColumnsRenderItems.Add(rendererText, (int)ColumnItems.purchase_price_id);
            _treeViewItems.InsertColumn(-1, "Price ID", rendererText, "text", (int)ColumnItems.purchase_price_id);

            rendererText = new CellRendererText();//8
            _cellColumnsRenderItems.Add(rendererText, (int)ColumnItems.purchase_item_price);
            _treeViewItems.InsertColumn(-1, "Item price", rendererText, "text", (int)ColumnItems.purchase_item_price);

            rendererText = new CellRendererText(); //9
            _cellColumnsRenderItems.Add(rendererText, (int)ColumnItems.purchase_main_discount);
            _treeViewItems.InsertColumn(-1, "Main discount", rendererText, "text", (int)ColumnItems.purchase_main_discount);

            rendererText = new CellRendererText(); //10
            _cellColumnsRenderItems.Add(rendererText, (int)ColumnItems.purchase_additional_discount);
            _treeViewItems.InsertColumn(-1, "Additional discount", rendererText, "text", (int)ColumnItems.purchase_additional_discount);

            rendererText = new CellRendererText(); //11
            _cellColumnsRenderItems.Add(rendererText, (int)ColumnItems.purchase_deduction_amount);
            _treeViewItems.InsertColumn(-1, "Deduction amount", rendererText, "text", (int)ColumnItems.purchase_deduction_amount);

            rendererText = new CellRendererText(); //12
            _cellColumnsRenderItems.Add(rendererText, (int)ColumnItems.purchase_final_price);
            _treeViewItems.InsertColumn(-1, "Net price", rendererText, "text", (int)ColumnItems.purchase_final_price);
            
            rendererText = new CellRendererText(); //13
            _cellColumnsRenderItems.Add(rendererText, (int)ColumnItems.purchase_subtotal);
            _treeViewItems.InsertColumn(-1, "Subtotal", rendererText, "text", (int)ColumnItems.purchase_subtotal);
            
            rendererText = new CellRendererText(); //14
            _cellColumnsRenderItems.Add(rendererText, (int)ColumnItems.purchase_tax);
            _treeViewItems.InsertColumn(-1, "Tax", rendererText, "text", (int)ColumnItems.purchase_tax);
            
            rendererText = new CellRendererText();  //15       
            _cellColumnsRenderItems.Add(rendererText, (int)ColumnItems.location_name);
            _treeViewItems.InsertColumn(-1, "Location", rendererText, "text", (int)ColumnItems.location_name);


            rendererText = new CellRendererText();  //16      
            _cellColumnsRenderItems.Add(rendererText, (int)ColumnItems.condition_name);
            _treeViewItems.InsertColumn(-1, "Condition", rendererText, "text", (int)ColumnItems.condition_name);

            rendererText = new CellRendererText(); //16
            _cellColumnsRenderItems.Add(rendererText, (int)ColumnItems.state_name);
            _treeViewItems.InsertColumn(-1, "State", rendererText, "text", (int)ColumnItems.state_name);
           
           rendererText = new CellRendererText(); //17
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

        public void doChildProduct(object o, clsProduct prm){
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
                string is_active = "true";     
                if(cmbTransferType.ActiveText=="0"){
                    is_active = "";
                }
                ReferenceProduct refWidget = new ReferenceProduct(this,"dialog",20+Convert.ToInt32(cmbTransferType.ActiveText), new clStock{location=cmbSourceLocation.ActiveText,condition=cmbSourceCondition.ActiveText,is_active=is_active});
                popoverProduct.Add(refWidget);
                popoverProduct.SetSizeRequest(600, 300);
                refWidget.Show();          
                popoverProduct.ShowAll();
                return false;
            });
        }

        public void doChildSourceOrganization(object o, clsOrganization prm){
            GLib.Timeout.Add(5, () =>
            {
                GuiCl.RemoveAllWidgets(popoverSourceOrganization);
                Label popLabel = new Label(prm.name);
                popoverSourceOrganization.Add(popLabel);
                popoverSourceOrganization.SetSizeRequest(200, 20);
                popLabel.Show();
                textViewSourceOrganization.Buffer.Text = prm.name;
                sourceOrganizationId = prm.id;
                return false;
            });
        }
        private void ShowSourceOrganizationPopup(object sender, EventArgs e)
        {   GLib.Timeout.Add(5, () =>
            {     
                GuiCl.RemoveAllWidgets(popoverSourceOrganization);        
                ReferenceOrganization refWidget = new ReferenceOrganization(this,"dialog","source");
                popoverSourceOrganization.Add(refWidget);
                popoverSourceOrganization.SetSizeRequest(600, 300);
                refWidget.Show();          
                popoverSourceOrganization.ShowAll();
                return false;
            });
        }

        public void doChildDestinationOrganization(object o,clsOrganization prm){
            GLib.Timeout.Add(5, () =>
            {
                GuiCl.RemoveAllWidgets(popoverDestinationOrganization);
                Label popLabel = new Label(prm.name);
                popoverDestinationOrganization.Add(popLabel);
                popoverDestinationOrganization.SetSizeRequest(200, 20);
                popLabel.Show();
                textViewDestinationOrganization.Buffer.Text = prm.name;
                destinationOrganizationId = prm.id;
                return false;
            });
        }
        private void ShowDestinationOrganizationPopup(object sender, EventArgs e)
        {   GLib.Timeout.Add(5, () =>
            {     
                GuiCl.RemoveAllWidgets(popoverDestinationOrganization);        
                ReferenceOrganization refWidget = new ReferenceOrganization(this,"dialog","destination");
                popoverDestinationOrganization.Add(refWidget);
                popoverDestinationOrganization.SetSizeRequest(600, 300);
                refWidget.Show();          
                popoverDestinationOrganization.ShowAll();
                return false;
            });
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
        private Response DoCheckout()
        {
            Response resp = new Response();
            Boolean valid = false;
            TreeSelection selection = _treeViewTrans.Selection;
            TreeIter iter;
            if(selection.GetSelected( out iter)){
                Console.WriteLine("Selected Value:"+_lsModelTrans.GetValue (iter, 0).ToString()+_lsModelTrans.GetValue (iter, 1).ToString());
            }  

            
            //if(valid){
                // if(cmbTransferType.ActiveText=="1"){
                //     resp  = DoCheckoutTransferIn();
                // }else if(cmbTransferType.ActiveText=="2"){
                //     resp =  DoCheckoutInternal();
                // }else if(cmbTransferType.ActiveText=="3"){
                //     resp  = DoCheckoutTransferOut();
                // }
                // string sql = "select ti.*,tr.source_location_id,tr.source_condition_id, tr.destination_location_id, tr.destination_condition_id "+
                // "from transfer tr, transfer_item ti where tr.id=ti.transaction_id and tr.id="+lbTransactionId.Text;
                // Console.WriteLine(sql);
                // DataTable dt =  DbCl.fillDataTable(DbCl.getConn(), sql);
                // foreach (DataRow dr in dt.Rows)
                // {                                       
                //     sql = "update product set is_active=true where id="+dr["product_id"].ToString();
                //     Console.WriteLine(sql);
                //     DbCl.ExecuteTrans(DbCl.getConn(), sql);

                //     //kurangi  source
                //     CoreCl.InsertStockHistory(dr["transaction_id"].ToString(),"6", dr["product_id"].ToString(),dr["stock_id"].ToString(),dr["source_location_id"].ToString(),dr["source_condition_id"].ToString(),"-"+dr["quantity"].ToString());
                //     sql = "update stock set state=0 where id="+dr["stock_id"].ToString();
                //     Console.WriteLine(sql);
                //     DbCl.ExecuteTrans(DbCl.getConn(), sql);

                //     //tambah dest
                //     CoreCl.InsertStockHistory(dr["transaction_id"].ToString(),"6", dr["product_id"].ToString(),dr["stock_id"].ToString(),dr["destination_location_id"].ToString(),dr["destination_condition_id"].ToString(),dr["quantity"].ToString());
                //     sql = "update stock set state=0 where id="+dr["stock_id"].ToString();
                //     Console.WriteLine(sql);
                //     DbCl.ExecuteTrans(DbCl.getConn(), sql);

                // }
                string sql = "update transfer_item set state=0 where transaction_id="+lbTransactionId.Text;
                Console.WriteLine(sql);
                DbCl.ExecuteTrans(DbCl.getConn(), sql);
                SetTransactionModel("",entSearch.Text.Trim());  
                SelectedTrans(lbTransactionId.Text);
                SetItemModel(Convert.ToDouble(lbTransactionId.Text));
                TransactionReady();
                resp = new Response{ code = "20",description = "Success"} ;
            //}  
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