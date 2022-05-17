﻿using System;
using System.Collections.Generic;

namespace OrderService.Models
{
    public partial class Courier
    {
        public Courier()
        {
            OrderDetails = new HashSet<OrderDetail>();
        }

        public int Id { get; set; }
        public string CourierName { get; set; } = null!;
        public string PhoneNumber { get; set; } = null!;

        public virtual ICollection<OrderDetail> OrderDetails { get; set; }
    }
}