package com.esri.viewer
{
	//ren修改
	import flash.net.URLVariables;
	
	import mx.rpc.events.FaultEvent;
	import mx.rpc.events.ResultEvent;
	import mx.rpc.http.HTTPService;

	public class AppUtil
	{
		public function AppUtil()
		{
		}
		public static function req(url:String,params:URLVariables,onResult:Function,onFault:Function=null,method:String="POST",
								   useProxy:Boolean=false,resultFormat:String="text",requestTimeout:int=600000):void
		{
			var httpservice:HTTPService=new HTTPService();
			httpservice.url=url;
			httpservice.method=method;
			httpservice.useProxy=useProxy;
			httpservice.resultFormat=resultFormat;
			httpservice.requestTimeout=requestTimeout;
			
			httpservice.addEventListener(ResultEvent.RESULT,onResult);
			if(onFault !=null)
				httpservice.addEventListener(FaultEvent.FAULT,onFault);
			httpservice.send(params);
		}
	}
}