using System;
using Gtk;
using UI = Gtk.Builder.ObjectAttribute;

namespace Inventorifo.App
{
    class ReferenceWindow : Gtk.Bin
    {
        Inventorifo.Lib.LibDb DbCl = new Inventorifo.Lib.LibDb();

        //[UI] private Label _label1 = null;
        //[UI] private Button _button1 = null;

        private int _counter;
        private TreeView treeViewData;

        public ReferenceWindow(MainWindow parent,int jnstrans) : this(new Builder("ReferenceWindow.glade")) { }

        private ReferenceWindow(Builder builder) : base(builder.GetRawOwnedObject("ReferenceWindow"))
        {
            builder.Autoconnect(this);
            TreeView treeViewData = (TreeView)builder.GetObject("TreeViewData");
           // DeleteEvent += Window_DeleteEvent;
            //_button1.Clicked += Button1_Clicked;
           // ShowAll();
        }

        public void populateTree(string strfind, string barcode)
        {
            BtnBaru.Sensitive = true;
            Gtk.Application.Invoke(delegate
            {
                stoks = new ArrayList();
                if (strfind.Length > 0) {
                        strfind = "and (upper(item.nama) like upper('" + strfind + "%') or upper(item.kode) like upper('" + whrnama + "%')) " ;
                }else {
                        strfind = "";
                }
                    
                string sql ="";
                if(barcode.Length>0){
                        sql = "select item.kode,kategori.id as kategori,item.barcode, item.nama, satuan.id as satuan, stok.qty, stok.harga as harga_beli, harga.harga as harga_jual,stok.exp,harga.min_qty,ks.nama nm_kelstok " +
                            "from produk.item, produk.satuan, produk.stok, produk.harga,produk.kategori, produk.kelstok ks " +
                            "where item.kode= stok.kd_item " +
                            "and item.barcode = '"+barcode+"' " +
                            "and ks.kode=stok.kd_kelstok " +
                            "and item.id_satuan=satuan.id " +
                            "and item.id_kategori=kategori.id " +
                            "and item.kode=harga.kd_item " +
                            "order by stok.tgl_input desc";
                        Console.WriteLine(sql);
                }else{
                    sql = "select item.kode,kategori.id as kategori,item.barcode, item.nama, satuan.id as satuan, stok.qty, stok.harga as harga_beli, harga.harga as harga_jual,stok.exp,harga.min_qty,ks.nama nm_kelstok  " +
                    "from produk.item, produk.satuan, produk.stok, produk.harga,produk.kategori, produk.kelstok ks  " +
                        "where item.kode= stok.kd_item " +
                         "and ks.kode=stok.kd_kelstok " +
                        "" + strfind + " " +
                        "and item.id_satuan=satuan.id " +
                        "and item.id_kategori=kategori.id " +
                        "and item.kode=harga.kd_item " +
                        "order by stok.tgl_input desc";
                    Console.WriteLine(sql);
                }
                dttv = DbCl.fillDataTable(DbCl.getConn(), sql);
                foreach (DataRow dr in dttv.Rows)
                {
                    DateTime d = Convert.ToDateTime(dr[8].ToString());
                    string exp = d.ToString("yyyy-MM-dd");
                        stoks.Add(new Stok(dr[0].ToString(), Convert.ToInt32(dr[1].ToString()),dr[2].ToString(), dr[3].ToString(), Convert.ToInt32(dr[4].ToString()), Convert.ToDouble(dr[5].ToString()), Convert.ToDouble(dr[6].ToString()), Convert.ToDouble(dr[7].ToString()), exp, Convert.ToDouble(dr[9].ToString()), dr[10].ToString() ));
                }
                lstItem = new Gtk.ListStore(typeof(Stok));
                foreach (Stok sto in stoks)
                {
                    lstItem.AppendValues(sto);
                }
                treeViewData.Model = lstItem;
            });
        }

        private void Button1_Clicked(object sender, EventArgs a)
        {
            _counter++;
           // _label1.Text = "Hello World! This button has been clicked " + _counter + " time(s).";
        }
    }
}
