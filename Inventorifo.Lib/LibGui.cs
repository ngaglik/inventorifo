using System;
using Npgsql;
using System.Data;
using Gtk;
using Gdk;


namespace Inventorifo.Lib
{
    public class LibGui
    {
        public void RemoveAllWidgets(Gtk.Popover container)
        {
            foreach (Widget child in container.Children)
            {
                container.Remove(child);
                child.Destroy(); // Ensure the widget is completely removed
            }
        }
        public void SensitiveAllWidgets(Gtk.Box box, Boolean prm)
        {
            foreach (var child in box.Children)
            {
                child.Sensitive = prm;
            }
        }
    }
}