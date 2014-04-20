using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MovementAbility : MonoBehaviour
{
	public enum Direction
	{
		NONE,
		LEFT,
		RIGHT
	}

	public AnimationCurve m_ASDMovementCurve;
	public float m_ASDMovementCurveDuration = 1f;
	public float m_ASDMovementCurveMaxSpeed = 10f;
	public float m_ASDMovementCurveCurrentTime = 0f;
	
	public AnimationCurve m_ASDJumpingCurve;
	public float m_ASDJumpingCurveDuration = 1f;
	public float m_ASDJumpingCurveCurrentTime = 0f;

	public AnimationCurve m_ASDFallingCurve;
	public float m_ASDFallingCurveDuration = 1f;
	public float m_ASDFallingCurveCurrentTime = 0f;
	public float m_gravity = -10f;

	public Direction m_direction = Direction.NONE;

	public Transform m_rayOriginCB;
	public Transform m_rayOriginLB;
	public Transform m_rayOriginRB;

	public List<Transform> m_raysLeft;
	public List<Transform> m_raysRight;

	private LayerMask m_layerMask;
//	public Vector2 m_accel;
	public Vector2 m_vel;

//	public Vector2 m_termVel = new Vector2 (10f, 30f);
//	public float m_groundMovementAccel = 10f;
//	public float m_groundMovementFriction = 10f;
//	public float m_groundMovementDrag = 0.8f;

	public bool m_isInputMoving = false;
	public bool m_isFalling = false;

	public float m_horizMovement = 0f;

	public float DEBUG;

	void Awake ()
	{
		m_layerMask = ~(1 << gameObject.layer);
	}

	void FixedUpdate ()
	{
//		ApplyGravity ();
		Move ();
	}

//	void ForceTerminalVelocity ()
//	{
//		if (Mathf.Abs (m_vel.y) > m_termVel.y) {
//			m_vel.y = Mathf.Sign (m_vel.y) * m_termVel.y;
//		}
//		if (Mathf.Abs (m_vel.x) > m_termVel.x) {
//			m_vel.x = Mathf.Sign (m_vel.x) * m_termVel.x;
//		}
//	}

	void Move ()
	{
		Vector2 deltaPos = Vector2.zero;

		if (m_ASDMovementCurveCurrentTime > m_ASDMovementCurveDuration / 2f) {
			m_ASDMovementCurveCurrentTime = m_ASDMovementCurveDuration / 2f;
		}
		deltaPos.x = Time.fixedDeltaTime * (m_horizMovement * m_ASDMovementCurve.Evaluate (m_ASDMovementCurveCurrentTime / m_ASDMovementCurveDuration) * m_ASDMovementCurveMaxSpeed);

		if (!m_isInputMoving) {
			m_ASDMovementCurveCurrentTime = 0f;
		}
		m_ASDMovementCurveCurrentTime += Time.fixedDeltaTime * Mathf.Abs (m_horizMovement);

//		m_isFalling

//		m_vel.x += m_horizMovement * m_groundMovementAccel * Time.fixedDeltaTime;
//		if (Mathf.Approximately (m_horizMovement, 0f)) {
//			m_vel.x -= Mathf.Sign (m_vel.x) * m_groundMovementFriction * Time.fixedDeltaTime;
//		}
//		m_vel.x *= m_groundMovementDrag;

//		ForceTerminalVelocity ();

		float offset = 0.001f;

		Vector2 originCB = (Vector2)m_rayOriginCB.position;
		Vector2 originLB = (Vector2)m_rayOriginLB.position;
		Vector2 originRB = (Vector2)m_rayOriginRB.position;
//		RaycastHit2D hitCB = Physics2D.Raycast (originCB, Vector2.up, 1f, m_layerMask);
		RaycastHit2D hitLB = Physics2D.Raycast (originLB, -Vector2.up, offset, m_layerMask);
		RaycastHit2D hitRB = Physics2D.Raycast (originRB, -Vector2.up, offset, m_layerMask);
		if (hitLB.collider == null && hitRB.collider == null) {
			if (!m_isFalling) {
				m_ASDFallingCurveCurrentTime = 0f;
			}
			m_isFalling = true;
		}

//		Vector2 deltaPos = m_vel * Time.fixedDeltaTime;
		if (m_isFalling) {
			if (m_ASDFallingCurveCurrentTime > m_ASDFallingCurveDuration / 2f) {
				m_ASDFallingCurveCurrentTime = m_ASDFallingCurveDuration / 2f;
			}
			deltaPos.y = (m_ASDFallingCurve.Evaluate (m_ASDFallingCurveCurrentTime / m_ASDFallingCurveDuration) * m_gravity) * Time.fixedDeltaTime;

			m_ASDFallingCurveCurrentTime += Time.fixedDeltaTime;

			// If we're falling
//		if (deltaPos.y < 0) {
//			Vector2 origin = (Vector2)m_rayOriginCB.position;
//			RaycastHit2D hit = Physics2D.Raycast (origin, Vector2.up, deltaPos.y, m_layerMask);

			hitLB = Physics2D.Raycast (originLB + Vector2.right * offset, -Vector2.up, -deltaPos.y, m_layerMask);
			hitRB = Physics2D.Raycast (originRB - Vector2.right * offset, -Vector2.up, -deltaPos.y, m_layerMask);
			if (hitLB.collider) {
				deltaPos.y = (hitLB.point - originLB).y;
				m_isFalling = false;
				m_vel.y = 0f;
			}

			if (hitRB.collider) {
				float diff = (hitRB.point - originRB).y;
				if (diff < deltaPos.y) {
					deltaPos.y = diff;
				}
				m_isFalling = false;
				m_vel.y = 0f;
			}
		}


//		Debug.Log ("DeltaposX: " + deltaPos.x);
		if (deltaPos.x < 0f) {
			foreach (Transform rayOriginTransform in m_raysLeft) {
				Vector2 rayOrigin = (Vector2)rayOriginTransform.position;
				RaycastHit2D hit = Physics2D.Raycast (rayOrigin + Vector2.up * offset, Vector2.right, deltaPos.x, m_layerMask);
				if (hit.collider) {
					deltaPos.x = Mathf.Max (deltaPos.x, (hit.point - rayOrigin).x);
				}
			}
		} else if (deltaPos.x > 0f) {
			foreach (Transform rayOriginTransform in m_raysRight) {
				Vector2 rayOrigin = (Vector2)rayOriginTransform.position;
				RaycastHit2D hit = Physics2D.Raycast (rayOrigin + Vector2.up * offset, Vector2.right, deltaPos.x, m_layerMask);
				if (hit.collider) {
					deltaPos.x = Mathf.Min (deltaPos.x, (hit.point - rayOrigin).x);
				}
			}
		}

		DEBUG = deltaPos.y;

		Vector2 newPos = SnapToPixels ((Vector2)transform.position + deltaPos);
		transform.position = newPos;
	}

	float SnapToPixels (float val)
	{
		return Mathf.Round (val * 100f) / 100f;
	}

	Vector2 SnapToPixels (Vector2 val)
	{
		return new Vector2 (SnapToPixels (val.x), SnapToPixels (val.y));
	}
	
	// XXX: When standing on ground, should not ApplyGravity
//	void ApplyGravity ()
//	{
//		m_vel += m_gravity * Time.fixedDeltaTime;
//	}

	void OnDrawGizmos ()
	{
		Gizmos.color = Color.red; 
		Gizmos.DrawLine (m_rayOriginCB.position, m_rayOriginCB.position + ((Vector3)Vector2.up) * DEBUG);
	}


	public void MoveHoriz (float magnitude)
	{
		m_isInputMoving = true;

		Direction newDir = Direction.NONE;
		if (magnitude > 0f) {
			newDir = Direction.RIGHT;
		}

		if (magnitude < 0f) {
			newDir = Direction.LEFT;
		}

		if (Mathf.Approximately (magnitude, 0f)) {
			newDir = Direction.NONE;
			m_isInputMoving = false;
		}

		if (newDir != m_direction) {
			m_direction = newDir;
//			m_ASDMovementCurveCurrentTime = 0f;
		}

		m_horizMovement = magnitude;
	}
}