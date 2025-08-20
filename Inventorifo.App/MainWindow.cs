using System;
using Gtk;
using UI = Gtk.Builder.ObjectAttribute;
using Inventorifo.Lib.Model;

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
                tax="12",
                person_id="1",
                organization_id="1",
                organization_name="Apotek Enggal Sehat",
                organization_address="Jl. Pisang No1 Majenang",
                organization_phone_number="01923932939"
            };

            MenuItem purchaseMenuItem = (MenuItem)builder.GetObject("PurchaseMenuItem");
            MenuItem purchaseReturnMenuItem = (MenuItem)builder.GetObject("PurchaseReturnMenuItem");
            MenuItem saleMenuItem = (MenuItem)builder.GetObject("SaleMenuItem");
            MenuItem saleReturnMenuItem = (MenuItem)builder.GetObject("SaleReturnMenuItem");
            MenuItem cashDrawerMenuItem = (MenuItem)builder.GetObject("CashDrawerMenuItem");

            MenuItem transferMenuItem = (MenuItem)builder.GetObject("TransferMenuItem");
            MenuItem stockHistoryMenuItem = (MenuItem)builder.GetObject("StockHistoryMenuItem");

            MenuItem stockMenuItem = (MenuItem)builder.GetObject("StockMenuItem");
            MenuItem productMenuItem = (MenuItem)builder.GetObject("ProductMenuItem");
            MenuItem productGroupMenuItem = (MenuItem)builder.GetObject("ProductGroupMenuItem");
            
            MenuItem reportSaleMenuItem = (MenuItem)builder.GetObject("ReportSaleMenuItem");
            MenuItem reportPurchaseMenuItem = (MenuItem)builder.GetObject("ReportPurchaseMenuItem");
            MenuItem reportSaleReturnMenuItem = (MenuItem)builder.GetObject("ReportSaleReturnMenuItem");
            MenuItem reportPurchaseReturnMenuItem = (MenuItem)builder.GetObject("ReportPurchaseReturnMenuItem");

            MenuItem customerMenuItem = (MenuItem)builder.GetObject("CustomerMenuItem");
            MenuItem priceRoleMenuItem = (MenuItem)builder.GetObject("PriceRoleMenuItem");
            MenuItem supplierMenuItem = (MenuItem)builder.GetObject("SupplierMenuItem");
            MenuItem personMenuItem = (MenuItem)builder.GetObject("PersonMenuItem");
            MenuItem organizationMenuItem = (MenuItem)builder.GetObject("OrganizationMenuItem");

            MenuItem aboutMenuItem = (MenuItem)builder.GetObject("AboutMenuItem");
            profilMenuBar = (MenuItem)builder.GetObject("ProfilMenuBar");
            mainBox = (Box)builder.GetObject("MainBox");

            
            
            DeleteEvent += Window_DeleteEvent;
            cashDrawerMenuItem.Activated += CashDrawerMenuItem_Activated;
            purchaseMenuItem.Activated += PurchaseMenuItem_Activated;
            purchaseReturnMenuItem.Activated += PurchaseReturnMenuItem_Activated;
            saleMenuItem.Activated += SaleMenuItem_Activated;
            saleReturnMenuItem.Activated += SaleReturnMenuItem_Activated;
            transferMenuItem.Activated += TransferMenuItem_Activated;
            stockHistoryMenuItem.Activated += StockHistoryMenuItem_Activated;
            reportSaleMenuItem.Activated += reportSaleMenuItem_Activated;
            reportPurchaseMenuItem.Activated += reportPurchaseMenuItem_Activated;
            reportSaleReturnMenuItem.Activated += reportSaleReturnMenuItem_Activated;
            reportPurchaseReturnMenuItem.Activated += reportPurchaseReturnMenuItem_Activated;

            stockMenuItem.Activated += StockMenuItem_Activated;
            productMenuItem.Activated += ProductMenuItem_Activated;
            productGroupMenuItem.Activated += ProductGroupMenuItem_Activated;
            personMenuItem.Activated += PersonMenuItem_Activated;
            organizationMenuItem.Activated += OrganizationMenuItem_Activated;
            supplierMenuItem.Activated += SupplierMenuItemActivated;
            customerMenuItem.Activated += CustomerMenuItem_Activated;
            priceRoleMenuItem.Activated += PriceRoleMenuItem_Activated;

            aboutMenuItem.Activated += AboutMenuItem_Activated;
            //_button1.Clicked += Button1_Clicked;     
                 
            Maximize();
            //( id,  person_id, person_name,  person_address, person_phone_number,  level, is_active
            Login();

           // kene CoreCl.InsertJournal(4,2000,200,"11","description","application_id","user_id");
        }

        private void Login(){
            user = new UserLogin("1","1", "admin","addres","phone","1","admin","true","1","admin");
            this.profilMenuBar.Label = this.user.person_name +"::"+this.user.level_name;
        }
        private void Window_DeleteEvent(object sender, DeleteEventArgs a)
        {
            Application.Quit();
        }

        private void CashDrawerMenuItem_Activated(object sender, EventArgs a)
        {
            Gtk.Application.Invoke(delegate
            {
                ClearMainBox();
                CashDrawer refWidget = new CashDrawer(this);
                mainBox.PackStart(refWidget, true, true, 5);
                refWidget.ShowAll();
            });
        }
        
        private void PurchaseMenuItem_Activated(object sender, EventArgs a)
        {
            ClearMainBox();
            clTransaction filterTrans = new clTransaction {transaction_date = DateTime.Now.ToString("yyyy-MM-dd"), transaction_type_id="1"};
            TransactionPurchase transWidget = new TransactionPurchase(this,filterTrans);
            mainBox.PackStart(transWidget, false, true, 5);
            transWidget.ShowAll();
        }
        private void PurchaseReturnMenuItem_Activated(object sender, EventArgs a){
            string message = "Oh sorry, UNDER CONSTRUCTION MENU ";
            MessageDialog md = new MessageDialog(null, DialogFlags.DestroyWithParent, MessageType.Error, ButtonsType.Close, message);
            md.Run();
            md.Destroy();
            // ClearMainBox();
            // clTransaction filterTrans = new clTransaction {transaction_date = DateTime.Now.ToString("yyyy-MM-dd"), transaction_type_id="3"};
            // TransactionPurchaseReturn transWidget = new TransactionPurchaseReturn(this,filterTrans);
            // mainBox.PackStart(transWidget, false, true, 5);
            // transWidget.ShowAll();
        }
        private void SaleMenuItem_Activated(object sender, EventArgs a)
        {
            // string message = "Oh sorry, UNDER CONSTRUCTION MENU ";
            // MessageDialog md = new MessageDialog(null, DialogFlags.DestroyWithParent, MessageType.Error, ButtonsType.Close, message);
            // md.Run();
            // md.Destroy();
            ClearMainBox();
            clTransaction filterTrans = new clTransaction {transaction_date = DateTime.Now.ToString("yyyy-MM-dd"), transaction_type_id="2"};
            TransactionSale transWidget = new TransactionSale(this,filterTrans);
            mainBox.PackStart(transWidget, false, true, 5);
            transWidget.ShowAll();
        }
        private void SaleReturnMenuItem_Activated(object sender, EventArgs a){
            string message = "Oh sorry, UNDER CONSTRUCTION MENU ";
            MessageDialog md = new MessageDialog(null, DialogFlags.DestroyWithParent, MessageType.Error, ButtonsType.Close, message);
            md.Run();
            md.Destroy();
            // ClearMainBox();
            // clTransaction filterTrans = new clTransaction {transaction_date = DateTime.Now.ToString("yyyy-MM-dd"), transaction_type_id="4"};
            // TransactionSaleReturn transWidget = new TransactionSaleReturn(this,filterTrans);
            // mainBox.PackStart(transWidget, false, true, 5);
            // transWidget.ShowAll();
        }
        private void TransferMenuItem_Activated(object sender, EventArgs a)
        {
            // string message = "Oh sorry, UNDER CONSTRUCTION MENU ";
            // MessageDialog md = new MessageDialog(null, DialogFlags.DestroyWithParent, MessageType.Error, ButtonsType.Close, message);
            // md.Run();
            // md.Destroy();
            ClearMainBox();
            clTransfer filterTrans = new clTransfer {transaction_date = DateTime.Now.ToString("yyyy-MM-dd"), transaction_type_id="1"};
            WarehouseTransfer transWidget = new WarehouseTransfer(this,null);
            mainBox.PackStart(transWidget, false, true, 5);
            transWidget.ShowAll();            
          
        }
        private void StockHistoryMenuItem_Activated(object sender, EventArgs a)
        {
            Gtk.Application.Invoke(delegate
            {
                ClearMainBox();
                WarehouseStockHistory refWidget = new WarehouseStockHistory(this,null);
                mainBox.PackStart(refWidget, true, true, 5);
                refWidget.ShowAll();
            });
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
                clStock filter = new clStock {is_active="true",short_name=""};
                ReferenceProduct refWidget = new ReferenceProduct(this,"widget",0, filter);
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
        private void OrganizationMenuItem_Activated(object sender, EventArgs a)
        {
            Gtk.Application.Invoke(delegate
            {
                ClearMainBox();
                ReferenceOrganization refWidget = new ReferenceOrganization(this, "widget", null);
                mainBox.PackStart(refWidget, true, true, 5);
                refWidget.ShowAll();
            });
        }
        private void reportSaleMenuItem_Activated(object sender, EventArgs a)
        {
            Gtk.Application.Invoke(delegate
            {
                ClearMainBox();
                clTransaction filterTrans = new clTransaction {transaction_date = DateTime.Now.ToString("yyyy-MM-dd"), transaction_type_id="2"};
                ReportTransactionSale refWidget = new ReportTransactionSale(this, "widget", filterTrans);
                mainBox.PackStart(refWidget, true, true, 5);
                refWidget.ShowAll();
            });
        }
        

        private void reportPurchaseMenuItem_Activated(object sender, EventArgs a)
        {
            Gtk.Application.Invoke(delegate
            {
                ClearMainBox();
                clTransaction filterTrans = new clTransaction {transaction_date = DateTime.Now.ToString("yyyy-MM-dd"), transaction_type_id="1"};
                ReportTransactionPurchase refWidget = new ReportTransactionPurchase(this, "widget", filterTrans);
                mainBox.PackStart(refWidget, true, true, 5);
                refWidget.ShowAll();
            });
        }
        private void reportSaleReturnMenuItem_Activated(object sender, EventArgs a)
        {
            Gtk.Application.Invoke(delegate
            {
                ClearMainBox();
                clTransaction filterTrans = new clTransaction {transaction_date = DateTime.Now.ToString("yyyy-MM-dd"), transaction_type_id="4"};
                ReportTransactionSaleReturn refWidget = new ReportTransactionSaleReturn(this, "widget", filterTrans);
                mainBox.PackStart(refWidget, true, true, 5);
                refWidget.ShowAll();
            });
        }
        private void reportPurchaseReturnMenuItem_Activated(object sender, EventArgs a)
        {
            Gtk.Application.Invoke(delegate
            {
                ClearMainBox();
                clTransaction filterTrans = new clTransaction {transaction_date = DateTime.Now.ToString("yyyy-MM-dd"), transaction_type_id="3"};
                ReportTransactionPurchaseReturn refWidget = new ReportTransactionPurchaseReturn(this, "widget", filterTrans);
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
        private void PriceRoleMenuItem_Activated(object sender, EventArgs a)
        {
            Gtk.Application.Invoke(delegate
            {
                ClearMainBox();
                ReferencePriceRole refWidget = new ReferencePriceRole(this,null);
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
