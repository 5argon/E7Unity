using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
	
public struct Bool
{
	private byte Value;
	
	public static implicit operator Bool(bool b)
	{
		return new Bool() { Value = Convert.ToByte(b) };
	}
	
	public static implicit operator bool(Bool b)
	{
		return b.Value == 1;
	}
}
