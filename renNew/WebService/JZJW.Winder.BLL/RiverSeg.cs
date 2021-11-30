
namespace JZJW.Winder.BLL
{
    public class RiverSeg
    {
        public RiverSeg() { }

        /*public RiverSeg(string sid, int rgnid, int bslen, int bsval)
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
        */
        //ren
        public RiverSeg(string sid, long rgnid, long bslen, long bsval, long rgnidnew)
        {
            this.sid = sid;
            this.REGIONID = rgnid;
            this.BINSTRVAL = bsval;
            this.BINSTRLEN = bslen;
            this.REGIONIDNEW = rgnidnew;
        }
        public long REGIONID { get; set; }
        public long BINSTRLEN { get; set; }
        public long BINSTRVAL { get; set; }
        public long REGIONIDNEW { get; set; }

        public long REGHIGHID { get; set; }
        public string shape { get; set; }
        public string sid { get; set; }
    }
}
