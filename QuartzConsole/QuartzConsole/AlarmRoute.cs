using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuartzConsole
{
    [Serializable]
    class AlarmRoute
    {
        public string Id { get; set; }
        public int CustomerID { get; set; }
        public int OrgId { get; set; }
        public string Category { get; set; }
        public List<AlarmRouteRule> Rules { get; set; }
        public AlarmSchedule Schedule {get;set;}
    }
}
