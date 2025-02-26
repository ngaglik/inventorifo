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
            mainBox = (Box)builder.GetObject("MainBox");

            DeleteEvent += Window_DeleteEvent;
            purchaseMenuItem.Activated += PurchaseMenuItem_Activated;
            productMenuItem.Activated += ProductMenuItem_Activated;
            //_button1.Clicked += Button1_Clicked;            
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
        private void ClearMainBox(){
            foreach (Widget child in mainBox.Children)
            {
                mainBox.Remove(child);
                child.Destroy(); // Properly free memory
            }
        }
    }
}
