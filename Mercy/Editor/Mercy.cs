/*
/// Copyright (c) 2016 Sirawat Pitaksarit / 5argon, Exceed7 Experiments LP
/// http://www.exceed7.com
*/

﻿using UnityEngine;
﻿using UnityEditor;
using System.Text;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text.RegularExpressions;

using System;

public class Mercy {

	private class GameObjectComponentProperty
	{
		private System.Type component;
		private PropertyInfo pi;
		public bool IsOk { get; private set;}

		public System.Type ComponentType
		{
			get{return component;}
		}

		public GameObjectComponentProperty(System.Type component, PropertyInfo pi)
		{
			this.component = component;
			this.pi = pi;
		}

		public bool CheckField(System.Type type)
		{
			return component == type;
		}

		public bool CheckPropertyName(string propertyName)
		{
			return pi.Name == propertyName;
		}

		public void MarkOkIfMatch(System.Type component, string propertyName)
		{
			if(this.component == component && pi.Name == propertyName)
			{
				IsOk = true;
			}
		}

		public override string ToString()
		{
			return component.Name + " <- " + pi.Name;
		}

	}

	[UnityEditor.Callbacks.DidReloadScripts]
	public static void CheckMercy()
	{
		//Debug.Log("Did someone call a doctor?");
		List<GameObjectComponentProperty> gcps = new List<GameObjectComponentProperty>();
		//Search all GameObjects in the scene for [Mercy] property.
		GameObject[] allObjects = UnityEngine.Object.FindObjectsOfType<GameObject>() ;
		foreach(GameObject go in allObjects)
		{
			foreach (MonoBehaviour script in go.GetComponents<MonoBehaviour>())
			{
				MonoScript ms = MonoScript.FromMonoBehaviour(script);
				System.Type type = ms.GetClass();
				PropertyInfo[] propertyInfo = type.GetProperties();
				foreach(PropertyInfo pi in propertyInfo)
				{
					object[] attributes = pi.GetCustomAttributes(true);
					for(int i = 0; i < attributes.Length; i++)
					{
						if(attributes[i] is MercyAttribute)
						{
							gcps.Add(new GameObjectComponentProperty(type,pi));
						}
					}
				}
			}
		}

		//Check against script that has [MercyEntry] n this scene
		foreach(GameObject go in allObjects)
		{
			foreach (MonoBehaviour script in go.GetComponents<MonoBehaviour>())
			{
				MonoScript ms = MonoScript.FromMonoBehaviour(script);
				System.Type type = ms.GetClass();
				foreach(object attribute in  type.GetCustomAttributes(true))
				{
					if(attribute is MercyEntryAttribute)
					{
						//Begin checking this class
						FieldInfo[] fieldInfo = type.GetFields();
						HashSet<FieldInfo> fieldDeclared = new HashSet<FieldInfo>();
						Dictionary<string,System.Type> stringToComponentType = new Dictionary<string,System.Type>();
						foreach(FieldInfo fi in fieldInfo)
						{
							//Debug.Log("Add " + fi.Name + " "    +  fi.FieldType);
							stringToComponentType.Add(fi.Name, fi.FieldType);
						}

						//Is the class with [Mercy] declared in this file?
						HashSet<System.Type> typeDeclared = new HashSet<System.Type>();
						HashSet<System.Type> typeNotDeclared = new HashSet<System.Type>();
						foreach(GameObjectComponentProperty gcp in gcps)
						{
							bool found = false;
							foreach(FieldInfo fi in fieldInfo)
							{
								if(gcp.CheckField(fi.FieldType))
								{
									found = true;
									fieldDeclared.Add(fi);
								}
							}
							if(found)
							{
								typeDeclared.Add(gcp.ComponentType);
							}
							else
							{
								typeNotDeclared.Add(gcp.ComponentType);
							}
						}

						/*
						foreach(System.Type t in typeDeclared)
						{
						//Debug.Log("You did well. (" + t.Name + ")");
					}
					*/
					foreach(System.Type t in typeNotDeclared)
					{
						Debug.LogError("Mercy : You forgot to declare (" + t.Name + ") in [MercyEntry]!");
					}

					Regex regex = new Regex("({)(.*)(})", RegexOptions.Singleline);
					Regex regexInner = new Regex("{(.*?)}", RegexOptions.Singleline);
					foreach (Match match in regex.Matches(ms.text))
					{
						foreach (Match match2 in regexInner.Matches(match.Groups[2].Value))
						{
							using (StringReader reader = new StringReader(match2.Value))
							{
								string line = string.Empty;
								do
								{
									line = reader.ReadLine();
									if (line != null)
									{
										if(line.Contains("="))
										{
											string leftSide = line.Split('=')[0];
											string[] dotSplit = leftSide.Split('.');
											if(dotSplit.Length == 2)
											{
												string leftDot = dotSplit[0].Trim();
												string rightDot = dotSplit[1].Trim();
												//Check assignment!

												foreach(GameObjectComponentProperty gcp in gcps)
												{
													//Debug.Log("LD " + leftDot + " RD " + rightDot);
													if(stringToComponentType.ContainsKey(leftDot))
													{
														gcp.MarkOkIfMatch(stringToComponentType[leftDot],rightDot);
													}
												}
											}
										}
									}

								} while (line != null);
							}
						}
					}
					//Now check if any is not ok..
					foreach(GameObjectComponentProperty gcp in gcps)
					{
						//Debug.Log("LD " + leftDot + " RD " + rightDot);
						if(gcp.IsOk)
						{
							//Debug.Log("Good! You injected " + gcp.ToString() + " !");
						}
						else
						{
							Debug.LogError("Mercy : You forgot to inject (" + gcp.ToString() + ") in [MercyEntry]!");
						}
					}


				}
			}
		}
	}

}

}
