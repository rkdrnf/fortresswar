using UnityEngine;
using System.Collections;
using Server;

public class Rope : Projectile {

    private HingeJoint2D stickHJ;
    private SpringJoint2D ropeSJ;
    private bool isSticked = false;
    private ServerPlayer ropeSource;

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
            ConnectRope(tile.gameObject);
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

            ConnectRope(character.gameObject);
        }
    }

    void Update()
    {
        if (isSticked && ropeSource)
        {
            foreach(Transform prevChild in transform)
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

    void ConnectRope(GameObject target)
    {
        if (isSticked) return;

        ropeSource = PlayerManager.Inst.Get(owner);

        StickToTarget(target);

        ServerPlayer player = PlayerManager.Inst.Get(owner);

        ropeSJ = gameObject.AddComponent<SpringJoint2D>();
        ropeSJ.connectedBody = player.rigidbody2D;
        ropeSJ.distance = 2f;
        ropeSJ.frequency = 3;
        ropeSJ.dampingRatio = 2;

        player.Roped(this);
    }

    void StickToTarget(GameObject target)
    {
        isSticked = true;
        RaycastHit2D hit = Physics2D.Raycast(transform.position, target.transform.position - transform.position, 5);

        stickHJ = gameObject.AddComponent<HingeJoint2D>();
        stickHJ.connectedBody = target.rigidbody2D;
        stickHJ.anchor = transform.InverseTransformPoint(hit.point);
        stickHJ.connectedAnchor = target.transform.InverseTransformPoint(hit.point);
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
