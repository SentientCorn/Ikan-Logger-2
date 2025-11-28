using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IkanLogger.Models
{
    public class CatchLog
    {
        public int idlog { get; set; }
        public DateTime logdate { get; set; }
        public string notes    { get; set; }
        public User? iduser { get; set; }
        public double totalweight { get; set; }
        public double totalprice { get; set; }
    }
}
