using System;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace ActiveRagdoll
{
    public class RagDollLocoMotion:MonoBehaviour
    {
        [Header("References")] 
        [SerializeField] private Transform _cameraTransform;
        
        [Tooltip("LocoMotion Body References")]
        [SerializeField] private Rigidbody _locoMotionBody;
        [SerializeField] private float _rayLength;
        [SerializeField] private float rideHeight;
        [SerializeField] private float rideSpring = 4000f;
        [SerializeField] private float ridedamper = 200f;
        [SerializeField] private LayerMask _groundLayer;
        
        [SerializeField] private float moveSpeed=4f;
        public float acceleration = 35f;
        public float maxAccelForce = 100f;
        
        
        
        
        private bool _isGrounded;
        private float relVel;
        private Rigidbody _hitBody;
        private float v, h;
        private Vector3 fwd, right;
        private Vector3 _moveInput;
        private float _accelForce;
        private Vector3 _goalVel;
        
        void Awake()
        {
            if (_locoMotionBody != null)
            {
                _locoMotionBody.freezeRotation = true;
            }
        }

        private void Update()
        {
            h = Input.GetAxis("Horizontal");
            v = Input.GetAxis("Vertical");
            fwd = _cameraTransform ? Vector3.ProjectOnPlane(_cameraTransform.forward, Vector3.up).normalized
                : Vector3.forward;
            
            right = _cameraTransform    
                ? Vector3.ProjectOnPlane(_cameraTransform.right, Vector3.up).normalized
                : Vector3.right;
            
            _moveInput = Vector3.ClampMagnitude(fwd * v + right * h, 1f);
        }

        private void FixedUpdate()
        {
            FloatBody();
            Move();
        }

        private void FloatBody()
        {
            if(_locoMotionBody==null)
            {
                return;
            }
            if(!Physics.Raycast(_locoMotionBody.position,Vector3.down,out RaycastHit hit, _rayLength, _groundLayer,QueryTriggerInteraction.Ignore))
            {
                return;
            }
            _isGrounded=hit.distance<=rideHeight;
            if (!_isGrounded)
                return;

            relVel = Vector3.Dot(Vector3.down, _locoMotionBody.linearVelocity);
            
            _hitBody = hit.rigidbody;
            if (_hitBody) relVel -= Vector3.Dot(Vector3.down, _hitBody.linearVelocity);
            
            
            float x= hit.distance- rideHeight;

            float force = (x * rideSpring) - (relVel * ridedamper);
            _locoMotionBody.AddForce(Vector3.down * force);

            if (_hitBody) _hitBody.AddForceAtPosition(Vector3.down * -force, hit.point);

        }

        private void Move()
        {
            float _accelForce = acceleration;
            Vector3 targetVel = _moveInput * moveSpeed;
            _goalVel = Vector3.MoveTowards(_goalVel, targetVel, _accelForce * Time.fixedDeltaTime);
 
            Vector3 needed = (_goalVel - _locoMotionBody.linearVelocity) / Time.fixedDeltaTime;
            needed.y = 0f;  
            needed = Vector3.ClampMagnitude(needed, maxAccelForce);
 
            _locoMotionBody.AddForce(needed * _locoMotionBody.mass);
        }
    }
}