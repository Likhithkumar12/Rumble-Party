using System;
using Unity.VisualScripting;
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

        [Header("Ragdoll link")]
        [SerializeField] private Rigidbody _pelvis;
        [Tooltip("Pelvis, or the CHEST if the torso sags.")]
        [SerializeField] private Rigidbody _uprightBody;

        [SerializeField] private float pelvisHoldSpring = 500f;
        [SerializeField] private float pelvisHoldDamper = 20f;

        [SerializeField] private float uprightSpring = 1200f;
        [SerializeField] private float uprightDamper = 80f;
        [Tooltip("Degrees per second.")]
        [SerializeField] private float turnSpeed = 360f;
        [Tooltip("Set 180 if the robot walks backwards.")]
        [SerializeField] private float forwardOffset = 0f;

        private Quaternion _restRotation;
        private float _initialYaw;
        private float _currentYaw;
        
        
        
        
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
            if(_uprightBody== null)_uprightBody = _pelvis;
            _restRotation = _uprightBody.rotation;
            _initialYaw= transform.eulerAngles.y;
            _currentYaw = _initialYaw;

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
            HoldPelvis();
            StayUpRight();
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

        private void HoldPelvis()
        {
            if (_pelvis == null) return;
            
            Vector3 delta = _locoMotionBody.position - _pelvis.position;
            Vector3 force = delta * pelvisHoldSpring - _pelvis.linearVelocity * pelvisHoldDamper;
            _pelvis.AddForce(force, ForceMode.Acceleration);
        }

        private void StayUpRight()
        {
            if (_uprightBody == null) return;
            Vector3 vel = _locoMotionBody.linearVelocity;
            vel.y = 0f;
            if (vel.magnitude > 0.01f)
            {
                float desiredYaw = Mathf.Atan2(vel.x, vel.z) * Mathf.Rad2Deg + forwardOffset;
                _currentYaw = Mathf.MoveTowardsAngle(_currentYaw, desiredYaw, turnSpeed * Time.fixedDeltaTime);
            }
            float yawDelta =
                Mathf.DeltaAngle(_initialYaw, _currentYaw);
            
            Quaternion target =
                Quaternion.AngleAxis(yawDelta, Vector3.up) * _restRotation;
            
            Quaternion current = _uprightBody.rotation;

            if (Quaternion.Dot(target, current) < 0f)
            {
                current = new Quaternion(-current.x, -current.y, -current.z, -current.w);
            }
            Quaternion toGoal = target* Quaternion.Inverse(current);
            toGoal.ToAngleAxis(out float angle, out Vector3 axis);
            
            if (angle > 180f) angle -= 360f;
            axis.Normalize();
            
            Vector3 torque =
                axis * (angle * Mathf.Deg2Rad * uprightSpring)
                - _uprightBody.angularVelocity * uprightDamper;
            
            _uprightBody.AddTorque(torque);

        }
    }
}