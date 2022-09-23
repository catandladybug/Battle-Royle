using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class PlayerControl : MonoBehaviourPun
{

    [Header("Info")]
    public int id;

    [Header("Stats")]
    public float moveSpeed;
    public float jumpForce;

    [Header("Components")]
    public Rigidbody rig;
    public Player photonPlayer;

    [PunRPC]
    public void Initialize (Player player)
    {

        id = player.ActorNumber;
        photonPlayer = player;

        GameManager.instance.players[id - 1] = this;

        if(!photonView.IsMine)
        {

            GetComponentInChildren<Camera>().gameObject.SetActive(false);
            rig.isKinematic = true;

        }

    }

    void Update()
    {

        Move();

        if (Input.GetKeyDown(KeyCode.Space)) 
            TryJump();

    }

    void Move()
    {

        // get input axis
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        // calculate a direction relative to where we're facing
        Vector3 dir = (transform.forward * z + transform.right * x) * moveSpeed;
        dir.y = rig.velocity.y;

        // set that as velocity
        rig.velocity = dir;

    }

    void TryJump()
    {

        // create ray facing down
        Ray ray = new Ray(transform.position, Vector3.down);

        // shoot raycast
        if (Physics.Raycast(ray, 1.5f))
            rig.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);

    }
}
