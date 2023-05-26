using System.Collections;
using UnityEngine;

public class Note : MonoBehaviour {

    [SerializeField] ReferenceTransform[] _positions;
    //[SerializeField] bool[] _landscape;
    public KMSelectable MagnetPressable;
    [SerializeField] GameObject _magnetStatic;
    [SerializeField] TextMesh _text;
    [Space]
    [SerializeField] MovableObject _backupPaper;
    [SerializeField] TextMesh _backupText;
    [Space]
    [SerializeField] Resource[] _freight;
    [SerializeField] Resource[] _passengers;
    [Space]
    [SerializeField] BombHelper _bombHelper;
    Resource[] _list = new Resource[8];
    Resource[] _backupList = new Resource[14];

    /// <summary>
    /// Initializiaton
    /// </summary>
    void Start() {
        int r = UnityEngine.Random.Range(0, _positions.Length);
        transform.localPosition = _positions[r].Position;
        transform.localRotation = _positions[r].Rotation;
        StartCoroutine(DisableMagnet());
        MagnetPressable.OnInteract += delegate { _bombHelper.GenericButtonPress(MagnetPressable, false, 0.05f); _backupPaper.MoveToggleLoop(); return false; };
    }

    /// <summary>
    /// Enables a second paper to display resources on
    /// </summary>
    void AddBackupPaper() {
        MagnetPressable.gameObject.SetActive(true);
        _magnetStatic.SetActive(false);
        _backupPaper.gameObject.SetActive(true);
        _backupText.text = string.Empty;
    }

    /// <summary>
    /// Hardcoded long name detector. I can't be bothered writing a string length parser thing right now. Could probably just count characters in the string but Is and Ws ruin that for me right now.
    /// </summary>
    /// <param name="resource"></param>
    /// <returns></returns>
    bool ResourceHasLongName(Resource resource) {
        string[] longNames = new string[] { "Farming Equipment", "Military Hardware", "Electrical Transformer", "Nuclear Reactor Pressure Vessel" };
        foreach (string name in longNames) {
            if (name == resource.DisplayName) {
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// Hardcoded long name into double line name converter. I can't be bothered writing a string length parser thing right now.
    /// </summary>
    /// <param name="resource"></param>
    /// <returns></returns>
    string GetMultilineLongResourceName(Resource resource) {
        // todo: These arrays should probably go elsewhere too
        string[] longNames = new string[] { "Farming Equipment", "Military Hardware", "Electrical Transformer", "Nuclear Reactor Pressure Vessel" };
        string[] multilineNames = new string[] { "Farming \n   Equipment", "Military \n   Hardware", "Electrical \n   Transformer", "Nuclear Reactor \n   Pressure Vessel" };
        for (int i = 0; i < longNames.Length; i++) {
            if (longNames[i] == resource.DisplayName) {
                return multilineNames[i];
            }
        }
        return resource.DisplayName;
    }

    /// <summary>
    /// Checks if the next slot is empty or if it exists at all
    /// </summary>
    /// <param name="slot"></param>
    /// <returns></returns>
    bool IsNextSlotNull(int slot) {
        if (slot < _list.Length - 1) {
            // empty slot found on the first list and it isn't the last slot. Is the next slot empty too?
            return _list[slot + 1] == null;
        }
        if (slot >= _list.Length && slot < _backupList.Length + _list.Length - 1) {
            // empty slot found on the second list and it isn't the last slot. Is the next slot empty too?
            return _backupList[slot - _list.Length + 1] == null;
        }
        // slot found, but its the last one on the list, so there's no slot after
        return false;
    }

    /// <summary>
    /// Find two free lines in a row
    /// </summary>
    /// <param name="resource"></param>
    public void FindDoubleLine(Resource resource) {
        int listSlot = FindResource(null);
        if ((listSlot == _list.Length - 1 || listSlot == -1) && _backupPaper.gameObject.activeInHierarchy == false) {
            // no room on first list and second list doesnt exist yet. Enable it.
            Debug.LogFormat("[Railway Cargo Loading #{0}] There's no room for a new double-line item on the note (a). Adding another.", _bombHelper.ModuleId);
            AddBackupPaper();
            listSlot = FindResource(null, _list.Length);
        }
        while (!IsNextSlotNull(listSlot)) {
            // no room for double line item here, keep searching
            listSlot = FindResource(null, listSlot + 1);
            if (listSlot == -1 && _backupPaper.gameObject.activeInHierarchy == false) {
                Debug.LogWarningFormat("[Railway Cargo Loading #{0}] There's no room for a new double-line item on the note (b). Adding another.", _bombHelper.ModuleId);
                AddBackupPaper();
                listSlot = FindResource(null, _list.Length);
            }
            // we've exhausted all our spots (this shouldn't happen)
            if (listSlot == -1 || listSlot == _list.Length + _backupList.Length - 1) {
                Debug.LogFormat("[Railway Cargo Loading #{0}] There is no space on either note for {1}, for some reason.", _bombHelper.ModuleId, resource.DisplayName);
                return;
            }
        }
        if (listSlot >= _list.Length) {
            // slots found on second list
            Debug.LogFormat("[Railway Cargo Loading #{0}] Adding to the notes a double-line item {1} at position {2}, which is on the second note.", _bombHelper.ModuleId, resource.DisplayName, listSlot);
            _backupList[listSlot - _list.Length] = resource;
            _backupList[listSlot - _list.Length + 1] = resource;
        }
        else if (listSlot == -1)
        {
			Debug.LogFormat("[Railway Cargo Loading #{0}] There is no space on the notes for {1}. Is this because the bomb exploded and the notes no longer exist?", _bombHelper.ModuleId, resource.DisplayName);
			return;
		}
        else {
            // slots found on first list
            Debug.LogFormat("[Railway Cargo Loading #{0}] Adding to the notes a double-line item {1} at position {2}, which is on the first note.", _bombHelper.ModuleId, resource.DisplayName, listSlot);
            _list[listSlot] = resource;
            _list[listSlot + 1] = resource;
        }
    }

    /// <summary>
    /// Find a free line
    /// </summary>
    /// <param name="resource"></param>
    void FindSingleLine(Resource resource) {
        int listSlot = FindResource(null);
        if (listSlot == -1 && _backupPaper.gameObject.activeInHierarchy == false) {
            // There's no more room on the list, add a new list and search it for a free slot
            Debug.LogFormat("[Railway Cargo Loading #{0}] There's no room for a new single-line item on the note. Adding another.", _bombHelper.ModuleId);
            AddBackupPaper();
            listSlot = FindResource(null, listSlot + 1);
        }
        if (listSlot >= _list.Length) {
            // slot found, it's on the second paper
            Debug.LogFormat("[Railway Cargo Loading #{0}] Adding to the notes a single-line item {1} at position {2}, which is on the second note.", _bombHelper.ModuleId, resource.DisplayName, listSlot);
            _backupList[listSlot - _list.Length] = resource;
        }
		else if (listSlot == -1)
		{
			Debug.LogFormat("[Railway Cargo Loading #{0}] There is no space on the notes for {1}. Is this because the bomb exploded and the notes no longer exist?", _bombHelper.ModuleId, resource.DisplayName);
			return;
		}
		else {
            Debug.LogFormat("[Railway Cargo Loading #{0}] Adding to the notes a single-line item {1} at position {2}, which is on the first note.", _bombHelper.ModuleId, resource.DisplayName, listSlot);
            _list[listSlot] = resource;
        }

    }

    /// <summary>
    /// Updates the text on the notes
    /// </summary>
    public void UpdateText() {
        // TODO: Dragon of a method. Rip it apart.
        // First, we're clearing any resource no longer needed to be shown
        for (int i = 0; i < _list.Length; i++) {
            if (_list[i] == null || _list[i].Count <= _list[i].MaximumDisappearCount) {
                _list[i] = null;
            }
            if (_backupPaper.gameObject.activeInHierarchy == true) {
                if (_backupList[i] == null || _backupList[i].Count <= _backupList[i].MaximumDisappearCount) {
                    _backupList[i] = null;
                }
            }
        }
        // Then, we're adding any new resources to the list
        foreach (Resource resource in _freight) {
            if (resource.Count > resource.MinimumDisplayCount && FindResource(resource) == -1) {
                if (ResourceHasLongName(resource)) {
                    FindDoubleLine(resource);
                }
                else {
                    FindSingleLine(resource);
                }
            }
        }
        // Lastly, we'll write out the text. Passengers go on top of the first list
        string note = string.Empty;
        int totalPax = 0;
        foreach (Resource pax in _passengers) {
            totalPax += pax.Count;
        }
        note += string.Format("Passengers: {0}\n", Mathf.Max(totalPax, 0));
        note += string.Format("* w/ Checked\n   Baggage: {0}\n", Mathf.Max(_passengers[1].Count, 0));
        note += string.Format("Middle-Class: {0}\n", Mathf.Max(_passengers[2].Count, 0));
        note += string.Format("Rich: {0}\n\n ", Mathf.Max(_passengers[3].Count, 0));
        
        // Then add freight to the bottom of the first list
        for (int i = 0; i < _list.Length; i++) {
            if (_list[i] == null) {
                note += "\n";
            }
            else {
                if (ResourceHasLongName(_list[i])) {
                    string name = GetMultilineLongResourceName(_list[i]);
                    note += string.Format("{0} {1}\n", _list[i].Count, name);
                    i++;
                }
                else {
                    note += string.Format("{0} {1}\n", _list[i].Count, _list[i].DisplayName);
                }
            }
            // Add some semi-random spaces to make the note look more handwritten and not as computerizedly spaced
            if (i == 2 || i == 4 || i == 5) {
                note += " ";
            }
        }
        _text.text = note;

        Debug.LogFormat("[Railway Cargo Loading #{0}] The note now says: \"", _bombHelper.ModuleId);
        Debug.LogFormat("[Railway Cargo Loading #{0}] {1}", _bombHelper.ModuleId, note.Replace("\n", string.Format("\n[Railway Cargo Loading #{0}] ", _bombHelper.ModuleId)));

        // And then do the backup paper
        if (_backupPaper.gameObject.activeInHierarchy == true) {
            string backupNote = string.Empty;
            for (int i = 0; i < _backupList.Length; i++) {
                if (_backupList[i] == null) {
                    backupNote += "\n";
                }
                else {
                    if (ResourceHasLongName(_backupList[i])) {
                        string name = GetMultilineLongResourceName(_backupList[i]);
                        backupNote += string.Format("{0} {1}\n", _backupList[i].Count, name);
                        i++;
                    }
                    else {
                        backupNote += string.Format("{0} {1}\n", _backupList[i].Count, _backupList[i].DisplayName);
                    }
                }
                // semi-random spaces
                if (i == 0 || i == 2 || i == 3) {
                    backupNote += " ";
                }
            }
            _backupText.text = backupNote;
            Debug.LogFormat("[Railway Cargo Loading #{0}] \"", _bombHelper.ModuleId);
            Debug.LogFormat("[Railway Cargo Loading #{0}] The second note now says: \"", _bombHelper.ModuleId);
            Debug.LogFormat("[Railway Cargo Loading #{0}] {1}", _bombHelper.ModuleId, backupNote.Replace("\n", string.Format("\n[Railway Cargo Loading #{0}] ", _bombHelper.ModuleId)));
        }
        Debug.LogFormat("[Railway Cargo Loading #{0}] \"", _bombHelper.ModuleId);
    }

    /// <summary>
    /// Finds a resource of specific kind in _list.
    /// </summary>
    /// <param name="Type"></param>
    /// <returns></returns>
    int FindResource(Resource resource, int startAt = 0) {
        // main list
        for (int i = startAt; i < _list.Length; i++) {
            if (_list[i] == resource) {
                return i;
            }
        }
        // backup list
        if (_backupPaper.gameObject.activeInHierarchy == true) {
            for (int i = Mathf.Max(0, startAt - _list.Length); i < _backupList.Length; i++) {
                if (_backupList[i] == resource) {
                    return i + _list.Length;
                }
            }
        }
        return -1;
    }

    /// <summary>
    /// Disables the magnet after one frame
    /// </summary>
    /// <returns></returns>
    IEnumerator DisableMagnet() {
        yield return null;
        MagnetPressable.gameObject.SetActive(false);
    }
}
