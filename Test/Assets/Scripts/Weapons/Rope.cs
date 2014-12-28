using UnityEngine;
using System.Collections;
using Server;
using Const;
using S2C = Packet.S2C;

public class Rope : Projectile {

    private HingeJoint2D stickHJ;
    private SpringJoint2D ropeSJ;

    private Client.PlayerBehaviour C_ropeSource;

    public GameObject ropeImagePrefab;
    private float imageWidth;

    S2C.RopeStickInfo stickInfo;

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

        OnSetRopeStuck(pck.stickInfo);
    }

    protected override void OnInit()
    {
        if (Network.isServer)
        {
            PlayerManager.Inst.Get(owner).OnFireRope(this);
        }
    }

    void Start()
    {
        imageWidth = ropeImagePrefab.GetComponent<SpriteRenderer>().sprite.texture.width / 8f;
        stickInfo = new S2C.RopeStickInfo();
        stickInfo.isSticked = false;
    }


    protected override void OnCollideToTile(Collider2D targetCollider)
    {
        if (stickInfo.isSticked) return;

        Tile tile = targetCollider.gameObject.GetComponent<Tile>();
        if (tile)
        {
            ConnectRope(tile.gameObject, tile.ID, ObjectType.TILE);
        }
    }

    protected override void OnCollideToPlayer(Collider2D targetCollider)
    {
        if (stickInfo.isSticked) return;

        //When Hit My Player
        ServerPlayer character = targetCollider.gameObject.GetComponent<ServerPlayer>();
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
        if (C_ropeSource)
        {
            foreach(Transform prevChild in transform)
            {
                Destroy(prevChild.gameObject);
            }

            Vector2 startPoint = C_ropeSource.transform.position;
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
            C_ropeSource = Client.C_PlayerManager.Inst.Get(owner);

        StickToTarget(target, targetID, objType);

        ServerPlayer player = PlayerManager.Inst.Get(owner);

        ropeSJ = gameObject.AddComponent<SpringJoint2D>();
        ropeSJ.connectedBody = player.rigidbody2D;
        ropeSJ.distance = 5f;
        ropeSJ.frequency = 2;
        ropeSJ.dampingRatio = 1;

        player.Roped(this);
    }

    void StickToTarget(GameObject target, long targetID, ObjectType objType)
    {
        RaycastHit2D hit = Physics2D.Raycast(transform.position, target.transform.position - transform.position, 5);

        stickHJ = gameObject.AddComponent<HingeJoint2D>();
        stickHJ.connectedBody = target.rigidbody2D;
        stickHJ.anchor = transform.InverseTransformPoint(hit.point);
        stickHJ.connectedAnchor = target.transform.InverseTransformPoint(hit.point);
        rigidbody2D.velocity = new Vector2(0f, 0f);


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
                targetObj = Client.C_PlayerManager.Inst.Get((int)info.targetID).gameObject;
                break;

            case ObjectType.PROJECTILE:
                targetObj = ProjectileManager.Inst.Get(info.targetID).gameObject;
                break;

            case ObjectType.TILE:
                targetObj = Game.Inst.map.GetTile((int)info.targetID).gameObject;
                break;

            default:
                return;
        }

        C_ropeSource = Client.C_PlayerManager.Inst.Get(owner);

        transform.position = info.position;
        stickHJ = gameObject.AddComponent<HingeJoint2D>();
        stickHJ.connectedBody = targetObj.rigidbody2D;
        stickHJ.anchor = info.anchor;
        stickHJ.connectedAnchor = info.targetAnchor;
        rigidbody2D.velocity = new Vector2(0f, 0f);
    }
    

    public void Cut()
    {
        Unstick();
        Network.RemoveRPCs(networkView.viewID);
        Network.Destroy(gameObject);
    }

    void Unstick()
    {
        if (stickHJ != null)
            Destroy(stickHJ);

        rigidbody2D.mass = 1;
    }
}
