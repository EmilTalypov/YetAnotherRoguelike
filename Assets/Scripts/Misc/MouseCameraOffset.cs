using Unity.Cinemachine;
using UnityEngine;

public class MouseCameraOffset : MonoBehaviour {
    [SerializeField] private float _maxOffsetRadius = 2f;
    [SerializeField] private float _smoothTime = 5f;

    private CinemachineCameraOffset _cameraOffset;
    private Vector2 _currentMouseOffset;
    private Vector2 _offsetVelocity;

    private void Start() {
        _cameraOffset = GetComponent<CinemachineCameraOffset>();
    }

    private void Update() {
        Vector2 mouseScreenPos = new(
            Input.mousePosition.x / Screen.width,
            Input.mousePosition.y / Screen.height
        );

        Vector2 mouseOffsetDirection = new(
            (mouseScreenPos.x - 0.5f) * 2f,
            (mouseScreenPos.y - 0.5f) * 2f
        );

        float deadzone = 0.1f;

        if (mouseOffsetDirection.magnitude < deadzone)
            mouseOffsetDirection = Vector2.zero;
        else
            mouseOffsetDirection = mouseOffsetDirection.normalized *
                ((mouseOffsetDirection.magnitude - deadzone) / (1 - deadzone));

        Vector2 targetOffset = mouseOffsetDirection * _maxOffsetRadius;

        _currentMouseOffset = Vector2.SmoothDamp(
            _currentMouseOffset,
            targetOffset,
            ref _offsetVelocity,
            _smoothTime
        );

        _cameraOffset.Offset = new(
            _currentMouseOffset.x,
            _currentMouseOffset.y,
            0
        );
    }
}
