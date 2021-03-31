using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
	private Animator thisAnim;
	private Rigidbody rigid;
	public float groundDistance = 0.3f;
	public float JumpForce = 500;
	public LayerMask whatIsGround;


	void Start()
	{
		thisAnim = GetComponent<Animator>();
		rigid = GetComponent<Rigidbody>();
	}


	void Update()
	{
		var h = Input.GetAxis("Horizontal");
		var v = Input.GetAxis("Vertical");

		thisAnim.SetFloat("Speed", v);
		thisAnim.SetFloat("TurningSpeed", h);
		if (Input.GetButtonDown("Jump"))
		{
			rigid.AddForce(Vector3.up * JumpForce);
			thisAnim.SetTrigger("Jump");
		}
		if (Physics.Raycast(transform.position + (Vector3.up * 0.1f), Vector3.down, groundDistance, whatIsGround))
		{
			thisAnim.SetBool("Grounded", true);
			thisAnim.applyRootMotion = true;
		}
		else
		{
			thisAnim.SetBool("Grounded", false);
		}

	}

}