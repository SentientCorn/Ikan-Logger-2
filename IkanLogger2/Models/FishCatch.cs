using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IkanLogger.Models
{
    public class FishCatch
    {
        public int idfishcatch { get; set; }
        public double weight { get; set; }
        public Fish? fishid { get; set; }
        public CatchLog? idLog { get; set; }
        public double saleprice { get; set; }
    }
}
