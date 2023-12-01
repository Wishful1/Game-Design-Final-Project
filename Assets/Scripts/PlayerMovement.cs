using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour 
{    
	public CharacterController2D controller;
	public Animator animator;
	public LayerMask enemyLayers;

	public float runSpeed = 40f;

	public float horizontalMove = 0f;
	public float superTimer;
	bool jump = false;
	bool crouch = false;
	bool kick = true;
	bool iskicking = false;
	bool canMove = true;
	float attackCooldown = 1f;
	float attackLength = 0.25f;
	float attackTimer = 1f;
	public Transform AttackPoint;
	public float attackRange = 1.25f;
	public AudioSource swingSound;
	void Start()
	{
		var aSources = GetComponents<AudioSource>();
		swingSound = aSources[5];
	}
	// Update is called once per frame
	void Update() 
	{
		attackTimer += Time.deltaTime;
		superTimer -= Time.deltaTime;
		horizontalMove =  Input.GetAxisRaw("Horizontal") * runSpeed;
		animator.SetFloat("Speed", Mathf.Abs(horizontalMove));

		if (Input.GetButtonDown("Jump"))
		{
			jump = true;
			animator.SetBool("IsJumping", true);
			animator.SetBool("CanFlyKick", true);
		}
		 
		if (Input.GetKeyDown("c"))
		{
			crouch = true;
			animator.SetBool("IsCrouching", true);
		} 
		else if (Input.GetKeyUp("c"))
		{
			crouch = false;
			animator.SetBool("IsCrouching", false);
		}
		if (Input.GetKeyDown(KeyCode.LeftShift) && attackTimer > attackCooldown)
		{
			kick = true;
			animator.SetTrigger("Kick");
			animator.SetBool("IsJumping", false);
			animator.SetBool("IsKicking", true);
			iskicking = true;
			attackTimer = 0;
			swingSound.Play();
		}
		if (iskicking == true)
		{
			attackFunction();
		}
		if (attackTimer > attackLength)
		{
			iskicking = false;
		}
		if (superTimer <= 0)
		{
			animator.SetBool("IsSuper", false);
			//animator.SetTrigger("Switch");
			GameObject.FindWithTag("Super").GetComponent<ParticleSystem>().enableEmission = false;
			runSpeed = 40f;
			attackCooldown = 1f;
			attackRange = 1.25f;
		}
	}
	public void activateSuper()
	{
		animator.SetBool("IsSuper", true);
		animator.SetTrigger("Switch");
		GameObject.FindWithTag("Super").GetComponent<ParticleSystem>().enableEmission = true;
		GameObject firePick = GameObject.Find("FirePickup");
		Destroy (firePick.gameObject);
		superTimer = 4f;
		runSpeed = 60f;
		attackCooldown = 0.8f;
		attackRange = 1.5f;
	}
	void attackFunction()
	{
		Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(AttackPoint.position, attackRange, enemyLayers);
		foreach(Collider2D enemy in hitEnemies)
		{
			enemy.GetComponent<AiPatrol>().killSelf();
		}
	}
	void OnDrawGizmosSelected()
	{
		if (AttackPoint == null)
		{
			return;
			Gizmos.DrawWireSphere(AttackPoint.position, attackRange);
		}
	}
	  
	public void onLanding()
	{
		animator.SetBool("CanFlyKick", false);
		animator.SetBool("IsJumping", false);	
	}
	void FixedUpdate ()
	{
		// Move our character
		controller.Move(horizontalMove * Time.fixedDeltaTime, crouch, jump);
		jump = false;
		kick = false;
	}
}