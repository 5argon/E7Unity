using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Text.RegularExpressions;
using I2.Loc;

public class LocalizeBracket : MonoBehaviour {

	public Text text;

	void Start () {
		MatchEvaluator eval = new MatchEvaluator(GetLocalized);
		string newText = Regex.Replace(text.text,@"\{(.*)\}",eval);
		text.text = newText;
	}

	private string GetLocalized(Match match)
	{
		//Debug.Log(match.Groups[1].Value);
        return ScriptLocalization.Get(match.Groups[1].Value);
	}
}
