using System;
using System.Collections.Generic;
using Gtk;
using System.Data;
using Pango;

namespace Inventorifo.App
{
    //[Section(ContentType = typeof(EditableCellsSection), Category = Category.Widgets)]
    class InstallmentsPaid : Gtk.Box
    {
        Inventorifo.Lib.LibDb DbCl = new Inventorifo.Lib.LibDb ();
        private TreeView _treeView;
        private ListStore _itemsModel;
        //private ListStore numbers_model;
        private Dictionary<CellRenderer, int> _cellColumnsRender;
        private List<clPayment> _clsPayment;

        public object parent;
        public string prm;

        public InstallmentsPaid(object parent, string prm) : base(Orientation.Vertical, 3)
        {
            this.parent=parent;
            this.prm = prm;

            Label lbTitle = new Label();
            lbTitle.Text = "Installments Paid";
            lbTitle.ModifyFont(FontDescription.FromString("Arial 18"));
            this.PackStart(lbTitle, false, true, 0);
            
            _cellColumnsRender = new Dictionary<CellRenderer, int>();

            ScrolledWindow sw = new ScrolledWindow
            {
                ShadowType = ShadowType.EtchedIn
            };
            sw.SetPolicy(PolicyType.Automatic, PolicyType.Automatic);

            this.PackStart(sw, true, true, 0);

            /* create tree view */
            _treeView = new TreeView();
            _treeView.Selection.Mode = SelectionMode.Single;

            AddColumns();
            //_treeView.Columns[4].Visible = false;

            CreateItemsModel(prm);
            sw.Add(_treeView);

            /* some buttons */
            Box hbox = new Box(Orientation.Horizontal, 4)
            {
                Homogeneous = true
            };
            this.PackStart(hbox, false, false, 0);

        }
      
        private enum ColumnItem
        { //
            id,
            transaction_id,
            payment_date,
            payment_amount,
            user_id,
            user_name,
            Num
        };

        private enum ColumnNumber
        {
            Text,
            Num
        };
        
        private void CreateItemsModel(string transaction_id)
        {      
            string whrid = "";
            if(transaction_id!="") whrid = "and py.transaction_id="+transaction_id+ " ";

                //ListStore model;
                _itemsModel = null;
                TreeIter iter;
                /* create array */
                _clsPayment = new List<clPayment>();

                string sql = "SELECT py.id,py.transaction_id,TO_CHAR(py.payment_date,'yyyy-mm-dd') payment_date, py.amount payment_amount,usr.id user_id,usr.name user_name "+
                        "FROM payment py,(select usr.id, pers.name, pers.phone_number from person pers,userlogin usr where usr.person_id=pers.id) usr "+
                        "WHERE usr.id=py.user_id "+ whrid +
                        "ORDER by py.id desc";
                        Console.WriteLine(sql);
                clPayment pay;
                DataTable dttv = DbCl.fillDataTable(DbCl.getConn(), sql);
                foreach (DataRow dr in dttv.Rows)
                {                    
                    pay = new clPayment{    
                        id=dr["id"].ToString(),
                        transaction_id=dr["transaction_id"].ToString(),
                        payment_date=dr["payment_date"].ToString(),
                        payment_amount=dr["payment_amount"].ToString(),
                        user_id=dr["user_id"].ToString(),
                        user_name=dr["user_name"].ToString()
                    };
                                    
                    _clsPayment.Add(pay);
                }

                /* create list store */
                //
                _itemsModel = new ListStore(typeof(string), typeof(string),typeof(string), typeof(string),typeof(string), typeof(string));

                /* add items */
                for (int i = 0; i < _clsPayment.Count; i++)
                {
                    iter = _itemsModel.Append();
                    _itemsModel.SetValue(iter, (int)ColumnItem.id, _clsPayment[i].id);
                    _itemsModel.SetValue(iter, (int)ColumnItem.transaction_id, _clsPayment[i].transaction_id);
                    _itemsModel.SetValue(iter, (int)ColumnItem.payment_date, _clsPayment[i].payment_date);
                    _itemsModel.SetValue(iter, (int)ColumnItem.payment_amount, _clsPayment[i].payment_amount);
                    _itemsModel.SetValue(iter, (int)ColumnItem.user_id, _clsPayment[i].user_id);
                    _itemsModel.SetValue(iter, (int)ColumnItem.user_name, _clsPayment[i].user_name);
                }
                _treeView.Model = _itemsModel;  
            
        }

        private void AddColumns()
        {
            CellRendererText rendererText = new CellRendererText();
            _cellColumnsRender.Add(rendererText, (int)ColumnItem.id);
            _treeView.InsertColumn(-1, "ID", rendererText, "text", (int)ColumnItem.id);

            rendererText = new CellRendererText();
            _cellColumnsRender.Add(rendererText, (int)ColumnItem.payment_date);
            _treeView.InsertColumn(-1, "Date", rendererText, "text", (int)ColumnItem.payment_date);            

            rendererText = new CellRendererText();
            _cellColumnsRender.Add(rendererText, (int)ColumnItem.payment_amount);
            _treeView.InsertColumn(-1, "Amount", rendererText, "text", (int)ColumnItem.payment_amount);

            rendererText = new CellRendererText();
            _cellColumnsRender.Add(rendererText, (int)ColumnItem.user_name);
            _treeView.InsertColumn(-1, "User", rendererText, "text", (int)ColumnItem.user_name);
        }        
        
        private bool SeparatorRow(ITreeModel model, TreeIter iter)
        {
            TreePath path = model.GetPath(iter);
            int idx = path.Indices[0];

            return idx == 5;
        }
    }
}