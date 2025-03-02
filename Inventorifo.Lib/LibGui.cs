using System;
using Npgsql;
using System.Data;
using Gtk;
using Gdk;


namespace Inventorifo.Lib
{
    public class LibGui
    {
        public void RemoveAllWidgets(Container container)
        {
            foreach (Widget child in container.Children)
            {
                container.Remove(child);
                child.Destroy(); // Ensure the widget is completely removed
            }
        }
    }
}