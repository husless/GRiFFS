package widgets.LeftNav
{
	
	import flash.events.Event;
	import flash.events.FocusEvent;
	import flash.events.KeyboardEvent;
	import flash.ui.Keyboard;
	
	import mx.core.ClassFactory;
	
	import spark.components.List;
	import spark.components.supportClasses.ItemRenderer;
	
	// these events bubble up from the BookmarkItemRenderer
	[Event(name="netcdfClick", type="flash.events.Event")]
	[Event(name="netcdfDelete", type="flash.events.Event")]
	
	public class NetCDFDataGroup extends List
	{
		public function NetCDFDataGroup()
		{
			super();
			this.itemRenderer = new ClassFactory(NetCDFItemRenderer);
			addEventListener(FocusEvent.FOCUS_IN, focusHandler);
			addEventListener(FocusEvent.FOCUS_OUT, focusHandler);
			arrowKeysWrapFocus = true;
		}
		
		//--------------------------------------------------------------------------
		//
		//  Focus Management
		//
		//--------------------------------------------------------------------------
		
		private function focusHandler(event:FocusEvent):void
		{
			if (event.type == FocusEvent.FOCUS_IN)
			{
				selectedIndex = 0;
			}
			else if (event.type == FocusEvent.FOCUS_OUT)
			{
				selectedIndex = -1;
			}
		}
		
		//--------------------------------------------------------------------------
		//
		//  Keyboard Management
		//
		//--------------------------------------------------------------------------
		
		private var keyboardPressed:Boolean = false;
		
		override protected function keyDownHandler(event:KeyboardEvent):void
		{
			super.keyDownHandler(event);
			
			if (event.keyCode != Keyboard.SPACE && event.keyCode != Keyboard.DELETE)
			{
				return;
			}
			
			keyboardPressed = true;
			event.updateAfterEvent();
		}
		
		override protected function keyUpHandler(event:KeyboardEvent):void
		{
			super.keyUpHandler(event);
			
			if (event.keyCode != Keyboard.SPACE && event.keyCode != Keyboard.DELETE)
			{
				return;
			}
			
			if (enabled && keyboardPressed)
			{
				keyboardPressed = false;
				var netcdf:NetCDF = selectedItem as NetCDF;
				var renderer:ItemRenderer = getItemRenderer(netcdf);
				
				if (renderer)
				{
					// Mimic mouse click on the button.
					/*
					if (event.keyCode == Keyboard.DELETE && netcdf.userCreated)
					{
						renderer.dispatchEvent(new Event("bookmarkDelete", true)); // bubbles
					}
					*/
					if (event.keyCode == Keyboard.SPACE)
					{
						renderer.dispatchEvent(new Event("netcdfClick", true)); // bubbles
					}
				}
			}
			event.updateAfterEvent();
		}
		
		private function getItemRenderer(item:Object):ItemRenderer
		{
			var n:int = dataGroup.numElements;
			for (var i:int = 0; i < n; i++)
			{
				var renderer:ItemRenderer = dataGroup.getElementAt(i) as ItemRenderer;
				if (renderer && renderer.data == item)
				{
					return renderer;
				}
			}
			return null;
		}
	}
	
}
