using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Utility
{
    public static void showHide(GameObject gameObject)
    {
        gameObject.SetActive(!gameObject.activeSelf);
    }

    public static void showHide(GameObject gameObject, bool active) {
        gameObject.SetActive(active);
    }
}
