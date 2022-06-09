using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Move : MonoBehaviour, ITFunc
{
	public Transform self;
    public bool networked = true;

    [Header("mode 1 - wasd")]
    public bool wasdIn = false;
    public bool raw = false;

    [Header("mode 2 - target")]
    public bool stopOnLoss=true;
    public Transform target;
    public float stopDist = 0;
    bool hadTarget = false;

    [Header("mode 3 - pong correction")]
    public bool pong=false;

    [Header("overlay - correction")]
    [SerializeField] Vector3 correction = Vector3.zero;
    public float correctionMult = 1f;
    [Range(0f, 1f)] public float correctionDecay = 0f;

    [Header("specs")]
    public bool flatZ = true;
    public bool quadraticAccel = false;
    [SerializeField] Vector3 preference = Vector3.zero;
    [SerializeField] Vector3 move = Vector2.up;
    public float speed = 10f;
    public bool normalize = true;
    public Space moveSpace = Space.World;
    float curSpeed;
	[Header("props")]
	public InteractMoveProps movingUpdater;
	
    // Update is called once per frame
    void Update()
    {
		if(self == null)
			self = transform;
		
        curSpeed = Time.deltaTime*speed;

        float hor = 0;
        float ver = 0;
        if(wasdIn){
            if(raw){
                hor = Input.GetAxisRaw("Horizontal");
                ver = Input.GetAxisRaw("Vertical");
            }else{
                hor = Input.GetAxis("Horizontal");
                ver = Input.GetAxis("Vertical");
                if(quadraticAccel){
                    hor*=hor*Mathf.Sign(hor);
                    ver*=ver*Mathf.Sign(ver);
                }
            }
            move = new Vector3(hor, ver);
			if(movingUpdater != null){
				movingUpdater.value = hor != 0 || ver!= 0 ? 1 : 0;
				movingUpdater.UpdateProp();
			}
        }else if(target != null){
            var mag = move.magnitude;
            move = (target.position - self.position).normalized * mag;
            hadTarget = true;
        }else {
            if(stopOnLoss && hadTarget){
                move = Vector3.zero;
                hadTarget = false;
            }
        }
        

        Vector3 next = move + correction * correctionMult;
        
        if(flatZ){
            next.z = 0;
        }
        if(normalize)
            next = next.normalized * curSpeed;
        else 
            next = next * curSpeed;

        if((self.position + next).magnitude <= stopDist)
            next = Vector3.Lerp(Vector3.zero, next, 0.3f);
        
        self.Translate(next, moveSpace);
        correction*=1-correctionDecay;
        if(pong && correction.sqrMagnitude < 0.1f && move != preference)
            move = preference;
    }

    public void AddCorrection(Vector3 correction){
        if(pong){
            move += correction;
            move = move.normalized;
        }
        this.correction += correction;
    }

    void OnDrawGizmos(){
        Gizmos.color = Color.green;
        Gizmos.DrawRay(transform.position, transform.TransformDirection(move));   
        Gizmos.color = Color.yellow;
        Gizmos.DrawRay(transform.position, transform.TransformDirection(correction));   
    }

    public void Func(string name, Transform value){
        if(name == "SetTarget")
            SetTarget(value);
    }

    public void SetTarget(Transform target){
        this.target = target;
    }
	
}

