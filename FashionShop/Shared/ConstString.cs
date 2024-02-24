namespace FashionShop.Shared
{
    public static class Const
    {
        //Admin
        public const string ADMINSESSION = "ADMIN";
        public const string ADMINIDSESSION = "ADMINID";
        public const string ADMINNAMESESSION = "ADMINNAME";
        // role
        public const string ROLE = "ROLE";
        public const string ADMIN = "Admin";
        public const string USER = "User";
        //User
        public const string USERSESSION = "USER";
        public const string USERIDSESSION = "USERID";
        public const string USERNAMESESSION = "USERNAME";
        public const string USERPHONESESSION = "USERPHONE";
        //Import
        public const string IMPORTSESSION = "IMPORT";
        public const string IMPORTID = "IMPORTID";
        //Cart
        public const string CARTSESSION = "CART";
        // Cart vitual
        public const string CARTVITUALSESSION = "CARTVITUAL";
        //Order
        public const string ORDERSESSION = "ORDER";
        //Checkout
        public const string CHECKOUTSESSION = "CHECKOUT";
        public const string RECHECKOUTORDERIDSESSION = "RECHECKOUTORDERID";
        public const string PAYWAY = "PAYWAY";
        public const string PAYSTATUS = "PAYSTATUS";
        //Sell
        public const string SELL = "SELL";
        public const string SELLID = "SELLID";
        // Reason
        public const string REFUSEREASON = "Lý do khác";
        // Colors
        public const string COLOR = "COLOR";
    }
    public static class StatusConst
    {
        // Order
        public const string WAITCONFIRM = "WAITCONFIRM";
        public const string WAITSETUP = "WAITSETUP";
        public const string WAITSHIP = "WAITSHIP";
        public const string SHIPPING = "SHIPPING";
        public const string DELIVERED = "DELIVERED";
        public const string DONE = "DONE";
        public const string RETURN = "RETURN";
        public const string CANCEL = "CANCEL";

        // Product
        public const string COMINGEND = "COMINGEND";
        public const string MORE = "MORE";
    }
    public static class PayConst
    {
        public const string ONLINE = "ONLINE";
        public const string OFFLINE = "OFFLINE";
    }
    public static class PayStatusConst
    {
        public const string DONE = "DONE";
        public const string NODONE = "NODONE";
    }

	public static class VoucherTypeConst
	{
		public const string VOUCHERSHIP = "VOUCHERSHIP";
		public const string VOUCHERCUSTOMER = "VOUCHERCUSTOMER";
		public const string VOUCHERPRODUCT = "VOUCHERPRODUCT";
		public const string VOUCHERCATEGORY = "VOUCHERCATEGORY";
	}
}
