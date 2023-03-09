using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

namespace orbital_camera_package.Runtime.Camera
{
    public class CameraController : MonoBehaviour, Controls.ICameraActions
    {
        [Header("Movement")] 
        public UnityEngine.Camera cam;
        public float normalSpeed;
        public float fastSpeed;
        private float _movementSpeed;
        public float movementTime;
        private Vector2 _moveDelta;
        private Vector3 _newPosition;
        private Vector3 _moveDragStartPosition = Vector3.zero;
        private Vector3 _moveDragCurrentPosition = Vector3.zero;

        [Header("Rotation")] 
        public float rotationAmount;
        private float _rotateDelta;
        private Quaternion _newRotation;
        private Vector3 _rotateDragStartPosition = Vector3.zero;
        private Vector3 _rotateDragCurrentPosition = Vector3.zero;

        [Header("Zoom")] 
        public Transform cameraTransform;
        public Vector3 zoomAmount;
        private Vector3 _newZoom;
        
        #region CONTROLS
        
        private Controls _controls;

        private void OnEnable()
        {
            if (_controls == null)
            {
                _controls = new Controls();
                _controls.Camera.SetCallbacks(this);
            }
            _controls.Camera.Enable();
        }

        private void OnDisable()
        {
            _controls.Disable();
        }

        #endregion

        private void Start()
        {
            var transform1 = transform;
            _newPosition = transform1.position;
            _newRotation = transform1.rotation;
            _newZoom = cameraTransform.localPosition;

            _movementSpeed = normalSpeed;
        }

        private void LateUpdate()
        {
            HandleMovement();
        }

        private void HandleMovement()
        {
            // -- Movement
            var transform1 = transform;
            _newPosition += (transform1.right * (_movementSpeed * _moveDelta.x));
            _newPosition += (transform1.forward * (_movementSpeed * _moveDelta.y));
            transform.position = Vector3.Lerp(transform1.position, _newPosition, Time.deltaTime * movementTime);
            
            // -- Rotation
            _newRotation *= Quaternion.Euler(Vector3.up * (rotationAmount * _rotateDelta));
            transform.rotation = Quaternion.Lerp(transform1.rotation, _newRotation, Time.deltaTime * movementTime);
            
            // -- Zoom
            cameraTransform.localPosition =
                Vector3.Lerp(cameraTransform.localPosition, _newZoom, Time.deltaTime * movementTime);
        }

        public void OnMove(InputAction.CallbackContext context)
        {
            _moveDelta = context.ReadValue<Vector2>();
        }

        public void OnSpeedUp(InputAction.CallbackContext context)
        {
            _movementSpeed = context.canceled ? normalSpeed : fastSpeed;
        }

        public void OnRotate(InputAction.CallbackContext context)
        {
            _rotateDelta = context.ReadValue<float>();
        }

        public void OnZoom(InputAction.CallbackContext context)
        {
            _newZoom += zoomAmount * context.ReadValue<float>();
        }

        public void OnDragAndMove(InputAction.CallbackContext context)
        {
            const string buttonControlPath = "/Mouse/leftButton";

            if (context.started)
            {
                if (context.control.path != buttonControlPath) return;
                
                var dragStartRay = cam.ScreenPointToRay(Mouse.current.position.ReadValue());
                var dragStartPlane = new Plane(Vector3.up, Vector3.zero);

                if (dragStartPlane.Raycast(dragStartRay, out var dragStartEntry))
                {
                    _moveDragStartPosition = dragStartRay.GetPoint(dragStartEntry);
                }
            }
            else if (context.performed)
            {
                if (context.control.path != buttonControlPath) return;
                
                var dragCurrentRay = cam.ScreenPointToRay(Mouse.current.position.ReadValue());
                var dragCurrentPlane = new Plane(Vector3.up, Vector3.zero);

                if (!dragCurrentPlane.Raycast(dragCurrentRay, out var dragCurrentEntry)) return;
                
                _moveDragCurrentPosition = dragCurrentRay.GetPoint(dragCurrentEntry);
                _newPosition = transform.position + _moveDragStartPosition - _moveDragCurrentPosition;
            }
        }

        public void OnDragAndRotate(InputAction.CallbackContext context)
        {
            const string buttonControlPath = "/Mouse/rightButton";

            if (context.started)
            {
                if (context.control.path != buttonControlPath) return;

                var dragStartRay = cam.ScreenPointToRay(Mouse.current.position.ReadValue());
                var dragStartPlane = new Plane(Vector3.up, Vector3.zero);

                if (dragStartPlane.Raycast(dragStartRay, out var dragStartEntry))
                {
                    _rotateDragStartPosition = dragStartRay.GetPoint(dragStartEntry);
                }
            }
            else if (context.performed)
            {
                if (context.control.path != buttonControlPath) return;

                var dragCurrentRay = cam.ScreenPointToRay(Mouse.current.position.ReadValue());
                var dragCurrentPlane = new Plane(Vector3.up, Vector3.zero);
                
                if (!dragCurrentPlane.Raycast(dragCurrentRay, out var dragCurrentEntry)) return;

                _rotateDragCurrentPosition = dragCurrentRay.GetPoint(dragCurrentEntry);
                Vector3 difference = _rotateDragStartPosition - _rotateDragCurrentPosition;
                _rotateDragStartPosition = _rotateDragCurrentPosition;
                _newRotation *= Quaternion.Euler(Vector3.up * -difference.x * 5f);
            }
        }
    }
}
