﻿using System.Numerics;

namespace CustomerRankService.Models
{
    public class Customer
    {
        public long CustomerId { get; set; }
        public decimal Score { get; set; }
        public int Rank { get; set; }    
    }
}
