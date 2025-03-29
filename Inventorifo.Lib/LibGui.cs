using System;
using Npgsql;
using System.Data;
using Gtk;
using Gdk;
using Pango;


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
        public void SetDisableAllColumn(Gtk.TreeView tree){
            for (int a=0;  a<tree.NColumns; a++)
            {
                Console.WriteLine(a);
                CellRendererText retrievedRenderer = GetCellRendererText(tree, a);
                retrievedRenderer.Editable = false;
                retrievedRenderer.Foreground = "black";                
            }
        }
        public void SetEnableColumn(Gtk.TreeView tree, List<int> cols){
            for (int a=0;  a<cols.Count; a++)
            {
                Console.WriteLine(cols[a]);
                CellRendererText retrievedRenderer = GetCellRendererText(tree, cols[a]);
                retrievedRenderer.Editable = true;
                retrievedRenderer.Foreground = "green";                
            }
        }
        public CellRendererText GetCellRendererText(TreeView treeView, int columnIndex)
        {
            if (columnIndex < treeView.Columns.Length)
            {
                TreeViewColumn column = treeView.Columns[columnIndex];
                foreach (var renderer in column.Cells)
                {
                    if (renderer is CellRendererText textRenderer)
                    {
                        return textRenderer;
                    }
                }
            }
            return null; // Return null if not found
        }
    }
}