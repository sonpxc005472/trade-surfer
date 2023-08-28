using OKX.Api.Enums;

namespace OkxSurfer.Common
{
    public class BestOC
    {
        public string? Instrument { get; set; }
        public OkxPeriod OkxPeriod { get; set; }
        public double OC { get; set; }
    }
}
