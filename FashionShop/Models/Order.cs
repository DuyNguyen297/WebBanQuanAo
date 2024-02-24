using FashionShop.Models.Base;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Xml.Linq;
using System;
using System.ComponentModel.DataAnnotations.Schema;
using FashionShop.Shared;
using System.Collections.Generic;

namespace FashionShop.Models
{
    [Table("Order")]
    public class Order : BaseCodeEntity
    {
        [DisplayName("Tên người nhận")]
        public string Name { get; set; }
        [DisplayName("Giảm giá")]
        public decimal? Discount { get; set; } = 0;

        [DisplayName("Ngày giao")]
        public DateTime? ShipDate { get; set; }
        [DisplayName("Tiền ship")]
        public decimal? ShipFee { get; set; } = 30000;
        [DisplayName("Ngày nhận")]
        public DateTime? ReceiveDate { get; set; }
        [DisplayName("Số điện thoại")]
        public string Phone { get; set; }

        [DisplayName("Địa chỉ")]
        public string Address { get; set; }

        [DisplayName("Tổng tiền")]
        public decimal? Total { get; set; }

        [DisplayName("Trạng thái")]
        [DefaultValue("WAITCONFIRM")]
        public string Status { get; set; } = StatusConst.WAITCONFIRM;
        public string PayWay { get; set; }
        [DisplayName("Trạng thái thanh toán")]
        public string PayStatus { get; set; }
        [DisplayName("Lý do hủy/trả hàng")]
        public string Reason { get; set; }
        [DisplayName("Đơn hàng rác/Không ảnh hưởng số lượng sản phẩm và doanh thu")]
        public bool IsBlackOrder { get; set; } = false;
        // Người đặt hàng
        public string CustomerId { get; set; }
        [ForeignKey("CustomerId")]
        public Customer Customer { set; get; }
        // người xử lí đơn hàng
        // Sản phẩm
        [ForeignKey("CreateUserId")]
		public Account Account { set; get; }
        public ICollection<OrderDetail> OrderDetails { get; set; }
    }
}
