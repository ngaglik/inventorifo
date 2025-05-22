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
        LibDb DbCl = new LibDb ();

<<<<<<< HEAD
        public void SetActiveComboBoxText(Gtk.ComboBoxText sender, string pattern){
            var store = (ListStore)sender.Model;
                    int index = 0;
                    foreach (object[] row in store)
                    {
                        if (pattern == row[0].ToString())
                        {
                            sender.Active = index;
                            break;
                        }
                        index++;
                    }
        }

=======
        public void SetActiveComboBoxText(ComboBoxText sender, string pattern){
            var store = (ListStore) sender.Model;
            int index = 0;
            foreach (object[] row in store)
            {
                if (pattern == row[0].ToString())
                {
                    sender.Active = index;
                    break;
                }
                index++;
            }
        }
>>>>>>> 30a991b6c5ef6bf685a8d2776ddbcb7712b11bd0
        public void FillComboBoxText(Gtk.ComboBoxText sender, string sql, int selectedId)
        {
            Gtk.Application.Invoke(delegate
            {
                Gtk.ListStore ls = new ListStore(typeof(string), typeof(string));
                Console.WriteLine(sql);
                DataTable dt = DbCl.fillDataTable(DbCl.getConn(), sql);
                foreach (DataRow dr in dt.Rows)
                {
                    ls.AppendValues(dr[0].ToString(), dr[1].ToString());
                }
                sender.Clear();
                Gtk.CellRendererText text = new Gtk.CellRendererText();
                sender.Model = ls;
                sender.PackStart(text, false);
                sender.AddAttribute(text, "text", 1);
                sender.Active = selectedId;
            });
        }

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