using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClothSystem : MonoBehaviour
{
    [Header("========== 點的資訊 ==========")]
    public int              SideUnitCount = 10;                                 // 邊長總共要有幾個點
    public int              ClothHeight = 5;
    public float              UnitSide = 0.6f;                                       // 點跟點的距離

    
    public Texture              texture;
    public Material             twoSideMat;
    public List<Vector3>        VertexArray             = new List<Vector3>();
    public List<GameObject>     ColliderArray           = new List<GameObject>();
    public List<Vector2>        UVArray                 = new List<Vector2>();
    public List<int>            TrianglesIndexArray     = new List<int>();
    private MeshFilter          meshFilter;
    private MeshRenderer        meshRender;

    [Header("========== 力的資訊 ==========")]
    public float            Mass = 1;
    public float            GravityAddSpeed = -9.81f;                           // 重力加速度
    public int              UseForceStatus = 0;
    //////////////////////////////////////////////////////////////////////////////
    // Use Force Status
    //////////////////////////////////////////////////////////////////////////////
    // 0    => Euler
    // 1    => Runge-Kutta 2
    // 2    => Runge-Kutta 4
    private List<SpringSystem> SpringArray          = new List<SpringSystem>();
    private List<Vector3> SpeedArray                = new List<Vector3>();

    private void Start()
    {
        // 加上 Component
        meshFilter = this.gameObject.AddComponent<MeshFilter>();
        meshRender = this.gameObject.AddComponent<MeshRenderer>();

        // 創建 Mesh
        Mesh mesh = new Mesh();
        meshFilter.mesh = mesh;
        meshRender.material = twoSideMat;
        meshRender.material.mainTexture = texture;

        #region 增加 Mesh 的資料
        float offset = SideUnitCount / 2 * UnitSide;
        for (int i = 0; i < SideUnitCount; i++)
            for (int j = 0; j < SideUnitCount; j++)
            {
                #region 點
                VertexArray.Add(new Vector3(i * UnitSide - offset, ClothHeight, j * UnitSide - offset));
                #endregion
                #region UV
                float CorrentPrograssI = (float)i / (SideUnitCount - 1);
                float CorrentPrograssJ = (float)j / (SideUnitCount - 1);

                UVArray.Add(new Vector2(CorrentPrograssI, CorrentPrograssJ));
                #endregion
                #region 移動資訊
                SpeedArray.Add(Vector3.zero);
                #endregion
                #region 彈簧資訊
                AddSpringWithIndex(i * SideUnitCount + j);
                #endregion
                #region Collider                
                GameObject colliderObject = new GameObject("ColliderObject");
                colliderObject.transform.SetParent(this.transform);
                colliderObject.transform.position = new Vector3(i * UnitSide - offset, ClothHeight, j * UnitSide - offset);
                MyRigidbody myRigidbody = colliderObject.AddComponent<MyRigidbody>();
                //Rigidbody rigidbody = colliderObject.AddComponent<Rigidbody>();
                //rigidbody.useGravity = false;
                //rigidbody.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
                SphereCollider sCollider = colliderObject.AddComponent<SphereCollider>();
                sCollider.radius = 0.3f;
                colliderObject.AddComponent<Collider>();
                ColliderArray.Add(colliderObject);
                #endregion
            }
        #endregion
        #region 三角形的 Index
        for (int i = 0; i < SideUnitCount - 1; i++)
            for (int j = 0; j < SideUnitCount - 1; j++)
            {
                // Index 資訊
                int CurrentTrianglesIndex = i * SideUnitCount + j;
                TrianglesIndexArray.Add(CurrentTrianglesIndex);
                TrianglesIndexArray.Add(CurrentTrianglesIndex + 1);
                TrianglesIndexArray.Add(CurrentTrianglesIndex + SideUnitCount);

                TrianglesIndexArray.Add(CurrentTrianglesIndex + 1);
                TrianglesIndexArray.Add(CurrentTrianglesIndex + 1 + SideUnitCount);
                TrianglesIndexArray.Add(CurrentTrianglesIndex + SideUnitCount);
            }
        #endregion

        // 給進 Mesh 裡
        mesh.vertices = VertexArray.ToArray();
        mesh.uv = UVArray.ToArray();
        mesh.triangles = TrianglesIndexArray.ToArray();
    }

    private void FixedUpdate()
    {
        // 彈簧
        Vector3[] tempSpeedArray = new Vector3[SpeedArray.Count];
        for (int i = 0; i < SpringArray.Count; i++)
        {
            // 拿 Index
            int StartIndex = SpringArray[i].ConnectIndexStart;
            int EndIndex = SpringArray[i].ConnectIndexEnd;

            // 拿資料
            Vector3 StartSpeed = SpeedArray[StartIndex];
            Vector3 EndSpeed = SpeedArray[EndIndex];
            Vector3 StartPos = VertexArray[StartIndex];
            Vector3 EndPos = VertexArray[EndIndex];

            Vector3 tempForce = SpringArray[i].CountForce(StartSpeed, EndSpeed, StartPos, EndPos);
            tempSpeedArray[StartIndex] += tempForce / Mass * Time.fixedDeltaTime;
            tempSpeedArray[EndIndex] -= tempForce / Mass * Time.fixedDeltaTime;
        }

        // 把 temp Speed Array 加進 Speed 裡
        for (int i = 0; i < SpeedArray.Count; i++)
        {
            SpeedArray[i] += tempSpeedArray[i];
            // 加上 Gravity 的值
            SpeedArray[i] += Vector3.up * GravityAddSpeed * Time.fixedDeltaTime;                 // V += a △t

            bool isCollision = ColliderArray[i].GetComponent<Collider>().RayCaster(SpeedArray[i]);
            if (isCollision)
            {
                //SpeedArray[i] *= -1.0f;
                
                SpeedArray[i] = Vector3.zero;
                //SpeedArray[i] -= ColliderArray[i].GetComponent<Collider>().curRelativeSpeed * 1.0f - Vector3.up * GravityAddSpeed * Time.fixedDeltaTime;
            }
            else
            {                
                
            }


            



        }
        // 要固定的點
        SpeedArray[SideUnitCount * SideUnitCount - 1] = Vector3.zero;
        SpeedArray[SideUnitCount - 1] = Vector3.zero;

        for (int i = 0; i < SideUnitCount; i++)
            for(int j = 0; j < SideUnitCount; j++)
            {
                int CurrentIndex = i * SideUnitCount + j;
                //更新Collider位置
                Vector3 result = Vector3.zero;
                switch (UseForceStatus)
                {                   
                    // Euler => 就是直接算結果，不作微分跟積分 
                    case 0:
                        result = EulerMethod(CurrentIndex, Time.fixedDeltaTime);
                        break;
                    case 1:
                        result = RungeKutta2(CurrentIndex, Time.fixedDeltaTime);
                        break;
                    case 2:
                        result = RungeKutta4(CurrentIndex, Time.fixedDeltaTime);
                        break;
                }
                if (ColliderArray[CurrentIndex].GetComponent<Collider>().isCollision)
                {
                }
                else
                {
                    VertexArray[CurrentIndex] += result;
                }                
                ColliderArray[CurrentIndex].transform.position = VertexArray[CurrentIndex];                
            }
            meshFilter.mesh.vertices = VertexArray.ToArray();
    }
    

    // x     5     6
    //    x  3  4
    // x  x  0  1  2
    //    x  x  7
    // x     x     8
    // 彈簧資訊 (8 個方向 + 往外 8 格) => 因為會重複，所以只要加右上的彈簧就好
    private void AddSpringWithIndex(int index)
    {
        int NextIndex;

        // 1
        NextIndex = index + 1;
        if (NextIndex / SideUnitCount == index / SideUnitCount)
            SpringArray.Add(new SpringSystem(index, NextIndex, (float)UnitSide));

        // 2
        NextIndex = index + 2;
        if (NextIndex / SideUnitCount == index / SideUnitCount)
            SpringArray.Add(new SpringSystem(index, NextIndex, (float)UnitSide * 2));

        // 3
        NextIndex = index + SideUnitCount;
        if (NextIndex / SideUnitCount < SideUnitCount)
            SpringArray.Add(new SpringSystem(index, NextIndex, (float)UnitSide));

        // 4
        NextIndex = index + SideUnitCount + 1;
        if (NextIndex / SideUnitCount < SideUnitCount && NextIndex /SideUnitCount == index / SideUnitCount + 1)
            SpringArray.Add(new SpringSystem(index, NextIndex, (float)UnitSide * Mathf.Sqrt(2)));

        // 5
        NextIndex = index + SideUnitCount * 2;
        if (NextIndex / SideUnitCount < SideUnitCount)
            SpringArray.Add(new SpringSystem(index, NextIndex, (float)UnitSide  * 2));

        // 6
        NextIndex = index + 2 + SideUnitCount * 2;
        if (NextIndex / SideUnitCount < SideUnitCount && NextIndex / SideUnitCount == (NextIndex - 2) / SideUnitCount)
            SpringArray.Add(new SpringSystem(index, NextIndex, (float)UnitSide * Mathf.Sqrt(2) * 2));

        // 7
        NextIndex = index - SideUnitCount + 1;
        if (NextIndex >= 0 && NextIndex / SideUnitCount == (NextIndex - 1) / SideUnitCount && NextIndex - 1 >= 0)
            SpringArray.Add(new SpringSystem(index, NextIndex, (float)UnitSide * Mathf.Sqrt(2)));

        // 8
        NextIndex = index - SideUnitCount * 2 + 2;
        if (NextIndex >= 0 && NextIndex / SideUnitCount == (NextIndex - 2) / SideUnitCount && NextIndex - 2 >= 0)
            SpringArray.Add(new SpringSystem(index, NextIndex, (float)UnitSide * Mathf.Sqrt(2) * 2));
    }

    // 一般的 Euler 方法
    private Vector3 EulerMethod(int CurrentIndex, float GapTime)
    {
        return SpeedArray[CurrentIndex] * GapTime;
    }
    // 算下一段時間，會加上下一秒的力
    private Vector3 EulerMethodWithAppendForce(int CurrentIndex, float GapTime, Vector3 AppendForce)
    {
        return (SpeedArray[CurrentIndex] + AppendForce) * GapTime;
    }
    private Vector3 RungeKutta2(int CurrentIndex, float GapTime)
    {
        if (CurrentIndex == SideUnitCount - 1 || CurrentIndex == SideUnitCount * SideUnitCount - 1)
            return Vector3.zero;
        #region K1
        Vector3 K1 = EulerMethod(CurrentIndex, GapTime);
        #endregion
        #region K2
        Vector3 AppendSpeedK2 = Vector3.up * GravityAddSpeed * Time.deltaTime / 2;                                  // V = a t
        for (int i = 0; i < SpringArray.Count; i++)
            if (SpringArray[i].ConnectIndexStart == CurrentIndex || SpringArray[i].ConnectIndexEnd == CurrentIndex)
            {
                // 拿 Index
                int StartIndex = SpringArray[i].ConnectIndexStart;
                int EndIndex = SpringArray[i].ConnectIndexEnd;

                // 拿資料
                Vector3 StartSpeed = SpeedArray[StartIndex];
                Vector3 EndSpeed = SpeedArray[EndIndex];
                Vector3 StartPos = VertexArray[StartIndex] + EulerMethod(StartIndex, GapTime / 2);
                Vector3 EndPos = VertexArray[EndIndex] + EulerMethod(EndIndex, GapTime / 2);

                Vector3 tempForce = SpringArray[i].CountForce(StartSpeed, EndSpeed, StartPos, EndPos);

                if (CurrentIndex == SpringArray[i].ConnectIndexStart)
                    AppendSpeedK2 += tempForce / Mass * Time.fixedDeltaTime;
                else
                    AppendSpeedK2 -= tempForce / Mass * Time.fixedDeltaTime;
            }

        Vector3 K2 = EulerMethodWithAppendForce(CurrentIndex, GapTime / 2, AppendSpeedK2);
        #endregion
        return K2;
    }
    private Vector3 RungeKutta4(int CurrentIndex, float GapTime)
    {
        if (CurrentIndex == SideUnitCount - 1 || CurrentIndex == SideUnitCount * SideUnitCount - 1)
            return Vector3.zero;
        #region K1
        Vector3 K1 = EulerMethod(CurrentIndex, GapTime);
        #endregion
        #region K2
        Vector3 AppendSpeedK2 = Vector3.up * GravityAddSpeed * Time.fixedDeltaTime / 2;                                  // V = a t
        for (int i = 0; i < SpringArray.Count; i++)
            if (SpringArray[i].ConnectIndexStart == CurrentIndex || SpringArray[i].ConnectIndexEnd == CurrentIndex)
            {
                // 拿 Index
                int StartIndex = SpringArray[i].ConnectIndexStart;
                int EndIndex = SpringArray[i].ConnectIndexEnd;

                // 拿資料
                Vector3 StartSpeed = SpeedArray[StartIndex];
                Vector3 EndSpeed = SpeedArray[EndIndex];
                Vector3 StartPos = VertexArray[StartIndex] + EulerMethod(StartIndex, GapTime / 2);
                Vector3 EndPos = VertexArray[EndIndex] + EulerMethod(EndIndex, GapTime / 2);

                Vector3 tempForce = SpringArray[i].CountForce(StartSpeed, EndSpeed, StartPos, EndPos);

                if (CurrentIndex == SpringArray[i].ConnectIndexStart)
                    AppendSpeedK2 += tempForce / Mass * Time.fixedDeltaTime;
                else
                    AppendSpeedK2 -= tempForce / Mass * Time.fixedDeltaTime;
            }

        Vector3 K2 = EulerMethodWithAppendForce(CurrentIndex, GapTime / 2, AppendSpeedK2);
        #endregion
        #region K3
        Vector3 AppendSpeedK3 = Vector3.up * GravityAddSpeed * Time.deltaTime / 2;                                  // V = a t
        for (int i = 0; i < SpringArray.Count; i++)
            if (SpringArray[i].ConnectIndexStart == CurrentIndex || SpringArray[i].ConnectIndexEnd == CurrentIndex)
            {
                // 拿 Index
                int StartIndex = SpringArray[i].ConnectIndexStart;
                int EndIndex = SpringArray[i].ConnectIndexEnd;

                // 拿資料
                Vector3 StartSpeed = SpeedArray[StartIndex];
                Vector3 EndSpeed = SpeedArray[EndIndex];
                Vector3 StartPos = VertexArray[StartIndex] + EulerMethodWithAppendForce(CurrentIndex, GapTime / 2, AppendSpeedK2);
                Vector3 EndPos = VertexArray[EndIndex] + EulerMethodWithAppendForce(CurrentIndex, GapTime / 2, AppendSpeedK2);

                Vector3 tempForce = SpringArray[i].CountForce(StartSpeed, EndSpeed, StartPos, EndPos);

                if (CurrentIndex == SpringArray[i].ConnectIndexStart)
                    AppendSpeedK3 += tempForce / Mass * Time.fixedDeltaTime;
                else
                    AppendSpeedK3 -= tempForce / Mass * Time.fixedDeltaTime;
            }

        Vector3 K3 = EulerMethodWithAppendForce(CurrentIndex, GapTime / 2, AppendSpeedK3);
        #endregion
        #region K4
        Vector3 AppendSpeedK4 = Vector3.up * GravityAddSpeed * Time.deltaTime / 2;                                  // V = a t
        for (int i = 0; i < SpringArray.Count; i++)
            if (SpringArray[i].ConnectIndexStart == CurrentIndex || SpringArray[i].ConnectIndexEnd == CurrentIndex)
            {
                // 拿 Index
                int StartIndex = SpringArray[i].ConnectIndexStart;
                int EndIndex = SpringArray[i].ConnectIndexEnd;

                // 拿資料
                Vector3 StartSpeed = SpeedArray[StartIndex];
                Vector3 EndSpeed = SpeedArray[EndIndex];
                Vector3 StartPos = VertexArray[StartIndex] + EulerMethodWithAppendForce(CurrentIndex, GapTime, AppendSpeedK3);
                Vector3 EndPos = VertexArray[EndIndex] + EulerMethodWithAppendForce(CurrentIndex, GapTime, AppendSpeedK3);

                Vector3 tempForce = SpringArray[i].CountForce(StartSpeed, EndSpeed, StartPos, EndPos);

                if (CurrentIndex == SpringArray[i].ConnectIndexStart)
                    AppendSpeedK4 += tempForce / Mass * Time.fixedDeltaTime;
                else
                    AppendSpeedK4 -= tempForce / Mass * Time.fixedDeltaTime;
            }

        Vector3 K4 = EulerMethodWithAppendForce(CurrentIndex, GapTime, AppendSpeedK4);
        #endregion
        return K1 / 6 + K2 / 3 + K3 / 3 + K4 / 6;
    }
}
