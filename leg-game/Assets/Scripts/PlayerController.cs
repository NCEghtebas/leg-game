using UnityEngine;
using System.Collections;

[RequireComponent (typeof(MovementAbility))]
public class PlayerController : MonoBehaviour
{

	private MovementAbility m_movementAbility;

	void Awake ()
	{
		m_movementAbility = GetComponent<MovementAbility> ();
	}

	void Update ()
	{
		float horizontal = Input.GetAxis ("Horizontal");
//		float vertical = Input.GetAxis ("Vertical");
		m_movementAbility.MoveHoriz (horizontal);
	}
}
