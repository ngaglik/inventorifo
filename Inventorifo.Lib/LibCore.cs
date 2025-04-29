using System;
using Npgsql;
using System.Data;
using Gtk;
using Gdk;
using Pango;
using Inventorifo.Lib.Model;

namespace Inventorifo.Lib
{
    public class LibCore
    {
        LibDb DbCl = new LibDb ();

        public LibCore(){

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
            Console.WriteLine("filter============= product_id "+ filter.product_id);
            Console.WriteLine("filter============= location "+ filter.location);
            Console.WriteLine("filter============= condition "+ filter.condition);
            Console.WriteLine("filter============= price_id "+ filter.price_id);

                string whrlocgroup="", whractive="", whrlocation="", whrcondition="", whrid="", whrbarcode="", whrfind = "", whrtype="";
                
                if(filter.is_active is null || filter.is_active=="") {
                }else{
                    whractive = "and prod.is_active = true ";
                }
                if(filter.barcode is null || filter.barcode=="") {
                }else{
                    whrbarcode = "and prod.barcode =  '" + filter.barcode + "' ";
                }
                if(filter.location is null || filter.location=="") {
                }else{
                    whrlocation = "and stock.location =  '" + filter.location + "' ";
                }
                if(filter.condition is null || filter.condition=="") {
                }else{
                    whrcondition = "and stock.condition =  '" + filter.condition + "' ";
                }
                if(transaction_type==20){                    
                   // whrtype = " and store_quantity>0 ";                    
                }
                if(transaction_type==2 ) {
                        whrlocgroup = "and locgr.id=2 ";
                        whrtype = "and store_quantity>0 "; 
                    }
                
                if(filter.product_id is null || filter.product_id == ""){
                    if(filter.short_name is null || filter.short_name==""){
                        return new DataTable();
                    }else{
                        if(filter.short_name.Length>=2){
                            whrfind = "and (upper(prod.name) like upper('" + filter.short_name + "%') or upper(prod.short_name) like upper('" + filter.short_name + "%')) ";
                        }else{
                            return new DataTable();
                        }
                    }               
                }else{
                    whrid = " and prod.id= "+filter.product_id + " ";
                }
                
                string sql = "SELECT prod.id, prod.short_name, prod.name product_name, prod.barcode, "+
                "case when store_quantity is null then 0 else store_quantity end store_quantity, "+
                "case when global_quantity is null then 0 else global_quantity end global_quantity, "+
                "prod.product_group, prodgr.name product_group_name "+
                "FROM product_group prodgr, product prod "+
                "left outer join (select sum(quantity) store_quantity,product_id,location_group from stock,location loc,location_group locgr where state=0 "+whrcondition+whrlocation+" and stock.location=loc.id and loc.location_group=locgr.id "+whrlocgroup+" group by product_id,loc.id) prstore on prstore.product_id=prod.id "+
                "left outer join (select sum(quantity) global_quantity,product_id from stock where state=0 group by product_id) prglobal  on prglobal.product_id=prod.id "+
                "WHERE prod.product_group = prodgr.id "+ whrfind + whrbarcode + whrtype +whrid+
                "ORDER by prod.name asc";
                Console.WriteLine(sql);
                return DbCl.fillDataTable(DbCl.getConn(), sql);
        }
        public DataTable fillDtTransactionPurchase(string transaction_id, string date, string strfind){
            string whrfind = "",whrid="", whrdate="";
            if(transaction_id!="") whrid = "and tr.id="+transaction_id+ " ";
            if(strfind!="") whrfind = "and (upper(cus.organization_name) like upper('" + strfind + "%') or upper(pers.name) like upper('" + strfind + "%') )";
            if(date!="") whrdate = "and tr.transaction_date::date = '"+date+"'::date ";

            string sql = "select tr.id,tr.reference_id, "+
            "tr.supplier_id,sup.organization_name,sup.organization_address,sup.organization_phone_number,pers.name person_name,pers.phone_number person_phone_number, "+
            "tr.transaction_type transaction_type_id, ty.name transaction_type_name, TO_CHAR(tr.transaction_date,'yyyy-mm-dd') transaction_date, tr.transaction_amount, tr.return_amount, "+
            "tr.payment_group_id, py.name payment_group_name, tr.payment_amount, "+
            "tr.user_id, usr.name user_name,  "+
            "tr.state, st.name state_name, st.fgcolor state_fgcolor, st.bgcolor state_bgcolor,  "+
            "tr.application_id, tr.tax_amount, tr.is_tax "+
            "from transaction tr left outer join supplier sup on tr.supplier_id=sup.id "+
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
        public DataTable fillDtTransactionSale(string transaction_id, string date, string strfind){
            string whrfind = "",whrid="", whrdate="";
            if(transaction_id!="") whrid = "and tr.id="+transaction_id+ " ";
            if(strfind!="") whrfind = "and (upper(cus.organization_name) like upper('" + strfind + "%') or upper(pers.name) like upper('" + strfind + "%') )";
            if(date!="") whrdate = "and tr.transaction_date::date = '"+date+"'::date ";

            string sql = "select tr.id,tr.reference_id, "+
                "tr.customer_id,cus.organization_name,cus.organization_address,cus.organization_phone_number,pers.name person_name,pers.phone_number person_phone_number, "+
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
                whrid+ whrfind+ whrdate +
                "ORDER by tr.id desc";
                Console.WriteLine(sql);
                return DbCl.fillDataTable(DbCl.getConn(), sql);
        }

        public DataTable fillDtTransactionItem(string transaction_id, string state){
            string whrstate = "";
            if(state!="") whrstate = "and ti.state="+state +" ";

            string sql = "select ti.id, ti.transaction_id, ti.product_id, pr.short_name product_short_name, pr.name product_name,ti.stock_id, "+
            "case when ti.tax is null then 0 else ti.tax end tax, ti.state, state.name state_name, "+
            "ti.quantity, st.unit,un.name unit_name, "+
            "price.id price_id, case when price.purchase_price is null then 0 else price.purchase_price end purchase_price, case when price.price is null then 0 else price.price end price, "+
            "st.location, lo.name location_name, st.condition, co.name condition_name "+
            "from transaction_item_state state, transaction_item ti, product pr, stock st "+
            "LEFT OUTER JOIN price on price.id = st.price_id "+
            "left outer join unit un on un.id=st.unit "+
            "left outer join condition co on st.condition=co.id "+
            "left outer join location lo on st.location=lo.id "+
            "where ti.product_id=pr.id and ti.stock_id=st.id and state.id=ti.state "+
            "and ti.transaction_id="+transaction_id.ToString()+ " "+whrstate+
            "ORDER by ti.id desc";
            //tekan kene
            Console.WriteLine(sql);            
            return DbCl.fillDataTable(DbCl.getConn(), sql);
        }

        public DataTable fillDtTransfer(string transaction_id, string date, string strfind){
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
        public DataTable fillDtTransferItem(string transaction_id, string state){
            string whrstate = "";
            if(state!="") whrstate = "and ti.state="+state +" ";

            string sql = "select ti.id, ti.transfer_id, ti.product_id, pr.short_name product_short_name, pr.name product_name,ti.stock_id, "+
            "case when ti.tax is null then 0 else ti.tax end tax, ti.state, state.name state_name, "+
            "ti.quantity, st.unit,un.name unit_name, "+
            "price.id price_id, case when price.purchase_price is null then 0 else price.purchase_price end purchase_price, case when price.price is null then 0 else price.price end price, "+
            "st.location, lo.name location_name, st.condition, co.name condition_name "+
            "from transaction_item_state state, transfer_item ti, product pr, stock st "+
            "LEFT OUTER JOIN price on price.id = st.price_id "+
            "left outer join unit un on un.id=st.unit "+
            "left outer join condition co on st.condition=co.id "+
            "left outer join location lo on st.location=lo.id "+
            "where ti.product_id=pr.id and ti.stock_id=st.id and state.id=ti.state "+
            "and ti.transfer_id="+transaction_id.ToString()+ " "+whrstate+
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
            string sql = "select purchase_price from stock, price where stock.price_id=price.id and stock.product_id="+product_id + " order by price.id desc limit 1";
            DataTable dt =  DbCl.fillDataTable(DbCl.getConn(), sql);
            foreach (DataRow dr in dt.Rows)
            { 
                purchase_price= Convert.ToDouble(dr["purchase_price"].ToString());
            }
            return purchase_price;
        }
    }
}