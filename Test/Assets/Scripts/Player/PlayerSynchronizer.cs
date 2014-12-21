using UnityEngine;
using System.Collections;

public class PlayerSynchronizer : MonoBehaviour {

    bool simulatePhysics = true;
    bool updatePosition = true;
    float physInterp = 0.6f;
    float netInterp = 0.03f;
    double ping = 0f;
    double jitter = 0f;
    bool isResponding = false;			//Updated by the script for diagnostic feedback of the status of this NetworkView
    string netCode = " (No Connection)";	//Updated by the script for diagnostic feedback of the status of this NetworkView
    int m;
    private Vector3 p;
    private float r;
    private State[] states = new State[10];
    private int stateCount = 0;

    PlayerBehaviour player;

    class State : System.Object {
        public Vector3 p;
        public float r;
        public double t;
 	
        public State(Vector3 p, float r, float t) {
            this.p = p;
            this.r = r;
            this.t = t;
        }
    }

    void OnSerializeNetworkView(BitStream stream, NetworkMessageInfo info)
    {
        if (stream.isWriting)
        {
            //We are the server, and have to keep track of relaying messages between connected clients
            /*if(stateCount == 0) return;
            p = states[0].p;
            r = states[0].r;
            m = (int)(Network.time - states[0].t) * 1000;	//m is the number of milliseconds that transpire between the packet's original send time and the time it is resent from the server to all the other clients
            stream.Serialize(ref p);
            stream.Serialize(ref r);
            stream.Serialize(ref m);
             * */

            p = rigidbody2D.position;
            r = rigidbody2D.rotation;
            m = 0;
            stream.Serialize(ref p);
            stream.Serialize(ref r);
            stream.Serialize(ref m);
        }

        //New packet recieved - add it to the states array for interpolation!
        else
        {
            stream.Serialize(ref p);
            stream.Serialize(ref r);
            stream.Serialize(ref m);
            State state = new State(p, r, (float)info.timestamp - (m > 0 ? ((float)m / 1000) : 0));
            if (stateCount == 0) states[0] = state;
            else if (state.t > states[0].t)  // newer packet then before. if old, discard
            {
                for (int k = states.Length - 1; k > 0; k--) states[k] = states[k - 1]; //shift push
                states[0] = state;
                if (player.IsMine()) Debug.Log("pck time : " + state.t);

            }
            //else Debug.Log(gameObject.name + ": Out-of-order state received and ignored (" + ((states[0].t - state.t) * 1000) + ")" + states[0].t + "---" + state.t + "---" + m + "---" + states[0].p.x + "---" + state.p.x);
            stateCount = Mathf.Min(stateCount + 1, states.Length);
        }
    }

    void Start()
    {
        return;
        if (Network.isServer) updatePosition = false;

        player = GetComponent<PlayerBehaviour>();
    }

    double Lerp(double from, double to, double t)
    {
        return from + ((to - from) * t);
    }

    void FixedUpdate() {
        return;
        if(!updatePosition || states[6] == null) return;
 
	    simulatePhysics = (Vector2.Distance(rigidbody2D.position, states[0].p) < 0.7);

        
        //jitter: ping(지연시간)과의 괴리시간
	    //jitter = Lerp(jitter, Mathf.Abs(ping - ((float)Network.time - states[0].t)), Time.fixedDeltaTime * .3f);
        
        //패킷이 안오면 Network.time은 커지고 state.t 는 고정되어서 핑이 점점 커진다.
        //패킷이동시간 Lerp
        //Debug.Log(Time.fixedDeltaTime);
        ping = Lerp(ping, Network.time - states[0].t, Time.fixedDeltaTime * .3f);
        if (player.IsMine()) Debug.Log("Ping: " + ping);
 
	    rigidbody2D.isKinematic = !simulatePhysics;
	    rigidbody2D.interpolation = (simulatePhysics ? RigidbodyInterpolation2D.Interpolate : RigidbodyInterpolation2D.None);
 
	    //Interpolation
	    double interpolationTime = (double)Network.time - netInterp; // Interpolation Start time

        if (player.IsMine()) Debug.Log("InterpolationTime: " + interpolationTime);
	    if (states[0].t > interpolationTime) {												// Target playback time should be present in the buffer
	    	for (int i = 0; i < stateCount; i++) {													// Go through buffer and find correct state to play back
	    		if (states[i] != null && (states[i].t <= interpolationTime || i == stateCount-1)) { //버퍼의 마지막 패킷 스테이트거나 InterpolationTime보다 전이면
	    			State rhs = states[Mathf.Max(i-1, 0)];							// The state one slot newer than the best playback state
	    			State lhs = states[i];											// The best playback state (closest to .1 seconds old)
	    			double l = rhs.t - lhs.t;											// Use the time between the two slots to determine if interpolation is necessary
	    			float t = 0.0f;													// As the time difference gets closer to 100 ms, t gets closer to 1 - in which case rhs is used
	    			if (l > 0.0001) t = (float)((interpolationTime - lhs.t) / l);					// if t=0 => lhs is used directly
                    //t가 클수록 앞 패킷과 뒤 패킷 시간 차가 큼.
	    			if(simulatePhysics) {
	    				rigidbody2D.position = Vector2.Lerp(rigidbody2D.position, Vector2.Lerp(lhs.p, rhs.p, (float)t), physInterp); 
	    				rigidbody2D.rotation = Mathf.Lerp(rigidbody2D.rotation, Mathf.Lerp(lhs.r, rhs.r, (float)t), physInterp);
	    				rigidbody2D.velocity = ((rhs.p - states[i + 1].p) / (float)(rhs.t - states[i + 1].t));
	    			}
	    			else {
	    				rigidbody2D.position = Vector2.Lerp(lhs.p, rhs.p, t);
	    				rigidbody2D.rotation = Mathf.Lerp(lhs.r, rhs.r, t);
	    			}
	    			isResponding = true;
	    			netCode = "";
	    			return;
	    		}
	    	}
	    }
 
	    //Extrapolation
	    else  {
	    	double extrapolationLength = (interpolationTime - states[0].t);
	    	if (extrapolationLength < 1f && states[0] != null && states[1] != null) {
	    		if(!simulatePhysics) {
	    			rigidbody2D.position = states[0].p + (((states[0].p - states[1].p) / (float)(states[0].t - states[1].t)) * (float)extrapolationLength);
	    			rigidbody2D.rotation = states[0].r;
	    		}
	    		isResponding = true;
	    		if(extrapolationLength < .5) netCode = ">";
	    		else netCode = " (Delayed)";
	    	}
	    	else {
	    		netCode = " (Not Responding)";
	    		isResponding = false;
	    	}
	    }
	    if(simulatePhysics && states[0].t > states[2].t) rigidbody2D.velocity = ((states[0].p - states[2].p) / (float)(states[0].t - states[2].t));
    }
}
