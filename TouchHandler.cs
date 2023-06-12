namespace BallToWall.Input
{
    using MessagePipe;
    using System;
    using UnityEngine;
    using UnityEngine.InputSystem;
    using VContainer.Unity;

    public enum ETouchState
    {
        Canceled,
        Performed,
    }

    public readonly struct TouchMessage : IEquatable<TouchMessage>
    {
        public readonly ETouchState State;

        public TouchMessage(ETouchState state)
        {
            State = state;
        }

        public bool Equals(TouchMessage other)
        {
            return State == other.State;
        }

        public override bool Equals(object obj)
        {
            return obj is TouchMessage other && Equals(other);
        }

        public override int GetHashCode()
        {
            return (int)State;
        }
    }

    public sealed class TouchHandler : ITouchHandler, IStartable, IDisposable
    {
        private UserInput _touchAction = default;
        public Vector2 CurrentPosition => _touchAction.User.TouchPosition.ReadValue<Vector2>();

        private readonly IPublisher<TouchMessage> _touchPublisher = default;

        private TouchMessage _performedTouchMessage = new(ETouchState.Performed);
        private TouchMessage _canceledTouchMessage = new(ETouchState.Canceled);

        public TouchHandler(IPublisher<TouchMessage> touchActionPublisher)
        {
            _touchPublisher = touchActionPublisher;
        }

        public void Start()
        {
            _touchAction = new();
            _touchAction.Enable();

            var action = _touchAction.User.TouchDetection;
            action.performed += OnTouchStarted;
            action.canceled += OnTouchCanceled;
        }

        public void Dispose()
        {
            _touchAction.Disable();

            var action = _touchAction.User.TouchPosition;
            action.started -= OnTouchStarted;
            action.canceled -= OnTouchCanceled;
        }

        private void OnTouchStarted(InputAction.CallbackContext context)
        {
            _touchPublisher.Publish(_performedTouchMessage);
        }

        private void OnTouchCanceled(InputAction.CallbackContext context)
        {
            _touchPublisher.Publish(_canceledTouchMessage);
        }
    }
}