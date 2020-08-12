using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public class QRCodeScanner : MonoBehaviour {

    List<QRCode> qrCodes = new List<QRCode>();

    [SerializeField] MeshRenderer _codeDisplay;
    [SerializeField] TextMesh _valueDisplay;
    [SerializeField] TextMesh _waitingDisplay;

	// Use this for initialization
	void Start () {
        GetQRCodeModules();
        StartCoroutine(Scan());
	}

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
        ChangeDisplay(false);
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
                    Debug.LogFormat("<PDA> QR Code module destroyed.");
                    qrCodes.RemoveAt(i);
                    continue;
                }
                if (ReadActive(qrCodes[i])) {
                    // found a match. 
                    yield return new WaitForSeconds(5f);    // simulates delay when scanning
                    ChangeDisplay(true);
                    int value = ReadValue(qrCodes[i]);
                    string valueStr = value.ToString();
                    valueStr = valueStr.Insert(4, Environment.NewLine);
                    _valueDisplay.text = valueStr;
                    _codeDisplay.material.mainTexture = qrCodes[i].Rend.material.mainTexture;
                    while (ReadActive(qrCodes[i])) {
                        yield return new WaitForSeconds(1f);
                    }
                    ChangeDisplay(false);
                }
            }
        }
    }

    void ChangeDisplay(bool qrFound) {
        _codeDisplay.gameObject.SetActive(qrFound);
        _valueDisplay.gameObject.SetActive(qrFound);
        _waitingDisplay.gameObject.SetActive(!qrFound);
    }

    bool ReadActive(QRCode qrc) {
        return (bool)qrc.FieldActive.GetValue(qrc.Component);
    }

    int ReadValue(QRCode qrc) {
        return (int)qrc.FieldValue.GetValue(qrc.Component);
    }

}
