using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class KimsGame : MonoBehaviour {

	[SerializeField] BombHelper _bombHelper;
	[SerializeField] KMBombModule _bombModule;
	[SerializeField] Grids _grid;

	[Space]

	[SerializeField] Sprite[] _possibleSprites;
	[SerializeField] SpriteRenderer[] _displayedSprites;
	[SerializeField] KMSelectable[] _spriteSelectables;
	[SerializeField] Renderer _beltRenderer;
	[SerializeField] KMSelectable _bigKnob;
	[SerializeField] MovableObject _bigKnobMovable;

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
	bool _knobEnabled;
	bool _answerPhase;
	bool _struck;
	int _randomGridIndex;

	const float TOPZ = 0.0701f;
	const float BOTTOMZ = -0.0687f;
	const float MINX = -0.0679f;
	const float MAXX = 0.01529f;
	const float MINZ = -0.05345f;
	const float MAXZ = 0.0543f;
	const float Y = 0.0208f;			// Todo: if any of these numbers are changed, change them in Grids.cs too. Maybe make grids.cs less fragile.
	const float MINDISTANCE = 0.000288f;


	void Awake() {
		_coordinates = new float[_displayedSprites.Length, 2];

		foreach (SpriteRenderer sprR in _displayedSprites) {
			sprR.sprite = null;
		}
	}

	void Start () {
		_knobEnabled = false;
		_answerPhase = false;
		PickSprites();
		CalculateSpritePositions();
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

	void Reset() {
		_knobEnabled = false;
		_answerPhase = false;
		PickSprites();
		CalculateSpritePositions();
		PlaceSpritesOnTop(false);
		StartCoroutine(ScrollSpritesIntoView(false));
		_pressedButtons.Clear();
		_correctPresses = 0;
		_struck = false;
	}

	/// <summary>
	/// Selects sprites to be remembered.
	/// </summary>
	void PickSprites() {
		List<Sprite> spriteList = _possibleSprites.ToList();
		for (int i = 0; i < _displayedSprites.Length; i++) {
			int index = UnityEngine.Random.Range(0, spriteList.Count);
			_displayedSprites[i].sprite = spriteList[index];
			Debug.LogFormat("[Kim's Game #{0}] Selected {1} as a {2} answer.", _bombHelper.ModuleId, spriteList[index].name, (_displayedSprites.Length - i <= _numberOfWrongAnswers) ? "DECOY" : "correct");
			spriteList.RemoveAt(index);
		}
	}

	/// <summary>
	/// Finds a position for every sprite on the conveyor belt.
	/// </summary>
	/// <param name="answerMode">Whether it also needs to calculate positions for incorrect sprites.</param>
	void CalculateSpritePositions() {

		_randomGridIndex = Random.Range(0, _grid.IconGrids.Length);
		Debug.LogFormat("[Kim's Game #{0}] Using Grid Layout {1}.", _bombHelper.ModuleId, _randomGridIndex);
		IconGrid grid = _grid.IconGrids[_randomGridIndex];
		List<int> randomSpots = new List<int> { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19 };

		for (int i = 0; i < _spriteSelectables.Length; i++) {
			int randomSpotIndex = Random.Range(0, randomSpots.Count);
			int randomSpot = randomSpots[randomSpotIndex];
			_coordinates[i, 0] = grid.Coordinates[randomSpot].x;
			_coordinates[i, 1] = grid.Coordinates[randomSpot].z;
			randomSpots.RemoveAt(randomSpotIndex);
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
				button.OnInteract += delegate { 
					_bombHelper.GenericButtonPress(button, false, 0.1f); 
					WrongButtonPress(button); 
					_bombHelper.PlayGameSound(KMSoundOverride.SoundEffect.ButtonRelease, button.transform); 
					return false; 
				};
			}
			else {
				button.OnInteract += delegate { 
					_bombHelper.GenericButtonPress(button, false, 0.1f); 
					CorrectButtonPress(button); 
					_bombHelper.PlayGameSound(KMSoundOverride.SoundEffect.ButtonRelease, button.transform); 
					return false; 
				};
			}
		}
	}

	void BigKnobTurned() {
		if (!_knobEnabled) {
			return;
		}
		_knobEnabled = false;
		// turning the big knob.
		_bigKnobMovable.MoveToggleLoop();
		if (_answerPhase) {
			// reset module
			Debug.LogFormat("[Kim's Game #{0}] -- Turned the big knob again. Resetting module. {1} --", _bombHelper.ModuleId, _struck ? "A strike has already been given. We will not strike you again." : "STRIKE!!!!" );
			StartCoroutine(ScrollSpritesOutOfView(true));
			if (!_struck) {
				Debug.LogFormat("[Kim's Game #{0}] -- STRIKE --", _bombHelper.ModuleId);
				_bombModule.HandleStrike();
			}
		}
		else {
			Debug.LogFormat("[Kim's Game #{0}] -- Turned the big knob. --", _bombHelper.ModuleId);
			StartCoroutine(ScrollSpritesOutOfView(false));
		}
	}

	void CorrectButtonPress(KMSelectable button) {
		if (!_answerPhase) {
			return;
		}
		if (_correctPresses >= _requiredPresses) {
			return;
		}
		if (_pressedButtons.Contains(button)) {
			Debug.LogFormat("[Kim's Game #{0}] Pressed {1} again.", _bombHelper.ModuleId, button.transform.GetChild(0).GetComponent<SpriteRenderer>().sprite.name);
			return;
		}
		_pressedButtons.Add(button);
		_correctPresses++;
		Debug.LogFormat("[Kim's Game #{0}] Pressed {1} ({2}/{3}).", _bombHelper.ModuleId, button.transform.GetChild(0).GetComponent<SpriteRenderer>().sprite.name, _correctPresses, _requiredPresses);
		if (_correctPresses >= _requiredPresses) {
			Debug.LogFormat("[Kim's Game #{0}] Solved.", _bombHelper.ModuleId);
			_bombModule.HandlePass();
			_knobEnabled = false;
			//StartCoroutine(ScrollSpritesOutOfView(true));
			_bombHelper.PlayGameSound(KMSoundOverride.SoundEffect.CorrectChime, this.transform);
		}
	}

	void WrongButtonPress(KMSelectable button) {
		if (!_answerPhase) {
			return;
		}
		if (_correctPresses >= _requiredPresses) {
			return;
		}
		Debug.LogFormat("[Kim's Game #{0}] STRIKE: Pressed decoy {1}.", _bombHelper.ModuleId, button.transform.GetChild(0).GetComponent<SpriteRenderer>().sprite.name);
		_bombModule.HandleStrike();
		_struck = true;
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

		_bombHelper.StopCustomSound();
		_bombHelper.PlayCustomSoundWithRef("Belt_Down", _beltRenderer.transform);

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
		_bombHelper.StopCustomSound();
		_bombHelper.PlayCustomSound("Belt_Stop", _beltRenderer.transform);
		for (int i = 0; i < _spriteSelectables.Length - dudAnswers; i++) {
			_spriteSelectables[i].transform.localPosition = new Vector3(_coordinates[i, 0], Y, _coordinates[i, 1]);
		}
		_knobEnabled = true;
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
		
		_bombHelper.PlayCustomSoundWithRef("Belt_Down", _beltRenderer.transform);

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
		if (_correctPresses >= _requiredPresses) {
			_bombHelper.StopCustomSound();
			_bombHelper.PlayCustomSound("Belt_Stop", _beltRenderer.transform);
		}
		else if (answerMode) {
			Reset();
		}
		else {
			StartCoroutine(InBetweenStages());
		}
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
				_beltGoesUpTimer = UnityEngine.Random.Range(.4f, .6f);
				_bombHelper.StopCustomSound();
				_bombHelper.PlayCustomSoundWithRef("Belt_Up", _beltRenderer.transform);
			}
			else {
				_beltGoesUpTimer = UnityEngine.Random.Range(.4f, .6f);
				_bombHelper.StopCustomSound();
				_bombHelper.PlayCustomSoundWithRef("Belt_Down", _beltRenderer.transform);
			}
		}

	}

	IEnumerator InBetweenStages() {
		_beltGoesUpTimer = 1f;
		_beltTexturePosition = _beltRenderer.material.GetTextureOffset("_MainTex");
		yield return null;
		MoveBelt();
		float elapsedTime = 0;
		
		while (elapsedTime < _delayBetweenStages) {
			MoveBelt();
			yield return null;
			elapsedTime += Time.deltaTime;
		}
		_answerPhase = true;
		CalculateSpritePositions();
		PlaceSpritesOnTop(true);
		StartCoroutine(ScrollSpritesIntoView(true));
	}

	#region TwitchPlays

	#pragma warning disable 414
	public readonly string TwitchHelpMessage = "Turn the knob with '!{0} knob'. " +
		"Use '!{0} press 2 5 8' to press the 2nd, 5th, and 8th button from the bottom. Use '!{0} highlight 2 5 8' to highlight buttons to make sure you've got the ones you intend. "
		+ "Bottom most button is 1, top most button is 20.";
	#pragma warning restore 414

	int _currentGrid;
	List<KMSelectable> _selectablesSorted;
	Coroutine _twitchDehighlightRoutine;

	[Space]
	[SerializeField] float _tpHighlightTime;

	void SortGridForTP() {
		_currentGrid = _randomGridIndex;
		_selectablesSorted = _spriteSelectables.OrderBy(coord => coord.transform.position.z).ToList<KMSelectable>();
	}

	IEnumerator RemoveHighlights() {
		yield return new WaitForSeconds(_tpHighlightTime);
		foreach (KMSelectable selectable in _spriteSelectables) {
			selectable.transform.GetChild(1).GetChild(0).gameObject.SetActive(false);
		}
		_twitchDehighlightRoutine = null;
	}

	public IEnumerator ProcessTwitchCommand(string command) {
		command = command.ToLowerInvariant().Trim();

		if (command == "knob") {
			_bigKnob.OnInteract();
			yield return null;
		}
		else {
			List<string> split = command.Split(new char[] { ' ' }, System.StringSplitOptions.RemoveEmptyEntries).ToList();
			if (split[0] != "press" && split[0] != "highlight") {
				yield break;
			}
			if (split.Count <= 1) {
				yield break;
			}
			if (!_answerPhase) {
				yield return "sendtochat Not in answer phase.";
			}
			else {
				List<int> buttonsToPress = new List<int>();
				for (int i = 1; i < split.Count; i++) {
					int button;
					bool success = int.TryParse(split[i], out button);
					if (!success || button > _spriteSelectables.Length || button <= 0) {
						yield break;
					}
					buttonsToPress.Add(button);
				}
				if (_currentGrid != _randomGridIndex || _selectablesSorted == null) {
					SortGridForTP();
				}
				if (split[0] == "press") {
					foreach (int button in buttonsToPress) {
						yield return new WaitForSeconds(0.1f);
						_selectablesSorted[button - 1].OnInteract();
					}
				}
				else if (split[0] == "highlight") {
					foreach (int button in buttonsToPress) {
						_selectablesSorted[button - 1].transform.GetChild(1).GetChild(0).gameObject.SetActive(true);
					}
					yield return null;
					if (_twitchDehighlightRoutine != null) {
						StopCoroutine(_twitchDehighlightRoutine);
					}
					_twitchDehighlightRoutine = StartCoroutine(RemoveHighlights());
				}
			}
		}
	}

	IEnumerator TwitchHandleForcedSolve() {
		while (true) {
			if (_answerPhase) {
				break;
			}
			else if (!_answerPhase && _knobEnabled) {
				_bigKnob.OnInteract();
			}
			yield return true;
		}
		for (int i = 0; i < _requiredPresses; i++) {
			yield return new WaitForSeconds(0.1f);
			while (_spriteSelectables[i].transform.localPosition.z > MAXZ) {
				yield return true;
			}
			_spriteSelectables[i].OnInteract();
		}
	}



	#endregion

}
