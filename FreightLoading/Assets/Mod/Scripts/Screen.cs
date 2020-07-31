using System.Collections;
using UnityEngine;

public class Screen : MonoBehaviour {
    [SerializeField] GameObject _on;
    [SerializeField] GameObject _off;
    // todo: Not really neat code here
    [SerializeField] GameObject _bg1;
    [SerializeField] GameObject _bg2;

    Coroutine _coroutine;

    /// <summary>
    /// Turn the top screen on, and the bottom as well if bottomToo is true.
    /// </summary>
    /// <param name="longHaul"></param>
    public void TurnOn(bool on)
    {
        _on.gameObject.SetActive(on);
        _off.gameObject.SetActive(!on);
    }

    /// <summary>
    /// Turn screen on with a delay before doing so
    /// </summary>
    /// <param name="on"></param>
    /// <param name="delay"></param>
    public void TurnOnDelay(bool on, float delay)
    {
        if (_coroutine != null)
        {
            StopCoroutine(_coroutine);
        }
        _coroutine = StartCoroutine(TurnOnDelayCoroutine(on, delay));
    }

    /// <summary>
    ///  Temoporary shuts off the display
    /// </summary>
    /// <param name="delay"></param>
    /// <param name="duration"></param>
    public void TemporaryShutOff(float delay, float duration, bool again = false) {
        if (!again && _coroutine != null) {
            StopCoroutine(_coroutine);
        }
        _coroutine = StartCoroutine(TemporaryShutOffCoroutine(delay, duration));
    }

    /// <summary>
    /// Change the display to the alternative background
    /// </summary>
    /// <param name="on"></param>
    /// <param name="delay"></param>
    /// <param name="delay2"></param>
    /// <param name="duration"></param>
    public void ChangeDisplay(bool on, float delay, float delay2, float duration)
    {
        if (_coroutine != null) {
            StopCoroutine(_coroutine);
        }
        _coroutine = StartCoroutine(ChangeDisplayCoroutine(delay, delay2, duration));
    }

    /// <summary>
    /// Coroutine for changing the display to the second background
    /// </summary>
    /// <param name="delay"></param>
    /// <param name="delay2"></param>
    /// <param name="duration"></param>
    /// <returns></returns>
    IEnumerator ChangeDisplayCoroutine(float delay, float delay2, float duration) {
        while (delay > 0) {
            delay -= Time.deltaTime;
            yield return null;
        }
        _bg1.SetActive(false);
        _bg2.SetActive(true);
        TurnOn(false);

        while (delay2 > 0) {
            delay2 -= Time.deltaTime;
            yield return null;
        }
        TurnOn(true);
        while (duration > 0) {
            duration -= Time.deltaTime;
            yield return null;
        }
        TurnOn(false);

    }

    /// <summary>
    /// Coroutine for turnign on screen with a delay
    /// </summary>
    /// <param name="on"></param>
    /// <param name="delay"></param>
    /// <returns></returns>
    IEnumerator TurnOnDelayCoroutine(bool on, float delay)
    {
        while (delay > 0)
        {
            delay -= Time.deltaTime;
            yield return null;
        }
        TurnOn(false);
    }

    /// <summary>
    /// Coroutine for temporarily shutting off the display
    /// </summary>
    /// <param name="delay"></param>
    /// <param name="duration"></param>
    /// <returns></returns>
    IEnumerator TemporaryShutOffCoroutine(float delay, float duration)
    {
        while (delay > 0)
        {
            delay -= Time.deltaTime;
            yield return null;
        }
        TurnOn(false);
        while (duration > 0)
        {
            duration -= Time.deltaTime;
            yield return null;
        }
        TurnOn(true);
    }

}
