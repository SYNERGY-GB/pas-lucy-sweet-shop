using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class EventItem {
    public string title = "None";
    public string description = "None";

    public List<string> textOnButton;

    public List<string> textOnResponse;
    public List<string> instructionResponse;

    public bool req = false; //True si existen requerimientos
    public float moneyReq = 0f;
    public float debtReq = 0f;
    public float equityReq = 0f;
    public int[] productReq;

    public bool specialReq = false;
    public int specialReqID = 0;
}
