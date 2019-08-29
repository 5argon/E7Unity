using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System.Text;
using TMPro;

//Use this to make a game credits!
//You can use # , ##, etc. like markdown to make it bigger!
//Markdown # will not work if you don't have enough elements of markdownHashSize AND markdownHashColor.

[ExecuteAlways]
public class TextAssetToText : MonoBehaviour
{
    public TextAsset textAsset;
    public TextMeshProUGUI text;
    public bool enableMarkdown;
    [Tooltip("If not enabled, you have to right click the component's header and select Read Text.")]
    public bool autoRead;
    [Tooltip("Array index 0 correspond to 1 #'s size and so on.")]
    public int[] markdownHashSize;
    [Tooltip("Array index 0 correspond to 1 #'s size and so on.")]
    public Color[] markdownHashColor;

    private StringReader stringReader;
    private StringBuilder stringBuider;

    public void Update()
    {
        if (autoRead)
        {
            ReadText();
        }
    }

    public void ReadText(TextAsset ta)
    {
        this.textAsset = ta;
        ReadText();
    }

    [ContextMenu("Read Text")]
    public void ReadText()
    {
        if (textAsset != null)
        {
            if (enableMarkdown)
            {
                stringReader = new StringReader(textAsset.text);
                stringBuider = new StringBuilder();
                while (true)
                {
                    string read = stringReader.ReadLine();
                    if (read != null)
                    {
                        stringBuider.AppendLine(MarkdownToRichText(read));
                    }
                    else
                    {
                        break;
                    }
                }
                text.text = stringBuider.ToString();
            }
            else
            {
                text.text = textAsset.text;
            }
        }
    }

    private string MarkdownToRichText(string input)
        => ConvertBulletPoint(ConvertMDHash(input));

    /// <summary>
    /// Should comes last as it applies alignment.
    /// </summary>
    private string ConvertBulletPoint(string input)
    {
        if (input.IndexOf("- ") == 0)
        {
            return $"<align=\"left\">{input}</align>";
        }
        return input;
    }

    private string ConvertMDHash(string input)
    {
        int hashCount = 0;
        string original = input;
        while (true)
        {
            if (input.IndexOf('#') == 0)
            {
                hashCount++;
                input = input.Substring(1);
            }
            else
            {
                break;
            }
        }

        //Trim one space after the hash
        input = input.TrimStart(' ');
        if (hashCount > 0 && markdownHashSize.Length >= hashCount && markdownHashColor.Length >= hashCount)
        {
            return string.Format("<color={1}><size={0}>{2}</size></color>", markdownHashSize[hashCount - 1], HexConverter(markdownHashColor[hashCount - 1]), input);
        }
        else
        {
            return original;
        }
    }

    private static string HexConverter(Color c)
    {
        return string.Format("#{0:X2}{1:X2}{2:X2}{3:X2}", (int)(c.r * 255), (int)(c.g * 255), (int)(c.b * 255), (int)(c.a * 255));
    }
}
