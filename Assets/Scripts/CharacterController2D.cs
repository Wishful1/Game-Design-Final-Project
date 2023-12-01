using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEngine.SceneManagement;


public class CharacterController2D : MonoBehaviour
{
	[SerializeField] private float m_JumpForce = 1000f;							// Amount of force added when the player jumps.
	[Range(0, 1)] [SerializeField] private float m_CrouchSpeed = .36f;			// Amount of maxSpeed applied to crouching movement. 1 = 100%
	[Range(0, .3f)] [SerializeField] private float m_MovementSmoothing = .05f;	// How much to smooth out the movement
	[SerializeField] private bool m_AirControl = false;							// Whether or not a player can steer while jumping;
	[SerializeField] private LayerMask m_WhatIsGround;							// A mask determining what is ground to the character
	[SerializeField] private Transform m_GroundCheck;							// A position marking where to check if the player is grounded.
	[SerializeField] private Transform m_CeilingCheck;							// A position marking where to check for ceilings
	[SerializeField] private Collider2D m_CrouchDisableCollider;				// A collider that will be disabled when crouching

	const float k_GroundedRadius = .2f; // Radius of the overlap circle to determine if grounded
	private bool m_Grounded;            // Whether or not the player is grounded.
	const float k_CeilingRadius = .2f; // Radius of the overlap circle to determine if the player can stand up
	public float timer;
	public float deathTimer = 3f;
	private Rigidbody2D m_Rigidbody2D;
	private bool m_FacingRight = true;  // For determining which way the player is currently facing.
	private bool canTakeHealth = true;
	private bool hasEntered;
	private bool hasGotHealth = false;
	private bool foundSave = false;
	private Vector3 m_Velocity = Vector3.zero;
	public Animator animator;
	public PlayerMovement movementCtrl;
	private int numHealth = 3;
	private int numHearts = 3;
	public Image[] hearts;
	public Image keyCardBool;
	public Sprite fullHeart;
	public Sprite emptyHeart;
	public Sprite Keycard;
	public Sprite KeycardEmpty;
	public AudioSource pickupSound;
	public AudioSource hurtSound;
	public AudioSource jumpSound;
	public AudioSource doorSound;
	public AudioSource deniedSound;

	[Header("Events")]
	[Space]

	public UnityEvent OnLandEvent;

	[System.Serializable]
	public class BoolEvent : UnityEvent<bool> { }

	public BoolEvent OnCrouchEvent;
	private bool m_wasCrouching = false;

	private void Start()
	{
		var aSources = GetComponents<AudioSource>();
		pickupSound = aSources[0];
		hurtSound = aSources[1];
		jumpSound = aSources[2];
		doorSound = aSources[3];
		deniedSound = aSources[4];
		m_Rigidbody2D = GetComponent<Rigidbody2D>();

		if (OnLandEvent == null)
			OnLandEvent = new UnityEvent();

		if (OnCrouchEvent == null)
			OnCrouchEvent = new BoolEvent();
		disableMovement();
		Physics2D.IgnoreLayerCollision(8,10, false);
		Physics2D.IgnoreLayerCollision(10,13, true);
		GameObject healthOld = GameObject.Find("PersistentHealth");
		if (GameObject.Find("PersistentHealth") != null)
		{
			numHealth = healthOld.GetComponent<UI>().loadHealth();
			foundSave = true;
		}
		GameObject player = GameObject.Find("Player");
		if (healthOld.GetComponent<UI>().getPrevScene() == 2)
		{
			player.transform.position = new Vector2(5.85f, -9.35f);
			Flip();
		}
		else if (healthOld.GetComponent<UI>().getPrevScene() == 3)
		{
			player.transform.position = new Vector2(-80f, 21.5f);
		}
		Scene currentScene = SceneManager.GetActiveScene();
		if (currentScene.name == "lvl 1")
		{
			healthOld.GetComponent<UI>().setPrevScene(1);
		}
		else if (currentScene.name == "lvl 2")
		{
			healthOld.GetComponent<UI>().setPrevScene(2);
		}
		else if (currentScene.name == "lvl 3")
		{
			healthOld.GetComponent<UI>().setPrevScene(3);
			Flip();
		}
	}
	void Update()
	{
		GameObject healthOld = GameObject.Find("PersistentHealth");
		timer -= Time.deltaTime;
		if (numHealth == 0)
		{
			deathTimer -= Time.deltaTime;
			if (deathTimer <= 0)
			{
				healthOld.GetComponent<UI>().setPrevScene(1);
				Application.LoadLevel ("lvl 1");
				Destroy (healthOld.gameObject);
			}
		}
		if (timer <= 0)
		{
			GetComponent<PlayerMovement>().enabled = true;
			animator.SetBool("IsHurting", false);
			hasEntered = false;
		}
		for (int i = 0; i < hearts.Length; i++)
		{
			if (i < numHealth)
			{
				hearts[i].sprite = fullHeart;
			}
			else
			{
				hearts[i].sprite = emptyHeart;
			}
		}
		if (healthOld.GetComponent<UI>().checkKey() == true)
		{
			keyCardBool.sprite = Keycard;
		}
		if (numHealth == 3)
		{
			Physics2D.IgnoreLayerCollision(8,13, true);
		}
		else
		{
			Physics2D.IgnoreLayerCollision(8,13, false);
		}
	}
	private void FixedUpdate()
	{
		bool wasGrounded = m_Grounded;
		m_Grounded = false;

		// The player is grounded if a circlecast to the groundcheck position hits anything designated as ground
		// This can be done using layers instead but Sample Assets will not overwrite your project settings.
		Collider2D[] colliders = Physics2D.OverlapCircleAll(m_GroundCheck.position, k_GroundedRadius, m_WhatIsGround);
		for (int i = 0; i < colliders.Length; i++)
		{
			if (colliders[i].gameObject != gameObject)
			{
				m_Grounded = true;
				if (wasGrounded)
					OnLandEvent.Invoke();
			}
		}
		if (m_Grounded == false)
		{
			animator.SetBool("IsFalling", true);
			animator.SetBool("CanFlyKick", true);
		}
		else
		{
			animator.SetBool("IsFalling", false);
		}
	}


	public void Move(float move, bool crouch, bool jump)
	{
		// If crouching, check to see if the character can stand up
		if (!crouch)
		{
			// If the character has a ceiling preventing them from standing up, keep them crouching
			if (Physics2D.OverlapCircle(m_CeilingCheck.position, k_CeilingRadius, m_WhatIsGround))
			{
				crouch = true;
			}
		}

		//only control the player if grounded or airControl is turned on
		if (m_Grounded || m_AirControl)
		{

			// If crouching
			if (crouch)
			{
				if (!m_wasCrouching)
				{
					m_wasCrouching = true;
					OnCrouchEvent.Invoke(true);
				}

				// Reduce the speed by the crouchSpeed multiplier
				move *= m_CrouchSpeed;

				// Disable one of the colliders when crouching
				if (m_CrouchDisableCollider != null)
					m_CrouchDisableCollider.enabled = false;
			} else
			{
				// Enable the collider when not crouching
				if (m_CrouchDisableCollider != null)
					m_CrouchDisableCollider.enabled = true;

				if (m_wasCrouching)
				{
					m_wasCrouching = false;
					OnCrouchEvent.Invoke(false);
				}
			}

			// Move the character by finding the target velocity
			Vector3 targetVelocity = new Vector2(move * 10f, m_Rigidbody2D.velocity.y);
			// And then smoothing it out and applying it to the character
			m_Rigidbody2D.velocity = Vector3.SmoothDamp(m_Rigidbody2D.velocity, targetVelocity, ref m_Velocity, m_MovementSmoothing);

			// If the input is moving the player right and the player is facing left...
			if (move > 0 && !m_FacingRight)
			{
				// ... flip the player.
				Flip();
			}
			// Otherwise if the input is moving the player left and the player is facing right...
			else if (move < 0 && m_FacingRight)
			{
				// ... flip the player.
				Flip();
			}
		}
		// If the player should jump...
		if (m_Grounded && jump)
		{
			// Add a vertical force to the player.
			m_Grounded = false;
			jumpSound.Play();
			m_Rigidbody2D.AddForce(new Vector2(0f, m_JumpForce));
		}
	}
	
	private void Flip()
	{
		// Switch the way the player is labelled as facing.
		m_FacingRight = !m_FacingRight;

		// Multiply the player's x local scale by -1.
		Vector3 theScale = transform.localScale;
		theScale.x *= -1;
		transform.localScale = theScale;
	}
	void OnTriggerEnter2D(Collider2D trig)
	{
		GameObject healthOld = GameObject.Find("PersistentHealth");
		if (trig.gameObject.CompareTag("falldet"))
		{
			healthOld.GetComponent<UI>().setPrevScene(1);
			Application.LoadLevel ("lvl 1");
			Destroy (healthOld.gameObject);
		}
		if (trig.gameObject.CompareTag("DoorTrig"))
		{
			GameObject doorTrig = GameObject.Find("OpenDoor");
			doorTrig.GetComponent<Renderer>().enabled = false;
			doorSound.Play();
		}
		if (trig.gameObject.CompareTag("DoorTrigLock") && healthOld.GetComponent<UI>().checkKey() == true)
		{
			GameObject doorTrig = GameObject.Find("ClosedDoor");
			doorTrig.GetComponent<Renderer>().enabled = false;
			doorTrig.GetComponent<BoxCollider2D>().enabled = false;
			doorSound.Play();
		}
		if (trig.gameObject.CompareTag("DoorTrigLock") && healthOld.GetComponent<UI>().checkKey() == false)
		{
			deniedSound.Play();
		}
		if (trig.gameObject.CompareTag("LVL1"))
		{
			Application.LoadLevel ("lvl 1");
		}
		if (trig.gameObject.CompareTag("LVL2"))
		{
			Application.LoadLevel ("lvl 2");
		}
		if (trig.gameObject.CompareTag("LVL3"))
		{
			Application.LoadLevel ("lvl 3");
		}
	}
	void OnTriggerExit2D(Collider2D trig)
	{
		GameObject healthOld = GameObject.Find("PersistentHealth");
		if (trig.gameObject.CompareTag("DoorTrig"))
		{
			GameObject doorTrig = GameObject.Find("OpenDoor");
			doorTrig.GetComponent<Renderer>().enabled = true;
			doorSound.Play();
		}
		if (trig.gameObject.CompareTag("DoorTrigLock") && healthOld.GetComponent<UI>().checkKey() == true)
		{
			GameObject doorTrig = GameObject.Find("ClosedDoor");
			doorTrig.GetComponent<Renderer>().enabled = true;
			doorTrig.GetComponent<BoxCollider2D>().enabled = true;
			doorSound.Play();
		}
	}
	void OnCollisionEnter2D(Collision2D col)
	{
		if (col.gameObject.CompareTag("Enemy") && numHealth!=0)
		{
			//Application.LoadLevel (Application.loadedLevel);
			disableMovement();
			if (!hasEntered)
			{
				numHealth -= 1;
				GameObject healthOld = GameObject.Find("PersistentHealth");
				if (foundSave)
				{
					healthOld.GetComponent<UI>().saveHealth(numHealth);
				}
				hurtSound.Play();
			}
			hasEntered = true;
			m_Rigidbody2D.velocity = new Vector2(0, 0);
			if (m_Grounded == true)
			{
				if (m_FacingRight == true)
				{
					m_Rigidbody2D.velocity = new Vector2(0, 0);
					m_Rigidbody2D.AddForce(new Vector2(-250, 250));
				}
				if (m_FacingRight == false)
				{
					m_Rigidbody2D.velocity = new Vector2(0, 0);
					m_Rigidbody2D.AddForce(new Vector2(250, 250));
				}
			}
			else
			{
				if (m_FacingRight == true)
				{
					m_Rigidbody2D.velocity = new Vector2(0, 0);
					m_Rigidbody2D.AddForce(new Vector2(-500, 500));
				}
				if (m_FacingRight == false)
				{
					m_Rigidbody2D.velocity = new Vector2(0, 0);
					m_Rigidbody2D.AddForce(new Vector2(500, 500));
				}
			}
			animator.SetBool("IsJumping", true);
			animator.SetBool("IsHurting", true);
			animator.SetTrigger("Hurt");
		}
		if (col.gameObject.CompareTag("Portal"))
		{
			Application.LoadLevel ("WinScreen");
			GameObject healthOld = GameObject.Find("PersistentHealth");
			healthOld.GetComponent<UI>().setPrevScene(1);
			Destroy (healthOld.gameObject);
		}
		if (col.gameObject.CompareTag("HealthPickup") && !hasGotHealth)
		{
			hasGotHealth = true;
			if (numHealth == 1)
			{
				numHealth =2;
				GameObject healthOld = GameObject.Find("PersistentHealth");
				if (foundSave)
				{
					healthOld.GetComponent<UI>().saveHealth(2);
				}
				GameObject healthPick = GameObject.Find("HealthPickup");
				Destroy (healthPick.gameObject);
			}
			else if (numHealth == 2)
			{
				numHealth =3;
				GameObject healthOld = GameObject.Find("PersistentHealth");
				if (foundSave)
				{
					healthOld.GetComponent<UI>().saveHealth(3);
				}
				GameObject healthPick = GameObject.Find("HealthPickup");
				Destroy (healthPick.gameObject);
			}
			pickupSound.Play();
		}
		if (col.gameObject.CompareTag("FirePickup"))
		{
			movementCtrl.activateSuper();
			pickupSound.Play();
		}
		if (col.gameObject.CompareTag("Key"))
		{
			GameObject key = GameObject.Find("Keycard");
			Destroy (key.gameObject);
			GameObject healthOld = GameObject.Find("PersistentHealth");
			healthOld.GetComponent<UI>().setKey();
			pickupSound.Play();
		}
		if (numHealth == 0)
		{
			disableMovementForever();
			animator.SetTrigger("Death");
			GameObject.FindWithTag("Super").GetComponent<ParticleSystem>().enableEmission = false;
		}
	}
	void disableMovement()
	{
		GetComponent<PlayerMovement>().enabled = false;
		timer = 0.75f;
	}	
	void disableMovementForever()
	{
		m_Rigidbody2D.velocity = new Vector2(0, 0);
		timer = float.MaxValue;
		GetComponent<PlayerMovement>().enabled = false;
		GetComponent<Rigidbody2D>();
		Physics2D.IgnoreLayerCollision(8,10, true);
	}	
}
