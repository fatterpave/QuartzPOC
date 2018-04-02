using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuartzConsole
{
    [Serializable]
    class AlarmRouteRule
    {
        public int Id { get; set; }
        public string Destination { get; set; }
        public int Repeat { get; set; }
        public int Duration { get; set; }
        public int Fleet { get; set; }
    }
}
