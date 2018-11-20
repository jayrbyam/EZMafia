using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class ClearPlayerPrefs {

	[MenuItem("Mine/Delete All PlayerPrefs")]
	static public void DeleteAllPlayerPrefs() {
		PlayerPrefs.DeleteAll();
	}
		
}
