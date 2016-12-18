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
using NinjaTrader.NinjaScript.DrawingTools;
#endregion

//This namespace holds Indicators in this folder and is required. Do not change it. 
namespace NinjaTrader.NinjaScript.Indicators
{
	public class ElderRay : Indicator
	{
		#region EMA Constants
		private double constant1;
		private double constant2;
		#endregion
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"Enter the description for your new custom Indicator here.";
				Name										= "ElderRay";
				Calculate									= Calculate.OnBarClose;
				IsOverlay									= false;
				DisplayInDataBox							= true;
				DrawOnPricePanel							= true;
				DrawHorizontalGridLines						= true;
				DrawVerticalGridLines						= true;
				PaintPriceMarkers							= true;
				ScaleJustification							= NinjaTrader.Gui.Chart.ScaleJustification.Right;
				//Disable this property if your indicator requires custom values that cumulate with each new market data event. 
				//See Help Guide for additional information.
				IsSuspendedWhileInactive					= true;
				Period						= 13;
				AddPlot(new Stroke(Brushes.LightSeaGreen, 2), PlotStyle.Bar, "BullPowerPlot");
				AddPlot(new Stroke(Brushes.BurlyWood, 2), PlotStyle.Bar, "BearPowerPlot");
				AddPlot(Brushes.Transparent, "EmaPlot");
				
			}
			else if (State == State.Configure)
			{				
				constant1 = 2.0 / (1 + Period);
				constant2 = 1 - (2.0 / (1 + Period));
			}
		}

		protected override void OnBarUpdate()
		{
			EmaPlot[0] = (CurrentBar == 0 ? Input[0] : Input[0] * constant1 + constant2 * EmaPlot[1]);
			BullPowerPlot[0] = High[0]-EmaPlot[0];
			BearPowerPlot[0] = Low[0]-EmaPlot[0];
		}

		#region Properties

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> BullPowerPlot
		{
			get { return Values[0]; }
		}

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> BearPowerPlot
		{
			get { return Values[1]; }
		}

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> EmaPlot
		{
			get { return Values[2]; }
		}
		
		[Range(1, int.MaxValue), NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Period", GroupName = "NinjaScriptParameters", Order = 0)]
		public int Period
		{ get; set; }
		#endregion

	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private ElderRay[] cacheElderRay;
		public ElderRay ElderRay(int period)
		{
			return ElderRay(Input, period);
		}

		public ElderRay ElderRay(ISeries<double> input, int period)
		{
			if (cacheElderRay != null)
				for (int idx = 0; idx < cacheElderRay.Length; idx++)
					if (cacheElderRay[idx] != null && cacheElderRay[idx].Period == period && cacheElderRay[idx].EqualsInput(input))
						return cacheElderRay[idx];
			return CacheIndicator<ElderRay>(new ElderRay(){ Period = period }, input, ref cacheElderRay);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.ElderRay ElderRay(int period)
		{
			return indicator.ElderRay(Input, period);
		}

		public Indicators.ElderRay ElderRay(ISeries<double> input , int period)
		{
			return indicator.ElderRay(input, period);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.ElderRay ElderRay(int period)
		{
			return indicator.ElderRay(Input, period);
		}

		public Indicators.ElderRay ElderRay(ISeries<double> input , int period)
		{
			return indicator.ElderRay(input, period);
		}
	}
}

#endregion
