using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpringSystem
{
    public int ConnectIndexStart;
    public int ConnectIndexEnd;
    public float OrgLength;

    private const float Ks = 500.0f;
    private const float Kd = 1.5f;

    public SpringSystem(int StartIndex, int EndIndex, float SideLength)
    {
        // 連接資訊
        ConnectIndexStart = StartIndex;
        ConnectIndexEnd = EndIndex;

        OrgLength = SideLength;
    }

    public Vector3 CountForce(Vector3 StartSpeed, Vector3 EndSpeed, Vector3 StartPos, Vector3 EndPos)
    {
        float dis = Vector3.Distance(StartPos, EndPos);
        return -(Ks * (dis - OrgLength) + Kd * Vector3.Dot(StartSpeed - EndSpeed, StartPos - EndPos) / dis)
            * (StartPos - EndPos) / dis;
    }
}