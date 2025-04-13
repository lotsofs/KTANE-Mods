﻿using raminrahimzada;
using System.Collections.Generic;
using System.Globalization;
using System.Collections;
using UnityEngine;
using System.Timers;
using System.Linq;

public class AnglesModule : MonoBehaviour {

	BombHelper _b;

	[SerializeField] TextMesh _displayText;
	[SerializeField] KMSelectable _buttonDisplayLeft;
	[SerializeField] KMSelectable _buttonDisplayRight;
	[SerializeField] KMSelectable _buttonNeedleLeftUp;
	[SerializeField] KMSelectable _buttonNeedleLeftDown;
	[SerializeField] KMSelectable _buttonNeedleMiddleUp;
	[SerializeField] KMSelectable _buttonNeedleMiddleDown;
	[SerializeField] KMSelectable _buttonNeedleRightUp;
	[SerializeField] KMSelectable _buttonNeedleRightDown;
	[SerializeField] KMSelectable _needle;

	private readonly int ANGLES_IN_PLAY = 9;
	private readonly decimal NEEDLE_SPEED = 5.0M;

	AngularNotation[] _selectableAngles;
	int _currentSelected = 0;
	int _solution = -1;
	decimal _supposedAngle = 0.0M;
	decimal _currentAngle = 0.0M;

	bool _solved = false;

	List<DecimalVector2> _submissions = new List<DecimalVector2>();

	bool _buttonHeld = false;
	Coroutine _needleTurnCoroutine;
	Coroutine _needleEjectCoroutine;
	//[SerializeField] Rigidbody _needlePhysics;

	AngularNotation GenerateAnyAngle() {
		//return new RelativeDirections(_b);
		int rand = Random.Range(0, 17);
		switch (rand) {
			case 0: return new RelativeDirections(_b);
			case 1: return new CompassPoints(_b);
			case 2: return new Arrows(_b);
			case 3: return new Degrees(_b);
			case 4: return new Radians(_b);
			case 5: return new Gradians(_b);
			case 6: return new Turns(_b);
			case 7: return new Hexacontades(_b);
			case 8: return new Pechus(_b);
			case 9: return new AkhnamZam(_b);
			case 10: return new Slopes(_b);
			case 11: return new CartesianCoordinates(_b);
			case 12: return new BAM(_b);
			case 13: return new TimeNotation(_b);
			case 14: return new DateNotation(_b);
			case 15: return new ZodiacSymbols(_b);
			case 16: return new FlagSemaphore(_b);
			default: return null;
		}
	}

	// Use this for initialization
	void Start() {
		_b = GetComponent<BombHelper>();

		_b.AddGenericButtonPresses(new List<float> { 0.2f, 0.2f, 0.1f, 0.1f, 0.1f, 0.1f, 0.1f, 0.1f, -1.0f });
		
		LogCompassNorth();
		GeneratePuzzle();

		_b.Module.OnActivate += OnActivate;
		_b.BombInfo.OnBombExploded += GeoGebraLog;
		_displayText.text = "";
	}

	void LogCompassNorth() {
		string firstLetter = _b.GetSerialFirstLetter().ToString();
		decimal piAmount;
		if ("ABCDEFG".Contains(firstLetter)) {
			_b.LogFormat("First letter in the serial is {0}, which is in sequence abcDefg for Dextral. North is to the right.", firstLetter);
		}
		else if ("HIJKLMNOP".Contains(firstLetter)) {
			_b.LogFormat("First letter in the serial is {0}, which is in sequence hijkLmnop for Left. North is to the left.", firstLetter);
		}
		else if ("QRSTUV".Contains(firstLetter)) {
			_b.LogFormat("First letter in the serial is {0}, which is in sequence qrstUv for Under. North is at the bottom.", firstLetter);
		}
		else if ("WXYZ".Contains(firstLetter)) {
			_b.LogFormat("First letter in the serial is {0}, which is in sequence wxyZ for Zenith. North is at the top.", firstLetter);
		}
		else {
			_b.Log("There are no letters in the serial number. There will be no compass points on this module.");
			return;
		}
	}

	void OnActivate() {
		_displayText.text = _selectableAngles[_currentSelected].Name;
		
		_buttonDisplayLeft.OnInteract += () => { 
			if (_solved) return false;
			_currentSelected = (_currentSelected + _selectableAngles.Length - 1) % _selectableAngles.Length; // Have to do this abomination of a formula because C#'s negative modulo makes no sense
			_displayText.text = _selectableAngles[_currentSelected].Name;
			return false; 
		};
		_buttonDisplayRight.OnInteract += () => {
			if (_solved) return false;
			_currentSelected = (_currentSelected + 1) % _selectableAngles.Length;
			_displayText.text = _selectableAngles[_currentSelected].Name;
			return false; 
		};

		_buttonNeedleLeftUp.OnInteract += () => { _needleTurnCoroutine = StartCoroutine(ButtonHeld(_buttonNeedleLeftUp)); return false; };
		_buttonNeedleLeftDown.OnInteract += () => { _needleTurnCoroutine = StartCoroutine(ButtonHeld(_buttonNeedleLeftDown)); return false; };
		_buttonNeedleMiddleUp.OnInteract += () => { _needleTurnCoroutine = StartCoroutine(ButtonHeld(_buttonNeedleMiddleUp)); return false; };
		_buttonNeedleMiddleDown.OnInteract += () => { _needleTurnCoroutine = StartCoroutine(ButtonHeld(_buttonNeedleMiddleDown)); return false; };
		_buttonNeedleRightUp.OnInteract += () => { _needleTurnCoroutine = StartCoroutine(ButtonHeld(_buttonNeedleRightUp)); return false; };
		_buttonNeedleRightDown.OnInteract += () => { _needleTurnCoroutine = StartCoroutine(ButtonHeld(_buttonNeedleRightDown)); return false; };

		_buttonNeedleLeftUp.OnInteractEnded += () => {		ButtonPressed(_buttonNeedleLeftUp); };
		_buttonNeedleLeftDown.OnInteractEnded += () => {	ButtonPressed(_buttonNeedleLeftDown); };
		_buttonNeedleMiddleUp.OnInteractEnded += () => {	ButtonPressed(_buttonNeedleMiddleUp); };
		_buttonNeedleMiddleDown.OnInteractEnded += () => {	ButtonPressed(_buttonNeedleMiddleDown); };
		_buttonNeedleRightUp.OnInteractEnded += () => {		ButtonPressed(_buttonNeedleRightUp); };
		_buttonNeedleRightDown.OnInteractEnded += () => {	ButtonPressed(_buttonNeedleRightDown); };

		_needle.OnInteract += () => { CheckSolution(); return false; };
	}

	void CheckSolution() {
		_submissions.Add(new DecimalVector2(DecimalMath.Cos(_currentAngle), DecimalMath.Sin(_currentAngle)));
		bool correct = _selectableAngles[_solution].Submit(_currentAngle);
		if (correct) {
			_solved = true;
			_b.Log("Module solved");
			_b.Solve();
			_needle.OnInteract = null;
			_needleEjectCoroutine = StartCoroutine(EjectNeedle());
			_displayText.text = "N/A";
			_b.PlayGameSound(KMSoundOverride.SoundEffect.ButtonRelease, this.transform);
			GeoGebraLog();
		}
		else {
			_b.Log("STRIKE!");
			_b.Strike();
		}
	}

	void ButtonPressed(KMSelectable button) {
		if (_solved) return;
		if (_buttonHeld == false) { return; }
		if (_needleTurnCoroutine != null ) {
			StopCoroutine(_needleTurnCoroutine);
			_needleTurnCoroutine = null;
		}
		if (button == _buttonNeedleLeftUp) {
			_supposedAngle += _selectableAngles[_currentSelected].LargeJump(true, _supposedAngle);
		}
		else if (button == _buttonNeedleLeftDown) {
			_supposedAngle += _selectableAngles[_currentSelected].LargeJump(false, _supposedAngle);
		}
		else if (button == _buttonNeedleMiddleUp) {
			_supposedAngle += _selectableAngles[_currentSelected].MediumJump(true, _supposedAngle);
		}
		else if (button == _buttonNeedleMiddleDown) {
			_supposedAngle += _selectableAngles[_currentSelected].MediumJump(false, _supposedAngle);
		}
		else if (button == _buttonNeedleRightUp) {
			_supposedAngle += _selectableAngles[_currentSelected].SmallJump(true, _supposedAngle);
		}
		else if (button == _buttonNeedleRightDown) {
			_supposedAngle += _selectableAngles[_currentSelected].SmallJump(false, _supposedAngle);
		}
	}

	IEnumerator EjectNeedle() {
		float timeElapsed = 0f;
		while (timeElapsed < 0.02f) {
			timeElapsed += Time.deltaTime;
			Vector3 pos = _needle.transform.localPosition;
			pos.y += Time.deltaTime;
			_needle.transform.localPosition = pos;
			yield return null;
		}
		while (timeElapsed < 0.5f) {
			timeElapsed += Time.deltaTime;
			yield return null;
		}
		while (timeElapsed < 4.0f) {
			timeElapsed += Time.deltaTime;
			Vector3 pos = _needle.transform.position;
			pos.z -= Time.deltaTime;
			_needle.transform.position = pos;
			yield return null;
		}
		//_needle.Parent = null;
		while (timeElapsed < 5.0f) {
			timeElapsed += Time.deltaTime;
			yield return null;
		}
		_needle.gameObject.SetActive(false);
		_needleEjectCoroutine = null;
	}

	IEnumerator ButtonHeld(KMSelectable button) {
		if (_solved) yield break;
		_buttonHeld = true;
		AngularNotation angle = _selectableAngles[_currentSelected];
		yield return new WaitForSeconds(0.5f);
		while (_currentAngle < 0) { _currentAngle += 2 * DecimalMath.Pi; }
		_currentAngle %= 2 * DecimalMath.Pi;
		if (button == _buttonNeedleLeftUp) {
			_supposedAngle = angle.LargeReset(true, _supposedAngle);
			if (_supposedAngle < _currentAngle) { _currentAngle -= 2 * DecimalMath.Pi; }
		}
		else if (button == _buttonNeedleLeftDown) {
			_supposedAngle = angle.LargeReset(false, _supposedAngle);
			if (_supposedAngle > _currentAngle) { _currentAngle += 2 * DecimalMath.Pi; }
		}
		else if (button == _buttonNeedleMiddleUp) {
			_supposedAngle = angle.MediumReset(true, _supposedAngle);
			if (_supposedAngle < _currentAngle) { _currentAngle -= 2 * DecimalMath.Pi; }
		}
		else if (button == _buttonNeedleMiddleDown) {
			_supposedAngle = angle.MediumReset(false, _supposedAngle);
			if (_supposedAngle > _currentAngle) { _currentAngle += 2 * DecimalMath.Pi; }
		}
		else if (button == _buttonNeedleRightUp) {
			_supposedAngle = angle.SmallReset(true, _supposedAngle);
			if (_supposedAngle < _currentAngle) { _currentAngle -= 2 * DecimalMath.Pi; }
		}
		else if (button == _buttonNeedleRightDown) {
			_supposedAngle = angle.SmallReset(false, _supposedAngle);
			if (_supposedAngle > _currentAngle) { _currentAngle += 2 * DecimalMath.Pi; }
		}
		_b.PlayGameSound(KMSoundOverride.SoundEffect.ButtonPress, this.transform);
		_buttonHeld = false;
		_needleTurnCoroutine = null;
	}

	void GeneratePuzzle() {
		int attempts = 0;
		int maxAttempts = 10;
		int eliminationRound = 0;

		HashSetComparer<AngularNotation> hSC = new HashSetComparer<AngularNotation>();

		List<string> logging = new List<string>();
		List<AngularNotation> shrinkingAngleList = new List<AngularNotation>();
		Dictionary<HashSet<AngularNotation>, long> femtoDistances = new Dictionary<HashSet<AngularNotation>, long>(hSC);

		// Start from the beginning
	retry:
		attempts++;
		logging.Clear();
		shrinkingAngleList.Clear();
		femtoDistances.Clear();
		eliminationRound = 0;

		if (attempts >= maxAttempts) {
			_b.LogWarning(string.Format("Failed to generate a puzzle after {0} attempts. Giving up.", attempts));
			_b.Solve();
			return;
		}

		_b.Log("=========Generating Angles==============================");
		// Generate angles, avoiding duplicates
		while (shrinkingAngleList.Count < ANGLES_IN_PLAY) { 
			AngularNotation a = GenerateAnyAngle();
			if (AngleListContains(shrinkingAngleList, a)) continue;
			if (a.Position.X == 0.0M && a.Position.Y == 0.0M) continue;
			shrinkingAngleList.Add(a);

			_b.Log(string.Format("Generated angle \"{0}\" with point position ( {1:0.00000000} , {2:0.00000000} )", a.Name, a.Position.X, a.Position.Y));
		}

		_selectableAngles = new AngularNotation[shrinkingAngleList.Count];
		shrinkingAngleList.CopyTo(_selectableAngles);

		NumberFormatInfo nfi = (NumberFormatInfo)CultureInfo.InvariantCulture.NumberFormat.Clone();
		nfi.NumberGroupSeparator = " ";

		_b.Log("=========Calculating Distances==========================");
		// Calculate distances between all points
		for (int from = 0; from < shrinkingAngleList.Count; from++) {
			AngularNotation angleA = shrinkingAngleList[from];
			for (int to = from + 1; to < shrinkingAngleList.Count; to++) {
				AngularNotation angleB = shrinkingAngleList[to];
				decimal distance = DecimalVector2.Distance(angleA.Position, angleB.Position);
				long femtoDistance = (long)(distance * 1000000000000000);
				HashSet<AngularNotation> key = new HashSet<AngularNotation> { angleA, angleB };
				femtoDistances.Add(key, femtoDistance);

				_b.Log(string.Format("{2} femtounits between \"{0}\" and \"{1}\"", angleA.Name, angleB.Name, femtoDistance.ToString("#,0", nfi)));
			}
		}

		_b.Log("=========Finding Shortest Distance======================");
	findShortestDistance:
		// Find the shortest distance
		List<HashSet<AngularNotation>> shortest = new List<HashSet<AngularNotation>>();
		eliminationRound++;

		for (int from = 0; from < shrinkingAngleList.Count; from++) {
			for (int to = from + 1; to < shrinkingAngleList.Count; to++) {
				HashSet<AngularNotation> set = new HashSet<AngularNotation> { shrinkingAngleList[from], shrinkingAngleList[to] };
				long distance = femtoDistances[set];
				if (shortest.Count == 0) {
					shortest.Add(set);
					//Debug.LogFormat("FIRST SHORTEST DISTANCE IS {0} TO {1}: {2}", _angularNotationList[from].Name, _angularNotationList[to].Name, distance.ToString("#,0", nfi));
					continue;
				}
				if (distance < femtoDistances[shortest[0]]) {
					shortest.Clear();
					shortest.Add(set);
					//Debug.LogFormat("A NEW SHORTEST DISTANCE HAS BEEN FOUND BETWEEN {0} AND {1}: {2}", _angularNotationList[from].Name, _angularNotationList[to].Name, distance.ToString("#,0", nfi));
					continue;
				}
				if (distance == femtoDistances[shortest[0]]) {
					shortest.Add(set);
					//Debug.LogFormat("A TIE HAS BEEN DISCOVERED FROM {0} TO {1}: {2}", _angularNotationList[from].Name, _angularNotationList[to].Name, distance.ToString("#,0", nfi));
					continue;
				}
			}
		}

		// Eliminate the shortest distance
		if (shortest.Count == 1) {
			_b.Log(string.Format("The shortest distance is {0} femtounits", femtoDistances[shortest[0]].ToString("#,0", nfi)));
			foreach (AngularNotation a in shortest[0]) {
				_b.Log(string.Format("Eliminating \"{0}\"", a.Name));
				a.PointName = "Elim" + eliminationRound;
				shrinkingAngleList.Remove(a);
			}
		}
		else if (shortest.Count > 1) { // Tiebreakers
			_b.Log(string.Format("There is a {0}-way tie for shortest distance which is {1} femtounits", shortest.Count, femtoDistances[shortest[0]].ToString("#,0", nfi)));
			bool allZero = true;
			foreach (var h in shortest) {
				if (femtoDistances[h] > 0) {
					allZero = false;
				}
			}

			List<AngularNotation> involvedPoints = new List<AngularNotation>();
			List<AngularNotation> doublyInvolvedPoints = new List<AngularNotation>();
			foreach (HashSet<AngularNotation> set in shortest) {
				foreach (AngularNotation a in set) {
					if (involvedPoints.Contains(a)) { doublyInvolvedPoints.Add(a); }
					else { involvedPoints.Add(a); }
				}
			}
			
			if (allZero) {
				// All involved distances are within each other.
				// Eg: |*    *****    *    *    |
				if (involvedPoints.Count % 2 == 1) {
					// There's an odd number, and there's ambiguity as to which ones to eliminate (must be even)
					_b.Log(string.Format("This is an odd numbered tie for length 0. Throwing it out."));
					foreach (string s in logging) { _b.LogHidden(s); }
					goto retry;
				}
				// Eliminate the points
				for (int i = 0; i < involvedPoints.Count; i++) {
					_b.Log(string.Format("Eliminating \"{0}\"", involvedPoints[i].Name));
					involvedPoints[i].PointName = "Elim" + eliminationRound;
					shrinkingAngleList.Remove(involvedPoints[i]);
				}
				goto checkRemainingAnglesCount;
			}

			if (doublyInvolvedPoints.Count > 1) {
				for (int from = 0; from < doublyInvolvedPoints.Count; from++) {
					for (int to = from + 1; to < doublyInvolvedPoints.Count; to++) {
						HashSet<AngularNotation> itemA = new HashSet<AngularNotation> { shrinkingAngleList[from], shrinkingAngleList[to] };
						foreach (HashSet<AngularNotation> itemB in shortest) {
							if (hSC.Equals(itemA, itemB)) {
								// Some involved points are in a sequence of longer than 3, which leads to ambiguities. Start over.
								// Eg: |*    *    *--*--*--*    *    *    *    |
								_b.Log(string.Format("There is an ambiguity because of a long sequence of subsequent near points. Throwing it out."));
								foreach (string s in logging) { _b.LogHidden(s); }
								goto retry;
							}
						}
					}
				}
			}

			if (doublyInvolvedPoints.Count == 0) {
				// Each distance involves separate points. We can just toss them all. We are able to eliminate all these without issue.
				// Eg: |*    *--*    *--*    *    *--*    *    |
				foreach (HashSet<AngularNotation> set in shortest) {
					foreach (AngularNotation a in set) {
						_b.Log(string.Format("Eliminating \"{0}\"", a.Name));
						a.PointName = "Elim" + eliminationRound;
						shrinkingAngleList.Remove(a);
					}
				}
				goto checkRemainingAnglesCount;
			}
			if (involvedPoints.Count == shortest.Count * 1.5f) {
				goto retry;
				//_b.Log("There is a tie where two points are both of equal distance to a shared third point.");
				//// Special rule with three point ties. Eliminate the outers. (Should be all that aren't doubly assigned)
				//// Eg: |*    *--*--*    *    *--*--*    *    |
				//foreach (AngularNotation a in involvedPoints) {
				//	if (!doublyInvolvedPoints.Contains(a)) {
				//		_b.Log(string.Format("Eliminating \"{0}\"", a.Name));
				//		a.PointName = "E" + eliminationRound;
				//		shrinkingAngleList.Remove(a);
				//	}
				//}
				//goto checkRemainingAnglesCount;
			}
			// All cases should've been covered above already, but just in case we encounter an odd one, throw it all away
			foreach (string s in logging) { _b.Log(s); }
			_b.LogWarning("WARNING: Encountered an unexpected case in the puzzle generation. Starting over.");
			goto retry;
		}

	checkRemainingAnglesCount:
		if (shrinkingAngleList.Count % 2 == 0) {
			foreach (string s in logging) { _b.Log(s); }
			_b.LogWarning("WARNING: An error occured during puzzle generation. At some point during validation, remaining angles was even.");
			goto retry;
		}
		_b.Log(string.Format("---------{0} angle{1} remain{2}--------------------------------", shrinkingAngleList.Count, shrinkingAngleList.Count == 1 ? "" : "s", shrinkingAngleList.Count == 1 ? "s" : ""));
		if (shrinkingAngleList.Count > 1) {
			goto findShortestDistance;
		}

		shrinkingAngleList[0].PointName = "Solution";
		_solution = _selectableAngles.IndexOf(x => x == shrinkingAngleList[0]);
		_b.Log(string.Format("Generated a puzzle in {0} attempt{2}. Solution: {1}", attempts, shrinkingAngleList[0].Name, attempts == 1 ? "" : "s"));
		foreach (string s in logging) { _b.Log(s); }

		GeoGebraLog();
	}

	void GeoGebraLog() {
		string geoGebraInput = "Execute[{";
		for (int i = 0; i < _selectableAngles.Length; i++) {
			geoGebraInput += string.Format("\"{0}_{{{1}}}=({2},{3})\",", _selectableAngles[i].PointName, _selectableAngles[i].Name, _selectableAngles[i].Position.X, _selectableAngles[i].Position.Y);
		}
		for (int i = 0; i < _submissions.Count; i++) {
			string name = !_solved || (i < _submissions.Count - 1) ? "Submission_{Strike " + i + 1 + "}" : "Submission_{Correct}";
			string color = !_solved || (i < _submissions.Count - 1) ? "red" : "green";
			geoGebraInput += string.Format("\"{0}=({1},{2})\",", name, _submissions[i].X, _submissions[i].Y);
			geoGebraInput += string.Format("\"SetColor({0},{1})\",", name, color);
		}

		geoGebraInput += "\"x^2+y^2=1\"}]";
		_b.Log("GeoGebra:");
		_b.Log(geoGebraInput);
		geoGebraInput = geoGebraInput.Replace("%", "%25");
		geoGebraInput = geoGebraInput.Replace(" ", "%20");
		geoGebraInput = geoGebraInput.Replace("\"", "%22");
		geoGebraInput = geoGebraInput.Replace("+", "%2B");
		_b.Log("https://www.geogebra.org/calculator?command=" + geoGebraInput);
	}

	bool AngleListContains(List<AngularNotation> list, AngularNotation a) {
		foreach (AngularNotation prevA in list) {
			if (prevA.Name == a.Name) return true;
		}
		return false;
	}

	void OnDestroy() {
		//if (_needlePhysics != null) Destroy(_needlePhysics.gameObject);
		if (_needleTurnCoroutine != null) { StopCoroutine(_needleTurnCoroutine); }
		if (_needleEjectCoroutine != null) { StopCoroutine(_needleEjectCoroutine); }
	}

	// Update is called once per frame
	void Update() {
		if (_supposedAngle == _currentAngle) return;
		decimal oldAngle = _currentAngle;
		if (_currentAngle < _supposedAngle) {
			_currentAngle += (decimal)Time.deltaTime * NEEDLE_SPEED;
			if (_currentAngle > _supposedAngle) _currentAngle = _supposedAngle;
		}
		else {
			_currentAngle -= (decimal)Time.deltaTime * NEEDLE_SPEED;
			if (_currentAngle < _supposedAngle) _currentAngle = _supposedAngle;
		}
		float currentAngleDegrees = (float)_currentAngle * 180.0f / Mathf.PI;
		float oldAngleDegrees = (float)oldAngle * 180.0f / Mathf.PI;
		float difference = currentAngleDegrees - oldAngleDegrees;

		_needle.transform.Rotate(Vector3.up, -difference);
	}

	#region twitchplays
	
	public readonly string TwitchHelpMessage = "'!{0} pull' to pull the needle. '!{0} [left|right] #' to press the main left or right button # times, or once if no # specified. '!{0} [TL|BL|TM|BM|TR|BR] #' for a button in the bottom right. '!{0} [HTL|HBL|HTM|HBM|HTR|HBR]' to hold a bottom right button. Commands can be combined: '!{0} left 2 HTL TM TR 5'. Also usable: '!{0} cycle'.";

	public IEnumerator ProcessTwitchCommand(string command) {
		command = command.Replace("[", "").Replace("]", "");
		command = command.ToUpperInvariant().Trim();
		List<string> split = command.Split(new char[] { ' ' }, System.StringSplitOptions.RemoveEmptyEntries).ToList();
		string previousCommand = "";
		List<string> commands = new List<string>();

		foreach (string c in split) {
			int n = 0;
			if (int.TryParse(c, out n)) {
				if (string.IsNullOrEmpty(previousCommand)) {
					yield break;
				}
				for (int i = 1; i < n; i++) { // we've already done this once, hence i = 1.
					commands.Add(previousCommand);
				}
				previousCommand = "";
				continue;
			}
			switch (c) {
				case "LEFT":
				case "L":
				case "RIGHT":
				case "R":
				case "TL":
				case "UL":
				case "LU":
				case "LT":
				case "BL":
				case "DL":
				case "LB":
				case "LD":
				case "TM":
				case "MT":
				case "UM":
				case "MU":
				case "DM":
				case "MD":
				case "BM":
				case "MB":
				case "TR":
				case "UR":
				case "RT":
				case "RU":
				case "DR":
				case "BR":
				case "RD":
				case "RB":
				case "HTL":
				case "HUL":
				case "HLU":
				case "HLT":
				case "HBL":
				case "HDL":
				case "HLB":
				case "HLD":
				case "HTM":
				case "HMT":
				case "HUM":
				case "HMU":
				case "HDM":
				case "HMD":
				case "HBM":
				case "HMB":
				case "HTR":
				case "HUR":
				case "HRT":
				case "HRU":
				case "HDR":
				case "HBR":
				case "HRD":
				case "HRB":
				case "PULL":
				case "CYCLE":
					previousCommand = c;
					commands.Add(c); 
					break;
				default:
					// unknown command
					yield break;
			}
		}
		foreach (string c in commands) {
			switch (c) {
				case "LEFT":
				case "L":
					_buttonDisplayLeft.OnInteract();
					break;
				case "RIGHT":
				case "R":
					_buttonDisplayRight.OnInteract();
					break;
				case "TL":
				case "UL":
				case "LU":
				case "LT":
					_buttonNeedleLeftUp.OnInteract();
					yield return new WaitForSeconds(0.05f);
					_buttonNeedleLeftUp.OnInteractEnded();
					break;
				case "BL":
				case "DL":
				case "LB":
				case "LD":
					_buttonNeedleLeftDown.OnInteract();
					yield return new WaitForSeconds(0.05f);
					_buttonNeedleLeftDown.OnInteractEnded();
					break;
				case "TM":
				case "MT":
				case "UM":
				case "MU":
					_buttonNeedleMiddleUp.OnInteract();
					yield return new WaitForSeconds(0.05f);
					_buttonNeedleMiddleUp.OnInteractEnded();
					break;
				case "DM":
				case "MD":
				case "BM":
				case "MB":
					_buttonNeedleMiddleDown.OnInteract();
					yield return new WaitForSeconds(0.05f);
					_buttonNeedleMiddleDown.OnInteractEnded();
					break;
				case "TR":
				case "UR":
				case "RT":
				case "RU":
					_buttonNeedleRightUp.OnInteract();
					yield return new WaitForSeconds(0.05f);
					_buttonNeedleRightUp.OnInteractEnded();
					break;
				case "DR":
				case "BR":
				case "RD":
				case "RB":
					_buttonNeedleRightDown.OnInteract();
					yield return new WaitForSeconds(0.05f);
					_buttonNeedleRightDown.OnInteractEnded();
					break;
				case "HTL":
				case "HUL":
				case "HLU":
				case "HLT":
					_buttonNeedleLeftUp.OnInteract();
					yield return new WaitForSeconds(0.6f);
					_buttonNeedleLeftUp.OnInteractEnded();
					break;
				case "HBL":
				case "HDL":
				case "HLB":
				case "HLD":
					_buttonNeedleLeftDown.OnInteract();
					yield return new WaitForSeconds(0.6f);
					_buttonNeedleLeftDown.OnInteractEnded();
					break;
				case "HTM":
				case "HMT":
				case "HUM":
				case "HMU":
					_buttonNeedleMiddleUp.OnInteract();
					yield return new WaitForSeconds(0.6f);
					_buttonNeedleMiddleUp.OnInteractEnded();
					break;
				case "HDM":
				case "HMD":
				case "HBM":
				case "HMB":
					_buttonNeedleMiddleDown.OnInteract();
					yield return new WaitForSeconds(0.6f);
					_buttonNeedleMiddleDown.OnInteractEnded();
					break;
				case "HTR":
				case "HUR":
				case "HRT":
				case "HRU":
					_buttonNeedleRightUp.OnInteract();
					yield return new WaitForSeconds(0.6f);
					_buttonNeedleRightUp.OnInteractEnded();
					break;
				case "HDR":
				case "HBR":
				case "HRD":
				case "HRB":
					_buttonNeedleRightDown.OnInteract();
					yield return new WaitForSeconds(0.6f);
					_buttonNeedleRightDown.OnInteractEnded();
					break;
				case "PULL":
					_needle.OnInteract();
					break;
				case "CYCLE":
					for (int i = 0; i < 9; i++) {
						_buttonDisplayLeft.OnInteract();
						yield return "trycancel";
						yield return new WaitForSeconds(2f);
					}
					break;
				default:
					yield break;
			}
			yield return "trycancel";
			yield return new WaitForSeconds(0.05f);
		}
	}

	IEnumerator TwitchHandleForcedSolve() {
		for (int i = 0; i < 9; i++) {
			_buttonDisplayLeft.OnInteract();
			yield return new WaitForSeconds(0.1f);
		}
		while (!_solved) {
			while (true) {
				if (_selectableAngles[_solution].Submit(_supposedAngle, false)) {
					while (_currentAngle != _supposedAngle) {
						yield return true;
					}
					yield return new WaitForSeconds(0.1f);
					if (_selectableAngles[_solution].Submit(_supposedAngle, false)) {
						_needle.OnInteract();
						break;
					}
				}

				DecimalVector2 handInPosition = new DecimalVector2(DecimalMath.Cos(_supposedAngle), DecimalMath.Sin(_supposedAngle));
				decimal distanceToDesired = DecimalVector2.Distance(handInPosition, _selectableAngles[_solution].Position);
				int index = -1;
				bool hold = false;
				KMSelectable button = null;

				for (int i = 0; i < 9; i++) {
					AngularNotation a = _selectableAngles[i];
					bool shorter = false;
					yield return null;
					shorter = TwitchCheckButtonPress(distanceToDesired, a.LargeJump(true, _supposedAngle), out distanceToDesired);
					if (shorter) { index = i; button = _buttonNeedleLeftUp; hold = false; }
					//yield return null;
					shorter = TwitchCheckButtonPress(distanceToDesired, a.LargeJump(false, _supposedAngle), out distanceToDesired);
					if (shorter) { index = i; button = _buttonNeedleLeftDown; hold = false; }
					//yield return null;
					shorter = TwitchCheckButtonPress(distanceToDesired, a.MediumJump(true, _supposedAngle), out distanceToDesired);
					if (shorter) { index = i; button = _buttonNeedleMiddleUp; hold = false; }
					//yield return null;
					shorter = TwitchCheckButtonPress(distanceToDesired, a.MediumJump(false, _supposedAngle), out distanceToDesired);
					if (shorter) { index = i; button = _buttonNeedleMiddleDown; hold = false; }
					//yield return null;
					shorter = TwitchCheckButtonPress(distanceToDesired, a.SmallJump(true, _supposedAngle), out distanceToDesired);
					if (shorter) { index = i; button = _buttonNeedleRightUp; hold = false; }
					//yield return null;
					shorter = TwitchCheckButtonPress(distanceToDesired, a.SmallJump(false, _supposedAngle), out distanceToDesired);
					if (shorter) { index = i; button = _buttonNeedleRightDown; hold = false; }
					yield return null;
					shorter = TwitchCheckButtonHold(distanceToDesired,  a.LargeReset(true, _supposedAngle), out distanceToDesired);
					if (shorter) { index = i; button = _buttonNeedleLeftUp; hold = true; }
					//yield return null;
					shorter = TwitchCheckButtonHold(distanceToDesired,  a.LargeReset(false, _supposedAngle), out distanceToDesired);
					if (shorter) { index = i; button = _buttonNeedleLeftDown; hold = true; }
					//yield return null;
					shorter = TwitchCheckButtonHold(distanceToDesired,  a.MediumReset(true, _supposedAngle), out distanceToDesired);
					if (shorter) { index = i; button = _buttonNeedleMiddleUp; hold = true; }
					//yield return null;
					shorter = TwitchCheckButtonHold(distanceToDesired,  a.MediumReset(false, _supposedAngle), out distanceToDesired);
					if (shorter) { index = i; button = _buttonNeedleMiddleDown; hold = true; }
					//yield return null;
					shorter = TwitchCheckButtonHold(distanceToDesired,  a.SmallReset(true, _supposedAngle), out distanceToDesired);
					if (shorter) { index = i; button = _buttonNeedleRightUp; hold = true; }
					//yield return null;
					shorter = TwitchCheckButtonHold(distanceToDesired,  a.SmallReset(false, _supposedAngle), out distanceToDesired);
					if (shorter) { index = i; button = _buttonNeedleRightDown; hold = true; }
				}
				if (hold) {
					while (_currentSelected != index) {
						_buttonDisplayRight.OnInteract();
						yield return new WaitForSeconds(0.05f);
					}
					button.OnInteract();
					yield return new WaitForSeconds(0.55f);
					button.OnInteractEnded();
				}
				else {
					while (_currentSelected != index) {
						_buttonDisplayRight.OnInteract();
						yield return new WaitForSeconds(0.05f);
					}
					button.OnInteract();
					yield return new WaitForSeconds(0.05f);
					button.OnInteractEnded();
				}
				//yield return new WaitForSeconds(0.05f);
			}
		}
	}

	public bool TwitchCheckButtonHold(decimal oldDistance, decimal r, out decimal newDistance) {
		decimal hypothesizedAngle = r;
		DecimalVector2 newPosition = new DecimalVector2(DecimalMath.Cos(hypothesizedAngle), DecimalMath.Sin(hypothesizedAngle));
		decimal distance = DecimalVector2.Distance(newPosition, _selectableAngles[_solution].Position);

		if (distance < oldDistance) {
			newDistance = distance;
			return true;
		}
		else {
			newDistance = oldDistance;
			return false;
		}
	}

	public bool TwitchCheckButtonPress(decimal oldDistance, decimal r, out decimal newDistance) {
		decimal hypothesizedAngle = _supposedAngle + r;
		DecimalVector2 newPosition = new DecimalVector2(DecimalMath.Cos(hypothesizedAngle), DecimalMath.Sin(hypothesizedAngle));
		decimal distance = DecimalVector2.Distance(newPosition, _selectableAngles[_solution].Position);

		if (distance < oldDistance) {
			newDistance = distance;
			return true;
		}
		else {
			newDistance = oldDistance;
			return false;
		}
	}
	#endregion
}
