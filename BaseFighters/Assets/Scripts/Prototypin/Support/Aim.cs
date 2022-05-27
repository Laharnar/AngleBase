using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Aim : MonoBehaviour, ITFunc
{
    [Header("mode 1 - mouse")]
    public bool mouse = false;
    [Header("mode 2 - target")]
    public bool autoTrack=false;
    public Transform target;
    public float close = 2;
    [Range(0f, 1f)]
    public float damp = 0.25f;
    public float rngWeight = 0.4f;
    public float outRadius = 3;

    [Header("specs")]
    public float rotationSpeed = 50f;
    Vector3 aimAt;
    Vector2 lastPos;
    int trig = 0;
    float closeOff;
    float lastReset;

    void Start(){
        aimAt = transform.position + transform.up;
    }

    // Update is called once per frame
    void Update()
    {
        bool aim = false;
        if(autoTrack && target!= null){
            aimAt = target.position;
            aim = true;
        }else if(mouse){
            aimAt = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            aim = true;
        }
        if(aim){
            var rSpeed = rotationSpeed;
            Vector2 targetDir = aimAt - transform.position;
            if(autoTrack){
                float sum = Mathf.Abs(targetDir.x) + Mathf.Abs(targetDir.y);
                
                // for for getting stuck into infinity.
                if(sum > closeOff){
                    trig = 0;
                }
                if(trig == 0){
                    closeOff = outRadius;
                    if(sum < close){
                        trig += 1;
                        targetDir *= 4;
                    }
                }else if(trig == 1){
                    targetDir *= Random.Range(1, 10);
                    targetDir += Random.insideUnitCircle * rngWeight;
                    if(sum < close/2f){
                        trig += 1;
                        closeOff += Random.Range(-close/2f, close/2f);
                    }
                }else if(trig == 2){
                    rSpeed *= damp;
                    if(sum > closeOff){
                        trig = 0;
                    }
                }
            }

            lastPos = (Vector2)transform.position + targetDir;
            var targetRot = Quaternion.LookRotation(Vector3.forward, targetDir);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, rSpeed/100f);
        }
    }

    void OnDrawGizmosSelected(){
        if(!Application.isPlaying){
            if(autoTrack){
                Gizmos.DrawWireSphere(transform.position, close);
                Gizmos.color = Color.gray;
                Gizmos.DrawWireSphere(transform.position, outRadius);
                Gizmos.color = Color.red;
                Gizmos.DrawWireSphere(transform.position, 0.5f);
                Gizmos.color = Color.yellow;
                Gizmos.DrawWireSphere(transform.position, 0.45f);
            }
            return;
        }
        if(autoTrack){
            Gizmos.DrawWireSphere(transform.position, close);
            Gizmos.color = Color.gray;
            Gizmos.DrawWireSphere(transform.position, outRadius);
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(aimAt, 0.5f);
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(lastPos, 0.45f);
        }
    }

    public void Func(string name, Transform value){
        if(name == "SetTarget")
            SetTarget(value);
    }

    public void SetTarget(Transform target){
        this.target = target;
    }
}

public interface ITFunc{
    void Func(string funcName, Transform value);
}