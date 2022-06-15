using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Aim : MonoBehaviour, ITFunc
{
	public Transform self;
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

    void OnEnable(){
		if(self == null)
			self = transform;
        aimAt = self.position + self.up;
		if(Camera.main == null)Debug.LogError("No main camera");
		else if(Camera.main.orthographic == false) Debug.LogError("Main cam should be ortographic... perspective doesn't work with input");
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
            Vector2 targetDir = aimAt - self.position;
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

            lastPos = (Vector2)self.position + targetDir;
            var targetRot = Quaternion.LookRotation(Vector3.forward, targetDir);
            self.rotation = Quaternion.Slerp(self.rotation, targetRot, rSpeed/100f);
        }
    }

    void OnDrawGizmosSelected(){
		if(self == null)self = transform;
        if(!Application.isPlaying){
            if(autoTrack){
                Gizmos.DrawWireSphere(self.position, close);
                Gizmos.color = Color.gray;
                Gizmos.DrawWireSphere(self.position, outRadius);
                Gizmos.color = Color.red;
                Gizmos.DrawWireSphere(self.position, 0.5f);
                Gizmos.color = Color.yellow;
                Gizmos.DrawWireSphere(self.position, 0.45f);
            }
            return;
        }
        if(autoTrack){
            Gizmos.DrawWireSphere(self.position, close);
            Gizmos.color = Color.gray;
            Gizmos.DrawWireSphere(self.position, outRadius);
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
	
	public void Func(List<string> args){
    }

    public void SetTarget(Transform target){
        this.target = target;
    }
}

public interface ITFunc{
    void Func(string funcName, Transform value);
    void Func(List<string> args);
}