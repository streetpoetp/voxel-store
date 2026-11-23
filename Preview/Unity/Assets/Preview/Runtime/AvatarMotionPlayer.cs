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
        [SerializeField] private bool loop = true;
        [SerializeField, Min(0.05f)] private float speed = 1f;

        private float elapsedSeconds;
        private bool playing;

        public AvatarPresentation Avatar => avatar;
        public AvatarMotion CurrentMotion { get; private set; }
        public AvatarMotion Motion => motion;
        public EquipmentSlotSide DriverSide => driverSide;
        public bool Loop => loop;
        public float Speed => speed;
        public bool IsPlaying =>
            Application.isPlaying &&
            playing;

        public void Configure(
            AvatarPresentation presentation,
            AvatarMotion configuredMotion,
            bool shouldLoop,
            float playbackSpeed)
        {
            avatar = presentation != null
                ? presentation
                : throw new ArgumentNullException(nameof(presentation));

            SetMotionSettings(
                configuredMotion,
                shouldLoop,
                playbackSpeed);
            CurrentMotion = configuredMotion;
            elapsedSeconds = 0f;
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

        public void SetDriverSide(
            EquipmentSlotSide side)
        {
            driverSide = side;
            if (avatar == null)
                return;

            if (playing)
            {
                avatar.SampleMotion(
                    CurrentMotion,
                    driverSide,
                    CurrentNormalizedTime());
            }
        }

        public void PlayConfigured()
        {
            Play(
                motion,
                loop,
                speed);
        }

        public void Play(AvatarMotion configuredMotion)
        {
            Play(
                configuredMotion,
                loop,
                speed);
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
            elapsedSeconds = 0f;
            playing = true;
            avatar.SampleMotion(
                CurrentMotion,
                driverSide,
                0f);
        }

        public void Stop()
        {
            playing = false;
            elapsedSeconds = 0f;
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
            elapsedSeconds = 0f;
            CurrentMotion = sampledMotion;
            driverSide = side;
            avatar.SampleMotion(
                sampledMotion,
                side,
                normalizedTime);
        }

        private void Update()
        {
            if (!Application.isPlaying ||
                !playing ||
                avatar == null)
            {
                return;
            }

            float duration =
                avatar.BodyAnimation.GetClipDuration(
                    CurrentMotion,
                    driverSide);
            elapsedSeconds +=
                Time.deltaTime *
                speed;

            float normalizedTime;
            if (loop)
            {
                normalizedTime =
                    Mathf.Repeat(
                        elapsedSeconds / duration,
                        1f);
            }
            else
            {
                normalizedTime =
                    Mathf.Clamp01(
                        elapsedSeconds / duration);
                if (elapsedSeconds >= duration)
                    playing = false;
            }

            avatar.SampleMotion(
                CurrentMotion,
                driverSide,
                normalizedTime);
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
            elapsedSeconds = 0f;
            ResetPresentation();
        }

        private void OnDestroy()
        {
            playing = false;
        }

        private float CurrentNormalizedTime()
        {
            float duration =
                avatar.BodyAnimation.GetClipDuration(
                    CurrentMotion,
                    driverSide);
            if (duration <= 0f)
                return 0f;

            float value =
                elapsedSeconds / duration;
            return loop
                ? Mathf.Repeat(value, 1f)
                : Mathf.Clamp01(value);
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

            if (avatar.BodyAnimation == null)
            {
                throw new InvalidOperationException(
                    "AvatarMotionPlayer requires authored Body animation.");
            }
        }

        private static void ValidateSpeed(
            float playbackSpeed)
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

        private static void ValidateNormalizedTime(
            float normalizedTime)
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
