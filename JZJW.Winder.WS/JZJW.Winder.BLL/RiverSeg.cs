using System;
using System.Collections.Generic;
using System.Text;

namespace JZJW.Winder.BLL
{
    public class RiverSeg
    {
        public RiverSeg() { }
        public RiverSeg(string sid, int rgnid, int bslen, int bsval)
        {
            this.sid = sid;
            this.regionid = rgnid;
            this.binstrlen = bslen;
            this.binstrval = bsval;
        }

        public string sid { get; set; }
        public int regionidnew { get; set; }
        public int regionid { get; set; }
        public int binstrlen { get; set; }
        public int binstrval { get; set; }
        public int reghighid { get; set; }
        public string shape { get; set; }
    }
}
