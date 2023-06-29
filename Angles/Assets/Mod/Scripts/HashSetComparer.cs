using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class HashSetComparer<T> : IEqualityComparer<HashSet<T>> {
	public bool Equals(HashSet<T> itemA, HashSet<T> itemB) {
		return itemA.SetEquals(itemB);
	}

	public int GetHashCode(HashSet<T> item) {
		int hCode = 0;
		foreach(T i in item) { 
			hCode ^= i.GetHashCode();
		}
		return hCode.GetHashCode();
	}
}
