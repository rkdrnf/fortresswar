using UnityEngine;
using System.Collections;
using Server;
using Const;
using S2C = Packet.S2C;
using Architecture;

public class Rope : Projectile {

    private HingeJoint2D stickHJ;
    private SpringJoint2D ropeSJ;

    ServerPlayer ownerPlayer;
    private ServerPlayer ropeSource;
    IRopable ropeTarget;

    public GameObject ropeImagePrefab;
    private float imageWidth;

    S2C.RopeStickInfo stickInfo;

    const int maxGap = 5;

    public LayerMask groundLayer;

    protected override void BroadcastInit()
    {
        S2C.RopeStatus pck = new S2C.RopeStatus(owner, transform.position, rigidbody2D.velocity, stickInfo);
        networkView.RPC("SetStatus", RPCMode.OthersBuffered, pck.SerializeToBytes());
    }

    [RPC]
    protected override void RequestCurrentStatus(NetworkMessageInfo info)
    {
        S2C.RopeStatus pck = new S2C.RopeStatus(owner, transform.position, rigidbody2D.velocity, stickInfo);
        Debug.Log("Stick: " + stickInfo);
        networkView.RPC("SetStatus", info.sender, pck.SerializeToBytes());
    }

    [RPC]
    protected override void SetStatus(byte[] pckData, NetworkMessageInfo info)
    {
        S2C.RopeStatus pck = S2C.RopeStatus.DeserializeFromBytes(pckData);

        owner = pck.owner;
        transform.position = pck.position;
        rigidbody2D.velocity = pck.velocity;

        ropeSource = PlayerManager.Inst.Get(owner);

        OnSetRopeStuck(pck.stickInfo);
    }

    protected override void OnInit()
    {
        if (Network.isServer)
        {
            PlayerManager.Inst.Get(owner).OnFireRope(this);
            ropeSource = PlayerManager.Inst.Get(owner);
            
            stickInfo = new S2C.RopeStickInfo();
            stickInfo.isSticked = false;
        }
    }

    protected override void OnAwake()
    {
        imageWidth = ropeImagePrefab.GetComponent<SpriteRenderer>().sprite.texture.width / 8f;
    }


    protected override void OnCollideToTile(Tile tile, Vector2 point)
    {
        if (stickInfo.isSticked) return;

        if (tile != null)
        {
            ConnectRope(tile);
        }
    }

    protected override void OnCollideToBuilding(Building building, Vector2 point)
    {
        if (stickInfo.isSticked) return;

        if (building != null)
        {
            ConnectRope(building);
        }
    }

    protected override void OnCollideToPlayer(ServerPlayer character, Vector2 point)
    {
        if (stickInfo.isSticked) return;

        //When Hit My Player
        if (character)
        {
            if (owner == character.GetOwner())
                return;

            if (character.IsDead())
                return;

            ConnectRope(character);
        }
    }

    void Update()
    {
        Rotate();
        DrawRope();

        if (Network.isServer && stickInfo.isSticked == false)
            base.RangeCheck();
    }

    void DrawRope()
    {
        if (ropeSource)
        {
            foreach (Transform prevChild in transform)
            {
                Destroy(prevChild.gameObject);
            }

            Vector2 startPoint = ropeSource.transform.position;
            Vector2 endPoint = transform.position;

            Vector2 direction = (endPoint - startPoint).normalized;

            Quaternion rot = Quaternion.FromToRotation(Vector3.right, direction);

            GameObject child;
            float ropeLength = (startPoint - endPoint).magnitude;
            int index = 0;
            for (float i = 0; i < ropeLength; i += imageWidth)
            {
                child = Instantiate(ropeImagePrefab) as GameObject;
                child.transform.position = startPoint + (index * direction * imageWidth);
                child.transform.parent = transform;
                child.transform.rotation = rot;
                index++;
            }
        }
    }

    void ConnectRope(IRopable ropable)
    {
        if (stickInfo.isSticked) return;

        Stick(ropable, transform.position);

        ConnectToOwner();
    }

    void ConnectToOwner()
    {
        ownerPlayer = PlayerManager.Inst.Get(owner);
        ropeSource = PlayerManager.Inst.Get(owner);

        ropeSJ = gameObject.AddComponent<SpringJoint2D>();
        ropeSJ.connectedBody = ownerPlayer.rigidbody2D;
        ropeSJ.distance = 5f;
        ropeSJ.frequency = 2;
        ropeSJ.dampingRatio = 1;

        ownerPlayer.RopeFired(this);
    }

    void Stick(IRopable ropable, Vector2 position)
    {
        rigidbody2D.velocity = new Vector2(0f, 0f);

        ropable.Roped(this, position);

        ropeTarget = ropable;

        stickInfo = new S2C.RopeStickInfo(true, ropable.GetRopableID(), position);
        networkView.RPC("SetRopeStuck", RPCMode.Others, stickInfo.SerializeToBytes());
    }

    public void MakeHingeJoint(Rigidbody2D targetBody, Vector2 position, Vector2 connectedPosition)
    {
        stickHJ = gameObject.AddComponent<HingeJoint2D>();
        stickHJ.connectedBody = targetBody;
        stickHJ.anchor = transform.InverseTransformPoint(position);
        stickHJ.connectedAnchor = connectedPosition; 
    }

    [RPC]
    void SetRopeStuck(byte[] pckData, NetworkMessageInfo info)
    {
        //ServerCheck

        OnSetRopeStuck(S2C.RopeStickInfo.DeserializeFromBytes(pckData));
    }

    void OnSetRopeStuck(S2C.RopeStickInfo info)
    {
        stickInfo = info;

        if (stickInfo.isSticked == false)
            return;

        IRopable ropable;
        switch(info.m_RID.m_type)
        {
            case ObjectType.TILE:
                ropable = TileManager.Inst.Get((int)info.m_RID.m_ID);
                break;

            case ObjectType.BUILDING:
                ropable = BuildingManager.Inst.Get((int)info.m_RID.m_ID);
                break;

            case ObjectType.PLAYER:
                ropable = PlayerManager.Inst.Get((int)info.m_RID.m_ID);
                break;

                /*
            case ObjectType.PROJECTILE:
                ropable = ProjectileManager.Inst.Get(info.m_RID.m_ID);
                break;
        */
            default:
                return;
        }

        transform.position = stickInfo.position;
        rigidbody2D.velocity = new Vector2(0f, 0f);

        ropable.Roped(this, info.position);

        ropeTarget = ropable;

        ropeSource = PlayerManager.Inst.Get(owner);
    }

    public void Cut()
    {
        Unstick();

        ownerPlayer.StopRoping();

        DestroyFromNetwork();
    }

    void Unstick()
    {
        if (stickHJ != null)
            Destroy(stickHJ);

        rigidbody2D.mass = 1;

        if (ropeTarget != null)
        {
            ropeTarget.CutInfectingRope(this);
        }
    }

    public void ModifyLength(float speed)
    {
        if (ropeSJ != null)
        {
            float dist = Vector2.Distance(transform.position, ropeSource.transform.position);

            //거리보다 로프가 짧으면 로프쪽, 아니면 캐릭터쪽 방향
            float ropeCharDist = (ropeSJ.distance - dist);
            Vector2 direction = (ropeSJ.distance - dist) * (ropeSource.transform.position - transform.position);

            RaycastHit2D hit = Physics2D.Raycast(ropeSource.transform.position, direction, range, groundLayer);

            //장애물이 있고, 캐릭터와 장애물 간 거리보다 캐릭터와 로프 길이가 
            float hitDist = Vector2.Distance(ropeSource.transform.position, hit.point);
            if (hit.collider != null && hitDist < 1.5)
            {
                if (ropeCharDist > hitDist)
                {
                    ropeSJ.distance = dist + hitDist;
                    return;
                }
                else if (ropeCharDist < -hit.distance)
                {
                    ropeSJ.distance = dist - hitDist;
                    return;
                }
            }

            ropeSJ.distance -= speed;

            if (ropeSJ.distance > range)
                ropeSJ.distance = range;

            if (ropeSJ.distance < 3)
                ropeSJ.distance = 3;
        }
    }

    public Vector2 GetNormalVector()
    {
        return transform.position - ropeSource.transform.position;
    }
}
