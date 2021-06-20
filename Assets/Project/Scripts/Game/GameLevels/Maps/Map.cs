using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Map", menuName = "New Map")]
public class Map : ScriptableObject {
	public string mapName;
	public AudioClip themeSong;
	public Sprite mapBackground;
}
