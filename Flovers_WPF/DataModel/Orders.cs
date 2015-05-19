﻿using System;

using SQLite;

namespace Flovers_WPF.DataModel
{
    [Table("orders")]
    public class Orders
    {
        [PrimaryKey, AutoIncrement, NotNull]
        public int orders_id { get; set; }

        [NotNull]
        public DateTime datetime { get; set; }

        [NotNull]
        public string address { get; set; }

        [NotNull]
        public double price { get; set; }

        [NotNull]
        public string status { get; set; }

        [Indexed, NotNull]
        public int clients_id { get; set; }

        public Orders() { }

        public Orders( string address, double price, string status )
        {
            this.address = address;
            this.price = price;
            this.status = status;
        }
    }
}

