using System;
using Gtk;
using UI = Gtk.Builder.ObjectAttribute;

namespace Inventorifo.App
{
    
    
    class MainWindow : Window
    {      
        public string application_id = "Inventorifo.App";
        //[UI] private Label _label1 = null;
        //[UI] private Button _button1 = null;
       // [UI] private Window TransactionWindow  = null;
       // [UI] private Window ReferenceWindow = null;
        //private int _counter;
        //private Window window;
        public Box mainBox;        
        public MainWindow() : this(new Builder("MainWindow.glade")) { }
        public UserLogin user;
        MenuItem profilMenuBar;
        public Overlay overlay;
        private MainWindow(Builder builder) : base(builder.GetRawOwnedObject("MainWindow"))
        {
            builder.Autoconnect(this);

            MenuItem purchaseMenuItem = (MenuItem)builder.GetObject("PurchaseMenuItem");
            MenuItem saleMenuItem = (MenuItem)builder.GetObject("SaleMenuItem");

            MenuItem stockMenuItem = (MenuItem)builder.GetObject("StockMenuItem");
            MenuItem productMenuItem = (MenuItem)builder.GetObject("ProductMenuItem");
            MenuItem productGroupMenuItem = (MenuItem)builder.GetObject("ProductGroupMenuItem");

            MenuItem customerMenuItem = (MenuItem)builder.GetObject("CustomerMenuItem");
            MenuItem supplierMenuItem = (MenuItem)builder.GetObject("SupplierMenuItem");
            MenuItem personMenuItem = (MenuItem)builder.GetObject("PersonMenuItem");

            MenuItem aboutMenuItem = (MenuItem)builder.GetObject("AboutMenuItem");
            profilMenuBar = (MenuItem)builder.GetObject("ProfilMenuBar");
            mainBox = (Box)builder.GetObject("MainBox");

            
            
            DeleteEvent += Window_DeleteEvent;
            purchaseMenuItem.Activated += PurchaseMenuItem_Activated;
            saleMenuItem.Activated += SaleMenuItem_Activated;

            stockMenuItem.Activated += StockMenuItem_Activated;
            productMenuItem.Activated += ProductMenuItem_Activated;
            productGroupMenuItem.Activated += ProductGroupMenuItem_Activated;
            personMenuItem.Activated += PersonMenuItem_Activated;
            supplierMenuItem.Activated += SupplierMenuItemActivated;
            customerMenuItem.Activated += CustomerMenuItem_Activated;

            aboutMenuItem.Activated += AboutMenuItem_Activated;
            //_button1.Clicked += Button1_Clicked;     
                 
            Maximize();
            //( id,  person_id, person_name,  person_address, person_phone_number,  level, is_active
            Login();
        }

        private void Login(){
            user = new UserLogin("1","1", "admin","addres","phone","1","admin","true","1","admin");
            this.profilMenuBar.Label = this.user.person_name +"::"+this.user.level_name;
        }
        private void Window_DeleteEvent(object sender, DeleteEventArgs a)
        {
            Application.Quit();
        }
        private void PurchaseMenuItem_Activated(object sender, EventArgs a)
        {
            ClearMainBox();
            TransactionPurchase transWidget = new TransactionPurchase(this,null);
            mainBox.PackStart(transWidget, false, true, 5);
            transWidget.ShowAll();

          //  MessageDialog md = new MessageDialog(null, DialogFlags.DestroyWithParent, MessageType.Error, ButtonsType.Close, "Purchase ");
          //  md.Run();
          //  md.Destroy();
        }
        private void SaleMenuItem_Activated(object sender, EventArgs a)
        {
            ClearMainBox();
            TransactionSale transWidget = new TransactionSale(this,null);
            mainBox.PackStart(transWidget, false, true, 5);
            transWidget.ShowAll();

          
        }
        private void StockMenuItem_Activated(object sender, EventArgs a)
        {
            Gtk.Application.Invoke(delegate
            {
                ClearMainBox();
                ReferenceStock refWidget = new ReferenceStock(this,null);
                mainBox.PackStart(refWidget, true, true, 5);
                refWidget.ShowAll();
            });
        }
        private void ProductMenuItem_Activated(object sender, EventArgs a)
        {
            Gtk.Application.Invoke(delegate
            {
                ClearMainBox();
                ReferenceProduct refWidget = new ReferenceProduct(this,"widget",null);
                mainBox.PackStart(refWidget, true, true, 5);
                refWidget.ShowAll();
            });
        }
        private void ProductGroupMenuItem_Activated(object sender, EventArgs a)
        {
            Gtk.Application.Invoke(delegate
            {
                ClearMainBox();
                ReferenceProductGroup refWidget = new ReferenceProductGroup(this,null);
                mainBox.PackStart(refWidget, true, true, 5);
                refWidget.ShowAll();
            });
        }
        private void PersonMenuItem_Activated(object sender, EventArgs a)
        {
            Gtk.Application.Invoke(delegate
            {
                ClearMainBox();
                ReferencePerson refWidget = new ReferencePerson(this, "widget", null);
                mainBox.PackStart(refWidget, true, true, 5);
                refWidget.ShowAll();
            });
        }
        private void SupplierMenuItemActivated(object sender, EventArgs a)
        {
            Gtk.Application.Invoke(delegate
            {
                ClearMainBox();
                ReferenceSupplier refWidget = new ReferenceSupplier(this,"widget",null);
                mainBox.PackStart(refWidget, true, true, 5);
                refWidget.ShowAll();
            });
        }
        private void CustomerMenuItem_Activated(object sender, EventArgs a)
        {
            Gtk.Application.Invoke(delegate
            {
                ClearMainBox();
                ReferenceCustomer refWidget = new ReferenceCustomer(this,null);
                mainBox.PackStart(refWidget, true, true, 5);
                refWidget.ShowAll();
            });
        }
        private void AboutMenuItem_Activated(object sender, EventArgs a){

            AboutDialog d = new();
            d.TransientFor = this;
            d.Authors = ["Ali Nugroho"];
            d.ProgramName = "Inventorifo";
            d.Version = "0.1.0";
            d.Comments = " Inventory FIFO";
            d.Copyright = "© 2025 Salatiga Indonesia";

            //if (Resources.GetTexture("logo.png") is Texture t)
            //    d.Logo = t;
            d.Show();

        }
        private void ClearMainBox(){
            foreach (Widget child in mainBox.Children)
            {
                mainBox.Remove(child);
                child.Destroy(); // Properly free memory
            }
        }
    }
    
    class UserLogin
    { //
        public UserLogin(string id, string person_id,string person_name, string person_address,string person_phone_number, string level,string level_name,string is_active,string application_id, string privilege){
        this.id = id;
        this.person_id = person_id;
        this.person_name = person_name;
        this.person_address = person_address;
        this.person_phone_number = person_phone_number;
        this.level = level;
        this.level_name = level_name;
        this.is_active = is_active;
        this.application_id = application_id;
        this.privilege = privilege;
        }
        public string id;
        public string person_id;
        public string person_name;
        public string person_address;
        public string person_phone_number;
        public string level;
        public string level_name;
        public string is_active;
        public string application_id;
        public string privilege;
    }
    class clPayment
    {    
        public string id { get; set; }
        public string transaction_id { get; set; }
        public string payment_date { get; set; }
        public string payment_amount { get; set; }
        public string user_id { get; set; }
        public string user_name { get; set; }
    }
    class clTransaction
    {    
        public string id { get; set; }
        public string reference_id { get; set; }
        public string supplier_id { get; set; }
        public string organization_name { get; set; }
        public string organization_address { get; set; }
        public string organization_phone_number { get; set; }
        public string person_name { get; set; }
        public string person_phone_number { get; set; }
        public string transaction_type_id { get; set; }
        public string transaction_type_name { get; set; }
        public string transaction_date { get; set; }
        public string transaction_amount { get; set; }
        public string return_amount { get; set; }
        public string payment_group_id { get; set; }
        public string payment_group_name { get; set; }
        public string payment_amount { get; set; }
        public string user_id { get; set; }
        public string user_name { get; set; }
        public string state { get; set; }
        public string state_name { get; set; }
        public string state_fgcolor { get; set; }
        public string state_bgcolor { get; set; }
        public string application_id { get; set; }
    }

    class clTransItem
    {                          
        public string id { get; set; }
        public string transaction_id { get; set; }
        public string product_id { get; set; }
        public string product_name { get; set; }
        public string stock_id { get; set; }
        public string quantity { get; set; }
        public string unit { get; set; }
        public string unit_name { get; set; }
        public string purchase_price { get; set; }
        public string price_id { get; set; }
        public string price { get; set; }
        public string tax { get; set; }
        public string state { get; set; }
        public string location { get; set; }
        public string location_name { get; set; }
        public string condition { get; set; }
        public string condition_name { get; set; }
    }
        
}
