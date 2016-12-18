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
	public class CesarSalad : Strategy
	{
		RSI _rsi;
		SMA _regime;
		ATR _atr;
		
		double _lowClose, _highClose;
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"Enter the description for your new custom Strategy here.";
				Name										= "CesarSalad";
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
				TraceOrders									= false;
				RealtimeErrorHandling						= RealtimeErrorHandling.StopCancelClose;
				StopTargetHandling							= StopTargetHandling.PerEntryExecution;
				BarsRequiredToTrade							= 20;
				// Disable this property for performance gains in Strategy Analyzer optimizations
				// See the Help Guide for additional information
				IsInstantiatedOnEachOptimizationIteration	= true;
				RangeHighLow					= 7;
				RangeMA					= 100;
				RSIEntryLevel					= 15;
				RSIExitLevel					= 30;
				RSIPeriod					= 5;
				ATRPeriod					= 10;
				ATRMultiple					= 0.5;
				InitialCapital					= 100000;
			}
			else if (State == State.Configure)
			{
				_rsi = RSI(this.RSIPeriod, 3);
				_rsi.Plots[1].Brush = Brushes.Brown;
				_rsi.Lines[0].Value =  this.RSIEntryLevel;
				_rsi.Lines[1].Value = this.RSIExitLevel;
				base.AddChartIndicator(_rsi);
				
				_regime = SMA(this.RangeMA);
				_regime.Plots[0].Width = 3;
				_regime.Plots[0].Brush = Brushes.AliceBlue;
				base.AddChartIndicator(_regime);
				
				_atr = ATR(this.ATRPeriod);
				base.AddChartIndicator(_atr);
				
				base.ClearOutputWindow();
				
			}
		}

		protected override void OnBarUpdate()
		{
			if(this.CurrentBar < this.RangeMA)
				return;
			SetState();
			if(IsFlat)
			{
			  if(IsRegimeBullish() && ReachedNewLowClose && ReachedLowRSILevel)
				  Buy();			  
			}
			else if(IsLong && ReachedNewHighClose && ReachedLongExitRSILevel)
			{
			 	base.ExitLong();	
			}
		}
		
		void SetState()
		{
		  _lowClose = MIN(this.Close, this.RangeHighLow)[1];
		  _highClose = MIN(this.Close, this.RangeHighLow)[1];
			
		  Draw.Line(this, "HIGH"+this.CurrentBar, 0, _highClose, 1, _highClose, Brushes.Orange);
		  Draw.Line(this, "LOW"+this.CurrentBar, 0, _lowClose, 1, _lowClose, Brushes.Blue);
		}
		
		
		void Buy()
		{
		  double entryPrice = this.TradingStocks ? Close[0] -(_atr[0] * this.ATRMultiple) : 10000;				  
		  int shares = (int) (this.InitialCapital / entryPrice);
		  base.EnterLongLimit(shares, entryPrice, "B"+base.CurrentBar);	  
		}
		

		bool IsRegimeBullish()	{	return Close[0] > _regime[0]; }		
		bool ReachedNewLowClose { get {return Close[0] < _lowClose;}}		
		bool ReachedLowRSILevel { get {return _rsi[0] < this.RSIEntryLevel;}}
		
		
		bool ReachedNewHighClose { get { return Close[0] > _highClose; }} 
		bool ReachedLongExitRSILevel { get { return _rsi[0] > this.RSIExitLevel; }}
		
		
		#region helpers
		bool IsFlat { get { return base.Position.MarketPosition == MarketPosition.Flat; } }
		bool IsLong { get { return base.Position.MarketPosition == MarketPosition.Long; } }
		bool IsShort { get { return base.Position.MarketPosition == MarketPosition.Short; } }		
		bool TradingStocks { get { return base.Instrument.MasterInstrument.InstrumentType == InstrumentType.Stock; } }
		bool TradingFutures { get { return base.Instrument.MasterInstrument.InstrumentType == InstrumentType.Future; } }
		
		#endregion
		
		#region Properties
		[NinjaScriptProperty]
		[Range(0, int.MaxValue)]
		[Display(ResourceType = typeof(Custom.Resource), Name="RangeHighLow", Description="Number of days to calc RSI high lows close. 0 will disable it.", Order=1, GroupName="NinjaScriptStrategyParameters")]
		public int RangeHighLow
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(ResourceType = typeof(Custom.Resource), Name="RangeMA", Description="Range of MA", Order=2, GroupName="NinjaScriptStrategyParameters")]
		public int RangeMA
		{ get; set; }

		[NinjaScriptProperty]
		[Range(0, double.MaxValue)]
		[Display(ResourceType = typeof(Custom.Resource), Name="RSIEntryLevel", Description="level to buy", Order=3, GroupName="NinjaScriptStrategyParameters")]
		public double RSIEntryLevel
		{ get; set; }

		[NinjaScriptProperty]
		[Range(0, double.MaxValue)]
		[Display(ResourceType = typeof(Custom.Resource), Name="RSIExitLevel", Description="RSI level to exit", Order=4, GroupName="NinjaScriptStrategyParameters")]
		public double RSIExitLevel
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(ResourceType = typeof(Custom.Resource), Name="RSIPeriod", Description="Period for RSI", Order=5, GroupName="NinjaScriptStrategyParameters")]
		public int RSIPeriod
		{ get; set; }

		[NinjaScriptProperty]
		[Range(0, int.MaxValue)]
		[Display(ResourceType = typeof(Custom.Resource), Name="ATRPeriod", Description="Period for ATR", Order=6, GroupName="NinjaScriptStrategyParameters")]
		public int ATRPeriod
		{ get; set; }

		[NinjaScriptProperty]
		[Range(0, double.MaxValue)]
		[Display(ResourceType = typeof(Custom.Resource), Name="ATRMultiple", Order=7, GroupName="NinjaScriptStrategyParameters")]
		public double ATRMultiple
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, double.MaxValue)]
		[Display(ResourceType = typeof(Custom.Resource), Name="InitialCapital", Order=8, GroupName="NinjaScriptStrategyParameters")]
		public double InitialCapital
		{ get; set; }
		#endregion

	}
}
