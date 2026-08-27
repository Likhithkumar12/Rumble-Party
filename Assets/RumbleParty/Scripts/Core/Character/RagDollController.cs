using System;
using System.Collections.Generic;
using UnityEngine;

namespace RumbleParty.Character
{
    public enum BoneGroup { Core, Head, Arm, Leg, Extremity }

    public class RagDollController : MonoBehaviour
    {
        [Serializable]
        public class Bone
        {
            public string name;
            public Transform animated;          // ghost rig bone
            public ConfigurableJoint joint;     // physical rig joint
            public BoneGroup group = BoneGroup.Core;
            [Range(0f, 3f)] public float strengthMultiplier = 1f;

            [NonSerialized] public Quaternion startLocalRotation;
            [NonSerialized] public Rigidbody body;
            [NonSerialized] public float runtimeMultiplier = 1f; // for per-limb limpness
        }

        [Header("Bones")]
        [SerializeField] Bone[] bones = Array.Empty<Bone>();

        [Header("Global Gains")]
        [Tooltip("Master muscle tone. 0 = fully limp ragdoll, 1 = fully driven.")]
        [Range(0f, 1f)] public float muscleStrength = 1f;
        [SerializeField] float spring = 800f;
        [SerializeField] float damper = 60f;
        [SerializeField] float maxForce = Mathf.Infinity;

        [Header("Per-group multipliers")]
        [SerializeField] float coreMultiplier = 1.5f;
        [SerializeField] float headMultiplier = 0.6f;
        [SerializeField] float armMultiplier = 0.5f;
        [SerializeField] float legMultiplier = 1.2f;
        [SerializeField] float extremityMultiplier = 0.3f;

        [Header("Response")]
        [Tooltip("Seconds for muscleStrength to reach a target set via SetMuscleStrength.")]
        [SerializeField] float strengthLerpTime = 0.35f;

        private float _targetStrength = 1f;
        private float _strengthVelocity;
        private readonly Dictionary<BoneGroup, float> _groupCache = new Dictionary<BoneGroup, float>();

        public bool IsLimp => muscleStrength < 0.15f;

        void Awake()
        {
            _targetStrength = muscleStrength;

            foreach (var b in bones)
            {
                if (b.joint == null) continue;
                b.startLocalRotation = b.joint.transform.localRotation;
                b.body = b.joint.GetComponent<Rigidbody>();
                b.joint.rotationDriveMode = RotationDriveMode.Slerp;
                b.runtimeMultiplier = 1f;
            }
        }

        void FixedUpdate()
        {
            // Smoothly approach the requested muscle tone rather than snapping.
            muscleStrength = Mathf.SmoothDamp(muscleStrength, _targetStrength,
                                              ref _strengthVelocity, strengthLerpTime);

            RefreshGroupCache();

            for (int i = 0; i < bones.Length; i++)
            {
                var b = bones[i];
                if (b.joint == null || b.animated == null) continue;

                float k = muscleStrength
                        * b.strengthMultiplier
                        * b.runtimeMultiplier
                        * _groupCache[b.group];

                b.joint.SetSlerpDrive(spring * k, damper * k, maxForce);
                b.joint.SetTargetRotationLocal(b.animated.localRotation, b.startLocalRotation);
            }
        }

        void RefreshGroupCache()
        {
            _groupCache[BoneGroup.Core]      = coreMultiplier;
            _groupCache[BoneGroup.Head]      = headMultiplier;
            _groupCache[BoneGroup.Arm]       = armMultiplier;
            _groupCache[BoneGroup.Leg]       = legMultiplier;
            _groupCache[BoneGroup.Extremity] = extremityMultiplier;
        }
        

        /// <summary>Request a new global muscle tone. 0 = collapse, 1 = full strength.</summary>
        public void SetMuscleStrength(float value, bool instant = false)
        {
            _targetStrength = Mathf.Clamp01(value);
            if (instant)
            {
                muscleStrength = _targetStrength;
                _strengthVelocity = 0f;
            }
        }

        /// <summary>Stiffen or loosen a single limb, e.g. tense the arm that is holding something.</summary>
        public void SetGroupMultiplier(BoneGroup group, float multiplier)
        {
            foreach (var b in bones)
                if (b.group == group) b.runtimeMultiplier = multiplier;
        }

        /// <summary>Stiffen/loosen bones whose name contains a substring, e.g. "LeftArm".</summary>
        public void SetLimbMultiplierByName(string contains, float multiplier)
        {
            foreach (var b in bones)
                if (b.name != null && b.name.Contains(contains))
                    b.runtimeMultiplier = multiplier;
        }

        public void ResetLimbMultipliers()
        {
            foreach (var b in bones) b.runtimeMultiplier = 1f;
        }
    }
}