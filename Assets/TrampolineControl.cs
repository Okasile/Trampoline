using System.Collections.Generic;
using UnityEngine;

public class TrampolineControl : MonoBehaviour
{
    [SerializeField]
    MeshFilter meshFilter;
    [SerializeField]
    Transform left, right;
    [SerializeField, Range(2, 20)]
    int castCounts = 10;
    [SerializeField]
    string layerName = "Default";

    public static TrampolineControl instance;

    Vector3[] points = new Vector3[50];
    Vector3 leftP, rightP, meshLeft, meshRight;

    Transform meshTran;     

    int[] triArray = new int[48 * 3];

    int layer;
    float z;

    Collider2D[] cs = new Collider2D[10];
    List<Vector3> xys = new List<Vector3>();

    Vector3 v3Temp;
    Vector2 v2Temp;

    void Start()
    {
        instance = this;

        v3Temp = new Vector3();
        v2Temp = new Vector2();

        layer = LayerMask.GetMask(layerName);
        z = transform.position.z;
        leftP = new Vector3(left.position.x,left.position.y,z);
        rightP = new Vector3(right.position.x, leftP.y, z);

        Mesh m = meshFilter.mesh;
        meshTran = meshFilter.transform;
        meshLeft = meshTran.InverseTransformPoint(leftP);
        meshRight = meshTran.InverseTransformPoint(rightP);

        points[0] =meshLeft;
        points[1] = meshRight;
        points[2] = meshRight;
        for (int i = 3; i < points.Length; i++)
        {
            points[i] = meshRight;
        }

        triArray[0] = 2;
        triArray[1] = 0;
        triArray[2] = 1;
        for (int i = 3, x = 3; i <= triArray.Length - 3; i += 3, x++)
        {
            triArray[i] = x;
            triArray[i + 1] = x - 1;
            triArray[i + 2] = 1;
        }

        m.vertices = points;
        m.triangles = triArray;

    }
    
    void Update()
    {
        ClearPoints();
        int counts = Physics2D.OverlapAreaNonAlloc(leftP, right.position, cs, layer);
        HitToSetPoints(cs, counts);
        
    }

    public void SetPoints(List<Vector3> xy)
    {
        List<Vector3> xys = SetV(xy);
        for (int i = 2, x = 0; x < xys.Count; x++, i++)
        {
            points[i] = meshTran.InverseTransformPoint(xys[x]);
        }
        meshFilter.mesh.vertices = points;
    }
    public void ClearPoints()
    {
        for (int i = 2; i < points.Length; i++)
        {
            if (points[i] == meshRight)
                break;
            points[i] = meshRight;
        }
        meshFilter.mesh.vertices = points;
    }

    /// <summary>
    /// 把点从左到右排列，把凹点去掉
    /// </summary>
    /// <param name="pointsXY"></param>
    /// <returns></returns>
    List<Vector3> SetV(List<Vector3> pointsXY)
    {
        pointsXY.Sort((x1, x2) => { return (int)Mathf.Sign(x1.x - x2.x); });
        if (pointsXY.Count < 2)
            return pointsXY;
        //检查凹点        
        for (int i = 0; i  < pointsXY.Count; i++)
        {
            int last = pointsXY.Count - 1;
            v3Temp = pointsXY[i];
            Vector3 p1;
            Vector3 p2;
            if (i == 0)
            {
                p1 = v3Temp - leftP;
            }
            else
            {
                p1 = v3Temp - pointsXY[i - 1];
            }
            if (i != last)
            {
                p2 = pointsXY[i + 1] - v3Temp;
            }
            else
            {
                p2 = rightP - pointsXY[last];
            }
            if (Vector3.Cross(p1, p2).z < 0)
            {
                pointsXY.RemoveAt(i);
                i =-1;
            }
        }
        return pointsXY;
    }

    /// <summary>
    /// 给发现的物体打点
    /// </summary>
    /// <param name="collision"></param>
    /// <param name="counts"></param>
    private void HitToSetPoints(Collider2D[] collision, int counts)
    {
        xys.Clear();
        for (int x = 0; x < counts; x++)
        {
            Collider2D c = collision[x];
            Vector3 leftPoint = Vector3.zero;
            v3Temp.Set(c.bounds.extents.x, c.bounds.extents.y, 0);
            leftPoint = c.bounds.center - v3Temp;
            float dis = c.bounds.size.x / castCounts;
            float disY = c.bounds.size.y;
            for (int i = 0; i < castCounts; i++)
            {
                v3Temp.Set(dis * i, 0, 0);
                v2Temp.Set(0, 1);
                RaycastHit2D hit2D = Physics2D.Raycast(leftPoint + v3Temp, v2Temp, disY * i,layer);
                if (hit2D)
                    xys.Add(new Vector3(hit2D.point.x,hit2D.point.y,z));
            }
        }        
        SetPoints(xys);
    }

    private void OnDrawGizmos()
    {        
        Gizmos.DrawLine(left.position, new Vector3(right.position.x, left.position.y, transform.position.z));
        Gizmos.DrawLine(right.position, new Vector3(right.position.x, left.position.y, transform.position.z));
        Gizmos.DrawLine(right.position, new Vector3(left.position.x, right.position.y, transform.position.z));
        Gizmos.DrawLine(left.position, new Vector3(left.position.x, right.position.y, transform.position.z));

    }
}
