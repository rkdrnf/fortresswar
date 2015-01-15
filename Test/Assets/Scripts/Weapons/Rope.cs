using UnityEngine;
using System.Collections;
using Server;
using Const;
using S2C = Packet.S2C;

public class Rope : Projectile {

    private HingeJoint2D stickHJ;
    private SpringJoint2D ropeSJ;

    ServerPlayer ownerPlayer;
    private ServerPlayer ropeSource;
    GameObject ropeTarget;

    public GameObject ropeImagePrefab;
    private float imageWidth;

    S2C.RopeStickInfo stickInfo;

    const int maxGap = 5;

    public LayerMask groundLayer;

    

    [RPC]
    protected override void RequestCurrentStatus(NetworkMessageInfo info)
    {
        S2C.RopeStatus pck = new S2C.RopeStatus(owner, transform.position, rigidbody2D.velocity, stickInfo);

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
        }
    }

    void Start()
    {
        imageWidth = ropeImagePrefab.GetComponent<SpriteRenderer>().sprite.texture.width / 8f;
        stickInfo = new S2C.RopeStickInfo();
        stickInfo.isSticked = false;
    }


    protected override void OnCollideToTile(Tile tile, Vector2 point)
    {
        if (stickInfo.isSticked) return;

        if (tile)
        {
            ConnectRope(tile.gameObject, tile.m_ID, ObjectType.TILE);
        }
    }

    protected override void OnCollideToBuilding(Building building, Vector2 point)
    {
        if (stickInfo.isSticked) return;

        if (building)
        {
            ConnectRope(building.gameObject, building.m_ID, ObjectType.BUILDING);
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

            ConnectRope(character.gameObject, character.GetOwner(), ObjectType.PLAYER);
            
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

    void ConnectRope(GameObject target, long targetID, ObjectType objType)
    {
        if (stickInfo.isSticked) return;

        if (ServerGame.Inst.isDedicatedServer == false)
            ropeSource = PlayerManager.Inst.Get(owner);

        StickToTarget(target, targetID, objType);

        ownerPlayer = PlayerManager.Inst.Get(owner);

        ropeSJ = gameObject.AddComponent<SpringJoint2D>();
        ropeSJ.connectedBody = ownerPlayer.rigidbody2D;
        ropeSJ.distance = 5f;
        ropeSJ.frequency = 2;
        ropeSJ.dampingRatio = 1;

        ownerPlayer.Roped(this);
    }

    void StickToTarget(GameObject target, long targetID, ObjectType objType)
    {
        RaycastHit2D hit = Physics2D.Raycast(transform.position, target.transform.position - transform.position, 5);

        stickHJ = gameObject.AddComponent<HingeJoint2D>();
        stickHJ.connectedBody = target.rigidbody2D;
        stickHJ.anchor = transform.InverseTransformPoint(hit.point);
        stickHJ.connectedAnchor = target.transform.InverseTransformPoint(hit.point);
        rigidbody2D.velocity = new Vector2(0f, 0f);

        if (target.tag == "Player")
        {
            target.GetComponent<ServerPlayer>().RopedToMe(this);
        }

        ropeTarget = target;


        stickInfo = new S2C.RopeStickInfo(true, targetID, objType, transform.position, stickHJ.anchor, stickHJ.connectedAnchor);
        networkView.RPC("SetRopeStuck", RPCMode.Others, stickInfo.SerializeToBytes());
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

        GameObject targetObj;

        switch(info.objType)
        {
            case ObjectType.PLAYER:
                targetObj = PlayerManager.Inst.Get((int)info.targetID).gameObject;
                break;

            case ObjectType.PROJECTILE:
                targetObj = ProjectileManager.Inst.Get(info.targetID).gameObject;
                break;

            case ObjectType.TILE:
                targetObj = Game.Inst.map.GetTile(Game.Inst.map.ToGridCoord((int)info.targetID)).gameObject;
                break;

            default:
                return;
        }

        ropeSource = PlayerManager.Inst.Get(owner);

        transform.position = targetObj.transform.TransformPoint(info.targetAnchor);
        stickHJ = gameObject.AddComponent<HingeJoint2D>();
        stickHJ.connectedBody = targetObj.rigidbody2D;
        stickHJ.anchor = info.anchor;
        stickHJ.connectedAnchor = info.targetAnchor;
        rigidbody2D.velocity = new Vector2(0f, 0f);
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

        if (ropeTarget.tag == "Player")
        {
            ropeTarget.GetComponent<ServerPlayer>().CutInfectingRope(this);
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
