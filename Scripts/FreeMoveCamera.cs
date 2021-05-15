using UnityEngine;
using UnityEngine.InputSystem;

namespace Game
{
	public class FreeMoveCamera : MonoBehaviour
	{
		// ----------------------------------------------------------------------------------------------------------------
		#region inspector

		[SerializeField] private Vector3 defaultRotation = new Vector3(45f, 0f, 0f);
		[SerializeField] private float defaultDistance = 5f;
		[SerializeField] private float farFocusFactor = 2f;
		[SerializeField] private float moveSpeed = 5f;
		[SerializeField] private float moveFasterFactor = 2f;
		[SerializeField] private float rotateSpeed = 100f;
		[SerializeField] private float zoomStep = 0.2f;

		[Space]
		[SerializeField] private InputActionReference inputMoveAxis;
		[SerializeField] private InputActionReference inputLookAxis;
		[SerializeField] private InputActionReference inputZoomAxis;
		[SerializeField] private InputActionReference inputWasdMoveButton;
		[SerializeField] private InputActionReference inputPanningButton;
		[SerializeField] private InputActionReference inputRotateButton;
		[SerializeField] private InputActionReference inputMoveFasterButton;

		#endregion
		// ----------------------------------------------------------------------------------------------------------------
		#region priv

		private Camera cam;
		private Transform tr;

		private bool canRotateAroundPivot;
		private bool canWASDMove;
		private bool canPanPivot;
		private bool canMoveFaster;

		private Transform homeTr;   // the focus when "Home" key is rpessed. 0x0x0 point in editor but activer character in play mode
		private Transform focusTr;  // last focused object
		private Vector3 camPivot;	// pivot/position that camera rotates around
		private float camDistance;  // distance from focised object

		#endregion
		// ----------------------------------------------------------------------------------------------------------------
		#region system

		private void Awake()
		{
			tr = GetComponent<Transform>();
			cam = GetComponent<Camera>();
		}

		private void Start()
		{
			SetHome(null, true);
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

			// * Rotate around pivot (Alt+LeftMouse)
			if (canRotateAroundPivot)
			{
				var v = inputLookAxis.action.ReadValue<Vector2>();
				var r = v * dt * rotateSpeed;

				tr.Rotate(0f, r.x, 0f, Space.World);
				tr.Rotate(-r.y, 0f, 0f);

				UpdateCameraPosition();
			}

			// * Panning (MiddleMouse or Ctrl+LeftMouse)
			if (canPanPivot)
			{
				// NOTE: improve this so that pan speed is such that focused object stay under mouse cursor
				var v = inputLookAxis.action.ReadValue<Vector2>() * dt * 3f;
				camPivot += (tr.right * -v.x) + (tr.up * -v.y);

				UpdateCameraPosition();
			}

			// * WASD movement (RightMouse)
			if (canWASDMove)
			{
				// update rotation
				var v = inputLookAxis.action.ReadValue<Vector2>();
				if (v != Vector2.zero)
				{
					var r = v * dt * rotateSpeed;
					tr.Rotate(0f, r.x, 0f, Space.World);
					tr.Rotate(-r.y, 0f, 0f);
					camPivot = tr.position + tr.rotation * new Vector3(0f, 0f, camDistance);
				}

				// update movement
				v = inputMoveAxis.action.ReadValue<Vector2>() * dt * moveSpeed * (canMoveFaster ? moveFasterFactor : 1f);
				camPivot += (tr.right * v.x) + (tr.forward * v.y);

				UpdateCameraPosition();
			}
		}

		private void UpdateCameraPosition()
		{
			tr.position = camPivot + tr.rotation * new Vector3(0f, 0f, -camDistance);
		}

		private static Bounds CalculateBounds(Transform t)
		{
			if (t == null)
			{
				return new Bounds(Vector3.zero, Vector3.one);
			}
			else
			{
				var bounds = new Bounds(t.position, Vector3.zero);
				var rens = t.gameObject.GetComponentsInChildren<Renderer>();
				foreach (var r in rens) bounds.Encapsulate(r.bounds);
				return bounds;
			}
		}

		#endregion
		// ----------------------------------------------------------------------------------------------------------------
		#region pub

		public void SetHome(Transform t, bool focusNow = false)
		{
			homeTr = t;
			if (focusNow) FocusOn(t);
		}

		public void FocusHome()
		{
			FocusOn(homeTr);
		}

		public void FocusOn(Transform t)
		{
			// reset rotation
			tr.rotation = Quaternion.Euler(defaultRotation);

			// get bounds
			var bounds = CalculateBounds(t);
			var sz = bounds.size;
			var radius = Mathf.Max(sz.x, Mathf.Max(sz.y, sz.z));

			// calc cam pivot point
			camPivot = bounds.center;

			// calc cam distance from point
			var newDist = radius / (Mathf.Sin(cam.fieldOfView * Mathf.Deg2Rad * 0.5f));
			if (newDist < Mathf.Epsilon || float.IsInfinity(newDist)) newDist = defaultDistance;

			// check if should zoom closer or further if repeat focus event
			if (focusTr == t)
			{
				var farDist = newDist * farFocusFactor;
				camDistance = camDistance < farDist ? farDist : newDist;
			}
			else
			{
				focusTr = t;
				camDistance = newDist;
			}

			UpdateCameraPosition();
		}

		#endregion
		// ----------------------------------------------------------------------------------------------------------------
		#region input events

		private void SetInputEnabled(bool enableInput)
		{
			inputZoomAxis.action.started -= OnZoom;
			inputZoomAxis.action.canceled -= OnZoom;
			inputWasdMoveButton.action.started -= OnCamWASDMove;
			inputWasdMoveButton.action.canceled -= OnCamWASDMove;
			inputPanningButton.action.started -= OnCamPanning;
			inputPanningButton.action.canceled -= OnCamPanning;
			inputRotateButton.action.started -= OnCamRotate;
			inputRotateButton.action.canceled -= OnCamRotate;
			inputMoveFasterButton.action.started -= OnMoveFaster;
			inputMoveFasterButton.action.canceled -= OnMoveFaster;

			if (enableInput)
			{
				inputZoomAxis.action.started += OnZoom;
				inputZoomAxis.action.canceled += OnZoom;
				inputWasdMoveButton.action.started += OnCamWASDMove;
				inputWasdMoveButton.action.canceled += OnCamWASDMove;
				inputPanningButton.action.started += OnCamPanning;
				inputPanningButton.action.canceled += OnCamPanning;
				inputRotateButton.action.started += OnCamRotate;
				inputRotateButton.action.canceled += OnCamRotate;
				inputMoveFasterButton.action.started += OnMoveFaster;
				inputMoveFasterButton.action.canceled += OnMoveFaster;

				inputMoveAxis.action.Enable();
				inputLookAxis.action.Enable();
				inputZoomAxis.action.Enable();
				inputWasdMoveButton.action.Enable();
				inputPanningButton.action.Enable();
				inputRotateButton.action.Enable();
				inputMoveFasterButton.action.Enable();
			}
			else
			{
				inputMoveAxis.action.Disable();
				inputLookAxis.action.Disable();
				inputZoomAxis.action.Disable();
				inputWasdMoveButton.action.Disable();
				inputPanningButton.action.Disable();
				inputRotateButton.action.Disable();
				inputMoveFasterButton.action.Disable();
			}
		}

		void OnZoom(InputAction.CallbackContext context)
		{
			if (context.started)
			{
				var v = context.ReadValue<float>();
				if (v > 0.0f)
				{
					if (camDistance > 0.0f) camDistance -= zoomStep;
					else camPivot += zoomStep * tr.forward;

					UpdateCameraPosition();
				}
				else if (v < 0.0f)
				{
					camDistance += zoomStep;
					UpdateCameraPosition();
				}
			}
		}

		void OnCamWASDMove(InputAction.CallbackContext context)
		{
			if (context.started) canWASDMove = true;
			else if (context.canceled) canWASDMove = false;
		}

		void OnCamPanning(InputAction.CallbackContext context)
		{
			if (context.started) canPanPivot = true;
			else if (context.canceled) canPanPivot = false;
		}

		void OnCamRotate(InputAction.CallbackContext context)
		{
			if (context.started) canRotateAroundPivot = true;
			else if (context.canceled) canRotateAroundPivot = false;
		}

		void OnMoveFaster(InputAction.CallbackContext context)
		{
			if (context.started) canMoveFaster = true;
			else if (context.canceled) canMoveFaster = false;
		}

		#endregion
		// ================================================================================================================
	}
}