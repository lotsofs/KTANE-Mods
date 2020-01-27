using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovableObject : MonoBehaviour {

    [SerializeField] bool _configure;
    [SerializeField] List<Vector3> _locations;
    [SerializeField] List<Quaternion> _rotations;
    int _index;
    Coroutine _moveRoutine;

    // TODO: Make this script recyclable so I can use it on other projects too

    private void Start() {
        if (_configure) {
            _locations.Add(transform.localPosition);
            _rotations.Add(transform.localRotation);
        }
        if (_locations.Count != _rotations.Count) {
            // TODO: throw a warning
        }
        _index = _locations.FindIndex(v => v == transform.localPosition);
    }

    /// <summary>
    /// Move object to next position in position array
    /// </summary>
    public void MoveToggleLoop() {
        _index++;
        if (_index >= _locations.Count) {
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
        float time = 0.3f; // TODO: This is hardcoded
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
