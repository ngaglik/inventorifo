namespace Inventorifo.App
{
	public class Response
	{
		public string code { get; set; }
		public string description { get; set; }
		public string reference_id { get; set; }
	}
	public class UserLogin
    { //
        public UserLogin(string id, string person_id,string person_name, string person_address,string person_phone_number, string level,string level_name,string is_active,string application_id, string privilege){
        this.id = id;
        this.person_id = person_id;
        this.person_name = person_name;
        this.person_address = person_address;
        this.person_phone_number = person_phone_number;
        this.level = level;
        this.level_name = level_name;
        this.is_active = is_active;
        this.application_id = application_id;
        this.privilege = privilege;
        }
        public string id;
        public string person_id;
        public string person_name;
        public string person_address;
        public string person_phone_number;
        public string level;
        public string level_name;
        public string is_active;
        public string application_id;
        public string privilege;
    }
    public class clsProduct{
        public string id { get; set; }
        public string short_name { get; set; }
        public string product_name { get; set; }
        public string store_quantity { get; set; }
        public string global_quantity { get; set; }
        public string barcode { get; set; }
        public string product_group { get; set; }     
        public string product_group_name { get; set; }
    }
    public class clStock
    {
        public string id { get; set; }
        public string product_id { get; set; }
        public string stock_id { get; set; }
        public string short_name { get; set; }
        public string product_name { get; set; }
        public string barcode { get; set; }
        public string quantity { get; set; }
        public string unit { get; set; }
        public string unit_name { get; set; }
        public string expired_date { get; set; }
        public string price_id { get; set; }
        public string purchase_price { get; set; }
        public string price { get; set; }
        public string product_group_id { get; set; }
        public string product_group_name { get; set; }
        public string location { get; set; }
        public string location_name { get; set; }
        public string location_group { get; set; }
        public string location_group_name { get; set; }
    }
    public class clPayment
    {    
        public string id { get; set; }
        public string transaction_id { get; set; }
        public string payment_date { get; set; }
        public string payment_amount { get; set; }
        public string user_id { get; set; }
        public string user_name { get; set; }
    }
    public class clTransaction
    {    
        public string id { get; set; }
        public string reference_id { get; set; }
        public string supplier_id { get; set; }
        public string customer_id { get; set; }
        public string organization_name { get; set; }
        public string organization_address { get; set; }
        public string organization_phone_number { get; set; }
        public string person_name { get; set; }
        public string person_phone_number { get; set; }
        public string transaction_type_id { get; set; }
        public string transaction_type_name { get; set; }
        public string transaction_date { get; set; }
        public string transaction_amount { get; set; }
        public string return_amount { get; set; }
        public string payment_group_id { get; set; }
        public string payment_group_name { get; set; }
        public string payment_amount { get; set; }
        public string user_id { get; set; }
        public string user_name { get; set; }
        public string state { get; set; }
        public string state_name { get; set; }
        public string state_fgcolor { get; set; }
        public string state_bgcolor { get; set; }
        public string application_id { get; set; }
    }

    class clTransItem
    {                          
        public string id { get; set; }
        public string transaction_id { get; set; }
        public string product_id { get; set; }
        public string product_short_name { get; set; }
        public string product_name { get; set; }
        public string stock_id { get; set; }
        public string quantity { get; set; }
        public string unit { get; set; }
        public string unit_name { get; set; }
        public string purchase_price { get; set; }
        public string price_id { get; set; }
        public string price { get; set; }
        public string tax { get; set; }
        public string state { get; set; }
        public string state_name { get; set; }
        public string location { get; set; }
        public string location_name { get; set; }
        public string condition { get; set; }
        public string condition_name { get; set; }
    }
}