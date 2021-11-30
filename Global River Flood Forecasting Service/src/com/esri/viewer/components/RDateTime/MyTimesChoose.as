package com.esri.viewer.components.RDateTime
{	
	import com.esri.viewer.components.RDateTime.CustomizeDateField;
	
	import flash.events.Event;
	
	import mx.containers.HBox;
	import mx.containers.Panel;
	import mx.controls.ComboBox;
	import mx.controls.Label;
	import mx.formatters.DateFormatter;
	import com.esri.viewer.components.RDateTime.MyDateChooser;

	public class MyTimesChoose extends Panel
	{
		public var timesBox:HBox;
		
		public var labMinute:Label;
		public var labSecond:Label;
		
		public var nmsHour:MyComboBox;
		public var nmsMinute:MyComboBox;
		public var nmsSecond:MyComboBox;
		
		private var parseDate:Date;
		
		//小时  
		private var dataSourceHour:Array=["00", "01", "02", "03", "04", "05", "06", "07", "08", "09", "10", 
			"11", "12", "13", "14", "15", "16", "17", "18", "19", "20", "21", "22", "23"];
		//分钟和秒  
		private var dataSourceMinuteSecond:Array=["00", "01", "02", "03", "04", "05", "06", "07", "08", "09", 
			"10", "11", "12", "13", "14", "15", "16", "17", "18", "19", "20", "21", "22", "23", "24", "25", 
			"26", "27", "28", "29", "30", "31", "32", "33", "34", "35", "36", "37", "38", "39", "40", "41", 
			"42", "43", "44", "45", "46", "47", "48", "49", "50", "51", "52", "53", "54", "55", "56", "57", 
			"58", "59"];
				
		public function MyTimesChoose()
		{
			super();
			
			this.width = 195;
			this.height = 23;
			
			this.setStyle("headerHeight", 0);  
			this.setStyle("borderStyle", "none");  
			this.setStyle("borderThicknessLeft", 1);
			this.setStyle("borderThicknessTop", 1);
			this.setStyle("borderThicknessRight", 1);  
			this.setStyle("cornerRadius", 0);
			
			timesBox=new HBox();  
			timesBox.setStyle("horizontalGap", "0");  
			timesBox.setStyle("verticalGap", "0");  
			timesBox.setStyle("verticalAlign", "middle");  
			timesBox.setStyle("backgroundColor", "white");  
			timesBox.setStyle("paddingLeft", "5");  
			timesBox.setStyle("paddingBottom", "2");  
			timesBox.setStyle("borderStyle", "none");
		}
		
		protected override function createChildren():void
		{
			super.createChildren();
			var dateChooser:MyDateChooser=this.parent as MyDateChooser;
			if (!nmsHour)  
			{  
				nmsHour=new MyComboBox();
				nmsHour.maxTime = 23;
				nmsHour.setFocus();				
				nmsHour.dataProvider = dataSourceHour;  
				nmsHour.addEventListener("change", updateValue);
				if(dateChooser.selectedDateTime == null)
					nmsHour.selectedIndex = 0;
				else
					nmsHour.selectedIndex = dateChooser.selectedDateTime.getHours();
				//nmsHour.selectedIndex = 0;
			}  
			
			if (!labMinute)  
			{  
				labMinute=new Label();  
				labMinute.width=10;  
				labMinute.text=":";  
			}  
			
			if (!nmsMinute)  
			{  
				nmsMinute=new MyComboBox();  
				nmsMinute.maxTime = 59;
				nmsMinute.dataProvider=dataSourceMinuteSecond;  
				nmsMinute.addEventListener("change", updateValue);
				if(dateChooser.selectedDateTime == null)
					nmsMinute.selectedIndex = 0;
				else
					nmsMinute.selectedIndex = dateChooser.selectedDateTime.getMinutes();
				//nmsMinute.selectedIndex = 0;
			}  
			
			if (!labSecond)  
			{  
				labSecond=new Label();  
				labSecond.width=10;  
				labSecond.text=":";  
			}  
			
			if (!nmsSecond)  
			{  
				nmsSecond=new MyComboBox();
				nmsSecond.maxTime = 59;
				nmsSecond.dataProvider=dataSourceMinuteSecond;  
				nmsSecond.addEventListener("change", updateValue);  
				if(dateChooser.selectedDateTime == null)
					nmsSecond.selectedIndex = 0;
				else
					nmsSecond.selectedIndex = dateChooser.selectedDateTime.getSeconds();
				//nmsSecond.selectedIndex = 0;
			}  
			
			timesBox.addChild(nmsHour);  
			timesBox.addChild(labMinute);  
			timesBox.addChild(nmsMinute);  
			timesBox.addChild(labSecond);  
			timesBox.addChild(nmsSecond);  
			
			this.addChild(timesBox);  
		}
		
		//当下拉时分秒下拉框的值改变的时候，动态修改日期控制的textinput的显示值  
		private function updateValue(event:Event):void  
		{  
			if (this.parent is MyDateChooser)  
			{  
				var dateChooser:MyDateChooser=this.parent as MyDateChooser;  
				//若没有选择日期则默认为当天  
				if (dateChooser.selectedDate == null)  
				{  
					parseDate = new Date();  
				}  
				else  
				{  
					parseDate = dateChooser.selectedDate;  
				}  
				
				if (dateChooser.owner is CustomizeDateField)  
				{  
					var dateField:CustomizeDateField = dateChooser.owner as CustomizeDateField;  
					dateField.labelFunction = formatDateTemp;  
				}  
			}
		}
		//日期显示格式  
		public function formatDateTemp(date:Date):String  
		{  
			if (date == null)  
			{  
				date=new Date();  
			}  
			var df:DateFormatter=new DateFormatter();
			df.formatString= "YYYY-MM-DD JJ:NN:SS";
			if(nmsHour && nmsMinute && nmsSecond){
				date.hours=(Number)(nmsHour.selectedItem);
				date.minutes=(Number)(nmsMinute.selectedItem);
				date.seconds=(Number)(nmsSecond.selectedItem);
				
				var dateChooser:MyDateChooser=this.parent as MyDateChooser;
				if (dateChooser.owner is CustomizeDateField)  
				{  
					var dateField:CustomizeDateField = dateChooser.owner as CustomizeDateField;  
					df.formatString= dateField.formatString;
				}
				dateChooser.selectedDate = date;
				dateChooser.selectedDateTime = date;
				var times:String=df.format(date);  
				
				return times;
			}else{
				return df.format(date);
			}
		}
	}
}