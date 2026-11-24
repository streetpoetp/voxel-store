using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace VoxelStore.Preview.Runtime
{
    [DisallowMultipleComponent]
    public sealed class AvatarMotionPlayer : MonoBehaviour
    {
        [SerializeField] private AvatarPresentation avatar;
        [FormerlySerializedAs("defaultMotion")]
        [SerializeField] private AvatarMotion motion =
            AvatarMotion.Idle;
        [SerializeField] private EquipmentSlotSide driverSide =
            EquipmentSlotSide.Right;
        [SerializeField] private string variant = string.Empty;
        [SerializeField] private bool loop = true;
        [SerializeField, Min(0.05f)] private float speed = 1f;

        private bool playing;

        public AvatarPresentation Avatar => avatar;
        public AvatarMotion CurrentMotion { get; private set; }
        public AvatarMotion Motion => motion;
        public EquipmentSlotSide DriverSide => driverSide;
        public string Variant =>
            AvatarMotionClipCandidates.NormalizeVariant(variant);
        public bool Loop => loop;
        public float Speed => speed;
        public bool IsPlaying =>
            Application.isPlaying &&
            playing &&
            avatar != null &&
            avatar.IsPlaying;

        public void Configure(
            AvatarPresentation presentation,
            AvatarMotion configuredMotion,
            bool shouldLoop,
            float playbackSpeed)
        {
            Configure(
                presentation,
                configuredMotion,
                shouldLoop,
                playbackSpeed,
                string.Empty);
        }

        public void Configure(
            AvatarPresentation presentation,
            AvatarMotion configuredMotion,
            bool shouldLoop,
            float playbackSpeed,
            string configuredVariant)
        {
            avatar = presentation != null
                ? presentation
                : throw new ArgumentNullException(nameof(presentation));

            SetMotionSettings(
                configuredMotion,
                shouldLoop,
                playbackSpeed,
                configuredVariant);
            CurrentMotion = configuredMotion;
            playing = false;
            ResetPresentation();
        }

        public void SetMotionSettings(
            AvatarMotion configuredMotion,
            bool shouldLoop,
            float playbackSpeed)
        {
            ValidateSpeed(playbackSpeed);
            motion = configuredMotion;
            loop = shouldLoop;
            speed = playbackSpeed;
        }

        public void SetMotionSettings(
            AvatarMotion configuredMotion,
            bool shouldLoop,
            float playbackSpeed,
            string configuredVariant)
        {
            SetMotionSettings(
                configuredMotion,
                shouldLoop,
                playbackSpeed);
            variant =
                AvatarMotionClipCandidates.NormalizeVariant(
                    configuredVariant);
        }

        public void SetVariant(string configuredVariant)
        {
            string normalized =
                AvatarMotionClipCandidates.NormalizeVariant(
                    configuredVariant);
            if (string.Equals(
                    variant,
                    normalized,
                    StringComparison.Ordinal))
            {
                return;
            }

            float phase =
                avatar != null
                    ? avatar.CurrentNormalizedPhase
                    : 0f;
            variant = normalized;

            if (playing && avatar != null)
            {
                avatar.StartMotionAtPhase(
                    CurrentMotion,
                    driverSide,
                    loop,
                    phase,
                    variant);
            }
        }

        public void SetEquipmentVariant(string configuredVariant)
        {
            if (avatar == null)
                return;

            bool changed = false;
            foreach (EquipmentPresentation item in avatar.Equipment)
                changed |= item.SetVariant(configuredVariant);

            if (!changed || !playing)
                return;

            float phase = avatar.CurrentNormalizedPhase;
            avatar.StartMotionAtPhase(
                CurrentMotion,
                driverSide,
                loop,
                phase,
                variant);
        }

        public void SetDriverSide(EquipmentSlotSide side)
        {
            if (driverSide == side)
                return;

            float phase =
                avatar != null
                    ? avatar.CurrentNormalizedPhase
                    : 0f;
            driverSide = side;

            if (playing && avatar != null)
            {
                avatar.StartMotionAtPhase(
                    CurrentMotion,
                    driverSide,
                    loop,
                    phase,
                    variant);
            }
        }

        public void PlayConfigured()
        {
            Play(motion, loop, speed);
        }

        public void Play(AvatarMotion configuredMotion)
        {
            Play(configuredMotion, loop, speed);
        }

        public void Play(
            AvatarMotion configuredMotion,
            bool shouldLoop,
            float playbackSpeed)
        {
            RequireConfigured();
            SetMotionSettings(
                configuredMotion,
                shouldLoop,
                playbackSpeed);

            CurrentMotion = configuredMotion;
            playing = true;
            avatar.StartMotion(
                CurrentMotion,
                driverSide,
                loop,
                variant);
        }

        public void Stop()
        {
            playing = false;
            ResetPresentation();
        }

        public void ResetMotion()
        {
            Stop();
        }

        public void SampleMotion(
            AvatarMotion sampledMotion,
            float normalizedTime)
        {
            SampleMotion(
                sampledMotion,
                normalizedTime,
                driverSide);
        }

        public void SampleMotion(
            AvatarMotion sampledMotion,
            float normalizedTime,
            EquipmentSlotSide side)
        {
            ValidateNormalizedTime(normalizedTime);
            RequireConfigured();

            playing = false;
            CurrentMotion = sampledMotion;
            driverSide = side;
            avatar.SampleMotion(
                sampledMotion,
                side,
                normalizedTime,
                variant);
        }

        public void SubmitNow()
        {
            RequireConfigured();
            avatar.SubmitNow();
        }

        private void Update()
        {
            if (!Application.isPlaying ||
                !playing ||
                avatar == null)
            {
                return;
            }

            avatar.Advance(Time.deltaTime * speed);
            if (!avatar.IsPlaying)
                playing = false;
        }

        private void OnEnable()
        {
            if (!Application.isPlaying ||
                avatar == null)
            {
                return;
            }

            PlayConfigured();
        }

        private void OnDisable()
        {
            playing = false;
            if (avatar != null &&
                avatar.IsProviderInitialized)
            {
                avatar.ResetPose();
            }
        }

        private void OnDestroy()
        {
            playing = false;
        }

        private void ResetPresentation()
        {
            if (avatar == null)
                return;

            avatar.ResetPose();
        }

        private void RequireConfigured()
        {
            if (avatar == null)
            {
                throw new InvalidOperationException(
                    "AvatarMotionPlayer is missing AvatarPresentation.");
            }

            if (!avatar.IsConfigured ||
                avatar.BodyAnimation == null)
            {
                throw new InvalidOperationException(
                    "AvatarMotionPlayer requires a configured provider-backed Body.");
            }
        }

        private static void ValidateSpeed(float playbackSpeed)
        {
            if (playbackSpeed <= 0f ||
                float.IsNaN(playbackSpeed) ||
                float.IsInfinity(playbackSpeed))
            {
                throw new ArgumentOutOfRangeException(
                    nameof(playbackSpeed),
                    "Playback speed must be a finite value greater than zero.");
            }
        }

        private static void ValidateNormalizedTime(float normalizedTime)
        {
            if (float.IsNaN(normalizedTime) ||
                float.IsInfinity(normalizedTime) ||
                normalizedTime < 0f ||
                normalizedTime > 1f)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(normalizedTime),
                    "Normalized sample time must be in [0,1].");
            }
        }
    }
}
