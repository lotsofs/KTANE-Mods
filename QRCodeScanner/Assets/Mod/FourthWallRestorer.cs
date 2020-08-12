using System;
using System.CodeDom;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using KModkit;
using Newtonsoft.Json;
using UnityEngine;

public class FourthWallRestorer : MonoBehaviour
{
    void Update()
    {

        //KMNeedyModule[] needies = FindObjectsOfType<KMNeedyModule>();
        //foreach (KMNeedyModule needy in needies) {
        //    transform.GetChild(0).GetComponent<MeshRenderer>().material.mainTexture = needy.transform.GetChild(3).GetChild(0).GetComponent<MeshRenderer>().material.mainTexture;
        //    // ^ changing texture works. Child 3 grandchild 0 of QR code.

        //    // V value works. Just need to print these two.

        //    Component comp = needy.GetComponent("QRCode");
        //    if (comp == null) {
        //        Debug.Log("Nope ... " + needy.gameObject.name);
        //    }
        //    Debug.Log("Found sth" + comp.gameObject.name);
        //    comp = needy.GetComponents(typeof(Component)).FirstOrDefault(c => c.GetType().FullName == "QRCode");
        //    if (comp == null) {
        //        Debug.LogFormat("FAILURE {0} game object has no {1} component. Components are: {2}", needy.name, name, needy.GetComponents(typeof(Component)).Select(c => c.GetType().FullName).Join(", "));
        //    }
        //    Debug.Log("SUCCESS");

        //    System.Reflection.FieldInfo field;
        //    Type type = comp.GetType();


        //    field = type.GetField("QRMessage", BindingFlags.NonPublic | BindingFlags.Instance);
        //    Debug.Log(type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static).Select(f => string.Format("{0} {1} {2}", f.IsPublic ? "public" : "private", f.FieldType.FullName, f.Name)).Join(","));
        //    Debug.Log(field);

        //    var value = field.GetValue(comp);
        //    Debug.Log(value);
        }

    }

    //void GetQRCodeModules() {
    //    qrCodes = new List<KMNeedyModule>();
    //    KMNeedyModule[] needies = FindObjectsOfType<KMNeedyModule>();
    //    foreach (KMNeedyModule needy in needies) {
    //        Component comp = needy.GetComponent("QRCode");
    //        if (comp == null) {
    //            continue;
    //        }

    //        FieldInfo field;
    //        Type type = comp.GetType();

    //        field = type.GetField("QRMessage", BindingFlags.NonPublic | BindingFlags.Instance);
    //    }
    //}


