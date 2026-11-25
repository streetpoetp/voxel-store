using System;
using System.Collections.Generic;
using Hero.Voxel.Actor.Runtime.Unity;
using Hero.Voxel.Runtime.Assets;
using UnityEngine;

namespace VoxelStore.Preview.Runtime
{
    [DisallowMultipleComponent]
    public sealed class VoxelModelMotionPlayer : MonoBehaviour
    {
        [SerializeField] private VoxelRenderPresentation model;
        [SerializeField] private AvatarPresentation composition;
        [SerializeField] private string clipName = "Idle";
        [SerializeField] private bool loop = true;
        [SerializeField, Min(0.05f)] private float speed = 1f;

        private bool playing;

        public VoxelRenderPresentation Model => model;
        public AvatarPresentation Composition => composition;
        public VoxelAnimationAsset Animation =>
            model != null ? model.Animation : null;
        public string ClipName => clipName ?? string.Empty;
        public bool Loop => loop;
        public float Speed => speed;
        public bool IsBody =>
            composition != null &&
            composition.IsBodyModel(model);
        public EquipmentPresentation Equipment =>
            composition != null
                ? composition.FindEquipment(model)
                : null;
        public bool IsPlaying =>
            Application.isPlaying &&
            playing &&
            model != null &&
            model.AnimationTrack != null &&
            model.AnimationTrack.IsPlaying;
        public float CurrentNormalizedPhase =>
            model != null &&
            model.AnimationTrack != null &&
            model.AnimationTrack.HasSelection
                ? model.AnimationTrack.NormalizedPhase
                : 0f;
        public float CurrentDuration =>
            model != null &&
            model.AnimationTrack != null &&
            model.AnimationTrack.HasSelection
                ? model.AnimationTrack.SelectedClip.Duration
                : 0f;
        public string CurrentClipName =>
            model != null &&
            model.AnimationTrack != null &&
            model.AnimationTrack.HasSelection
                ? model.AnimationTrack.SelectedClip.Clip.Name
                : string.Empty;

        public void Configure(
            VoxelRenderPresentation renderModel,
            AvatarPresentation avatarComposition,
            string configuredClip,
            bool shouldLoop,
            float playbackSpeed)
        {
            model = renderModel != null
                ? renderModel
                : throw new ArgumentNullException(nameof(renderModel));
            composition = avatarComposition;
            SetPlaybackSettings(
                configuredClip,
                shouldLoop,
                playbackSpeed);
            playing = false;

            if (Application.isPlaying)
                ResetMotion();
        }

        public void SetPlaybackSettings(
            string configuredClip,
            bool shouldLoop,
            float playbackSpeed)
        {
            if (string.IsNullOrWhiteSpace(configuredClip))
            {
                throw new ArgumentException(
                    "An exact voxel animation clip name is required.",
                    nameof(configuredClip));
            }

            ValidateSpeed(playbackSpeed);
            clipName = configuredClip.Trim();
            loop = shouldLoop;
            speed = playbackSpeed;
        }

        public void SetClip(string exactClipName)
        {
            if (string.IsNullOrWhiteSpace(exactClipName))
                throw new ArgumentException(
                    "An exact voxel animation clip name is required.",
                    nameof(exactClipName));

            string normalized = exactClipName.Trim();
            if (string.Equals(
                    clipName,
                    normalized,
                    StringComparison.Ordinal))
            {
                return;
            }

            clipName = normalized;
            if (playing && Application.isPlaying)
                PlayConfigured();
        }

        public void SetLoop(bool shouldLoop)
        {
            if (loop == shouldLoop)
                return;

            loop = shouldLoop;
            if (playing && Application.isPlaying)
                PlayConfigured();
        }

        public void SetSpeed(float playbackSpeed)
        {
            ValidateSpeed(playbackSpeed);
            speed = playbackSpeed;

            VoxelModelAnimationTrack track =
                model != null ? model.AnimationTrack : null;
            if (track != null && track.HasSelection)
                track.SetSpeed(speed);
        }

        public void PlayConfigured()
        {
            PlayExact(clipName, loop, speed);
        }

        public void PlayExact(
            string exactClipName,
            bool shouldLoop,
            float playbackSpeed)
        {
            RequireConfigured();
            SetPlaybackSettings(
                exactClipName,
                shouldLoop,
                playbackSpeed);

            VoxelModelAnimationTrack track =
                model.EnsureAnimationTrack();
            track.Play(
                new[] { clipName },
                shouldLoop,
                playbackSpeed);
            playing = true;
            RefreshComposition();
        }

        public void PlayCandidates(
            IReadOnlyList<string> candidates,
            bool? loopOverride = null,
            float playbackSpeed = 1f)
        {
            RequireConfigured();
            ValidateSpeed(playbackSpeed);
            VoxelModelAnimationTrack track =
                model.EnsureAnimationTrack();
            VoxelActorResolvedClip resolved =
                track.ResolveRequired(candidates);
            clipName = resolved.Clip.Name;
            loop = loopOverride ?? resolved.Loop;
            speed = playbackSpeed;
            track.Play(
                resolved,
                loopOverride,
                playbackSpeed);
            playing = true;
            RefreshComposition();
        }

        public void SampleExact(
            string exactClipName,
            float normalizedTime)
        {
            RequireConfigured();
            ValidateNormalizedTime(normalizedTime);
            if (string.IsNullOrWhiteSpace(exactClipName))
            {
                throw new ArgumentException(
                    "An exact voxel animation clip name is required.",
                    nameof(exactClipName));
            }

            clipName = exactClipName.Trim();
            VoxelModelAnimationTrack track =
                model.EnsureAnimationTrack();
            track.Select(new[] { clipName });
            track.SampleNormalized(normalizedTime);
            playing = false;
            RefreshComposition();
        }

        public void SampleCandidates(
            IReadOnlyList<string> candidates,
            float normalizedTime)
        {
            RequireConfigured();
            ValidateNormalizedTime(normalizedTime);
            VoxelModelAnimationTrack track =
                model.EnsureAnimationTrack();
            VoxelActorResolvedClip resolved =
                track.ResolveRequired(candidates);
            clipName = resolved.Clip.Name;
            track.Select(resolved);
            track.SampleNormalized(normalizedTime);
            playing = false;
            RefreshComposition();
        }

        public void Stop()
        {
            playing = false;
            VoxelModelAnimationTrack track =
                model != null ? model.AnimationTrack : null;
            track?.Stop();
        }

        public void ResetMotion()
        {
            playing = false;
            if (model == null)
                return;

            VoxelModelAnimationTrack track =
                model.AnimationTrack;
            if (track != null)
                track.Reset();
            RefreshComposition();
        }

        public void SubmitNow()
        {
            RequireConfigured();
            if (composition != null)
                composition.SubmitNow();
            else
            {
                model.SyncWorldMatrix();
                model.World.SubmitNow();
            }
        }

        private void Update()
        {
            if (!Application.isPlaying ||
                !playing ||
                model == null)
            {
                return;
            }

            VoxelModelAnimationTrack track =
                model.EnsureAnimationTrack();
            track.Advance(Time.deltaTime);
            RefreshComposition();
            if (!track.IsPlaying)
                playing = false;
        }

        private void OnEnable()
        {
            if (!Application.isPlaying || model == null)
                return;

            PlayConfigured();
        }

        private void OnDisable()
        {
            if (Application.isPlaying)
                ResetMotion();
        }

        private void RefreshComposition()
        {
            if (composition != null)
                composition.RefreshBindings();
            else
                model?.SyncWorldMatrix();
        }

        private void RequireConfigured()
        {
            if (model == null)
            {
                throw new InvalidOperationException(
                    "VoxelModelMotionPlayer is missing its model.");
            }

            if (model.Animation == null)
            {
                throw new InvalidOperationException(
                    "VoxelModelMotionPlayer model has no baked animation asset.");
            }
        }

        private static void ValidateSpeed(float value)
        {
            if (float.IsNaN(value) ||
                float.IsInfinity(value) ||
                value < 0.05f)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(value),
                    "Playback speed must be finite and at least 0.05.");
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