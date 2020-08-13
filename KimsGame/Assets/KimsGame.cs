using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class KimsGame : MonoBehaviour {

	// todo:
	// * Make the knob turn upon selection
	// * Disable the knob when it shouldn't be pressed
	// * Textures (particularly of the belt holder)
	// * Sounds
	// * Disable the buttons in stage 1
	// * Reset(?) upon strike.

	[SerializeField] BombHelper _bombHelper;
	[SerializeField] KMBombModule _bombModule;

	[Space]

	[SerializeField] Sprite[] _possibleSprites;
	[SerializeField] SpriteRenderer[] _displayedSprites;
	[SerializeField] KMSelectable[] _spriteSelectables;
	[SerializeField] Renderer _beltRenderer;
	[SerializeField] KMSelectable _bigKnob;

	[Space]

	[SerializeField] int _numberOfWrongAnswers;
	[SerializeField] float _scrollSpeed;
	[SerializeField] float _delayBetweenStages;

	float[,] _coordinates;
	int _correctPresses = 0;
	List<KMSelectable> _pressedButtons = new List<KMSelectable>();

	bool _beltGoesUp;
	float _beltGoesUpTimer;
	Vector2 _beltTexturePosition;
	int _requiredPresses;

	const float MINX = -0.0679f;
	const float MAXX = 0.01529f;
	const float MINZ = -0.05345f;
	const float MAXZ = 0.0543f;
	const float TOPZ = 0.0701f;
	const float BOTTOMZ = -0.0687f;
	const float Y = 0.0208f;
	const float MINDISTANCE = 0.000288f;


	void Awake() {
		_coordinates = new float[_displayedSprites.Length, 2];

		foreach (SpriteRenderer sprR in _displayedSprites) {
			sprR.sprite = null;
		}
	}

	void Start () {
		PickSprites();
		CalculateSpritePositions(false);
		PlaceSpritesOnTop(false);
		AssignButtonPresses();
		_bombModule.OnActivate += OnActivate;
		_requiredPresses = _spriteSelectables.Length - _numberOfWrongAnswers;
	}

	/// <summary>
	/// Module activation (when the lights come on)
	/// </summary>
	void OnActivate() {
		StartCoroutine(ScrollSpritesIntoView(false));
	}

	/// <summary>
	/// Selects sprites to be remembered.
	/// </summary>
	void PickSprites() {
		List<Sprite> spriteList = _possibleSprites.ToList();
		for (int i = 0; i < _displayedSprites.Length; i++) {
			int index = UnityEngine.Random.Range(0, spriteList.Count);
			_displayedSprites[i].sprite = spriteList[index];
			Debug.LogFormat("[Kim’s Game #{0}] Selected {1} as a {2} answer.", _bombHelper.ModuleId, spriteList[index].name, (_displayedSprites.Length - i <= _numberOfWrongAnswers) ? "DECOY" : "correct");
			spriteList.RemoveAt(index);
		}
	}

	/// <summary>
	/// Finds a position for every sprite on the conveyor belt.
	/// </summary>
	/// <param name="answerMode">Whether it also needs to calculate positions for incorrect sprites.</param>
	void CalculateSpritePositions(bool answerMode) {
		int dudAnswers = answerMode ? 0 : _numberOfWrongAnswers;
		for (int i = 0; i < _spriteSelectables.Length - dudAnswers; i++) {
			Restart:
			float x = UnityEngine.Random.Range(MINX, MAXX);
			float z = UnityEngine.Random.Range(MINZ, MAXZ);

			for (int j = 0; j < i; j++) {
				float diffX = Mathf.Abs(_coordinates[j, 0] - x);
				float diffZ = Mathf.Abs(_coordinates[j, 1] - z);
				float pythagoras = diffX * diffX + diffZ * diffZ;
				if (pythagoras < MINDISTANCE) {
					// our sprite collides with another sprite. Reposition.
					goto Restart;
				}
			}
			_coordinates[i, 0] = x;
			_coordinates[i, 1] = z;
		}
	}

	/// <summary>
	/// Places every sprite at the top of the conveyor belt.
	/// </summary>
	void PlaceSpritesOnTop(bool answerMode) {
		for (int i = 0; i < _spriteSelectables.Length; i++) {
			_spriteSelectables[i].transform.localPosition = new Vector3(_coordinates[i, 0], Y, TOPZ);
			if (i >= _spriteSelectables.Length - _numberOfWrongAnswers) {
				_spriteSelectables[i].gameObject.SetActive(answerMode);
			}
			//_spriteSelectables[i].transform.localPosition = new Vector3(_coordinates[i, 0], Y, _coordinates[i, 1]);
		}

	}

	void AssignButtonPresses() {
		_bigKnob.OnInteract += delegate { _bombHelper.GenericButtonPress(_bigKnob, true, 0.5f); BigKnobTurned(); return false; };
		for (int i = 0; i < _spriteSelectables.Length; i++) {
			KMSelectable button = _spriteSelectables[i];
			if (i >= _spriteSelectables.Length - _numberOfWrongAnswers) {
				button.OnInteract += delegate { _bombHelper.GenericButtonPress(button, true, 0.1f); WrongButtonPress(button);  return false; };
			}
			else {
				button.OnInteract += delegate { _bombHelper.GenericButtonPress(button, true, 0.1f); CorrectButtonPress(button); return false; };
			}
		}
	}

	void BigKnobTurned() {
		// turning the big knob.
		Debug.LogFormat("[Kim’s Game #{0}] -- Turned the big knob. --", _bombHelper.ModuleId);
		StartCoroutine(ScrollSpritesOutOfView(false));
	}

	void CorrectButtonPress(KMSelectable button) {
		if (_correctPresses >= _requiredPresses) {
			return;
		}
		if (_pressedButtons.Contains(button)) {
			Debug.LogFormat("[Kim’s Game #{0}] Pressed {1} again.", _bombHelper.ModuleId, button.transform.GetChild(0).GetComponent<SpriteRenderer>().sprite.name);
			return;
		}
		_pressedButtons.Add(button);
		_correctPresses++;
		Debug.LogFormat("[Kim’s Game #{0}] Pressed {1} ({2}/{3}).", _bombHelper.ModuleId, button.transform.GetChild(0).GetComponent<SpriteRenderer>().sprite.name, _correctPresses, _requiredPresses);
		if (_correctPresses >= _requiredPresses) {
			Debug.LogFormat("[Kim’s Game #{0}] Solved.", _bombHelper.ModuleId);
			_bombModule.HandlePass();
		}
	}

	void WrongButtonPress(KMSelectable button) {
		if (_correctPresses >= _requiredPresses) {
			return;
		}
		Debug.LogFormat("[Kim’s Game #{0}] STRIKE: Pressed decoy {1}.", _bombHelper.ModuleId, button.transform.GetChild(0).GetComponent<SpriteRenderer>().sprite.name);
		_bombModule.HandleStrike();
	}

	/// <summary>
	/// Scrolls the conveyor belt and the sprites placed on it into view
	/// </summary>
	/// <param name="answerMode">Whether it also needs to scroll incorrect sprites.</param>
	/// <returns></returns>
	IEnumerator ScrollSpritesIntoView(bool answerMode) {
		int dudAnswers = answerMode ? 0 : _numberOfWrongAnswers;

		float largestTravelTime = TOPZ - MINZ;
		float elapsedTime = largestTravelTime;
		Vector3 position;
		Vector2 beltTexturePosition = _beltRenderer.material.GetTextureOffset("_MainTex");
		while (true) {
			float moveDistance = Time.deltaTime * _scrollSpeed;
			// scroll icons down
			for (int i = 0; i < _spriteSelectables.Length - dudAnswers; i++) {
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

	/// <summary>
	/// Scrolls the conveyor belt and the sprites placed on it into view
	/// </summary>
	/// <param name="answerMode">Whether it also needs to scroll incorrect sprites.</param>
	/// <returns></returns>
	IEnumerator ScrollSpritesOutOfView(bool answerMode) {
		int dudAnswers = answerMode ? 0 : _numberOfWrongAnswers;

		float largestTravelTime = MAXZ - BOTTOMZ;
		float elapsedTime = largestTravelTime;
		Vector3 position;
		Vector2 beltTexturePosition = _beltRenderer.material.GetTextureOffset("_MainTex");
		while (true) {
			float moveDistance = Time.deltaTime * _scrollSpeed;
			// scroll icons down
			for (int i = 0; i < _spriteSelectables.Length - dudAnswers; i++) {
				position = _spriteSelectables[i].transform.localPosition;
				position.z -= moveDistance;
				if (largestTravelTime - (_coordinates[i, 1] - BOTTOMZ) <= elapsedTime) {
					_spriteSelectables[i].transform.localPosition = position;
				}
			}
			// move the belt texture
			beltTexturePosition.y -= moveDistance * 7.9f;   // offset for conveyor belt size
			_beltRenderer.material.SetTextureOffset("_MainTex", beltTexturePosition);

			// finalize step
			elapsedTime -= moveDistance;
			if (elapsedTime <= 0) {
				break;
			}
			yield return null;
		}
		StartCoroutine(InBetweenStages());
	}



	void MoveBelt() {
		float moveDistance = Time.deltaTime * _scrollSpeed;
		_beltTexturePosition.y -= moveDistance * 7.9f * (_beltGoesUp ? -1 : 1);   // offset for conveyor belt size
		_beltRenderer.material.SetTextureOffset("_MainTex", _beltTexturePosition);

		if (_beltGoesUpTimer > 0) {
			_beltGoesUpTimer -= Time.deltaTime;
		}
		else {
			_beltGoesUp = false;
			float rand = Random.Range(0f, 1f);
			if (rand < 0.4f) {
				_beltGoesUp = true;
				_beltGoesUpTimer = .5f;
			}
			else {
				_beltGoesUpTimer = .5f;
			}
		}

	}

	IEnumerator InBetweenStages() {
		_beltTexturePosition = _beltRenderer.material.GetTextureOffset("_MainTex");
		yield return null;
		MoveBelt();
		float elapsedTime = 0;
		for (int i = 0; i < _spriteSelectables.Length; i++) {
		Restart:
			yield return null;
			MoveBelt();
			elapsedTime += Time.deltaTime;
			float x = UnityEngine.Random.Range(MINX, MAXX);
			float z = UnityEngine.Random.Range(MINZ, MAXZ);
			for (int j = 0; j < i; j++) {
				float diffX = Mathf.Abs(_coordinates[j, 0] - x);
				float diffZ = Mathf.Abs(_coordinates[j, 1] - z);
				float pythagoras = diffX * diffX + diffZ * diffZ;
				if (elapsedTime > _delayBetweenStages) {
					Debug.LogWarningFormat("[Kim’s Game #{0}] WARNING!!!!!! Took too long to generate board without overlaps between stages.", _bombHelper.ModuleId);
				}
				if (pythagoras < MINDISTANCE || elapsedTime > _delayBetweenStages) {
					// our sprite collides with another sprite. Reposition.
					goto Restart;
				}
				yield return null;
				MoveBelt();
				elapsedTime += Time.deltaTime;
			}
			_coordinates[i, 0] = x;
			_coordinates[i, 1] = z;
		}
		while (elapsedTime < _delayBetweenStages) {
			MoveBelt();
			yield return null;
			elapsedTime += Time.deltaTime;
		}

		PlaceSpritesOnTop(true);
		StartCoroutine(ScrollSpritesIntoView(true));
	}
}
