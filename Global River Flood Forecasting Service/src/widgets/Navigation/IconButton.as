package widgets.Navigation
{

import spark.components.Button;

[Style(name="iconUp", type="*")]
[Style(name="iconOver", type="*")]
[Style(name="iconDown", type="*")]
[Style(name="iconDisabled", type="*")]

[Style(name="paddingLeft", type="Number")]
[Style(name="paddingRight", type="Number")]
[Style(name="paddingTop", type="Number")]
[Style(name="paddingBottom", type="Number")]

public class IconButton extends Button
{
    public function IconButton()
    {
        super();
    }
	
}

}
