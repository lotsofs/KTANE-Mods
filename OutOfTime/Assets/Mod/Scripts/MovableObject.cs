using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovableObject : MonoBehaviour {

    [SerializeField] List<Vector3> _locations;
    [SerializeField] List<Quaternion> _rotations;
    [SerializeField] List<string> _positionIds;
    [SerializeField] List<float> _speeds;
    int _index;
    Coroutine _moveRoutine;

    // TODO: Make this script recyclable so I can use it on other projects too

    private void Start() {
        if (_locations.Count != _rotations.Count) {
            // TODO: throw a warning
        }
        _index = _locations.FindIndex(v => v == transform.localPosition);
        if (_index == -1) {
            SetPosition(0);
        }
    }

    [ContextMenu("Add Position")]
    void AddPosition() {
        _locations.Add(transform.localPosition);
        _rotations.Add(transform.localRotation);
        _positionIds.Add(string.Empty);
        _speeds.Add(0);
    }

    public void SetPosition(int index) {
        if (index >= 0 && index < _locations.Count) {
            _index = index;
            transform.localPosition = _locations[index];
            transform.localRotation = _rotations[index];
        }
        else {
            // TODO: throw a warning or error
            _index = 0;
            transform.localPosition = _locations[0];
            transform.localRotation = _rotations[0];
        }
    }

    /// <summary>
    /// Move object to next position in position array
    /// </summary>
    public void MoveToggleLoop() {
        _index++;
        _index %= _locations.Count;
        if (_moveRoutine != null) {
            StopCoroutine(_moveRoutine);
        }
        _moveRoutine = StartCoroutine(MoveToggleLoopCoroutine());
    }

    /// <summary>
    /// Move object to next position in position array
    /// </summary>
    public void MoveTo(int index) {
        _index = index;
        if (_index >= _locations.Count || _index < 0) {
            _index = 0;
        }
        if (_moveRoutine != null) {
            StopCoroutine(_moveRoutine);
        }
        _moveRoutine = StartCoroutine(MoveToggleLoopCoroutine());
    }

    /// <summary>
    /// Coroutine for moving object to next position
    /// </summary>
    /// <returns></returns>
    IEnumerator MoveToggleLoopCoroutine() {
        float time = _speeds[_index];
        float elapsedTime = time;
        float distance = Vector3.Distance(transform.localPosition, _locations[_index]);
        float angle = Quaternion.Angle(transform.localRotation, _rotations[_index]);
        float rotationSpeed = angle / time;
        float movementSpeed = distance / time;
        while (elapsedTime > 0) {
            yield return null;
            transform.localPosition = Vector3.MoveTowards(transform.localPosition, _locations[_index], movementSpeed * Time.deltaTime);
            transform.localRotation = Quaternion.RotateTowards(transform.localRotation, _rotations[_index], rotationSpeed * Time.deltaTime);
            elapsedTime -= Time.deltaTime;
        }
        transform.localPosition = _locations[_index];
        transform.localRotation = _rotations[_index];
    }

}
