namespace BallToWall.PlayableObject
{
    using UnityEngine;
    using Input;
    using Cysharp.Threading.Tasks;
    using Utils;
    using VContainer;
    using MessagePipe;

    [RequireComponent(typeof(Rigidbody2D))]
    public sealed class PlayerBallMovement : MonoBehaviour
    {
        #region Variables

        private Rigidbody2D _rigidbody = default;

        [SerializeField] private Vector2 _topOffset;
        [SerializeField] private LayerMask _draggableLayer;
        [SerializeField] private PlayerBallData _playerBallData;

        private Transform _transform = default;
        private ITouchHandler _touchHandler = default;
        private Camera _mainCamera = default;

        private Vector2 _dropVector = default;
        private bool _isMoving;
        
        private Vector2 TouchWorldPosition => _mainCamera.ScreenToWorldPoint(_touchHandler.CurrentPosition);

        private bool CanDrag => Utils.RaycastCheck(TouchWorldPosition, _draggableLayer);

        #endregion

        [Inject]
        private void Construct(ITouchHandler touchHandler, ISubscriber<TouchMessage> touchActiobSubscriber)
        {
            _touchHandler = touchHandler;

            touchActiobSubscriber.Subscribe(message =>
            {
                switch(message.State)
                {
                    case ETouchState.Performed:
                        StartMoving();
                        break;
                    case ETouchState.Canceled:
                        StopMoving();
                        break;
                }
            });
        }

        private void Awake()
        {
            Init();
        }

        private void Init()
        {
            _rigidbody = GetComponent<Rigidbody2D>();
            _mainCamera = Camera.main;
            _transform = transform;

            _dropVector = new Vector2(0f, _playerBallData.DropForce);
        }

        private void StartMoving()
        {
            if (!CanDrag) return;

             _isMoving = true; 
            _rigidbody.simulated = false;
            
            Move().Forget();
        }

        private void StopMoving()
        {
            if (!_isMoving) return;

            _isMoving = false;
            _rigidbody.simulated = true;
            
            _rigidbody.velocity = _dropVector;
        }

        private async UniTaskVoid Move()
        {
            while (_isMoving)
            {
                if (CanDrag)
                {
                    _transform.position = Vector2.Lerp(_transform.position,
                        TouchWorldPosition + _topOffset,
                        _playerBallData.MovementSpeed * Time.deltaTime);
                }
                await UniTask.Yield();
            }
        }
    }
}