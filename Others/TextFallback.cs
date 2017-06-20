using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/*
If you are doing multilanguage Text, you might use Dynamic text.
But the texture generation time might be a problem when mostly it is
English so you might want to use non-dynamic mode.

But in character set mode there is no fallback font option.
So if suddenly you get Japanese, there is nothing to display since
the character is not in the texture of that font.

This script work with "ASCII default set" font. If there are any character
outside of that range, it will automatically change to fallback font.

If it is Arial (which is dynamic) probably you can surely display the text.
*/
public class TextFallback : Text{
	public Font fallbackFont;
}
