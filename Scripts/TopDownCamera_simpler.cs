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
		private Vector3 camPivot;
		private float camDistance;

		#endregion
		// ----------------------------------------------------------------------------------------------------------------
		#region system

		private void Awake()
		{
			tr = GetComponent<Transform>();
			FocusOn(null);
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
			// FIXME: need to consider UI interaction
			//if (EventSystem.current.IsPointerOverGameObject()) return;

			var dt = Time.deltaTime;

			if (panActive)
			{
				var v = inputLookAxis.action.ReadValue<Vector2>() * dt * 3f;
				camPivot += (tr.right * -v.x) + (tr.up * -v.y);

				UpdateCameraPosition();
				return; // do not rotate or move when panning
			}

			if (rotateActive)
			{
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

			if (moveActive)
			{
				var v = inputMoveAxis.action.ReadValue<Vector2>() * dt * moveSpeed * (moveFaster ? moveFasterFactor : 1f);
				var y = camPivot.y;
				camPivot += (tr.right * v.x) + (tr.forward * v.y);
				camPivot.y = y;

				UpdateCameraPosition();
			}
		}

		private void UpdateCameraPosition()
		{
			tr.position = camPivot + tr.rotation * new Vector3(0f, 0f, -camDistance);
		}

		#endregion
		// ----------------------------------------------------------------------------------------------------------------
		#region pub

		public void FocusOn(Transform t)
		{
			tr.rotation = Quaternion.Euler(defaultRotation);
			camPivot = t == null ? Vector3.zero : t.position;
			
			if (focusTr == t) // check if should zoom closer or further if repeat focus event
			{
				var farDist = defaultZoom * farFocusFactor;
				camDistance = camDistance < farDist ? farDist : defaultZoom;
			}
			else
			{
				focusTr = t;
				camDistance = defaultZoom;
			}

			UpdateCameraPosition();
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
				if (v > 0.0f && camDistance > minZoom)
				{
					camDistance -= zoomStep;
					if (camDistance < minZoom) camDistance = minZoom;
					UpdateCameraPosition();
				}
				else if (v < 0.0f && camDistance < maxZoom)
				{
					camDistance += zoomStep;
					if (camDistance > maxZoom) camDistance = maxZoom;
					UpdateCameraPosition();
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
			if (context.started) rotateActive = true;
			else if (context.canceled) rotateActive = false;
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