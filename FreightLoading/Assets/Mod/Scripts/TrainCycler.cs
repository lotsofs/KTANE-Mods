using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrainCycler : MonoBehaviour {

    //[SerializeField] TrainCar[] _trainCars;
    int _index = 0;
    int _stage = 0;
    List<TrainCar> _trainCars;
    [SerializeField] SpriteRenderer[] _carSprites;

    [Space]
    [SerializeField] ReferenceTransform _base;
    [SerializeField] ReferenceTransform _previousParking;
    [SerializeField] ReferenceTransform _previousParkingWide;
    [SerializeField] ReferenceTransform _despawnArea;
    [SerializeField] ReferenceTransform _derailPoint;
    [Space]
    [SerializeField] SecurityTimer _securityTimer;
    bool _transitioning = false;
    Coroutine _currentRoutine;

    /// <summary>
    /// Next train car
    /// </summary>
    /// <param name="trains"></param>
    /// <param name="stage"></param>
    public void NewStage(List<TrainCar> trains, int stage) {
        _securityTimer.NewCar(stage);   // TODO: This doesnt go in this script
        _stage = stage - 1;     // from 1 to 0 based
        _index = 0;
        _trainCars = trains;
        if (_stage <= 0 && !_transitioning) {
            NewTrain();
        }
    }

    /// <summary>
    /// Resets the entire train and starts over
    /// </summary>
    void NewTrain() {
        StopTrain();
        foreach (SpriteRenderer carSprite in _carSprites) {
            carSprite.transform.localPosition = _base.Position;
            carSprite.transform.localRotation = _base.Rotation;
            carSprite.gameObject.SetActive(true);
            carSprite.sprite = null;
        }
        DisplayTrain();
    }

    /// <summary>
    /// Display a train car and enables cyclign through it
    /// </summary>
    void DisplayTrain() {
        _carSprites[_stage].sprite = _trainCars[_index].Appearance;
        _transitioning = false;
    }

    /// <summary>
    /// Cycle through the list of displayable train cars
    /// </summary>
    /// <param name="forward"></param>
    public void Cycle(bool forward) {
        if (_transitioning) {
            return;
        }
        _index += forward ? 1 : -1;
        if (_index < 0) {
            _index = _trainCars.Count - 1;
        }
        else if (_index >= _trainCars.Count) {
            _index = 0;
        }
        DisplayTrain();
    }

    /// <summary>
    /// Returns the index of the currently displayed train
    /// </summary>
    /// <returns></returns>
    public int SelectCurrent() {
        if (_transitioning) {
            return -1;
        }
        _transitioning = true;
        return _index;
    }

    /// <summary>
    /// Stops the coroutine
    /// </summary>
    void StopTrain() {
        if (_currentRoutine != null) {
            StopCoroutine(_currentRoutine);
        }
    }

    /// <summary>
    /// Derail train
    /// </summary>
    /// <param name="sprite"></param>
    public void DerailTrain(Sprite sprite) {
        _carSprites[_stage].sprite = sprite;
        StopTrain();
        _currentRoutine = StartCoroutine(MoveTrainCoroutine(false, true));
    }

    /// <summary>
    /// Move train to next spot
    /// </summary>
    /// <param name="sprite"></param>
    public void MoveTrain(Sprite sprite) {
        bool wide = sprite.texture.width > 300;
        _carSprites[_stage].sprite = sprite;
        StopTrain();
        _currentRoutine = StartCoroutine(MoveTrainCoroutine(wide, false));
    }

    /// <summary>
    /// Start the victory lap
    /// </summary>
    /// <param name="delay"></param>
    /// <param name="delay2"></param>
    public void VictoryLap(float delay, float delay2) {
        StartCoroutine(VictoryLapCoroutine(delay, delay2));
    }

    /// <summary>
    /// Assemble all cars into a neat row, as trains are just rows of cars
    /// </summary>
    void AssembleTrain() {
        // todo: This entire script is hardcoded. Look at all of it.
        bool wideOffset = false;
        float x = 0;
        for (int i = 0; i < _carSprites.Length; i++) {
            x += 0.0545f;
            // previous car was wide, add some more distance between it and this
            if (wideOffset) {
                x += 0.0128f;
                wideOffset = false;
            }
            // current car is wide, add some more distance between this and previous car
            if (_carSprites[i].sprite.texture.width > 300) {
                x += 0.0128f;
                wideOffset = true;
            }
            _carSprites[i].transform.localPosition = new Vector3(x, 0.0164f, 0.015f);
            _carSprites[i].gameObject.SetActive(false);
            _carSprites[i].transform.localRotation = Quaternion.Euler(90, 0, 0);    // flip the train around, we're going right to left.
        }
    }

    /// <summary>
    /// Moves the entire train past the display
    /// </summary>
    /// <param name="delay"></param>
    /// <param name="delay2"></param>
    /// <returns></returns>
    IEnumerator VictoryLapCoroutine(float delay, float delay2) {
        // delay before starting at all
        while (delay > 0) {
            delay -= Time.deltaTime;
            yield return null;
        }
        // clear the current train and prepare for the victory lap
        StopTrain();
        AssembleTrain();
        _securityTimer.NewCar(16);
        // another delay before the train actually comes
        while (delay2 > 0) {
            delay2 -= Time.deltaTime;
            yield return null;
        }
        // here comes the train. Choo choo!
        // todo: hardcoded values in this entire block
        float speed = 0.06f;        
        int carsPassed = 0;
        while (carsPassed < 15) {
            foreach (SpriteRenderer car in _carSprites) {
                Vector3 pos = car.transform.localPosition;
                pos += (Vector3.left * Time.deltaTime * speed);
                car.transform.localPosition = pos;
                if (!car.gameObject.activeInHierarchy && car.transform.localPosition.x < 0.0678f && car.transform.localPosition.x > -0.1141f) {
                    car.gameObject.SetActive(true);
                }
                else if (car.gameObject.activeInHierarchy && car.transform.localPosition.x < -0.1141f) {
                    car.gameObject.SetActive(false);
                    carsPassed++;
                }
            }
            yield return null;
        }
        // clean up the mess, just in case
        foreach (SpriteRenderer car in _carSprites) {
            car.transform.localPosition = Vector3.zero;
            car.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// Coroutine for moving train to next parking spot
    /// </summary>
    /// <param name="wide"></param>
    /// <param name="derail"></param>
    /// <returns></returns>
    IEnumerator MoveTrainCoroutine(bool wide, bool derail) {
        // todo: Hardcoded values in here
        float time = 1.0f;
        float elapsedTime = time * 3;
        Vector3 velocity = Vector3.zero;
        Vector3 destination = _previousParking.Position;
        Quaternion rotation = _previousParking.Rotation;
        if (wide) {
            destination = _previousParkingWide.Position;
        }
        if (derail) {
            destination = _derailPoint.Position;
            rotation = _derailPoint.Rotation;
        }
        Transform car = _carSprites[_stage].transform;
        // move the first railcar to the right
        if (_stage < 1 || derail) {
            while (car.localPosition != destination) {
                car.localPosition = Vector3.SmoothDamp(car.localPosition, destination, ref velocity, time);
                car.localRotation = Quaternion.RotateTowards(car.localRotation, rotation, time / 10);
                elapsedTime -= Time.deltaTime;
                if (_transitioning && elapsedTime <= 0f) {
                    _index = 0;
                    if (derail) {
                        car.localPosition = _base.Position;
                        car.localRotation = _base.Rotation;
                        DisplayTrain();
                        yield break;
                    }
                    DisplayTrain();
                }
                yield return null;
            }
            car.localPosition = destination;
            _transitioning = false;
        }
        // this isn't the first rail car, move both the current and the previous which is still on screen
        else {
            Transform carP = _carSprites[_stage - 1].transform;
            Vector3 difference = destination - car.localPosition;
            Vector3 destinationP = carP.localPosition + difference;

            while (car.localPosition != destination) {
                car.localPosition = Vector3.SmoothDamp(car.localPosition, destination, ref velocity, time);
                car.localRotation = Quaternion.RotateTowards(car.localRotation, rotation, time / 10);
                carP.localPosition = Vector3.SmoothDamp(carP.localPosition, destinationP, ref velocity, time);
                if (carP.localPosition.x > _despawnArea.Position.x) {
                    carP.gameObject.SetActive(false);
                }
                elapsedTime -= Time.deltaTime;
                if (_transitioning && elapsedTime <= 0f) {
                    DisplayTrain();
                    _transitioning = false;
                    if (derail) {
                        NewTrain();
                    }
                }
                yield return null;
            }
            car.localPosition = destination;
            carP.localPosition = _despawnArea.Position;

        }
    }
}
