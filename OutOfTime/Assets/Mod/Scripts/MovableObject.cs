using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovableObject : MonoBehaviour {

    [SerializeField] List<Vector3> _locations;
    [SerializeField] List<Quaternion> _rotations;
    [SerializeField] List<string> _positionIds;
    [SerializeField] List<float> _durations;
    [SerializeField] float _uniformSpeed = 1f;
    int _currentIndex;
    Coroutine _moveRoutine;

    [NonSerialized] public float ExtendedRate;

    // TODO: Make this script recyclable so I can use it on other projects too

    private void Start() {
        //if (_locations.Count != _rotations.Count) {
        //    // TODO: throw a warning
        //}
        //_currentIndex = _locations.FindIndex(v => v == transform.localPosition);
        //if (_currentIndex == -1) {
        //    SetPosition(0);
        //}
    }

    [ContextMenu("Add Position")]
    void AddPosition() {
        _locations.Add(transform.localPosition);
        _rotations.Add(transform.localRotation);
        _positionIds.Add(string.Empty);
        _durations.Add(0);
    }

    public void SetPosition(int index) {
        if (index >= 0 && index < _locations.Count) {
            _currentIndex = index;
            transform.localPosition = _locations[index];
            transform.localRotation = _rotations[index];
        }
        else {
            // TODO: throw a warning or error
            _currentIndex = 0;
            transform.localPosition = _locations[0];
            transform.localRotation = _rotations[0];
        }
    }

    public void SetPosition(string id) {
        int index = _positionIds.IndexOf(id);
        if (index == -1) {
            Debug.LogError(string.Format("[LotsOfS] Tried to move {0} to unknown position '{1}'", this.gameObject.name, id));
		}
        
        if (index >= 0 && index < _locations.Count) {
            _currentIndex = index;
            transform.localPosition = _locations[index];
            transform.localRotation = _rotations[index];
        }
        else {
            // TODO: throw a warning or error
            _currentIndex = 0;
            transform.localPosition = _locations[0];
            transform.localRotation = _rotations[0];
        }
    }

    [ContextMenu("Test Move")]
    public void MoveToInbetween() {
        StartCoroutine(MoveToInbetweenCoroutine(1f, 0, 1, speed: 0.01f));
    }

    public void MoveToInbetween(float ratio) {
        StartCoroutine(MoveToInbetweenCoroutine(ratio, 0, 1));
    }

    public void MoveToInbetween(float ratio, float speed = -1, float duration = -1) {
        StartCoroutine(MoveToInbetweenCoroutine(ratio, 0, 1, speed, duration));
    }

    public void MoveToInbetween(float ratio, int minIndex, int maxIndex) {
        StartCoroutine(MoveToInbetweenCoroutine(ratio, minIndex, maxIndex));
    }

    public void MoveToInbetween(float ratio, int minIndex, int maxIndex, float speed = -1, float duration = -1) {
        StartCoroutine(MoveToInbetweenCoroutine(ratio, minIndex, maxIndex, speed, duration));
    }

    /// <summary>
    /// Coroutine for moving object to next position
    /// </summary>
    /// <returns></returns>
    IEnumerator MoveToInbetweenCoroutine(float ratio, int minIndex, int maxIndex, float speed = -1f, float duration = -1f) {
        // TODO: Make this consider rotations too.
        Vector3 startPos = transform.localPosition;
        Vector3 gapL = _locations[maxIndex] - _locations[minIndex];
        Vector3 destination = _locations[minIndex] + (ratio * gapL);
        float distance = Vector3.Distance(transform.localPosition, destination);

        if (speed < 0) {
            if (duration < 0) {
                speed = _uniformSpeed;
            }
            else {
                speed = distance / duration;
			}
        }
        float totalTime = distance / speed;
        float timeRemaining = totalTime;

        while (timeRemaining > 0) {
            yield return null;
			//transform.localPosition = Vector3.Lerp(startPos, destination, totalTime - timeRemaining);
			transform.localPosition = Vector3.MoveTowards(transform.localPosition, destination, speed * Time.deltaTime);
			timeRemaining -= Time.deltaTime;

            float distToMin = Vector3.Distance(transform.localPosition, _locations[minIndex]);
            float distToMax = Vector3.Distance(transform.localPosition, _locations[maxIndex]);
            ExtendedRate = distToMin / (distToMin + distToMax);
        }
        transform.localPosition = destination;
        ExtendedRate = ratio;
    }


    ///// <summary>
    ///// Move object to next position in position array
    ///// </summary>
    //public void MoveToggleLoop() {
    //    _index++;
    //    _index %= _locations.Count;
    //    if (_moveRoutine != null) {
    //        StopCoroutine(_moveRoutine);
    //    }
    //    _moveRoutine = StartCoroutine(MoveToggleLoopCoroutine());
    //}

    ///// <summary>
    ///// Move object to next position in position array
    ///// </summary>
    //public void MoveTo(int index) {
    //    _index = index;
    //    if (_index >= _locations.Count || _index < 0) {
    //        _index = 0;
    //    }
    //    if (_moveRoutine != null) {
    //        StopCoroutine(_moveRoutine);
    //    }
    //    _moveRoutine = StartCoroutine(MoveToggleLoopCoroutine());
    //}

    ///// <summary>
    ///// Coroutine for moving object to next position
    ///// </summary>
    ///// <returns></returns>
    //IEnumerator MoveToggleLoopCoroutine() {
    //    float time = _duration[_index];
    //    float elapsedTime = time;
    //    float distance = Vector3.Distance(transform.localPosition, _locations[_index]);
    //    float angle = Quaternion.Angle(transform.localRotation, _rotations[_index]);
    //    float rotationSpeed = angle / time;
    //    float movementSpeed = distance / time;
    //    while (elapsedTime > 0) {
    //        yield return null;
    //        transform.localPosition = Vector3.MoveTowards(transform.localPosition, _locations[_index], movementSpeed * Time.deltaTime);
    //        transform.localRotation = Quaternion.RotateTowards(transform.localRotation, _rotations[_index], rotationSpeed * Time.deltaTime);
    //        elapsedTime -= Time.deltaTime;
    //    }
    //    transform.localPosition = _locations[_index];
    //    transform.localRotation = _rotations[_index];
    //}

}
