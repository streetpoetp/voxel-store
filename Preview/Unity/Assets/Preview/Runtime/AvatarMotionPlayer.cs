using System;
using DG.Tweening;
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
        [SerializeField] private bool loop = true;
        [SerializeField, Min(0.05f)] private float speed = 1f;

        private Sequence sequence;
        private Vector3 neutralPosition;
        private Quaternion neutralRotation;
        private Vector3 neutralScale;
        private bool neutralCaptured;

        public AvatarPresentation Avatar => avatar;
        public AvatarMotion CurrentMotion { get; private set; }
        public AvatarMotion Motion => motion;
        public bool Loop => loop;
        public float Speed => speed;
        public bool IsPlaying =>
            Application.isPlaying &&
            sequence != null &&
            sequence.IsActive() &&
            sequence.IsPlaying();

        public void Configure(
            AvatarPresentation presentation,
            AvatarMotion motion,
            bool shouldLoop,
            float playbackSpeed)
        {
            avatar = presentation != null
                ? presentation
                : throw new ArgumentNullException(nameof(presentation));

            SetMotionSettings(
                motion,
                shouldLoop,
                playbackSpeed);
            CaptureNeutral();
            ResetPresentation();
        }

        public void SetMotionSettings(
            AvatarMotion motion,
            bool shouldLoop,
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

            this.motion = motion;
            loop = shouldLoop;
            speed = playbackSpeed;
        }

        public void PlayConfigured()
        {
            Play(
                motion,
                loop,
                speed);
        }

        public void Play(AvatarMotion motion)
        {
            Play(
                motion,
                loop,
                speed);
        }

        public void Play(
            AvatarMotion motion,
            bool shouldLoop,
            float playbackSpeed)
        {
            RequireConfigured();
            SetMotionSettings(
                motion,
                shouldLoop,
                playbackSpeed);

            KillSequence();
            ResetPresentation();

            CurrentMotion = motion;
            sequence = BuildSequence(motion);
            sequence
                .SetTarget(this)
                .SetUpdate(false);

            sequence.timeScale = playbackSpeed;
            if (shouldLoop)
            {
                sequence.SetLoops(
                    -1,
                    LoopType.Restart);
            }

            sequence.Play();
        }

        public void Stop()
        {
            KillSequence();
            ResetPresentation();
        }

        public void ResetMotion()
        {
            KillSequence();
            ResetPresentation();
        }

        public void SampleMotion(
            AvatarMotion motion,
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

            RequireConfigured();
            KillSequence();
            ResetPresentation();

            CurrentMotion = motion;
            sequence = BuildSequence(motion)
                .SetTarget(this)
                .SetUpdate(false);
            sequence.Pause();

            float duration = sequence.Duration(false);
            sequence.Goto(
                duration * normalizedTime,
                false);
        }

        private void Awake()
        {
            if (avatar != null)
                CaptureNeutral();
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
            KillSequence();
            ResetPresentation();
        }

        private void OnDestroy()
        {
            KillSequence();
        }

        private Sequence BuildSequence(
            AvatarMotion motion)
        {
            switch (motion)
            {
                case AvatarMotion.Idle:
                    return BuildIdle();
                case AvatarMotion.WalkForward:
                    return BuildWalkForward();
                case AvatarMotion.Attack:
                    return BuildAttack();
                case AvatarMotion.Hit:
                    return BuildHit();
                case AvatarMotion.Death:
                    return BuildDeath();
                default:
                    throw new ArgumentOutOfRangeException(
                        nameof(motion),
                        motion,
                        "Unsupported avatar motion.");
            }
        }

        private Sequence BuildIdle()
        {
            Sequence value = DOTween.Sequence();
            Vector3 up = avatar.LocalUp;

            value.Append(
                avatar.PresentationRoot
                    .DOLocalMove(
                        neutralPosition + up * 0.018f,
                        0.55f)
                    .SetEase(Ease.InOutSine));
            value.Join(
                avatar.PresentationRoot
                    .DOScale(
                        new Vector3(
                            neutralScale.x * 0.995f,
                            neutralScale.y * 1.008f,
                            neutralScale.z * 0.995f),
                        0.55f)
                    .SetEase(Ease.InOutSine));

            JoinEquipmentIdle(
                value,
                0.55f);

            value.Append(
                avatar.PresentationRoot
                    .DOLocalMove(
                        neutralPosition,
                        0.55f)
                    .SetEase(Ease.InOutSine));
            value.Join(
                avatar.PresentationRoot
                    .DOScale(
                        neutralScale,
                        0.55f)
                    .SetEase(Ease.InOutSine));

            JoinEquipmentRecovery(
                value,
                0.55f);

            return value;
        }

        private Sequence BuildWalkForward()
        {
            Sequence value = DOTween.Sequence();
            Vector3 up = avatar.LocalUp;
            Vector3 forward = avatar.LocalForward;

            value.Append(
                avatar.PresentationRoot
                    .DOLocalMove(
                        neutralPosition +
                        up * 0.20f +
                        forward * 0.10f,
                        0.14f)
                    .SetEase(Ease.OutQuad));
            value.Join(
                avatar.PresentationRoot
                    .DOLocalRotate(
                        EulerOffset(
                            neutralRotation,
                            -7f,
                            0f,
                            0f),
                        0.14f)
                    .SetEase(Ease.OutQuad));
            value.Join(
                avatar.PresentationRoot
                    .DOScale(
                        new Vector3(
                            neutralScale.x * 0.96f,
                            neutralScale.y * 1.06f,
                            neutralScale.z * 0.98f),
                        0.14f)
                    .SetEase(Ease.OutQuad));
            JoinEquipmentWalkRise(
                value,
                0.14f);

            value.Append(
                avatar.PresentationRoot
                    .DOLocalMove(
                        neutralPosition +
                        up * 0.27f +
                        forward * 0.16f,
                        0.08f)
                    .SetEase(Ease.OutSine));
            value.Join(
                avatar.PresentationRoot
                    .DOLocalRotate(
                        EulerOffset(
                            neutralRotation,
                            -11f,
                            0f,
                            0f),
                        0.08f)
                    .SetEase(Ease.OutSine));
            JoinEquipmentWalkApex(
                value,
                0.08f);

            value.Append(
                avatar.PresentationRoot
                    .DOLocalMove(
                        neutralPosition +
                        forward * 0.22f,
                        0.12f)
                    .SetEase(Ease.InQuad));
            value.Join(
                avatar.PresentationRoot
                    .DOLocalRotate(
                        EulerOffset(
                            neutralRotation,
                            5f,
                            0f,
                            0f),
                        0.12f)
                    .SetEase(Ease.InQuad));
            value.Join(
                avatar.PresentationRoot
                    .DOScale(
                        new Vector3(
                            neutralScale.x * 1.05f,
                            neutralScale.y * 0.94f,
                            neutralScale.z * 1.03f),
                        0.12f)
                    .SetEase(Ease.InQuad));
            JoinEquipmentWalkLand(
                value,
                0.12f);

            value.AppendInterval(0.045f);
            AppendRecovery(
                value,
                0.16f);

            return value;
        }

        private Sequence BuildAttack()
        {
            Sequence value = DOTween.Sequence();
            Vector3 forward = avatar.LocalForward;

            value.Append(
                avatar.PresentationRoot
                    .DOLocalMove(
                        neutralPosition -
                        forward * 0.08f,
                        0.16f)
                    .SetEase(Ease.OutQuad));
            value.Join(
                avatar.PresentationRoot
                    .DOLocalRotateQuaternion(
                        neutralRotation,
                        0.16f)
                    .SetEase(Ease.OutQuad));
            JoinEquipmentAttackWindup(
                value,
                0.16f);

            value.Append(
                avatar.PresentationRoot
                    .DOLocalMove(
                        neutralPosition +
                        forward * 0.28f,
                        0.09f)
                    .SetEase(Ease.InQuad));
            value.Join(
                avatar.PresentationRoot
                    .DOLocalRotateQuaternion(
                        neutralRotation,
                        0.09f)
                    .SetEase(Ease.InQuad));
            value.Join(
                avatar.PresentationRoot
                    .DOScale(
                        new Vector3(
                            neutralScale.x * 0.98f,
                            neutralScale.y * 0.98f,
                            neutralScale.z * 1.07f),
                        0.09f)
                    .SetEase(Ease.InQuad));
            JoinEquipmentAttackStrike(
                value,
                0.09f);

            value.AppendInterval(0.04f);

            value.Append(
                avatar.PresentationRoot
                    .DOLocalMove(
                        neutralPosition +
                        forward * 0.20f,
                        0.08f)
                    .SetEase(Ease.OutQuad));
            value.Join(
                avatar.PresentationRoot
                    .DOLocalRotateQuaternion(
                        neutralRotation,
                        0.08f)
                    .SetEase(Ease.OutQuad));
            JoinEquipmentAttackFollowThrough(
                value,
                0.08f);

            AppendRecovery(
                value,
                0.20f);

            return value;
        }

        private Sequence BuildHit()
        {
            Sequence value = DOTween.Sequence();
            Vector3 forward = avatar.LocalForward;
            Vector3 up = avatar.LocalUp;

            value.Append(
                avatar.PresentationRoot
                    .DOLocalMove(
                        neutralPosition -
                        forward * 0.18f +
                        up * 0.035f,
                        0.09f)
                    .SetEase(Ease.OutQuad));
            value.Join(
                avatar.PresentationRoot
                    .DOLocalRotate(
                        EulerOffset(
                            neutralRotation,
                            13f,
                            0f,
                            0f),
                        0.09f)
                    .SetEase(Ease.OutQuad));
            value.Join(
                avatar.PresentationRoot
                    .DOScale(
                        new Vector3(
                            neutralScale.x * 1.04f,
                            neutralScale.y * 0.94f,
                            neutralScale.z * 1.03f),
                        0.09f)
                    .SetEase(Ease.OutQuad));

            JoinEquipmentHit(
                value,
                0.09f);

            value.AppendInterval(0.045f);
            AppendRecovery(
                value,
                0.20f);

            return value;
        }

        private Sequence BuildDeath()
        {
            Sequence value = DOTween.Sequence();
            Vector3 forward = avatar.LocalForward;
            Vector3 up = avatar.LocalUp;

            value.Append(
                avatar.PresentationRoot
                    .DOLocalMove(
                        neutralPosition -
                        forward * 0.10f +
                        up * 0.05f,
                        0.12f)
                    .SetEase(Ease.OutQuad));
            value.Join(
                avatar.PresentationRoot
                    .DOLocalRotate(
                        EulerOffset(
                            neutralRotation,
                            10f,
                            0f,
                            0f),
                        0.12f)
                    .SetEase(Ease.OutQuad));
            JoinEquipmentHit(
                value,
                0.12f);

            value.Append(
                avatar.PresentationRoot
                    .DOLocalMove(
                        neutralPosition -
                        forward * 0.18f -
                        up * 0.12f,
                        0.28f)
                    .SetEase(Ease.InQuad));
            value.Join(
                avatar.PresentationRoot
                    .DOLocalRotate(
                        EulerOffset(
                            neutralRotation,
                            78f,
                            0f,
                            10f),
                        0.28f)
                    .SetEase(Ease.InQuad));

            JoinEquipmentDeath(
                value,
                0.28f);

            value.AppendInterval(0.28f);
            return value;
        }

        private void AppendRecovery(
            Sequence value,
            float duration)
        {
            value.Append(
                avatar.PresentationRoot
                    .DOLocalMove(
                        neutralPosition,
                        duration)
                    .SetEase(Ease.OutQuad));
            value.Join(
                avatar.PresentationRoot
                    .DOLocalRotateQuaternion(
                        neutralRotation,
                        duration)
                    .SetEase(Ease.OutQuad));
            value.Join(
                avatar.PresentationRoot
                    .DOScale(
                        neutralScale,
                        duration)
                    .SetEase(Ease.OutQuad));

            JoinEquipmentRecovery(
                value,
                duration);
        }

        private void JoinEquipmentIdle(
            Sequence value,
            float duration)
        {
            for (int i = 0; i < avatar.Equipment.Count; i++)
            {
                EquipmentPresentation item =
                    avatar.Equipment[i];
                float sign = EquipmentSign(i);

                value.Join(
                    item.MotionRoot
                        .DOLocalMove(
                            item.LocalUp * 0.008f,
                            duration)
                        .SetEase(Ease.InOutSine));
                value.Join(
                    item.MotionRoot
                        .DOLocalRotate(
                            new Vector3(
                                0f,
                                0f,
                                sign * 1.5f),
                            duration)
                        .SetEase(Ease.InOutSine));
            }
        }

        private void JoinEquipmentWalkRise(
            Sequence value,
            float duration)
        {
            for (int i = 0; i < avatar.Equipment.Count; i++)
            {
                EquipmentPresentation item =
                    avatar.Equipment[i];
                float sign = EquipmentSign(i);

                value.Join(
                    item.MotionRoot
                        .DOLocalMove(
                            item.LocalUp * 0.035f -
                            item.LocalForward * 0.025f,
                            duration)
                        .SetEase(Ease.OutQuad));
                value.Join(
                    item.MotionRoot
                        .DOLocalRotate(
                            new Vector3(
                                8f,
                                0f,
                                sign * 7f),
                            duration)
                        .SetEase(Ease.OutQuad));
            }
        }

        private void JoinEquipmentWalkApex(
            Sequence value,
            float duration)
        {
            for (int i = 0; i < avatar.Equipment.Count; i++)
            {
                EquipmentPresentation item =
                    avatar.Equipment[i];
                float sign = EquipmentSign(i);

                value.Join(
                    item.MotionRoot
                        .DOLocalMove(
                            item.LocalUp * 0.055f -
                            item.LocalForward * 0.015f,
                            duration)
                        .SetEase(Ease.OutSine));
                value.Join(
                    item.MotionRoot
                        .DOLocalRotate(
                            new Vector3(
                                12f,
                                sign * 4f,
                                sign * 10f),
                            duration)
                        .SetEase(Ease.OutSine));
            }
        }

        private void JoinEquipmentWalkLand(
            Sequence value,
            float duration)
        {
            for (int i = 0; i < avatar.Equipment.Count; i++)
            {
                EquipmentPresentation item =
                    avatar.Equipment[i];
                float sign = EquipmentSign(i);

                value.Join(
                    item.MotionRoot
                        .DOLocalMove(
                            item.LocalForward * 0.035f,
                            duration)
                        .SetEase(Ease.InQuad));
                value.Join(
                    item.MotionRoot
                        .DOLocalRotate(
                            new Vector3(
                                -6f,
                                0f,
                                -sign * 5f),
                            duration)
                        .SetEase(Ease.InQuad));
            }
        }

        private void JoinEquipmentAttackWindup(
            Sequence value,
            float duration)
        {
            for (int i = 0; i < avatar.Equipment.Count; i++)
            {
                EquipmentPresentation item =
                    avatar.Equipment[i];
                value.Join(
                    item.MotionRoot
                        .DOLocalMove(
                            -item.LocalForward * 0.06f +
                            item.LocalUp * 0.03f,
                            duration)
                        .SetEase(Ease.OutQuad));
                value.Join(
                    item.MotionRoot
                        .DOLocalRotateQuaternion(
                            Quaternion.identity,
                            duration)
                        .SetEase(Ease.OutQuad));
            }
        }

        private void JoinEquipmentAttackStrike(
            Sequence value,
            float duration)
        {
            for (int i = 0; i < avatar.Equipment.Count; i++)
            {
                EquipmentPresentation item =
                    avatar.Equipment[i];
                value.Join(
                    item.MotionRoot
                        .DOLocalMove(
                            item.LocalForward * 0.18f +
                            item.LocalUp * 0.035f,
                            duration)
                        .SetEase(Ease.InQuad));
                value.Join(
                    item.MotionRoot
                        .DOLocalRotateQuaternion(
                            Quaternion.identity,
                            duration)
                        .SetEase(Ease.InQuad));
            }
        }

        private void JoinEquipmentAttackFollowThrough(
            Sequence value,
            float duration)
        {
            for (int i = 0; i < avatar.Equipment.Count; i++)
            {
                EquipmentPresentation item =
                    avatar.Equipment[i];
                value.Join(
                    item.MotionRoot
                        .DOLocalMove(
                            item.LocalForward * 0.13f,
                            duration)
                        .SetEase(Ease.OutQuad));
                value.Join(
                    item.MotionRoot
                        .DOLocalRotateQuaternion(
                            Quaternion.identity,
                            duration)
                        .SetEase(Ease.OutQuad));
            }
        }

        private void JoinEquipmentHit(
            Sequence value,
            float duration)
        {
            for (int i = 0; i < avatar.Equipment.Count; i++)
            {
                EquipmentPresentation item =
                    avatar.Equipment[i];
                float sign = EquipmentSign(i);

                value.Join(
                    item.MotionRoot
                        .DOLocalMove(
                            -item.LocalForward * 0.08f +
                            item.LocalUp * 0.025f,
                            duration)
                        .SetEase(Ease.OutQuad));
                value.Join(
                    item.MotionRoot
                        .DOLocalRotate(
                            new Vector3(
                                12f,
                                sign * 8f,
                                sign * 18f),
                            duration)
                        .SetEase(Ease.OutQuad));
            }
        }

        private void JoinEquipmentDeath(
            Sequence value,
            float duration)
        {
            for (int i = 0; i < avatar.Equipment.Count; i++)
            {
                EquipmentPresentation item =
                    avatar.Equipment[i];
                float sign = EquipmentSign(i);

                value.Join(
                    item.MotionRoot
                        .DOLocalMove(
                            -item.LocalForward * 0.05f -
                            item.LocalUp * 0.10f +
                            item.LocalRight * sign * 0.07f,
                            duration)
                        .SetEase(Ease.InQuad));
                value.Join(
                    item.MotionRoot
                        .DOLocalRotate(
                            new Vector3(
                                55f,
                                sign * 18f,
                                sign * 65f),
                            duration)
                        .SetEase(Ease.InQuad));
            }
        }


        private void JoinEquipmentRecovery(
            Sequence value,
            float duration)
        {
            for (int i = 0; i < avatar.Equipment.Count; i++)
            {
                EquipmentPresentation item =
                    avatar.Equipment[i];

                value.Join(
                    item.MotionRoot
                        .DOLocalMove(
                            Vector3.zero,
                            duration)
                        .SetEase(Ease.OutQuad));
                value.Join(
                    item.MotionRoot
                        .DOLocalRotateQuaternion(
                            Quaternion.identity,
                            duration)
                        .SetEase(Ease.OutQuad));
                value.Join(
                    item.MotionRoot
                        .DOScale(
                            Vector3.one,
                            duration)
                        .SetEase(Ease.OutQuad));
            }
        }

        private void CaptureNeutral()
        {
            RequireConfigured();

            Transform root =
                avatar.PresentationRoot;
            neutralPosition =
                root.localPosition;
            neutralRotation =
                root.localRotation;
            neutralScale =
                root.localScale;
            neutralCaptured = true;
        }

        private void ResetPresentation()
        {
            if (!neutralCaptured ||
                avatar == null ||
                avatar.PresentationRoot == null)
            {
                return;
            }

            Transform root =
                avatar.PresentationRoot;
            root.localPosition =
                neutralPosition;
            root.localRotation =
                neutralRotation;
            root.localScale =
                neutralScale;

            avatar.ResetEquipmentMotion();
        }

        private void KillSequence()
        {
            if (sequence != null &&
                sequence.IsActive())
            {
                sequence.Kill(false);
            }

            sequence = null;

            if (avatar == null)
                return;

            avatar.PresentationRoot?.DOKill(false);
            foreach (EquipmentPresentation item in avatar.Equipment)
                item?.MotionRoot?.DOKill(false);
        }

        private void RequireConfigured()
        {
            if (avatar == null)
            {
                throw new InvalidOperationException(
                    "AvatarMotionPlayer is missing AvatarPresentation.");
            }

            if (avatar.PresentationRoot == null)
            {
                throw new InvalidOperationException(
                    "AvatarMotionPlayer requires PresentationRoot.");
            }
        }

        private static Vector3 EulerOffset(
            Quaternion neutral,
            float x,
            float y,
            float z)
        {
            return (
                neutral *
                Quaternion.Euler(
                    x,
                    y,
                    z)).eulerAngles;
        }

        private static float EquipmentSign(int index) =>
            index % 2 == 0 ? 1f : -1f;
    }
}
