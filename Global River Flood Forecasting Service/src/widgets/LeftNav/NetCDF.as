package widgets.LeftNav
{
	import flash.events.EventDispatcher;
	[Bindable]
	[RemoteClass(alias="widgets.LeftNav.NetCDF")]
	
	public class NetCDF extends EventDispatcher
	{
		public var name:String;		
		//public var icon:String;
		public var url:String;
		public var dateY:Number;
		public var org:String;
		
	}

}