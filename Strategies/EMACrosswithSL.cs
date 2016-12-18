#region Using declarations
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Xml.Serialization;
using NinjaTrader.Cbi;
using NinjaTrader.Gui;
using NinjaTrader.Gui.Chart;
using NinjaTrader.Gui.SuperDom;
using NinjaTrader.Gui.Tools;
using NinjaTrader.Data;
using NinjaTrader.NinjaScript;
using NinjaTrader.Core.FloatingPoint;
using NinjaTrader.NinjaScript.Indicators;
using NinjaTrader.NinjaScript.DrawingTools;
#endregion

//This namespace holds Strategies in this folder and is required. Do not change it. 
namespace NinjaTrader.NinjaScript.Strategies
{
	public class EMACrosswithSL : Strategy
	{
		private ATR _atr;
		private EMA _slow;
		private EMA _med;
		private EMA _fast;
				
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"Buy when Fast EMA > Med EMA > Slow EMA, sell when Price < Prev-ATR(10)";
				Name										= "EMACrosswithSL";
				Calculate									= Calculate.OnBarClose;
				EntriesPerDirection							= 1;
				EntryHandling								= EntryHandling.AllEntries;
				IsExitOnSessionCloseStrategy				= true;
				ExitOnSessionCloseSeconds					= 30;
				IsFillLimitOnTouch							= false;
				MaximumBarsLookBack							= MaximumBarsLookBack.TwoHundredFiftySix;
				OrderFillResolution							= OrderFillResolution.Standard;
				Slippage									= 0;
				StartBehavior								= StartBehavior.WaitUntilFlat;
				TimeInForce									= TimeInForce.Gtc;
				TraceOrders									= true;
				RealtimeErrorHandling						= RealtimeErrorHandling.StopCancelClose;
				StopTargetHandling							= StopTargetHandling.PerEntryExecution;
				BarsRequiredToTrade							= 20;
				// Disable this property for performance gains in Strategy Analyzer optimizations
				// See the Help Guide for additional information
				IsInstantiatedOnEachOptimizationIteration	= true;
				FastEma			= 5;
				MedEma			= 20;
				SlowEma			= 50;
				ATRPeriod		= 10;
				ATRMultipleSL 	= 2;
				ATRMultipleLimit = 0.5;
				InitialCapital	= 100000;
				AddLine(Brushes.Blue, 1, "ATR");
				AddPlot(Brushes.LimeGreen, "FastEmaLine");
				AddPlot(Brushes.Orange, "MedEmaLine");
				AddPlot(Brushes.Salmon, "SlowEmaLine");
				PrintTo = PrintTo.OutputTab2;
				
				Print("set default");
			}
			else if (State == State.Configure)
			{
				//AddDataSeries("ADANIPORTS", Data.BarsPeriodType.Day, 1, Data.MarketDataType.Last);
				_atr = ATR(this.ATRPeriod);
				_atr.Lines = Lines.Where(l=>l.Name == "ATR").ToArray();
				_fast = EMA(FastEma);
				_fast.Plots = Plots.Where(p=>p.Name=="FastEmaLine").ToArray();
				_med = EMA(MedEma);
				_med.Plots = Plots.Where(p=>p.Name=="MedEmaLine").ToArray();
				_slow = EMA(SlowEma);
				_slow.Plots = Plots.Where(p=>p.Name=="SlowEmaLine").ToArray();
				base.AddChartIndicator(_atr);
				base.AddChartIndicator(_fast);
				base.AddChartIndicator(_med);
				base.AddChartIndicator(_slow);	
				Print("configured");
								
				
			}
		}

		protected override void OnBarUpdate()
		{	
			if(this.CurrentBar < Math.Max(SlowEma, ATRPeriod))
				return;
			
						
			var f=_fast.Value[0];
			var m =_med.Value[0];
			var s =_slow.Value[0];
				
		    var fp =_fast.Value[1];
			var mp =_med.Value[1];
			var sp =_slow.Value[1];
			
					
			bool isBuyable = f > m && m > s;
			bool isPreviousBuyable = fp > mp && mp > sp;
			
			bool isSellable =  f<m&&m < s;			
			
			Print("BAR: " + CurrentBar +  " at " + this.Time[0] + " isBuyable? " + isBuyable + " is prevBuyable? " + isPreviousBuyable); 
			if(IsFlat && isBuyable && !isPreviousBuyable){
				double entryPrice = Close[0];// - GetDiscountAmount;
				Print(" bar " + CurrentBar + " at " + entryPrice + " F:" + f + " close[0]: " + Close[0] +  "Close.GetV(CB-1): " + Close.GetValueAt(CurrentBar-1)  );//+ " Discount is "+ GetDiscountAmount  
				Buy(entryPrice);
			}
			
			else if(IsLong && isSellable){ // DropInClose , isSellable
				Print("Selling at bar "+ CurrentBar  + " at " + this.Time[0]);
				base.ExitLong();				
			}
			
		}
		
		void Buy(double entryPrice){
		  		
		  int shares = (int) (this.InitialCapital / entryPrice);
		  base.EnterLongLimit(shares, entryPrice, "B"+base.CurrentBar);	 			  
		  Print("Drawing dot for bar " + CurrentBar + " at " + entryPrice);
		  Draw.Dot(this, "Limit" + CurrentBar, true, 0,entryPrice, Brushes.Azure,true);
		  


		}
				
		
		

		#region helpers
		
		bool DropInClose { get { return (Close[0] -Close[1]) > 0 &&((Close[0] -Close[1])  <= ( _atr.Value[1]*ATRMultipleSL)); }}
		double GetDiscountAmount { get { return (_atr.Value[0] * this.ATRMultipleLimit);} }
		bool IsFlat { get { return base.Position.MarketPosition == MarketPosition.Flat; } }
		bool IsLong { get { return base.Position.MarketPosition == MarketPosition.Long; } }
		bool IsShort { get { return base.Position.MarketPosition == MarketPosition.Short; } }		
		bool TradingStocks { get { return base.Instrument.MasterInstrument.InstrumentType == InstrumentType.Stock; } }
		bool TradingFutures { get { return base.Instrument.MasterInstrument.InstrumentType == InstrumentType.Future; } }
		
		#endregion
		#region Properties
		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(ResourceType = typeof(Custom.Resource), Name="FastEMA", Description="period for ema", Order=1, GroupName="NinjaScriptStrategyParameters")]
		public int FastEma
		{ get; set; }

		[NinjaScriptProperty]
		[Range(3, int.MaxValue)]
		[Display(ResourceType = typeof(Custom.Resource), Name="MedEMA", Description="period for 2nd ema", Order=2, GroupName="NinjaScriptStrategyParameters")]
		public int MedEma
		{ get; set; }

		[NinjaScriptProperty]
		[Range(10, int.MaxValue)]
		[Display(ResourceType = typeof(Custom.Resource), Name="SlowEMA", Description="period for 3rd ema", Order=3, GroupName="NinjaScriptStrategyParameters")]
		public int SlowEma
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(ResourceType = typeof(Custom.Resource), Name="ATRPeriod", Order=4, GroupName="NinjaScriptStrategyParameters")]
		public int ATRPeriod
		{ get; set; }

		[NinjaScriptProperty]
		[Range(0, double.MaxValue)]
		[Display(ResourceType = typeof(Custom.Resource), Name="ATRMultipleSL", Description="At what multiple of ATR do we wanna SL", Order=5, GroupName="NinjaScriptStrategyParameters")]
		public double ATRMultipleSL
		{ get; set; }
		
		[NinjaScriptProperty]
		[Range(0, double.MaxValue)]
		[Display(ResourceType = typeof(Custom.Resource), Name="ATRMultipleLimit", Description="At what multiple of ATR do we wanna sell", Order=5, GroupName="NinjaScriptStrategyParameters")]
		public double ATRMultipleLimit
		{ get; set; }

		[NinjaScriptProperty]
		[Range(100, double.MaxValue)]
		[Display(ResourceType = typeof(Custom.Resource), Name="InitialCapital", Order=6, GroupName="NinjaScriptStrategyParameters")]
		public double InitialCapital
		{ get; set; }


		[Browsable(false)]
		[XmlIgnore]
		public Series<double> FastEmaLine
		{
			get { return Values[0]; }
		}

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> MedEmaLine
		{
			get { return Values[1]; }
		}

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> SlowEmaLine
		{
			get { return Values[2]; }
		}
		#endregion

	}
}
