//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace ServerService
{
    using System;
    using System.Collections.Generic;
    
    public partial class sellout
    {
        public int ID { get; set; }
        public string customer_name { get; set; }
        public string product_name { get; set; }
        public Nullable<double> price { get; set; }
        public Nullable<int> volume { get; set; }
        public Nullable<System.DateTime> sell_time { get; set; }
    }
}