using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct WarpPoint
{
    public MAPNAME TargetScene;
    public Vector3 TargetPos;
    public Quaternion TargetRot;
}
