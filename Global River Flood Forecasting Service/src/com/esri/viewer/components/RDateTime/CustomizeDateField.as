package com.esri.viewer.components.RDateTime
{	
	import com.esri.viewer.components.RDateTime.MyDateChooser;
	
	import flash.events.Event;
	import flash.events.KeyboardEvent;
	import flash.events.MouseEvent;
	import flash.ui.Keyboard;
	
	import mx.controls.Alert;
	import mx.controls.DateField;
	import mx.core.ClassFactory;
	import mx.events.CalendarLayoutChangeEvent;
	import mx.events.DropdownEvent;
	import mx.formatters.DateFormatter;
	import mx.states.OverrideBase;
	import com.esri.viewer.components.RDateTime.MyDateChooser;
	
	public class CustomizeDateField extends DateField
	{
		private var isOpend:Boolean = true;
		private var isSelectedTime:Boolean = true;
		[Bindable]
		private var _selectedDateTime:Date;
		public function set selectedDateTime(value:Date):void{
			_selectedDateTime = value;
		}
		public function get selectedDateTime():Date{
			if(_selectedDateTime == null)
				return new Date();
			else
				return _selectedDateTime;
		}
		
		public function CustomizeDateField()
		{
			super();
			this.formatString = "YYYY-MM-DD JJ:NN:SS";
			this.width = 150;
			this.dayNames=["日", "一", "二", "三", "四", "五", "六"];  
			this.monthNames=["一", "二", "三", "四", "五", "六", "七", "八", "九", "十", "十一", "十二"];  
			this.dropdownFactory=new ClassFactory(MyDateChooser);  
			this.labelFunction = formatDate;
			this.yearNavigationEnabled = true;
			this.editable = false;
			this.addEventListener(MouseEvent.CLICK, timeClickeHandler);	
			this.addEventListener(KeyboardEvent.KEY_DOWN, handelKeyDown);
			this.addEventListener(DropdownEvent.OPEN,timeOpendHandler);
		}
		private function formatDate(currentDate:Date):String  
		{
			var dateFormatter:DateFormatter=new DateFormatter();
			//YYYY-MM-DD JJ:NN:SS此中JJ表示格式位0-23。若换成HH则显示格式位0-24.  
			dateFormatter.formatString= this.formatString;  
			var chooser:MyDateChooser = this.dropdown as MyDateChooser;
			var parseDate:Date;
			var times:String;
			if (chooser.selectedDate == null)  
			{  
				parseDate = selectedDateTime;
				times = dateFormatter.format(parseDate);
				chooser.selectedDateTime = parseDate;
				chooser.selectedDate = parseDate;
			}  
			else
			{  
				parseDate = chooser.selectedDate;
				times = chooser.times.formatDateTemp(parseDate);
			}
			return times;
		}
		
		//当选择日期时，日历选择器在打开（默认选择日期后就回关闭）  
		private function showDateChooser(event:CalendarLayoutChangeEvent):void  
		{
			if(event.target is CustomizeDateField){
				if(isOpend){
					this.close();					
					isOpend = false;
					this.removeEventListener(CalendarLayoutChangeEvent.CHANGE, showDateChooser);
				}else{
					open();
					isOpend = true;
				}
			}
		} 
		private function timeClickeHandler(event:MouseEvent):void{
			if(this.isOpend){
				open();
				this.addEventListener(CalendarLayoutChangeEvent.CHANGE, showDateChooser);  
			}
		}
		
		private function timeOpendHandler(event:DropdownEvent):void{
			var chooser:MyDateChooser = this.dropdown as MyDateChooser;
			isOpend = true;
			chooser.times.setFocus();
		}
		
		//此处处理在时分秒下拉框出现时，按下回车键，下拉框不会收回去的bug  
		private function handelKeyDown(event:KeyboardEvent):void  
		{  
			if (event.keyCode == Keyboard.ENTER)  
			{  
				var tempNmsDateChooser:MyDateChooser=this.dropdown as MyDateChooser;  
				tempNmsDateChooser.times.nmsHour.close();  
				tempNmsDateChooser.times.nmsMinute.close();  
				tempNmsDateChooser.times.nmsSecond.close();  
			}  
		}
		
		public function getDate():Date{
			return DateFormatter.parseDateString(this.text);
		}
		public function getDateString():String{
			return this.text;
		}
	}
}