using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.XR.WSA.Input;

public class KimsGame : MonoBehaviour {

	[SerializeField] BombHelper _bombHelper;
	[SerializeField] KMBombModule _bombModule;

	[Space]

	[SerializeField] Sprite[] _possibleSprites;
	[SerializeField] SpriteRenderer[] _displayedSprites;
	[SerializeField] KMSelectable[] _spriteSelectables;
	[SerializeField] Renderer _beltRenderer;

	float[,] _coordinates = new float[20, 2];

	const float MINX = -0.0679f;
	const float MAXX = 0.01529f;
	const float MINZ = -0.05345f;
	const float MAXZ = 0.0543f;
	const float TOPZ = 0.0701f;
	const float Y = 0.0208f;
	const float MINDISTANCE = 0.000288f;
	const float SCROLLSPEED = 0.01f;

	void Awake() {
		foreach (SpriteRenderer sprR in _displayedSprites) {
			sprR.sprite = null;
		}
	}

	// Use this for initialization
	void Start () {
		_bombModule.OnActivate += PickSprites;
		CalculateSpritePositions();
		PlaceSpritesOnTop();
		StartCoroutine(ScrollSpritesDown());
	}

	void PickSprites() {
		List<Sprite> spriteList = _possibleSprites.ToList();
		foreach (SpriteRenderer disR in _displayedSprites) {
			int index = UnityEngine.Random.Range(0, spriteList.Count);
			disR.sprite = spriteList[index];
			spriteList.RemoveAt(index);
		}
	}

	void CalculateSpritePositions() {
		for (int i = 0; i < _spriteSelectables.Length; i++) {
			Restart:
			float x = UnityEngine.Random.Range(MINX, MAXX);
			float z = UnityEngine.Random.Range(MINZ, MAXZ);

			for (int j = 0; j < i; j++) {
				float diffX = Mathf.Abs(_coordinates[j, 0] - x);
				float diffZ = Mathf.Abs(_coordinates[j, 1] - z);
				float pythagoras = diffX * diffX + diffZ * diffZ;
				if (pythagoras < MINDISTANCE) {
					// our sprite collides with another sprite. Reposition.
					Debug.Log("wrong " + i);
					goto Restart;
				}
			}
			_coordinates[i, 0] = x;
			_coordinates[i, 1] = z;
		}
	}

	void PlaceSpritesOnTop() {
		for (int i = 0; i < _spriteSelectables.Length; i++) {
			_spriteSelectables[i].transform.localPosition = new Vector3(_coordinates[i, 0], Y, TOPZ);
			//_spriteSelectables[i].transform.localPosition = new Vector3(_coordinates[i, 0], Y, _coordinates[i, 1]);
		}

	}

	IEnumerator ScrollSpritesDown() {
		float largestTravelTime = TOPZ - MINZ;
		float elapsedTime = largestTravelTime;
		Vector3 position;
		Vector2 beltTexturePosition = _beltRenderer.material.GetTextureOffset("_MainTex");
		while (true) {
			float moveDistance = Time.deltaTime * SCROLLSPEED;
			// scroll icons down
			for (int i = 0; i < _spriteSelectables.Length; i++) {
				position = _spriteSelectables[i].transform.localPosition;
				position.z -= moveDistance;
				if (TOPZ - _coordinates[i, 1] >= elapsedTime) {
					_spriteSelectables[i].transform.localPosition = position;
				}
			}
			// move the belt texture
			beltTexturePosition.y -= moveDistance * 7.9f;	// offset for conveyor belt size
			_beltRenderer.material.SetTextureOffset("_MainTex", beltTexturePosition);
			
			// finalize step
			elapsedTime -= moveDistance;
			if (elapsedTime <= 0) {
				break;
			}
			yield return null;
		}
	}
}
