using System;
using Npgsql;
using System.Data;
using Gtk;
using Gdk;
using Pango;
using Inventorifo.Lib.Model;
using BCrypt.Net;

namespace Inventorifo.Lib
{
    public class LibCore
    {
        LibDb DbCl = new LibDb ();
        public LibCore(){

        }
        public void InsertJournal(int idJurnal, double totalFinalPrice, double totalTax, string reference_id, string description, string user_id,  string application_id){
            string[] arrTax = { "210000", "700000", "701000" };
            string sql = "select journal_account from account_journal_template where id= "+idJurnal.ToString();
            DataTable dt = DbCl.fillDataTable(DbCl.getConn(), sql);
            foreach(DataRow dr in dt.Rows){
                string[] strAcc = dr["journal_account"].ToString().Split("><");
                string strD = strAcc[0].ToString();
                bool containsPlusD = strD.Contains("+");
                if(containsPlusD){
                    string[] arrAccD = strD.Split("+");
                    foreach (string accD in arrAccD)
                    {
                        if(arrTax.Contains(accD)){
                            Console.WriteLine("D:"+accD +" = " +totalTax.ToString());
                            sql = "insert into journal (transaction_date, account_id, description, reference_id, debet_amount, credit_amount, user_id, application_id) values "+
                            "(CURRENT_TIMESTAMP, '"+accD+"', '"+description+"', '"+reference_id+"', '"+totalTax.ToString()+"',0, '"+user_id+"', '"+application_id+"') ";
                            Console.WriteLine(sql);
                            DbCl.ExecuteTrans(DbCl.getConn(), sql);
                        }else{
                            Console.WriteLine("D:"+accD +" = " +totalFinalPrice.ToString());
                            sql = "insert into journal (transaction_date, account_id, description, reference_id, debet_amount, credit_amount, user_id, application_id) values "+
                            "(CURRENT_TIMESTAMP, '"+accD+"', '"+description+"', '"+reference_id+"', '"+totalFinalPrice.ToString()+"',0, '"+user_id+"', '"+application_id+"') ";
                            Console.WriteLine(sql);
                            DbCl.ExecuteTrans(DbCl.getConn(), sql);
                        }                        
                    }
                }else{
                    Console.WriteLine("D:"+strD +" = " + (totalFinalPrice+totalTax).ToString() );
                    sql = "insert into journal (transaction_date, account_id, description, reference_id, debet_amount, credit_amount, user_id, application_id) values "+
                            "(CURRENT_TIMESTAMP, '"+strD+"', '"+description+"', '"+reference_id+"', '"+(totalFinalPrice+totalTax).ToString()+"',0, '"+user_id+"', '"+application_id+"') ";
                            Console.WriteLine(sql);
                            DbCl.ExecuteTrans(DbCl.getConn(), sql);
                }

                string strC = strAcc[1].ToString();
                bool containsPlusC = strC.Contains("+");
                if(containsPlusC){
                    string[] arrAccC = strC.Split("+");
                    foreach (string accC in arrAccC)
                    {
                        if(arrTax.Contains(accC)){
                            Console.WriteLine("C:"+accC +" = " +totalTax.ToString());
                            sql = "insert into journal (transaction_date, account_id, description, reference_id, debet_amount, credit_amount, user_id, application_id) values "+
                            "(CURRENT_TIMESTAMP, '"+accC+"', '"+description+"', '"+reference_id+"',0, '"+totalTax.ToString()+"', '"+user_id+"', '"+application_id+"') ";
                            Console.WriteLine(sql);
                            DbCl.ExecuteTrans(DbCl.getConn(), sql);
                        }else{
                            Console.WriteLine("C:"+accC +" = " +totalFinalPrice.ToString());
                            sql = "insert into journal (transaction_date, account_id, description, reference_id, debet_amount, credit_amount, user_id, application_id) values "+
                            "(CURRENT_TIMESTAMP, '"+accC+"', '"+description+"', '"+reference_id+"',0, '"+totalFinalPrice.ToString()+"', '"+user_id+"', '"+application_id+"') ";
                            Console.WriteLine(sql);
                            DbCl.ExecuteTrans(DbCl.getConn(), sql);
                        }
                        
                    }
                }else{
                    Console.WriteLine("C:"+strC +" = " + (totalFinalPrice+totalTax).ToString() );
                    sql = "insert into journal (transaction_date, account_id, description, reference_id, debet_amount, credit_amount, user_id, application_id) values "+
                            "(CURRENT_TIMESTAMP, '"+strC+"', '"+description+"', '"+reference_id+"',0, '"+(totalFinalPrice+totalTax).ToString()+"', '"+user_id+"', '"+application_id+"') ";
                            Console.WriteLine(sql);
                            DbCl.ExecuteTrans(DbCl.getConn(), sql);
                }

                
            }
        }
        public double GetOutstandingBalance(string transaction_amount, string payment_amount){
            double total = 0;
            total = Convert.ToDouble(transaction_amount)-Convert.ToDouble(payment_amount);
            if(total<0) total = 0;
            return total;
        }
        public double CalculateSubtotal(string quantity, string final_price){
            double subtotal = 0;
            subtotal = Convert.ToDouble(quantity)*Convert.ToDouble(final_price);
            return subtotal;
        }
        public double CalculateFinalPrice(string item_price, string main_discount, string additional_discount, string deduction_amount){
            double final_price = 0;
            double ditem_price = Convert.ToDouble(item_price);
            double imain_discount = Convert.ToDouble(main_discount)/100;
            double iadditional_discount = Convert.ToDouble(additional_discount)/100;
            double ddeduction_amount = Convert.ToDouble(deduction_amount);            
            double discounted_main = ditem_price-(imain_discount*ditem_price);
            double discounted_additional = discounted_main-(iadditional_discount*discounted_main);
            final_price = discounted_additional-ddeduction_amount;
            return final_price;
        } 
        public void InsertStockHistory(string transaction_id,string transaction_type, string product_id, string stock_id, string location, string condition, string quantity ){
            double quantity_before = getQuantity(product_id,location,condition);
            double quantity_after = quantity_before + Convert.ToDouble(quantity);
            string sql = "insert into stock_history (transaction_id, input_date, transaction_type, product_id, stock_id, location, condition, quantity_before, quantity, quantity_after) "+
            "values ("+transaction_id+",CURRENT_TIMESTAMP, "+transaction_type+", "+product_id+", "+stock_id+", "+location+","+condition+", "+quantity_before.ToString()+","+quantity+", "+quantity_after.ToString()+") ";
            Console.WriteLine (sql);
            DbCl.ExecuteTrans(DbCl.getConn(), sql);
        }
        public double getQuantity(string product_id, string location, string condition){
            double qty = 0;
            string sql = "select sum(quantity) qty from stock where state=0 and quantity>0 and product_id='"+product_id+"' and location='"+location+"' and condition = '"+condition+"' group by product_id ";
            Console.WriteLine (sql);
            DataTable dt = DbCl.fillDataTable(DbCl.getConn(), sql);
            foreach(DataRow dr in dt.Rows){
                qty = Convert.ToDouble(dr["qty"].ToString());
            }
            return qty;
        }
        public DataTable fillDtPerson(string strfind)
        {
            string whrfind = "";
            if(strfind!="") whrfind = "and upper(pers.name) like upper('" + strfind + "%')  ";

            string sql = "SELECT pers.id , pers.name, pers.address,  pers.phone_number,  pers.is_active, pers.national_id_number, pers.tax_id_number,pers.health_insurance_id_number "+
                    "FROM person pers "+
                    "WHERE 1=1 "+ whrfind +
                    "ORDER by pers.name asc";
                    Console.WriteLine(sql);          
            return DbCl.fillDataTable(DbCl.getConn(), sql);
        }
        public DataTable fillDtCustomer(clsCustomer filter)
        {
            string whrfind = "",whrid="";
            DataTable dtout = new DataTable();
            if(filter.id is null || filter.id == ""){                    
                if(filter.person_name.Length>=2){
                     whrfind = "and (upper(pers.name) like upper('" + filter.person_name + "%')   )";
                }           
            }else{
                whrid = " and cust.id= "+filter.id + " ";
            }
            string sql = "SELECT cust.id ,pers.id person_id, pers.name person_name, pers.address person_address,  pers.phone_number person_phone_number, cust.customer_group , custgr.name customer_group_name, cust.is_active, cust.organization_name, cust.organization_address, cust.organization_phone_number, cust.organization_tax_id_number, "+
                        "pricerl.price, pricerl.tax tax_group_id,taxgr.name tax_group_name, pricerl.discount discount_group_id, discgr.name discount_group_name, pricerl.payment payment_group_id "+
                        "FROM customer cust left outer join person pers on cust.person_id=pers.id, customer_group custgr "+
                        "LEFT OUTER JOIN pricing_role pricerl on pricerl.customer_group=custgr.id " +
                        "LEFT OUTER JOIN tax_group taxgr on pricerl.tax=taxgr.id " +
                        "LEFT OUTER JOIN discount_group discgr on pricerl.discount=discgr.id " +
                        "WHERE cust.customer_group=custgr.id "+ whrfind +whrid+
                        "ORDER by pers.name asc";
                        Console.WriteLine(sql);
              
            dtout = DbCl.fillDataTable(DbCl.getConn(), sql);
            return dtout;
        }
        public DataTable fillDtOrganization(string strfind)
        {
            string whrfind = "";
            if(strfind!="") whrfind = "and upper(org.name) like upper('" + strfind + "%')  ";

            string sql = "SELECT org.id , org.name, org.address,  org.phone_number,  org.is_active, org.tax_id_number "+
                    "FROM organization org "+
                    "WHERE 1=1 "+ whrfind +
                    "ORDER by org.name asc";
                    Console.WriteLine(sql);          
            return DbCl.fillDataTable(DbCl.getConn(), sql);
        }
        public DataTable fillDtProduct(int transaction_type, clStock filter){
            Console.WriteLine("fillDtProduct");
            DataTable dtout = new DataTable();
                string whrlocgroup="", whractive="", whrlocation="", whrcondition="", whrid="", whrbarcode="", whrfind = "", whrtype="";
                Console.WriteLine("1 "+filter.is_active);
                if(filter.is_active is null || filter.is_active=="") {
                }else{
                    if(filter.is_active=="0") whractive = "and prod.is_active = true ";
                    if(filter.is_active=="01") whractive = "and prod.is_active = false ";
                    if(filter.is_active=="10") whractive = "";
                }
                Console.WriteLine("2 filter.barcode "+filter.barcode);
                if(filter.barcode is null || filter.barcode=="") {
                }else{
                    whrbarcode = "and prod.barcode =  '" + filter.barcode + "' ";
                }
                Console.WriteLine("3 filter.location "+filter.location);
                if(filter.location is null || filter.location=="") {
                }else{
                    whrlocation = "and stock.location =  '" + filter.location + "' ";
                }
                Console.WriteLine("4 filter.condition "+filter.condition);
                if(filter.condition is null || filter.condition=="") {
                }else{
                    whrcondition = "and stock.condition =  '" + filter.condition + "' ";
                }
                Console.WriteLine("transaction_type "+transaction_type.ToString());
                if(transaction_type==21 || transaction_type==22 ) { // transfer internal || trasnfer out
                    whrlocgroup = "and (case when global_quantity is null then 0 else global_quantity end) >0 ";
                }else if(transaction_type==2 ) {
                    whrlocgroup = "and locgr.id=2 ";
                    whrtype = "and (case when global_quantity is null then 0 else global_quantity end) >0 "; 
                }
                Console.WriteLine("5 "+filter.product_id);
                if(filter.product_id is null || filter.product_id == ""){
                    if(filter.short_name is null || filter.short_name==""){
                        dtout = new DataTable();
                    }else{
                        if(filter.short_name.Length>=2){
                            whrfind = "and (upper(prod.name) like upper('%" + filter.short_name + "%') or upper(prod.short_name) like upper('" + filter.short_name + "%')) ";
                        }else{
                            dtout = new DataTable();
                        }
                    }               
                }else{
                    whrid = " and prod.id= "+filter.product_id + " ";
                }
                Console.WriteLine("6 "+filter.product_id);
                string sql = "SELECT prod.id, prod.short_name, prod.name product_name, prod.barcode, prod.price1, prod.price2, prod.price3, prod.last_purchase_price, "+
                "case when global_quantity is null then 0 else global_quantity end global_quantity,  "+
                "prod.product_group, prodgr.name product_group_name "+
                "FROM product_group prodgr, product prod "+
                "left outer join (select sum(quantity) global_quantity,product_id from stock where state=0 "+whrlocation + whrcondition +" group by product_id) prglobal  on prglobal.product_id=prod.id "+
                "WHERE prod.product_group = prodgr.id "+ whrfind + whrbarcode + whrtype +whrid+whractive+ 
                " ORDER by prod.id desc";
                Console.WriteLine(sql);
                dtout = DbCl.fillDataTable(DbCl.getConn(), sql);
                return dtout;
        }
        public DataTable fillDtProductByLocation(int transaction_type, clStock filter){
            Console.WriteLine("fillDtProduct");
            DataTable dtout = new DataTable();
                string whrlocgroup="", whractive="", whrlocation="", whrcondition="", whrid="", whrbarcode="", whrfind = "", whrtype="";
                Console.WriteLine("1 "+filter.is_active);
                if(filter.is_active is null || filter.is_active=="") {
                }else{
                    if(filter.is_active=="0") whractive = "and prod.is_active = true ";
                    if(filter.is_active=="01") whractive = "and prod.is_active = false ";
                    if(filter.is_active=="10") whractive = "";
                }
                Console.WriteLine("2 "+filter.barcode);
                if(filter.barcode is null || filter.barcode=="") {
                }else{
                    whrbarcode = "and prod.barcode =  '" + filter.barcode + "' ";
                }
                Console.WriteLine("3 "+filter.location);
                if(filter.location is null || filter.location=="") {
                }else{
                    whrlocation = "and stock.location =  '" + filter.location + "' ";
                }
                Console.WriteLine("4 "+filter.condition);
                if(filter.condition is null || filter.condition=="") {
                }else{
                    whrcondition = "and stock.condition =  '" + filter.condition + "' ";
                }
                Console.WriteLine("transaction_type "+transaction_type.ToString());
                if(transaction_type==20){                    
                   // whrtype = " and store_quantity>0 ";                    
                }else if(transaction_type==21){                    
                   // whrtype = " and store_quantity>0 ";                    
                }else if(transaction_type==22){                    
                   // whrtype = " and store_quantity>0 ";                    
                }else if(transaction_type==2 ) {
                        whrlocgroup = "and locgr.id=2 ";
                        whrtype = "and store_quantity>0 "; 
                }
                Console.WriteLine("5 "+filter.product_id);
                if(filter.product_id is null || filter.product_id == ""){
                    if(filter.short_name is null || filter.short_name==""){
                        dtout = new DataTable();
                    }else{
                        if(filter.short_name.Length>=2){
                            whrfind = "and (upper(prod.name) like upper('%" + filter.short_name + "%') or upper(prod.short_name) like upper('" + filter.short_name + "%')) ";
                        }else{
                            dtout = new DataTable();
                        }
                    }               
                }else{
                    whrid = " and prod.id= "+filter.product_id + " ";
                }
                Console.WriteLine("6 "+filter.product_id);
                string sql = "SELECT prod.id, prod.short_name, prod.name product_name, prod.barcode, prod.price1, prod.price2, prod.price3, prod.last_purchase_price, "+
                "case when store_quantity is null then 0 else store_quantity end store_quantity, "+
                "case when global_quantity is null then 0 else global_quantity end global_quantity, "+
                "prod.product_group, prodgr.name product_group_name "+
                "FROM product_group prodgr, product prod "+
                "left outer join (select sum(quantity) store_quantity,product_id,location_group from stock,location loc,location_group locgr where state=0 "+whrcondition+whrlocation+" and stock.location=loc.id and loc.location_group=locgr.id "+whrlocgroup+" group by product_id,loc.id) prstore on prstore.product_id=prod.id "+
                "left outer join (select sum(quantity) global_quantity,product_id from stock where state=0 group by product_id) prglobal  on prglobal.product_id=prod.id "+
                "WHERE prod.product_group = prodgr.id "+ whrfind + whrbarcode + whrtype +whrid+whractive+
                "ORDER by prod.name asc";               
                Console.WriteLine(sql);
                dtout = DbCl.fillDataTable(DbCl.getConn(), sql);
                return dtout;
        }
        public DataTable fillDtTransactionPurchase(clTransaction filterTrans){
            Console.WriteLine("fillDtTransactionPurchase");            
            string whrid = "",whrtype="",whrdate="",whrfind="";
            if(filterTrans.id is null || filterTrans.id=="") {
            }else{
                whrid = "and tr.id = '"+filterTrans.id+"' ";
            }
            if(filterTrans.person_name is null || filterTrans.person_name=="") {
            }else{
                if(filterTrans.person_name.Length>=2){
                    whrfind = "and ( upper(pers.name) like upper('" + filterTrans.person_name + "%') or pers.phone_number ='" + filterTrans.person_name + "' ) ";
                }
            }
            if(filterTrans.transaction_type_id is null || filterTrans.transaction_type_id=="") {
            }else{
                whrtype = "and tr.transaction_type = '"+filterTrans.transaction_type_id+"' ";
            }
            if(filterTrans.transaction_date is null || filterTrans.transaction_date=="") {
            }else{
                whrdate = "and tr.transaction_date = '"+filterTrans.transaction_date+"' ";
            }

            string sql = "select tr.id,tr.reference_id, TO_CHAR(tr.reference_date, 'yyyy-mm-dd') reference_date,"+
            "tr.supplier_id, org.name organization_name, org.address organization_address, org.phone_number organization_phone_number, pers.name person_name,pers.phone_number person_phone_number, "+
            "tr.transaction_type transaction_type_id, ty.name transaction_type_name, TO_CHAR(tr.transaction_date,'yyyy-mm-dd') transaction_date, tr.transaction_amount, tr.return_amount, "+
            "tr.payment_group_id, py.name payment_group_name, tr.payment_amount, "+
            "tr.user_id, usr.name user_name,  "+
            "tr.state, st.name state_name, st.fgcolor state_fgcolor, st.bgcolor state_bgcolor,  "+
            "tr.application_id, tr.tax_amount, tr.is_tax "+
            "from transaction tr left outer join supplier sup on tr.supplier_id=sup.id "+
            "left outer join organization org on org.id=sup.organization_id "+
            "left outer join payment_group py on tr.payment_group_id = py.id "+
            "left outer join person pers on sup.person_id=pers.id, "+
            "transaction_state st, transaction_type ty, "+
            "(select usr.id, pers.name, pers.phone_number from person pers,userlogin usr where usr.person_id=pers.id) usr "+
            "where tr.transaction_type=1 and tr.state=st.id and tr.transaction_type=ty.id "+
            "and usr.id=tr.user_id "+
            whrid+ whrfind+ whrdate+
            "ORDER by tr.id desc";
            Console.WriteLine(sql);
            return DbCl.fillDataTable(DbCl.getConn(), sql);
        }
        public DataTable fillDtTransactionSale(clTransaction filterTrans){
            Console.WriteLine("fillDtTransactionSale");
            string whrid = "",whrtype="",whrdate="",whrfind="";
            if(filterTrans.id is null || filterTrans.id=="") {
            }else{
                whrid = "and tr.id = '"+filterTrans.id+"' ";
            }
            if(filterTrans.person_name is null || filterTrans.person_name=="") {
            }else{
                if(filterTrans.person_name.Length>=2){
                    whrfind = "and ( upper(pers.name) like upper('" + filterTrans.person_name + "%') or pers.phone_number ='" + filterTrans.person_name + "' ) ";
                }
            }
            if(filterTrans.transaction_type_id is null || filterTrans.transaction_type_id=="") {
            }else{
                whrtype = "and tr.transaction_type = '"+filterTrans.transaction_type_id+"' ";
            }
            if(filterTrans.transaction_date is null || filterTrans.transaction_date=="") {
            }else{
                whrdate = "and tr.transaction_date = '"+filterTrans.transaction_date+"' ";
            }
            string sql = "select tr.id,tr.reference_id, "+
                "tr.customer_id, cus.organization_name,cus.organization_address,cus.organization_phone_number,pers.name person_name,pers.phone_number person_phone_number, "+
                "tr.transaction_type transaction_type_id, ty.name transaction_type_name, TO_CHAR(tr.transaction_date,'yyyy-mm-dd') transaction_date, tr.transaction_amount, tr.return_amount, "+
                "tr.payment_group_id, py.name payment_group_name, tr.payment_amount, "+
                "tr.user_id, usr.name user_name,  "+
                "tr.state, st.name state_name, st.fgcolor state_fgcolor, st.bgcolor state_bgcolor,  "+
                "tr.application_id, tr.tax_amount, tr.is_tax "+
                "from transaction tr left outer join customer cus on tr.customer_id=cus.id "+
                "left outer join payment_group py on tr.payment_group_id = py.id "+
                "left outer join person pers on cus.person_id=pers.id, "+
                "transaction_state st, transaction_type ty, "+
                "(select usr.id, pers.name, pers.phone_number from person pers,userlogin usr where usr.person_id=pers.id) usr "+
                "where tr.transaction_type=2 and tr.state=st.id and tr.transaction_type=ty.id "+
                "and usr.id=tr.user_id "+
                whrid+ whrfind+ whrdate+
                "ORDER by tr.id desc";
                Console.WriteLine(sql);
                return DbCl.fillDataTable(DbCl.getConn(), sql);
        }
        public DataTable fillDtOrderTransactionItem(clTransaction filterTrans, clTransactionItem1 filterItem){
            Console.WriteLine("fillDtOrderTransactionItem");
            string whrfind = "",whrid="",whritemid="", whrdate="",whrtype="";
            ;
            if(filterTrans.id is null || filterTrans.id=="") {
            }else{
                whrid = "and tr.id = '"+filterTrans.id+"' ";
            }
            if(filterTrans.transaction_type_id is null || filterTrans.transaction_type_id=="") {
            }else{
                whrtype = "and tr.transaction_type = '"+filterTrans.transaction_type_id+"' ";
            }
            if(filterTrans.transaction_date is null || filterTrans.transaction_date=="") {
            }else{
                whrdate = "and tr.transaction_date = '"+filterTrans.transaction_date+"' ";
            }
            if(filterItem.id is null || filterItem.id=="") {
            }else{
                whritemid = "and ti.id = '"+filterItem.id+"' ";
            }
            if(filterItem.product_short_name is null || filterItem.product_short_name==""){
                //return new DataTable();
            }else{
                if(filterItem.product_short_name.Length>=2){
                    whrfind = "and (upper(pr.name) like upper('" + filterItem.product_name + "%') or upper(pr.short_name) like upper('" + filterItem.product_short_name + "%')) ";
                }
            }  
            string whrstate = "";
            //if(state!="") whrstate = "and ti.state="+state +" ";
            
             string sql = "select ti.id, ti.transaction_id, ti.product_id, pr.short_name product_short_name, pr.name product_name,ti.stock_id, "+
            "case when ti.tax is null then 0 else ti.tax end tax, ti.state, state.name state_name, "+
            "ti.quantity, st.unit,un.name unit_name, "+
            "pp.id purchase_price_id, pp.item_price purchase_item_price,pp.main_discount purchase_main_discount, pp.additional_discount purchase_additional_discount, pp.deduction_amount purchase_deduction_amount, pp.final_price purchase_final_price, 0 purchase_subtotal, pp.tax purchase_tax, "+
            "st.location, lo.name location_name, st.condition, co.name condition_name, "+
            "ti.item_price, ti.main_discount, COALESCE(ti.additional_discount, 0) additional_discount, COALESCE(ti.deduction_amount, 0)  deduction_amount, ROUND(get_final_price(pr.price1::numeric,ti.main_discount::numeric,ti.additional_discount::numeric, ti.deduction_amount::numeric )) final_price, 0 subtotal, ti.tax "+
            "from transaction tr, transaction_item_state state, transaction_order_item ti, product pr, stock st "+
            "LEFT OUTER JOIN purchase_price pp on pp.id = st.price_id "+
            "left outer join unit un on un.id=st.unit "+
            "left outer join condition co on st.condition=co.id "+
            "left outer join location lo on st.location=lo.id "+
            "where tr.id=ti.transaction_id and ti.product_id=pr.id and ti.stock_id=st.id and state.id=ti.state "+
             whrid+whrstate+
            "ORDER by ti.id desc";
            //tekan kene
            Console.WriteLine(sql);            
            return DbCl.fillDataTable(DbCl.getConn(), sql);
        }
        public DataTable fillDtTransactionItem(clTransaction filterTrans, clTransactionItem1 filterItem){
            Console.WriteLine("fillDtTransactionItem");
            string whrfind = "",whrid="",whritemid="", whrdate="",whrtype="";
            ;
            if(filterTrans.id is null || filterTrans.id=="") {
            }else{
                whrid = "and tr.id = '"+filterTrans.id+"' ";
            }
            if(filterTrans.transaction_type_id is null || filterTrans.transaction_type_id=="") {
            }else{
                whrtype = "and tr.transaction_type = '"+filterTrans.transaction_type_id+"' ";
            }
            if(filterTrans.transaction_date is null || filterTrans.transaction_date=="") {
            }else{
                whrdate = "and tr.transaction_date = '"+filterTrans.transaction_date+"' ";
            }
            if(filterItem.id is null || filterItem.id=="") {
            }else{
                whritemid = "and ti.id = '"+filterItem.id+"' ";
            }
            if(filterItem.product_short_name is null || filterItem.product_short_name==""){
                //return new DataTable();
            }else{
                if(filterItem.product_short_name.Length>=2){
                    whrfind = "and (upper(pr.name) like upper('" + filterItem.product_name + "%') or upper(pr.short_name) like upper('" + filterItem.product_short_name + "%')) ";
                }
            }  
            string whrstate = "";
            //if(state!="") whrstate = "and ti.state="+state +" ";

            
             string sql = "select ti.id, ti.transaction_id, ti.product_id, pr.short_name product_short_name, pr.name product_name,ti.stock_id, "+
            "case when ti.tax is null then 0 else ti.tax end tax, ti.state, state.name state_name, "+
            "ti.quantity, st.unit,un.name unit_name, "+
            "pp.id purchase_price_id, pp.item_price purchase_item_price,pp.main_discount purchase_main_discount, pp.additional_discount purchase_additional_discount, pp.deduction_amount purchase_deduction_amount, pp.final_price purchase_final_price, 0 purchase_subtotal, pp.tax purchase_tax, "+
            "st.location, lo.name location_name, st.condition, co.name condition_name, "+
            "ti.item_price item_price, ti.main_discount, COALESCE(ti.additional_discount, 0) additional_discount, COALESCE(ti.deduction_amount, 0)  deduction_amount, ROUND(get_final_price(pr.price1::numeric,ti.main_discount::numeric,ti.additional_discount::numeric, ti.deduction_amount::numeric )) final_price, 0 subtotal, ti.tax "+
            "from transaction tr, transaction_item_state state, transaction_item ti, product pr, stock st "+
            "LEFT OUTER JOIN purchase_price pp on pp.id = st.price_id "+
            "left outer join unit un on un.id=st.unit "+
            "left outer join condition co on st.condition=co.id "+
            "left outer join location lo on st.location=lo.id "+
            "where tr.id=ti.transaction_id and ti.product_id=pr.id and ti.stock_id=st.id and state.id=ti.state "+
             whrid+whrstate+
            "ORDER by ti.id desc";
            //tekan kene
            Console.WriteLine(sql);            
            return DbCl.fillDataTable(DbCl.getConn(), sql);
        }
        public DataTable fillDtReportTransaction(clTransaction filterTrans, clTransactionItem1 filterItem){
            Console.WriteLine("fillDtReportTransaction");
            string whrfind = "",whrid="",whritemid="", whrdate="",whrtype="";
            ;
            if(filterTrans.id is null || filterTrans.id=="") {
            }else{
                whrid = "and tr.id = '"+filterTrans.id+"' ";
            }
            if(filterTrans.transaction_type_id is null || filterTrans.transaction_type_id=="") {
            }else{
                whrtype = "and tr.transaction_type = '"+filterTrans.transaction_type_id+"' ";
            }
            if(filterTrans.transaction_date is null || filterTrans.transaction_date=="") {
            }else{
                whrdate = "and tr.transaction_date = '"+filterTrans.transaction_date+"' ";
            }
            if(filterItem.id is null || filterItem.id=="") {
            }else{
                whritemid = "and ti.id = '"+filterItem.id+"' ";
            }
            if(filterItem.product_short_name is null || filterItem.product_short_name==""){
                //return new DataTable();
            }else{
                if(filterItem.product_short_name.Length>=2){
                    whrfind = "and (upper(pr.name) like upper('" + filterItem.product_name + "%') or upper(pr.short_name) like upper('" + filterItem.product_short_name + "%')) ";
                }
            }  
            string sql = "select TO_CHAR(tr.input_date, 'yyyy-mm-dd') input_date, ti.id, ti.transaction_id, ti.product_id, pr.short_name product_short_name, pr.name product_name,ti.stock_id, "+
            "case when ti.tax is null then 0 else ti.tax end tax, ti.state, state.name state_name, "+
            "ti.quantity, st.unit,un.name unit_name, "+
            "pp.id purchase_price_id, pp.item_price purchase_item_price,pp.main_discount purchase_main_discount, pp.additional_discount purchase_additional_discount, pp.deduction_amount purchase_deduction_amount, pp.final_price purchase_final_price, 0 purchase_subtotal, pp.tax purchase_tax, "+
            "st.location, lo.name location_name, st.condition, co.name condition_name, TO_CHAR(st.expired_date,'yyyy-mm-dd') expired_date, st.unit "+
            "from transaction tr, transaction_item_state state, transaction_item ti, product pr, stock st "+
            "LEFT OUTER JOIN purchase_price pp on pp.id = st.price_id "+
            "left outer join unit un on un.id=st.unit "+
            "left outer join condition co on st.condition=co.id "+
            "left outer join location lo on st.location=lo.id "+
            "where tr.id=ti.transaction_id and ti.product_id=pr.id and ti.stock_id=st.id and state.id=ti.state "+
            whrfind+ whrdate + whrid+whrtype+whritemid+
            "ORDER by tr.id desc, pr.short_name asc";
            //tekan kene
            Console.WriteLine(sql);            
            return DbCl.fillDataTable(DbCl.getConn(), sql);
        }
        public DataTable fillDtTransactionPurchaseReturn(string transaction_id, string date, string strfind){
            Console.WriteLine("fillDtTransactionPurchaseReturn");
            string whrfind = "",whrid="", whrdate="";
            if(transaction_id!="") whrid = "and tr.id="+transaction_id+ " ";
            if(strfind!="") whrfind = "and (upper(org.name) like upper('" + strfind + "%') or upper(pers.name) like upper('" + strfind + "%') )";
            if(date!="") whrdate = "and tr.transaction_date::date = '"+date+"'::date ";

            string sql = "select tr.id,tr.reference_id, "+
            "tr.supplier_id, org.name organization_name, org.address organization_address, org.phone_number organization_phone_number, pers.name person_name,pers.phone_number person_phone_number, "+
            "tr.transaction_type transaction_type_id, ty.name transaction_type_name, TO_CHAR(tr.transaction_date,'yyyy-mm-dd') transaction_date, tr.transaction_amount, tr.return_amount, "+
            "tr.payment_group_id, py.name payment_group_name, tr.payment_amount, "+
            "tr.user_id, usr.name user_name,  "+
            "tr.state, st.name state_name, st.fgcolor state_fgcolor, st.bgcolor state_bgcolor,  "+
            "tr.application_id, tr.tax_amount, tr.is_tax "+
            "from transaction tr left outer join supplier sup on tr.supplier_id=sup.id "+
            "left outer join organization org on org.id=sup.organization_id "+
            "left outer join payment_group py on tr.payment_group_id = py.id "+
            "left outer join person pers on sup.person_id=pers.id, "+
            "transaction_state st, transaction_type ty, "+
            "(select usr.id, pers.name, pers.phone_number from person pers,userlogin usr where usr.person_id=pers.id) usr "+
            "where tr.transaction_type=3 and tr.state=st.id and tr.transaction_type=ty.id "+
            "and usr.id=tr.user_id "+
            whrid+ whrfind+ whrdate+
            "ORDER by tr.id desc";
            Console.WriteLine(sql);
            return DbCl.fillDataTable(DbCl.getConn(), sql);
        }
        public DataTable fillDtTransactionSaleReturn(string transaction_id, string date, string strfind){
            Console.WriteLine("fillDtTransactionSaleReturn");
            string whrfind = "",whrid="", whrdate="";
            if(transaction_id!="") whrid = "and tr.id="+transaction_id+ " ";
            if(strfind!="") whrfind = "and (upper(cus.organization_name) like upper('" + strfind + "%') or upper(pers.name) like upper('" + strfind + "%') )";
            if(date!="") whrdate = "and tr.transaction_date::date = '"+date+"'::date ";

            string sql = "select tr.id,tr.reference_id, "+
                "tr.customer_id,cus.organization_name, cus.organization_address,cus.organization_phone_number,pers.name person_name,pers.phone_number person_phone_number, "+
                "tr.transaction_type transaction_type_id, ty.name transaction_type_name, TO_CHAR(tr.transaction_date,'yyyy-mm-dd') transaction_date, tr.transaction_amount, tr.return_amount, "+
                "tr.payment_group_id, py.name payment_group_name, tr.payment_amount, "+
                "tr.user_id, usr.name user_name,  "+
                "tr.state, st.name state_name, st.fgcolor state_fgcolor, st.bgcolor state_bgcolor,  "+
                "tr.application_id, tr.tax_amount, tr.is_tax "+
                "from transaction tr left outer join customer cus on tr.customer_id=cus.id "+
                "left outer join payment_group py on tr.payment_group_id = py.id "+
                "left outer join person pers on cus.person_id=pers.id, "+
                "transaction_state st, transaction_type ty, "+
                "(select usr.id, pers.name, pers.phone_number from person pers,userlogin usr where usr.person_id=pers.id) usr "+
                "where tr.transaction_type=4 and tr.state=st.id and tr.transaction_type=ty.id "+
                "and usr.id=tr.user_id "+
                whrid+ whrfind+ whrdate +
                "ORDER by tr.id desc";
                Console.WriteLine(sql);
                return DbCl.fillDataTable(DbCl.getConn(), sql);
        }
        public DataTable fillDtTransfer(string transaction_id, string date, string strfind){
            Console.WriteLine("fillDtTransfer");
            string whrfind = "",whrid="", whrdate="";
            if(transaction_id!="") whrid = "and tr.id="+transaction_id+ " ";
            if(strfind!="") whrfind = "and (upper(org1.name) like upper('" + strfind + "%') or upper(org2.name) like upper('" + strfind + "%') )";
            if(date!="") whrdate = "and tr.transfer_date::date = '"+date+"'::date ";

            string sql = "select tr.id,tr.reference_id, tr.transfer_type_id, ty.name transfer_type_name, TO_CHAR(tr.transfer_date,'yyyy-mm-dd') transfer_date, "+
            "source_organization_id,org1.name source_organization_name, source_location_id,loc1.name source_location_name, source_condition_id, con1.name source_condition_name, "+
            "destination_organization_id,org2.name destination_organization_name ,destination_location_id, loc2.name destination_location_name, destination_condition_id, con2.name destination_condition_name, "+
            "tr.user_id, usr.name user_name,  tr.state, st.name state_name, st.fgcolor state_fgcolor, st.bgcolor state_bgcolor,  tr.application_id "+
            "from  transfer tr "+
            "left join organization org1 on org1.id = tr.source_organization_id "+
            "left join organization org2 on org2.id = tr.destination_organization_id "+
            "left join location loc1 on loc1.id = tr.source_location_id "+
            "left join location loc2 on loc2.id = tr.destination_location_id "+
            "left join condition con1 on con1.id = tr.source_condition_id "+
            "left join condition con2 on con2.id = tr.destination_condition_id, "+
            "transaction_state st, transfer_type ty, (select usr.id, pers.name, pers.phone_number from person pers,userlogin usr where usr.person_id=pers.id) usr "+
            "where tr.state=st.id and tr.transfer_type_id=ty.id and usr.id=tr.user_id "+
                whrid+ whrfind+ whrdate +
                "ORDER by tr.id desc";
                Console.WriteLine(sql);
                return DbCl.fillDataTable(DbCl.getConn(), sql);
        }
        public DataTable fillDtOrderTransferItem(clTransfer filterTrans, clTransactionItem1 filterItem){
            Console.WriteLine("fillDtOrderTransferItem");
            string whrfind = "",whrid="",whritemid="", whrdate="",whrtype="";
            ;
            if(filterTrans.id is null || filterTrans.id=="") {
            }else{
                whrid = "and tr.id = '"+filterTrans.id+"' ";
            }
            if(filterTrans.transaction_type_id is null || filterTrans.transaction_type_id=="") {
            }else{
                whrtype = "and tr.transaction_type = '"+filterTrans.transaction_type_id+"' ";
            }
            if(filterTrans.transaction_date is null || filterTrans.transaction_date=="") {
            }else{
                whrdate = "and tr.transaction_date = '"+filterTrans.transaction_date+"' ";
            }
            if(filterItem.id is null || filterItem.id=="") {
            }else{
                whritemid = "and ti.id = '"+filterItem.id+"' ";
            }
            if(filterItem.product_short_name is null || filterItem.product_short_name==""){
                //return new DataTable();
            }else{
                if(filterItem.product_short_name.Length>=2){
                    whrfind = "and (upper(pr.name) like upper('" + filterItem.product_name + "%') or upper(pr.short_name) like upper('" + filterItem.product_short_name + "%')) ";
                }
            }  
            string whrstate = "";
            //if(state!="") whrstate = "and ti.state="+state +" ";
            
             string sql = "select ti.id, ti.transaction_id, ti.product_id, pr.short_name product_short_name, pr.name product_name,ti.stock_id, "+
            "case when ti.tax is null then 0 else ti.tax end tax, ti.state, state.name state_name, "+
            "ti.quantity, st.unit,un.name unit_name, "+
            "pp.id purchase_price_id, pp.item_price purchase_item_price,pp.main_discount purchase_main_discount, pp.additional_discount purchase_additional_discount, pp.deduction_amount purchase_deduction_amount, pp.final_price purchase_final_price, 0 purchase_subtotal, pp.tax purchase_tax, "+
            "st.location, lo.name location_name, st.condition, co.name condition_name "+
            "from transfer tr, transaction_item_state state, transfer_order_item ti, product pr, stock st "+
            "LEFT OUTER JOIN purchase_price pp on pp.id = st.price_id "+
            "left outer join unit un on un.id=st.unit "+
            "left outer join condition co on st.condition=co.id "+
            "left outer join location lo on st.location=lo.id "+
            "where tr.id=ti.transaction_id and ti.product_id=pr.id and ti.stock_id=st.id and state.id=ti.state "+
             whrid+whrstate+
            "ORDER by ti.id desc";
            //tekan kene
            Console.WriteLine(sql);            
            return DbCl.fillDataTable(DbCl.getConn(), sql);
        }
        public DataTable fillDtTransferItem(clTransfer filterTrans, clTransactionItem1 filterItem){
           Console.WriteLine("fillDtTransferItem");
            string whrfind = "",whrid="",whritemid="", whrdate="",whrtype="";
            ;
            if(filterTrans.id is null || filterTrans.id=="") {
            }else{
                whrid = "and tr.id = '"+filterTrans.id+"' ";
            }
            
            if(filterTrans.transaction_date is null || filterTrans.transaction_date=="") {
            }else{
                whrdate = "and tr.transaction_date = '"+filterTrans.transaction_date+"' ";
            }
            if(filterItem.id is null || filterItem.id=="") {
            }else{
                whritemid = "and ti.id = '"+filterItem.id+"' ";
            }
            if(filterItem.product_short_name is null || filterItem.product_short_name==""){
                //return new DataTable();
            }else{
                if(filterItem.product_short_name.Length>=2){
                    whrfind = "and (upper(pr.name) like upper('" + filterItem.product_name + "%') or upper(pr.short_name) like upper('" + filterItem.product_short_name + "%')) ";
                }
            }  
            string whrstate = "";
            //if(state!="") whrstate = "and ti.state="+state +" ";

            
             string sql = "select ti.id, ti.transaction_id, ti.product_id, pr.short_name product_short_name, pr.name product_name,ti.stock_id, "+
            "case when ti.tax is null then 0 else ti.tax end tax, ti.state, state.name state_name, "+
            "st.quantity, st.unit,un.name unit_name, "+
            "pp.id purchase_price_id, pp.item_price purchase_item_price,pp.main_discount purchase_main_discount, pp.additional_discount purchase_additional_discount, pp.deduction_amount purchase_deduction_amount, pp.final_price purchase_final_price, 0 purchase_subtotal, pp.tax purchase_tax, "+
            "st.location, lo.name location_name, st.condition, co.name condition_name "+
            "from transfer tr, transaction_item_state state, transfer_item ti, product pr, stock st "+
            "LEFT OUTER JOIN purchase_price pp on pp.id = st.price_id "+
            "left outer join unit un on un.id=st.unit "+
            "left outer join condition co on st.condition=co.id "+
            "left outer join location lo on st.location=lo.id "+
            "where tr.id=ti.transaction_id and ti.product_id=pr.id and ti.stock_id=st.id and state.id=ti.state "+
             whrid+whrstate+
            "ORDER by ti.id desc";
            //tekan kene
            Console.WriteLine(sql);            
            return DbCl.fillDataTable(DbCl.getConn(), sql);
        }

        public double GetPaymentAmount(string transaction_id){
            double total = 0;
            string sql = "select * from payment where transaction_id="+transaction_id;
            DataTable dt =  DbCl.fillDataTable(DbCl.getConn(), sql);
            foreach (DataRow dr in dt.Rows)
            { 
                total+= Convert.ToDouble(dr["amount"].ToString());
            }
            return total;
        }
        public double GetLastSalePrice(string product_id){
            double price = 0;
            string sql = "select price from stock, price where stock.price_id=price.id and stock.product_id="+product_id + " order by price.id desc limit 1";
            DataTable dt =  DbCl.fillDataTable(DbCl.getConn(), sql);
            foreach (DataRow dr in dt.Rows)
            { 
                price= Convert.ToDouble(dr["price"].ToString());
            }
            return price;
        }
        public double GetLastPurchasePrice(string product_id){
            double purchase_price = 0;
            string sql = "select purchase_price from stock, price where stock.purchase_price_id=purchase_price.id and stock.product_id="+product_id + " order by purchase_price.id desc limit 1";
            DataTable dt =  DbCl.fillDataTable(DbCl.getConn(), sql);
            foreach (DataRow dr in dt.Rows)
            { 
                purchase_price= Convert.ToDouble(dr["purchase_price"].ToString());
            }
            return purchase_price;
        }
    }


    public static class PasswordHasher
    {
        public static string Hash(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password);
        }

        public static bool Verify(string password, string hashedPassword)
        {
            return BCrypt.Net.BCrypt.Verify(password, hashedPassword);
        }
    }

}