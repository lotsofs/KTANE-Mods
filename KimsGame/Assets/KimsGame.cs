using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class KimsGame : MonoBehaviour {

	[SerializeField] BombHelper _bombHelper;
	[SerializeField] KMBombModule _bombModule;

	[Space]

	[SerializeField] Sprite[] _possibleSprites;
	[SerializeField] SpriteRenderer[] _displayedSprites;


	void Awake() {
		foreach (SpriteRenderer sprR in _displayedSprites) {
			sprR.sprite = null;
		}
	}

	// Use this for initialization
	void Start () {
		_bombModule.OnActivate += PickSprites;
	}

	void PickSprites() {
		List<Sprite> spriteList = _possibleSprites.ToList();
		foreach (SpriteRenderer disR in _displayedSprites) {
			int index = UnityEngine.Random.Range(0, spriteList.Count);
			disR.sprite = spriteList[index];
			spriteList.RemoveAt(index);
		}
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
