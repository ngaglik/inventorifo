public class Stock
	{
		public Stock(double product_id, string short_name, string product_name, string barcode, double quantity,int unit, string unit_name, double purchase_price, double price, string expired_date, int product_group_id, string product_group_name, double stock_id, double price_id )
		{
            this.product_id = product_id;
			this.barcode = barcode;
			this.product_name = product_name;
			this.unit = unit;
			this.quantity = quantity;
			this.purchase_price = purchase_price;
			this.price = price;
			this.expired_date = expired_date;
			this.short_name = short_name;
			this.product_group_name = product_group_name;
			this.product_group_id = product_group_id;
            this.stock_id = stock_id;
            this.price_id = price_id;
        }
        public double product_id;
		public string short_name;
		public string product_name;
		public string barcode;
		public double quantity;
		public int unit;
		public string unit_name;
		public double purchase_price;
		public double price;
		public string expired_date;
        public int product_group_id;
		public string product_group_name;
		public double stock_id; 
        public double price_id;
	}