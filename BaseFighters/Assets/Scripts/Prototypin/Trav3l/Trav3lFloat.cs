using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Trav3lFloat : MonoBehaviour
{
    public LayerMask mask;
    public List<Transform> order;
    public List<Vector3> localDir;
    public int step = 1;
    public int perStep = 1;
    int curId = 0;
    public Move move;
    public bool gizmos = true;
    public Color gizmosCol = Color.grey;

    void Update()
    {
        Vector3 correction = Vector3.zero;
        if(order.Count == 0)
            return;
        List<Vector3> dirs = new List<Vector3>();
        foreach (var item in order)
            dirs.Add(item.position - transform.position);
        foreach (var item in localDir)
            dirs.Add(item);
        RayAll(perStep, step, ref curId, dirs, transform.position, ref correction, mask);
        move.AddCorrection(correction);
    }

    static void RayAll(int perStep, int step, ref int curId, List<Vector3> dirs, Vector3 tpos, ref Vector3 correction, LayerMask mask){
        for (int i = 0; i < perStep; i++)
        {
            Vector3 dir = dirs[curId];
            RaycastHit hit;
            if (Physics.Raycast(tpos, dir, out hit, dir.magnitude, mask, QueryTriggerInteraction.Collide))
            {
                correction += -dir;
            }
            // internal steps
            if (i < perStep - 1)
                curId = (curId + 1) % dirs.Count;
        }
        // jump steps
        curId = (curId + (step)) % dirs.Count;
    }

    void OnDrawGizmos(){
        if(!gizmos)
            return;
        Gizmos.color = gizmosCol;
        var tpos = transform.position;
        Gizmos.DrawWireSphere(tpos, 0.5f);
        for (int i = 0; i < order.Count; i++)
            if(order[i] != null)
            Gizmos.DrawLine(tpos, order[i].position);
        for (int i = 0; i < localDir.Count; i++){
            Gizmos.DrawRay(tpos, localDir[i]);    
        }
        Gizmos.color = Color.red;
        List<Vector3> dirs = new List<Vector3>();
        foreach (var item in order)
            dirs.Add(item.position - transform.position);
        foreach (var item in localDir)
            dirs.Add(item);
        Gizmos.DrawRay(tpos, dirs[curId]);     
    }
}