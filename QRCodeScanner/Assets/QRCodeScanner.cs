using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public class QRCodeScanner : MonoBehaviour {

    List<QRCode> qrCodes = new List<QRCode>();
    bool _focus = false;


    [SerializeField] MeshRenderer _codeDisplay;
    [SerializeField] TextMesh _valueDisplay;
    [SerializeField] TextMesh _waitingDisplay;
    [SerializeField] TextMesh _scanningDisplay;
    [SerializeField] KMSelectable _selectable;

	// Use this for initialization
	void Start () {
        GetQRCodeModules();
        StartCoroutine(Scan());
        //LogMissionID();
        _selectable.OnInteract += () => { _focus = true; return true; };
        _selectable.OnDefocus += () => { _focus = false; };
    }

    //void LogMissionID() {
    //    Component gameplayState = GameObject.Find("GameplayState(Clone)").GetComponent("GameplayState");
    //    Type type = gameplayState.GetType();
    //    FieldInfo fieldMission = type.GetField("MissionToLoad", BindingFlags.Public | BindingFlags.Static);
    //    string currentMission = fieldMission.GetValue(gameplayState).ToString();
    //    Debug.LogFormat("Current mission is: {0}", currentMission);
    //}

    void GetQRCodeModules() {
        KMNeedyModule[] needies = FindObjectsOfType<KMNeedyModule>();
        foreach (KMNeedyModule needy in needies) {
            Component comp = needy.GetComponent("QRCode");
            if (comp == null) {
                continue;
            }

            FieldInfo fieldValue;
            FieldInfo fieldActive;
            Type type = comp.GetType();

            fieldValue = type.GetField("QRMessage", BindingFlags.NonPublic | BindingFlags.Instance);
            fieldActive = type.GetField("_isReady", BindingFlags.NonPublic | BindingFlags.Instance);

            MeshRenderer rend = needy.transform.GetChild(3).GetChild(0).GetComponent<MeshRenderer>();

            QRCode code = new QRCode {
                Component = comp,
                FieldActive = fieldActive,
                FieldValue = fieldValue,
                Module = needy,
                Type = type,
                Rend = rend,
            };

            qrCodes.Add(code);
        }
    }


    IEnumerator Scan() {
        yield return null;
        ChangeDisplay(0);
        while (true) {
            yield return null;
            if (qrCodes.Count == 0) {
                // no qr codes found. wait a bit then search for some.
                yield return new WaitForSeconds(10f);
                GetQRCodeModules();
                continue;
            }
            for (int i = qrCodes.Count - 1; i >= 0; i--) {
                yield return null;
                if (qrCodes[i].Module.gameObject == null) {
                    // qr code was destroyed.
                    Debug.LogFormat("[QR Code Scanner] QR Code module destroyed.");
                    qrCodes.RemoveAt(i);
                    continue;
                }
                if (ReadActive(qrCodes[i])) {
                    // found a match. 
                    Debug.LogFormat("[QR Code Scanner] Found an active QR Code module.");
                    _codeDisplay.material.mainTexture = qrCodes[i].Rend.material.mainTexture;
                    while (_focus == false) {
                        yield return null;
					}
                    yield return new WaitForSeconds(0.5f);
                    Debug.LogFormat("[QR Code Scanner] Pretending to scan the QR Code.");
                    ChangeDisplay(1);
                    int value = ReadValue(qrCodes[i]);
                    string valueStr = value.ToString();
                    valueStr = valueStr.Insert(4, Environment.NewLine);
                    _valueDisplay.text = valueStr;

                    // simulate delay when scanning
                    _scanningDisplay.text = "Scan-\nning";
                    float delay = UnityEngine.Random.Range(4f, 7f);
                    int dots = 0;
                    while (true) {
                        switch (dots % 4) {
                            case 0:
                                _scanningDisplay.text = "Scan-\nning";
                                break;
                            case 1:
                                _scanningDisplay.text = "Scan-\nning.";
                                break;
                            case 2:
                                _scanningDisplay.text = "Scan-\nning..";
                                break;
                            case 3:
                                _scanningDisplay.text = "Scan-\nning...";
                                break;
                        }
                        dots++;
                        if (delay < 1f) {
                            break;
						}
                        delay -= 1f;
                        yield return new WaitForSeconds(1f);
                    }
                    yield return new WaitForSeconds(delay);   
                    if (_focus == false) {
                        Debug.LogFormat("[QR Code Scanner] 'Scan' aborted");
                        _waitingDisplay.text = "Scan\nFailed";
                        ChangeDisplay(0);
                        continue;
					}

                    Debug.LogFormat("[QR Code Scanner] 'Scan' complete with the following result: " + valueStr);
                    ChangeDisplay(2);
                    while (ReadActive(qrCodes[i])) {
                        yield return new WaitForSeconds(1f);
                    }
                    Debug.LogFormat("[QR Code Scanner] QR Code Module solved. Blanking.");
                    _waitingDisplay.text = "No QR\nCodes\nFound";
                    ChangeDisplay(0);
                }
            }
        }
    }

    void ChangeDisplay(int state) {
        switch (state) {
            default:
            case 0: // no QR active
                _codeDisplay.gameObject.SetActive(false);
                _valueDisplay.gameObject.SetActive(false);
                _waitingDisplay.gameObject.SetActive(true);
                _scanningDisplay.gameObject.SetActive(false);
                break;
            case 1: // QR found, scanning
                _codeDisplay.gameObject.SetActive(true);
                _valueDisplay.gameObject.SetActive(false);
                _waitingDisplay.gameObject.SetActive(false);
                _scanningDisplay.gameObject.SetActive(true);
                break;
            case 2: // QR scanned
                _codeDisplay.gameObject.SetActive(true);
                _valueDisplay.gameObject.SetActive(true);
                _waitingDisplay.gameObject.SetActive(false);
                _scanningDisplay.gameObject.SetActive(false); 
                break;
		}
    }

    bool ReadActive(QRCode qrc) {
        return (bool)qrc.FieldActive.GetValue(qrc.Component);
    }

    int ReadValue(QRCode qrc) {
        return (int)qrc.FieldValue.GetValue(qrc.Component);
    }

}
