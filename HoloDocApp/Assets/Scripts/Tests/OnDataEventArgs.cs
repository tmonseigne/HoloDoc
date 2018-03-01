using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OnDataEventArgs {

    public OnDataEventArgs(int [] data)
    {
        Data = data;
    }

    public int[] Data;
}
