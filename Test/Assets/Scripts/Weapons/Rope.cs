using UnityEngine;
using System.Collections;
using Server;
using Const;
using S2C = Packet.S2C;

public class Rope : Projectile {

    private HingeJoint2D stickHJ;
    private SpringJoint2D ropeSJ;
    private bool isSticked = false;
    private Client.PlayerBehaviour C_ropeSource;

    public GameObject ropeImagePrefab;
    private float imageWidth;

    void Start()
    {
        imageWidth = ropeImagePrefab.GetComponent<SpriteRenderer>().sprite.texture.width / 8f;
    }

    protected override void OnCollideToTile(Collider2D targetCollider)
    {
        if (isSticked) return;

        Tile tile = targetCollider.gameObject.GetComponent<Tile>();
        if (tile)
        {
            ConnectRope(tile.gameObject, tile.ID, ObjectType.TILE);
        }
    }

    protected override void OnCollideToPlayer(Collider2D targetCollider)
    {
        if (isSticked) return;

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
        if (isSticked && C_ropeSource)
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
        if (isSticked) return;

        if (ServerGame.Inst.isDedicatedServer == false)
            C_ropeSource = Client.C_PlayerManager.Inst.Get(owner);

        StickToTarget(target, targetID, objType);

        ServerPlayer player = PlayerManager.Inst.Get(owner);

        ropeSJ = gameObject.AddComponent<SpringJoint2D>();
        ropeSJ.connectedBody = player.rigidbody2D;
        ropeSJ.distance = 2f;
        ropeSJ.frequency = 3;
        ropeSJ.dampingRatio = 2;

        player.Roped(this);
    }

    void StickToTarget(GameObject target, long targetID, ObjectType objType)
    {
        isSticked = true;
        RaycastHit2D hit = Physics2D.Raycast(transform.position, target.transform.position - transform.position, 5);

        stickHJ = gameObject.AddComponent<HingeJoint2D>();
        stickHJ.connectedBody = target.rigidbody2D;
        stickHJ.anchor = transform.InverseTransformPoint(hit.point);
        stickHJ.connectedAnchor = target.transform.InverseTransformPoint(hit.point);
        rigidbody2D.velocity = new Vector2(0f, 0f);


        S2C.RopeStuck pck = new S2C.RopeStuck(targetID, objType, transform.position, stickHJ.anchor, stickHJ.connectedAnchor);

        BroadcastRopeStuck(pck);
    }

    void BroadcastRopeStuck(S2C.RopeStuck pck)
    {
        networkView.RPC("SetRopeStuck", RPCMode.Others, pck.SerializeToBytes());
    }

    [RPC]
    void SetRopeStuck(byte[] pckData, NetworkMessageInfo info)
    {
        //ServerCheck

        S2C.RopeStuck pck = S2C.RopeStuck.DeserializeFromBytes(pckData);

        GameObject targetObj;

        switch(pck.objType)
        {
            case ObjectType.PLAYER:
                targetObj = Client.C_PlayerManager.Inst.Get((int)pck.targetID).gameObject;
                break;

            case ObjectType.PROJECTILE:
                targetObj = ProjectileManager.Inst.Get(pck.targetID).gameObject;
                break;

            case ObjectType.TILE:
                targetObj = Game.Inst.map.GetTile((int)pck.targetID).gameObject;
                break;

            default:
                return;
        }

        isSticked = true;

        C_ropeSource = Client.C_PlayerManager.Inst.Get(owner);

        transform.position = pck.ropePos;
        stickHJ = gameObject.AddComponent<HingeJoint2D>();
        stickHJ.connectedBody = targetObj.rigidbody2D;
        stickHJ.anchor = transform.InverseTransformPoint(pck.ropeAnchor);
        stickHJ.connectedAnchor = targetObj.transform.InverseTransformPoint(pck.targetAnchor);
        rigidbody2D.velocity = new Vector2(0f, 0f);
    }
    

    public void Cut()
    {
        Unstick();
        Destroy(gameObject);
    }

    void Unstick()
    {
        if (stickHJ != null)
            Destroy(stickHJ);

        rigidbody2D.mass = 1;
    }
}
