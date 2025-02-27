using System;
using Gtk;
using UI = Gtk.Builder.ObjectAttribute;

namespace Inventorifo.App
{
    class MainWindow : Window
    {
        //[UI] private Label _label1 = null;
        //[UI] private Button _button1 = null;
       // [UI] private Window TransactionWindow  = null;
       // [UI] private Window ReferenceWindow = null;
        //private int _counter;
        //private Window window;
        private Box mainBox;

        public MainWindow() : this(new Builder("MainWindow.glade")) { }

        private MainWindow(Builder builder) : base(builder.GetRawOwnedObject("MainWindow"))
        {
            builder.Autoconnect(this);
            MenuItem purchaseMenuItem = (MenuItem)builder.GetObject("PurchaseMenuItem");
            MenuItem productMenuItem = (MenuItem)builder.GetObject("ProductMenuItem");
            MenuItem productGroupMenuItem = (MenuItem)builder.GetObject("ProductGroupMenuItem");
            MenuItem aboutMenuItem = (MenuItem)builder.GetObject("AboutMenuItem");
            mainBox = (Box)builder.GetObject("MainBox");

            DeleteEvent += Window_DeleteEvent;
            purchaseMenuItem.Activated += PurchaseMenuItem_Activated;
            productMenuItem.Activated += ProductMenuItem_Activated;
            productGroupMenuItem.Activated += ProductGroupMenuItem_Activated;
            aboutMenuItem.Activated += AboutMenuItem_Activated;
            //_button1.Clicked += Button1_Clicked;            
            Maximize();
        }

        private void Window_DeleteEvent(object sender, DeleteEventArgs a)
        {
            Application.Quit();
        }
        private void PurchaseMenuItem_Activated(object sender, EventArgs a)
        {
            ClearMainBox();
            TransactionWindow transWidget = new TransactionWindow(this,1);
            mainBox.PackStart(transWidget, false, false, 5);
            transWidget.ShowAll();

          //  MessageDialog md = new MessageDialog(null, DialogFlags.DestroyWithParent, MessageType.Error, ButtonsType.Close, "Purchase ");
          //  md.Run();
          //  md.Destroy();
        }
        private void ProductMenuItem_Activated(object sender, EventArgs a)
        {
            ClearMainBox();
            ReferenceWindow refWidget = new ReferenceWindow(this,1);
            mainBox.PackStart(refWidget, false, false, 5);
            refWidget.ShowAll();
           // MessageDialog md = new MessageDialog(null, DialogFlags.DestroyWithParent, MessageType.Error, ButtonsType.Close, "Product ");
           // md.Run();
           // md.Destroy();
        }
        private void ProductGroupMenuItem_Activated(object sender, EventArgs a)
        {
            ClearMainBox();
            //TreeModelDemo refWidget = new TreeModelDemo();
           // mainBox.PackStart(refWidget, false, false, 5);
          //  refWidget.ShowAll();
           // MessageDialog md = new MessageDialog(null, DialogFlags.DestroyWithParent, MessageType.Error, ButtonsType.Close, "Product ");
           // md.Run();
           // md.Destroy();
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
}
