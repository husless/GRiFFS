package com.esri.viewer.components.RDateTime
{
	import flash.events.MouseEvent;
	
	import mx.controls.DateChooser;
	import mx.events.CalendarLayoutChangeEvent;

	public class MyDateChooser extends DateChooser
	{
		public var times:MyTimesChoose;
		private var _selectedDateTime:Date;
		public function set selectedDateTime(value:Date):void{
			_selectedDateTime = value;
		}
		public function get selectedDateTime():Date{
			return _selectedDateTime;
		}
				
		public function MyDateChooser()
		{
			super();
			this.width = 195;
			this.height = 195;
			this.setStyle("fontsize","12");
			times = new MyTimesChoose();
			times.x = 0;
			times.y = this.height;
			this.showToday = true;
		}
		protected override function createChildren():void{
			super.createChildren();
			addChild(times);
		}
	}
}