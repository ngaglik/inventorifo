using System;
using Gtk;
using UI = Gtk.Builder.ObjectAttribute;

namespace Inventorifo.App
{
    class ReferenceWindow : Gtk.Bin
    {
        //[UI] private Label _label1 = null;
        //[UI] private Button _button1 = null;

        private int _counter;

        public ReferenceWindow(MainWindow parent,int jnstrans) : this(new Builder("ReferenceWindow.glade")) { }

        private ReferenceWindow(Builder builder) : base(builder.GetRawOwnedObject("ReferenceWindow"))
        {
            builder.Autoconnect(this);

           // DeleteEvent += Window_DeleteEvent;
            //_button1.Clicked += Button1_Clicked;
           // ShowAll();
        }

        

        private void Button1_Clicked(object sender, EventArgs a)
        {
            _counter++;
           // _label1.Text = "Hello World! This button has been clicked " + _counter + " time(s).";
        }
    }
}
