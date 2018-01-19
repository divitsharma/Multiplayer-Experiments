using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerUI : MonoBehaviour {

    [SerializeField]
    RectTransform thrusterFuelFill;

    private PlayerController controller;

    public void SetPlayerController (PlayerController _cntrllr)
    {
        Debug.Log("celled");
        controller = _cntrllr;
    }

    void Update ()
    {
        SetFuelAmount(controller.GetThrusterFuelAmount());
    }

    void SetFuelAmount (float _amt)
    {
        thrusterFuelFill.localScale = new Vector3(1f, _amt, 1f);
    }
}
