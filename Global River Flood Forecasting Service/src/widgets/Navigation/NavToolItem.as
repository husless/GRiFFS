package widgets.Navigation
{

[Bindable]
public class NavToolItem
{
    public function NavToolItem(
        name:String = "",
        icon:String = "",
        visible:Boolean = false
        )
    {
        this.toolName = name;
        this.toolIcon = icon;
        this.toolVisible = visible;
    }

    public var toolName:String;
    public var toolIcon:String;
    public var toolVisible:Boolean;
}

}
