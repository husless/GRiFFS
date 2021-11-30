package com.esri.viewer.components.RDateTime
{
	import flash.events.Event;
	
	
	import mx.collections.ArrayCollection;
	import mx.controls.ComboBox;
	import mx.controls.TextInput;

	public class MyComboBox extends ComboBox
	{
		
		//时分秒下拉框的填充色  
		private var arryBackgroundColor:Array=["white", "white", "white", "white"];
		private var dataArray:ArrayCollection = new ArrayCollection();
		[Bindable]public var maxTime:int = 23;
		public function MyComboBox()
		{
			super();
			this.restrict = "0-9";
			this.editable = true;			
			this.width=55;
			this.height=18;
			this.setStyle("fontSize", "10");  
			this.setStyle("cornerRadius", "0");
			this.setStyle("fillColors", arryBackgroundColor);
		}
		
		override protected function textInput_changeHandler(event:Event):void{ 
			super.textInput_changeHandler(event); 
			FilterByKey(event); 
		}
		
		//过滤数据 
		private function FilterByKey(event:Event):void{			
			//获取查询的数据
			var tempstr:String = this.text;				
			if(parseInt(tempstr) > maxTime)
				tempstr = maxTime.toString();
			if(tempstr.length > 2)
				tempstr = tempstr.substring(0,2);
			this.text = tempstr;
			this.dropdown.selectedIndex = parseInt(tempstr);
			this.dropdown.verticalScrollPosition = parseInt(tempstr);
			this.open(); 
		}
	}
}