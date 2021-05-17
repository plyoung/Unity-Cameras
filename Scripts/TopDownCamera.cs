using UnityEngine;
using UnityEngine.InputSystem;

namespace Game
{
	public class TopDownCamera : MonoBehaviour
	{
		// ----------------------------------------------------------------------------------------------------------------
		#region inspector

		[SerializeField] private Vector3 defaultRotation = new Vector3(45f, -45f, 0f);
		
		[Space]
		[SerializeField] private float focusSmoothing = 5f;
		[SerializeField] private float zoomSmoothing = 10f;

		[Space]
		[SerializeField] private float moveSpeed = 5f;
		[SerializeField] private float moveFasterFactor = 5f;

		[Space]
		[SerializeField] private bool canRotate = true;
		[SerializeField] private float rotateSpeed = 100f;

		[Space]
		[SerializeField] private bool canTilt = true;
		[SerializeField] private float tiltSpeed = 50f;
		[SerializeField] private float minTilt = 15f;
		[SerializeField] private float maxTilt = 75f;

		[Space]
		[SerializeField] private float defaultZoom = 5f;
		[SerializeField] private float farFocusFactor = 2f;
		[SerializeField] private float zoomStep = 0.3f;
		[SerializeField] private float zoomfastStep = 1f;
		[SerializeField] private float minZoom = 3f;
		[SerializeField] private float maxZoom = 15f;

		[Space]
		[SerializeField] private InputActionReference inputMoveAxis;
		[SerializeField] private InputActionReference inputLookAxis;
		[SerializeField] private InputActionReference inputZoomAxis;
		[SerializeField] private InputActionReference inputMoveButton;
		[SerializeField] private InputActionReference inputPanningButton;
		[SerializeField] private InputActionReference inputRotateButton;
		[SerializeField] private InputActionReference inputMoveFasterButton;

		#endregion
		// ----------------------------------------------------------------------------------------------------------------
		#region priv

		private Transform tr;

		private bool rotateActive;
		private bool moveActive;
		private bool panActive;
		private bool moveFaster;

		private Transform focusTr;
		private Transform followTr;
		private bool canUnfollow;
		private AnimFloat camDistance;
		private AnimVector3 camPivot;
		private AnimQuaternion camRotator;

		#endregion
		// ----------------------------------------------------------------------------------------------------------------
		#region system

		private void Awake()
		{
			tr = GetComponent<Transform>();

			camPivot = new AnimVector3(Vector3.zero, focusSmoothing);
			camDistance = new AnimFloat(defaultZoom, zoomSmoothing);
			camRotator = new AnimQuaternion(Quaternion.Euler(defaultRotation), focusSmoothing, UpdateCameraRotation);

			Focus(null, true, true);
		}

		private void OnEnable()
		{
			SetInputEnabled(true);
		}

		private void OnDisable()
		{
			SetInputEnabled(false);
		}

		private void Update()
		{
			var dt = Time.deltaTime;
			
			var updateCam = false;
			if (camPivot.Update(dt)) updateCam = true;
			if (camDistance.Update(dt)) updateCam = true;
			if (camRotator.Update(dt)) updateCam = true;
			if (updateCam) UpdateCameraPosition();

			UpdateInput(dt);
		}

		private void LateUpdate()
		{
			if (followTr != null)
			{
				camPivot.Target = followTr.position;
			}
		}

		private void UpdateInput(float dt)
		{
			// FIXME: need to consider UI interaction
			//if (EventSystem.current.IsPointerOverGameObject()) return;

			if (panActive && canUnfollow)
			{
				StopFollow();

				var v = inputLookAxis.action.ReadValue<Vector2>() * dt * 3f;
				var pos = camPivot.Value;
				pos += (tr.right * -v.x) + (tr.up * -v.y);
				camPivot.Value = pos;

				UpdateCameraPosition();
				return; // do not rotate or move when panning
			}

			if (rotateActive)
			{
				camRotator.Stop();

				var v = inputLookAxis.action.ReadValue<Vector2>();

				if (canRotate)
				{
					tr.Rotate(0f, v.x * dt * rotateSpeed, 0f, Space.World);
				}

				if (canTilt)
				{
					var r = tr.eulerAngles.x;
					if (v.y < 0.0f && r < maxTilt)
					{
						tr.Rotate(-v.y * dt * tiltSpeed, 0f, 0f);
					}
					else if (v.y > 0.0f && r > minTilt)
					{
						tr.Rotate(-v.y * dt * tiltSpeed, 0f, 0f);
					}
				}

				UpdateCameraPosition();
			}

			if (moveActive && canUnfollow)
			{
				var v = inputMoveAxis.action.ReadValue<Vector2>();
				if (v.x != 0.0f || v.y != 0.0f)
				{
					StopFollow();

					v *= dt * moveSpeed * (moveFaster ? moveFasterFactor : 1f);

					var pos = camPivot.Value;
					var y = pos.y;
					pos += (tr.right * v.x) + (tr.forward * v.y);
					pos.y = y;
					camPivot.Value = pos;

					UpdateCameraPosition();
				}
			}
		}

		private void UpdateCameraPosition()
		{
			tr.position = camPivot.Value + tr.rotation * new Vector3(0f, 0f, -camDistance.Value);
		}

		private void UpdateCameraRotation()
		{
			tr.rotation = camRotator.Value;
		}

		#endregion
		// ----------------------------------------------------------------------------------------------------------------
		#region pub

		public void Follow(Transform t, bool canUnfollow)
		{
			if (t != null) Focus(t);

			followTr = t;
			this.canUnfollow = canUnfollow;
		}

		public void StopFollow()
		{
			camPivot.Stop();
			camDistance.Stop();
			camRotator.Stop();

			followTr = null;
			canUnfollow = true;
		}

		public void Focus(Transform t, bool instant = false, bool forceDefaultRot = false)
		{
			StopFollow();

			var targetPos = t == null ? Vector3.zero : t.position;
			var targetDis = camDistance.Target;

			if (focusTr == t) // check if should zoom closer or further if repeat focus event
			{
				var farDist = defaultZoom * farFocusFactor;
				targetDis = targetDis < farDist ? farDist : defaultZoom;
			}
			else
			{
				focusTr = t;
				targetDis = defaultZoom;
			}

			if (instant)
			{
				camPivot.Value = targetPos;
				camDistance.Value = targetDis;
				if (forceDefaultRot) tr.rotation = Quaternion.Euler(defaultRotation);
				UpdateCameraPosition();
			}
			else
			{
				camPivot.Target = targetPos;
				camDistance.Target = targetDis;
				if (forceDefaultRot) camRotator.SetTarget(tr.rotation, Quaternion.Euler(defaultRotation));
			}
		}

		#endregion
		// ----------------------------------------------------------------------------------------------------------------
		#region input events

		private void SetInputEnabled(bool enableInput)
		{
			SetInputEnabled(enableInput, inputMoveAxis, null);
			SetInputEnabled(enableInput, inputLookAxis, null);
			SetInputEnabled(enableInput, inputZoomAxis, OnZoom);
			SetInputEnabled(enableInput, inputPanningButton, OnCamPanning);
			SetInputEnabled(enableInput, inputRotateButton, OnCamRotate);
			SetInputEnabled(enableInput, inputMoveFasterButton, OnMoveFaster);

			// perma enable "move" if no button defined for it
			if (!SetInputEnabled(enableInput, inputMoveButton, OnCamMove)) moveActive = true;
		}

		private bool SetInputEnabled(bool enableInput, InputActionReference input, System.Action<InputAction.CallbackContext> callback)
		{
			if (input == null)
			{
				return false;
			}
			else
			{
				if (callback != null)
				{
					input.action.started -= callback;
					input.action.canceled -= callback;
				}

				if (enableInput)
				{
					if (callback != null)
					{
						input.action.started += callback;
						input.action.canceled += callback;
					}

					input.action.Enable();
				}
				else
				{
					input.action.Disable();
				}

				return true;
			}
		}

		private void OnZoom(InputAction.CallbackContext context)
		{
			if (context.started)
			{
				var v = context.ReadValue<float>();
				if (v > 0.0f && camDistance.Target > minZoom)
				{
					var f = camDistance.Target - (moveFaster ? zoomfastStep : zoomStep);
					if (f < minZoom) f = minZoom;
					camDistance.Target = f;
				}
				else if (v < 0.0f && camDistance.Target < maxZoom)
				{
					var f = camDistance.Target + (moveFaster ? zoomfastStep : zoomStep);
					if (f > maxZoom) f = maxZoom;
					camDistance.Target = f;
				}
			}
		}

		private void OnCamMove(InputAction.CallbackContext context)
		{
			if (context.started) moveActive = true;
			else if (context.canceled) moveActive = false;
		}

		private void OnCamPanning(InputAction.CallbackContext context)
		{
			if (context.started) panActive = true;
			else if (context.canceled) panActive = false;
		}

		private void OnCamRotate(InputAction.CallbackContext context)
		{
			if (canRotate || canTilt)
			{
				if (context.started) rotateActive = true;
				else if (context.canceled) rotateActive = false;
			}
		}

		private void OnMoveFaster(InputAction.CallbackContext context)
		{
			if (context.started) moveFaster = true;
			else if (context.canceled) moveFaster = false;
		}

		#endregion
		// ================================================================================================================
	}
}