using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public static class ExtensionMethods
{
	public static bool IsNullOrEmpty<T>(this IEnumerable<T> list)
	{
		if (list == null || !list.Any())
			return true;
		return false;
	}
}
