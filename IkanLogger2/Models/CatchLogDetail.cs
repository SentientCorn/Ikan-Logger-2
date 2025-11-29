using System;
using System.Collections.Generic;

namespace IkanLogger2.Models
{
    public class CatchLogDetail
    {
        public int idlog { get; set; }
        public DateTime logdate { get; set; }
        public string notes { get; set; }
        public double totalweight { get; set; }
        public double totalprice { get; set; }
        public List<FishCatchDetail> Catches { get; set; } = new List<FishCatchDetail>();
    }

    public class FishCatchDetail
    {
        public int idfishcatch { get; set; }
        public string fishname { get; set; }
        public double weight { get; set; }
        public double saleprice { get; set; }
    }
}