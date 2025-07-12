namespace Inventorifo.Lib.Model
{
    public class clsConfig
    {
        public string id { get; set; }
        public string app_id { get; set; }
        public string app_version { get; set; }
        public string country_code { get; set; }
        public string currency { get; set; }
        public string tax { get; set; }
        public string person_id { get; set; }
        public string organization_id { get; set; }
        public string organization_name { get; set; }
        public string organization_address { get; set; }
        public string organization_phone_number { get; set; }
    }
	public class Response
	{
		public string code { get; set; }
		public string description { get; set; }
		public string reference_id { get; set; }
	}
    public class clsPerson{
        public string id { get; set; }
        public string name { get; set; }
        public string address { get; set; }
        public string phone_number { get; set; }
        public string is_active { get; set; }
        public string national_id_number { get; set; }
        public string tax_id_number { get; set; }
        public string health_insurance_id_number { get; set; }
    }
    public class clsOrganization{
        public string id { get; set; }
        public string name { get; set; }
        public string address { get; set; }
        public string phone_number { get; set; }
        public string is_active { get; set; }
        public string tax_id_number { get; set; }
    }
    public class clsSupplier{
        public string id { get; set; }
        public string organization_id { get; set; }
        public string organization_name { get; set; }
        public string organization_address { get; set; }
        public string organization_phone_number { get; set; }
        public string organization_tax_id_number { get; set; }
        public string person_id { get; set; }
        public string person_name { get; set; }
        public string person_address { get; set; }
        public string person_phone_number { get; set; }
        public string is_active { get; set; }
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
        public string is_active { get; set; }
        public string price1 { get; set; }
        public string price2 { get; set; }
        public string price3 { get; set; }
        public string last_purchase_price { get; set; }
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
        public string organization { get; set; }
        public string organization_name { get; set; }
        public string location { get; set; }
        public string location_name { get; set; }
        public string location_group { get; set; }
        public string location_group_name { get; set; }
        public string condition { get; set; }
        public string condition_name { get; set; }
        public string is_active { get; set; }
    }
    public class clStockHistory
    {
        public string id { get; set; }
        public string transaction_id { get; set; }
        public string input_date { get; set; }
        public string transaction_type { get; set; }
        public string transaction_type_name { get; set; }
        public string product_id { get; set; }
        public string stock_id { get; set; }
        public string short_name { get; set; }
        public string product_name { get; set; }
        public string barcode { get; set; }
        public string quantity_before { get; set; }
        public string quantity { get; set; }
        public string quantity_after { get; set; }
        public string unit { get; set; }
        public string unit_name { get; set; }
        public string product_group_id { get; set; }
        public string product_group_name { get; set; }
        public string location { get; set; }
        public string location_name { get; set; }
        public string location_group { get; set; }
        public string location_group_name { get; set; }
        public string condition { get; set; }
        public string condition_name { get; set; }
        public string is_active { get; set; }
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
        public string reference_date { get; set; }
        public string supplier_id { get; set; }
        public string customer_id { get; set; }
        public string organization_id { get; set; }
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
        public string tax_amount { get; set; }
        public string is_tax { get; set; }

    }

    public class clJournal
    {    
        public string id { get; set; }
        public string account_id { get; set; }
        public string account_name { get; set; }
        public string transaction_date { get; set; }
        public string reference_id { get; set; }
        public string description { get; set; }
        public string debet_amount { get; set; }
        public string credit_amount { get; set; }
        public string user_id { get; set; }
        public string person_name { get; set; }
        public string application_id { get; set; }
    }

    public class clTransactionItem1
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
        public string purchase_price_id { get; set; }
        public string purchase_item_price { get; set; }
        public string purchase_main_discount { get; set; }
        public string purchase_additional_discount { get; set; }
        public string purchase_deduction_amount { get; set; }
        public string purchase_final_price { get; set; }
        public string purchase_subtotal { get; set; }
        public string purchase_tax { get; set; }
        public string item_price { get; set; }
        public string main_discount { get; set; }
        public string additional_discount { get; set; }
        public string deduction_amount { get; set; }
        public string final_price { get; set; }
        public string subtotal { get; set; }
        public string tax { get; set; }
        public string state { get; set; }
        public string state_name { get; set; }
        public string location { get; set; }
        public string location_name { get; set; }
        public string condition { get; set; }
        public string condition_name { get; set; }
    }

    public class clTransactionItem
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

    public class clReportTransaction
    {                          
        public string id { get; set; }
        public string input_date { get; set; }
        public string transaction_id { get; set; }
        public string product_id { get; set; }
        public string product_short_name { get; set; }
        public string product_name { get; set; }
        public string stock_id { get; set; }
        public string quantity { get; set; }
        public string unit { get; set; }
        public string unit_name { get; set; }
        public string purchase_price { get; set; }
        public string purchase_price_id { get; set; }
        public string price_id { get; set; }
        public string price { get; set; }
        public string tax { get; set; }
        public string state { get; set; }
        public string state_name { get; set; }
        public string location { get; set; }
        public string location_name { get; set; }
        public string condition { get; set; }
        public string condition_name { get; set; }
        public string expired_date { get; set; }
    }
    public class clTransfer
    {
        public string id  { get; set; }
        public string transaction_date { get; set; }
        public string transaction_type_id { get; set; }
        public string transaction_type_name { get; set; }
        public string reference_id { get; set; }
        public string reference_date { get; set; }
        public string source_organization_id { get; set; }
        public string source_organization_name { get; set; }
        public string source_location_id { get; set; }
        public string source_location_name { get; set; }
        public string source_condition_id { get; set; }
        public string source_condition_name { get; set; }
        public string destination_organization_id { get; set; }
        public string destination_organization_name { get; set; }
        public string destination_location_id { get; set; }
        public string destination_location_name { get; set; }
        public string destination_condition_id { get; set; }
        public string destination_condition_name { get; set; }
        public string user_id { get; set; }
        public string user_name { get; set; }
        public string state { get; set; }
        public string state_name { get; set; }
        public string state_fgcolor { get; set; }
        public string state_bgcolor { get; set; }
        public string application_id { get; set; }
    }
    public class clTransferItem
    {                          
        public string id { get; set; }
        public string transfer_id { get; set; }
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