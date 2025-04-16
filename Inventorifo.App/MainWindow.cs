using System;
using Gtk;
using UI = Gtk.Builder.ObjectAttribute;

namespace Inventorifo.App
{
        
    class MainWindow : Window
    {      
        Inventorifo.Lib.LibDb DbCl = new Inventorifo.Lib.LibDb ();
        Inventorifo.Lib.LibGui GuiCl = new Inventorifo.Lib.LibGui ();
        Inventorifo.Lib.LibCore CoreCl = new Inventorifo.Lib.LibCore ();
        
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
        public clsConfig conf;
        MenuItem profilMenuBar;
        public Overlay overlay;
        private MainWindow(Builder builder) : base(builder.GetRawOwnedObject("MainWindow"))
        {
            builder.Autoconnect(this);
            
            conf = new clsConfig{
                id="1", 
                app_id="1", 
                app_version="0.1.0",
                country_code="ID",
                currency="Rp",
                tax="12"
            };

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
                ReferenceProduct refWidget = new ReferenceProduct(this,"widget",0);
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
                ReferenceCustomer refWidget = new ReferenceCustomer(this,"widget",null);
                mainBox.PackStart(refWidget, true, true, 5);
                refWidget.ShowAll();
            });
        }
        private void AboutMenuItem_Activated(object sender, EventArgs a){

            AboutDialog d = new();
            d.TransientFor = this;
            d.Authors = ["Ali Nugroho, S.Kom  <metunonton@outlook.com>"];
            d.ProgramName = "Inventorifo";
            d.Version = "0.1.0";
            d.Comments = "Presented by efisiensi.org\nThe platform that becomes the real deal when your organization hits the efficiency button";
            d.Copyright = "© 2025 Indonesia";

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
    
    
        
}
