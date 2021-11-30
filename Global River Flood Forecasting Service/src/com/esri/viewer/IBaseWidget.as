///////////////////////////////////////////////////////////////////////////
// Copyright (c) 2010-2011 Esri. All Rights Reserved.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//    http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
///////////////////////////////////////////////////////////////////////////
package com.esri.viewer
{

import com.esri.ags.Map;

import flash.events.IEventDispatcher;

/**
 * IBaseWidget defines the messages between WidgetManager and BaseWidget. It enables
 * Dependency Injection (DI) implementation that allows loose coupling the
 * detail widget logic.
 *
 * <p>Once a widget module is loaded into the container, the WidgetManager will
 * cast the instance of the module into IBaseWidget.</p>
 */
public interface IBaseWidget extends IEventDispatcher
{
    /**
     * Give a session generated ID to the widget so that the widget can be
     * identified during inter-component communication.
     *
     * @param value a number generated by the session.
     */
    function set widgetId(value:Number):void;
    function get widgetId():Number;

    /**
     * Widget title is coming from the configuration file. The title text is passed
     * into the widget implementation.
     *
     * @param value the title string from configuration file, config.xml
     */
    function set widgetTitle(value:String):void;
    function get widgetTitle():String;

    /**
     * A widget can have 40x40 icon image. The URL of the icon image file is specified
     * in the config.xml file.
     *
     * @param value the URL string of the icon image file. Flex supports JPG and PNG.
     */
    function set widgetIcon(value:String):void;
    function get widgetIcon():String;

    /**
     * A widget can have its own configuration file. The container and widget manager don't
     * have the knowledge of the configuration content. The URL of the configuration file is
     * specified in the config.xml. The widget is responsible for handling the configuration
     * file. This file can be in any format the widget developer prefer.
     *
     * @param value The URL of the configuration file.
     */
    function set config(value:String):void;
    function get config():String;

    /**
     * The XML type of configuration data.
     * @see configData
     */
    function set configXML(value:XML):void;
    function get configXML():XML;

    /**
     * For advanced widget developer, the global configuration data is available for use.
     *
     * @param value the global configuration data
     * @see ConfigData
     */
    function set configData(value:ConfigData):void;
    function get configData():ConfigData;

    /**
     * The widget manager can set the preload state of the widget.
     *
     * @param value the preload token string.
     */
    function setPreload(value:String):void;

    /**
     * Get the widget state.
     */
    function getState():String;

    /**
     * The widget manager can change the state of the widget, such as close the widget.
     * TODO: define standard state.
     *
     * @param value the state token string.
     */
    function setState(value:String):void;

    /**
     * Pass the map reference to the widget.
     *
     * @para value the map instance reference.
     */
    function set map(value:Map):void;
    function get map():Map;

    function get initialWidth():Number;
    function set initialWidth(value:Number):void;

    function get initialHeight():Number;
    function set initialHeight(value:Number):void;

    function setXYPosition(x:Number, y:Number):void;
    function setRelativePosition(left:String, right:String, top:String, bottom:String):void;

    function get horizontalCenter():Object;
    function set horizontalCenter(value:Object):void;

    function get verticalCenter():Object;
    function set verticalCenter(value:Object):void;

    function get isDraggable():Boolean;
    function set isDraggable(value:Boolean):void;

    function get isResizeable():Boolean;
    function set isResizeable(value:Boolean):void;

    function get isPartOfPanel():Boolean;
    function set isPartOfPanel(value:Boolean):void;

    function get proxyUrl():String;

	//ren修改
	function get scmid():String;
    function run():void;

    function setInfoConfig(value:String, waitForCreationComplete:Boolean = true):void
}

}
