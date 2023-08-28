using OKX.Api.Enums;
using OKX.Api.Models.MarketData;
using OkxSurfer.Builder;
using OkxSurfer.Common;
using OkxSurfer.Common.Extensions;

namespace OkxSurfer.OCScanner
{
    public class Scanner
    {
        /* Sample Pairs */
        private static readonly List<string> Samples = new List<string> { "API3-USDT-SWAP" };
        //        private static readonly List<string> Samples = new List<string> { "XRP-USDT-SWAP", "SOL-USDT-SWAP", "MKR-USDT-SWAP" };

        private static readonly List<OkxPeriod> PeriodSamples = new List<OkxPeriod> { OkxPeriod.OneMinute };
        
        public async static Task<List<BestOC>> Run()
        {
            var bestOCs = new List<BestOC>();
            var apiClient = ClientFactory.GetRestApiClient();
            foreach (var sample in Samples)
            {
                foreach(var periodTime in PeriodSamples)
                {
                    var marketCandles = await apiClient.MarketData.GetCandlesticksAsync(sample, periodTime);
                    var bestOCLong = GetBestOcFromTimePeriod(marketCandles.Data.ToList(), TradeType.Long);
                    var bestOCShort = GetBestOcFromTimePeriod(marketCandles.Data.ToList(), TradeType.Short);
                    Console.WriteLine($"{sample}: {periodTime.ToString()}: Long - {bestOCLong.PnlCount}: OC - {bestOCLong.Oc}: Win - {bestOCLong.WinCount}: Lose - {bestOCLong.LoseCount}");
                    Console.WriteLine($"{sample}: {periodTime.ToString()}: Short - {bestOCShort.PnlCount}: OC - {bestOCShort.Oc}: Win - {bestOCShort.WinCount}: Lose - {bestOCShort.LoseCount}");
                }    
                
            }
            
            return bestOCs;
        }

        private const double AmountPerOrder = 1000;

        private static TotalPnl GetBestOcFromTimePeriod(List<OkxCandlestick> data, TradeType tradeType)
        {
            var dataCount = data.Count();       
            List<TotalPnl> result = new List<TotalPnl>();
            for (double i = 2; i < 3; i += 1)
            {
                int winCount = 0, loseCount = 0, reduce = 0, currentPosition = 0;
                double pnlTotal = 0;
                bool isPlacedOrder = false;
                decimal? currentTP = null;
                for(int j = dataCount - (1 + currentPosition); j >= 0; j--)
                {
                    var currentData = data[j];
                    var closePrice = currentData.Close;
                    var hightPrice = currentData.High;
                    var lowPrice = currentData.Low;
                    var openPrice = currentData.Open;

                    var ocPrice = currentData.OCToPrice(i, tradeType);
                    var tpPrice = currentTP ?? currentData.OCToPrice(i/2, tradeType);                    
                    if (tradeType == TradeType.Short)
                    {
                        var reducePrice = tpPrice + (openPrice - tpPrice)*(reduce/100);
                        if (!isPlacedOrder && hightPrice < ocPrice)
                        {                            
                            continue;
                        }
                        currentTP = tpPrice;
                        isPlacedOrder = true;
                        if (lowPrice > reducePrice)
                        {
                            reduce = 30;
                            continue;
                        }
                        else
                        {
                            reduce = 0;
                            isPlacedOrder = false;
                            if(ocPrice > reducePrice)
                            {
                                winCount++;                                
                            }
                            else
                            {
                                loseCount++;
                            }
                            pnlTotal += Convert.ToDouble((ocPrice - reducePrice) / ocPrice * 100);
                        }                        
                    }
                    else
                    {
                        var reducePrice = tpPrice - (openPrice - tpPrice) * (reduce / 100);
                        if (!isPlacedOrder && lowPrice > ocPrice)
                        {
                            continue;
                        }
                        currentTP = tpPrice;
                        isPlacedOrder = true;
                        if (hightPrice < reducePrice)
                        {
                            reduce = 30;
                            continue;
                        }
                        else
                        {
                            reduce = 0;
                            isPlacedOrder = false;
                            if (ocPrice < reducePrice)
                            {
                                winCount++;
                            }
                            else
                            {
                                loseCount++;
                            }
                            pnlTotal += Convert.ToDouble((reducePrice - ocPrice) / ocPrice * 100);
                        }
                    }
                }
                result.Add(new TotalPnl
                {
                    WinCount = winCount,
                    LoseCount = loseCount,
                    PnlCount = pnlTotal,
                    Oc = i
                });
            }
            return result.MaxBy(r => r.PnlCount) ?? new TotalPnl();
        }
    }
}