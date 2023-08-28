using OKX.Api.Models.MarketData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OkxSurfer.Common.Extensions
{
    public static class PriceExtension
    {
        public static decimal OCToPrice(this OkxCandlestick okxCandlestick, double oc, TradeType tradeType)
        {
            var price = okxCandlestick.Open * Convert.ToDecimal(oc/100);
            if(tradeType == TradeType.Short)
            {
                price = okxCandlestick.Open + price;
            }
            else
            {
                price = okxCandlestick.Open - price;
            }
            return price;
        }
    }
}
