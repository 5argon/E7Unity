using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class TextTagger {

    /// <summary>
    /// Example : FF0000
    /// </summary>
    public static string ColorTo6Hex(Color c) => string.Format("{0:X2}{1:X2}{2:X2}{3:X2}", (int)(c.r * 255), (int)(c.g * 255), (int)(c.b * 255), (int)(c.a * 255));

    /// <summary>
    /// Example : #FF0000
    /// </summary>
    public static string ColorToSharp6Hex(Color c) => $"#{ColorTo6Hex(c)}";

    /// <summary>
    /// Use it in Rich Text and the text will be colored.
    /// </summary>
	public static string ColorToRichTag(Color c, string content) => $"<color={ColorToSharp6Hex(c)}>{content}</color>";

}
