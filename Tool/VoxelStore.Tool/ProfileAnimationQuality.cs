using System.Numerics;
using Hero.Voxel.Tool;

namespace VoxelStore.Tool;

/// <summary>Mechanical motion checks; passing this does not confer visual acceptance.</summary>
internal sealed class ProfileAnimationQuality
{
    private const int DenseIntervals = 64;
    private const float PositionTolerance = 0.0001f; // model units
    private const float UnitlessTolerance = 0.0001f;

    internal List<string> InvalidTakes { get; } = new();
    internal List<string> DuplicateTakes { get; } = new();
    internal bool Complete => InvalidTakes.Count == 0 && DuplicateTakes.Count == 0;

    private sealed record Sample(VoxelCompiledPartPose[] Poses, float Influence);

    internal static ProfileAnimationQuality Evaluate(
        AnimationProfilePolicy policy, AssetManifest asset, VoxelCompilation compilation)
    {
        ProfileAnimationCoverage.Evaluate(policy, asset, compilation.Animations).RequireComplete(asset.Id);
        var result = new ProfileAnimationQuality();
        var groups = compilation.Animations.GroupBy(clip =>
        {
            var identity = SemanticAnimationClipName.Parse(clip.Name, policy.Families.Keys);
            return (identity.Motion, identity.Side);
        });

        foreach (var family in groups)
        {
            var clips = family.OrderBy(clip => clip.Name, StringComparer.Ordinal).ToArray();
            var phases = SamplePhases(clips);
            var trajectories = new List<(string Name, Sample[] Samples)>();
            foreach (var clip in clips)
            {
                Sample[] samples;
                try
                {
                    ValidateInfluenceKeys(clip);
                    samples = phases.Select(phase => EvaluateSample(compilation, clip, phase)).ToArray();
                }
                catch (Exception ex) when (ex is InvalidDataException or VoxelSpecException or ArgumentException)
                {
                    result.InvalidTakes.Add($"{clip.Name}: {ex.Message}");
                    continue;
                }

                if (samples.All(sample => Same(sample, samples[0], compilation.VoxelsPerUnit)))
                    result.InvalidTakes.Add($"{clip.Name}: no authored motion");
                if (clip.Loop && !Same(samples[0], samples[^1], compilation.VoxelsPerUnit))
                    result.InvalidTakes.Add($"{clip.Name}: loop seam does not close");
                if (asset.Kind == "equipment")
                {
                    if (family.Key.Motion == "Drop" &&
                        (!Near(samples[0].Influence, 1f) || !Near(samples[^1].Influence, 0f)))
                        result.InvalidTakes.Add($"{clip.Name}: Drop must release its reference from 1 to terminal 0");
                    if (family.Key.Motion == "Attack" &&
                        (!Near(samples[0].Influence, 1f) || !Near(samples[^1].Influence, 1f)))
                        result.InvalidTakes.Add($"{clip.Name}: Attack must start and finish at its attachment reference");
                }

                foreach (var previous in trajectories)
                    if (samples.Zip(previous.Samples, (a, b) => Same(a, b, compilation.VoxelsPerUnit)).All(equal => equal))
                        result.DuplicateTakes.Add($"{clip.Name} duplicates {previous.Name}");
                trajectories.Add((clip.Name, samples));
            }
        }
        return result;
    }

    // Uniform sampling plus authored keys and midpoints catches short-lived
    // poses and equivalent re-keying without comparing serialized channels.
    private static float[] SamplePhases(IEnumerable<CompiledAnimationClip> clips)
    {
        var phases = new SortedSet<float>(Enumerable.Range(0, DenseIntervals + 1)
            .Select(i => i / (float)DenseIntervals));
        foreach (var clip in clips)
        {
            foreach (var track in clip.Tracks)
                foreach (var channel in new[] { track.Position, track.Rotation, track.Scale })
                    if (channel != null)
                        foreach (var key in channel.Keys)
                            if (float.IsFinite(key.Time) && key.Time >= 0f && key.Time <= 1f)
                                phases.Add(key.Time);
            if (clip.ReferenceInfluence != null)
                foreach (var key in clip.ReferenceInfluence.Keys)
                    if (float.IsFinite(key.Time) && key.Time >= 0f && key.Time <= 1f)
                        phases.Add(key.Time);
        }
        float[] ordered = phases.ToArray();
        for (int i = 1; i < ordered.Length; i++)
            phases.Add((ordered[i - 1] + ordered[i]) * 0.5f);
        return phases.ToArray();
    }

    private static void ValidateInfluenceKeys(CompiledAnimationClip clip)
    {
        if (clip.ReferenceInfluence == null)
            return;
        float last = -1f;
        foreach (var key in clip.ReferenceInfluence.Keys)
        {
            if (!float.IsFinite(key.Time) || !float.IsFinite(key.Value) ||
                key.Time < 0f || key.Time > 1f || key.Time <= last ||
                key.Value < 0f || key.Value > 1f)
                throw new InvalidDataException("invalid reference-influence key");
            last = key.Time;
        }
    }

    private static Sample EvaluateSample(VoxelCompilation compilation, CompiledAnimationClip clip, float phase)
    {
        var evaluation = VoxelCompiledPoseEvaluator.Evaluate(compilation, clip, phase);
        var poses = evaluation.PartPoses.ToArray();
        if (poses.Length == 0)
            throw new InvalidDataException("no authored part to animate");
        for (int i = 0; i < poses.Length; i++)
        {
            var pose = poses[i];
            if (!Finite(pose.Position) || !Finite(pose.Scale) ||
                !float.IsFinite(pose.Rotation.LengthSquared()) || pose.Rotation.LengthSquared() < UnitlessTolerance ||
                pose.Scale.X < UnitlessTolerance || pose.Scale.Y < UnitlessTolerance || pose.Scale.Z < UnitlessTolerance)
                throw new InvalidDataException($"unsupported finite positive TRS at phase {phase}");
            var matrix = evaluation.GetPartModelMatrix(i);
            if (!Matrix4x4.Decompose(matrix, out var scale, out var rotation, out var position) ||
                !Finite(scale) || !Finite(position) || !float.IsFinite(rotation.LengthSquared()) ||
                scale.X < UnitlessTolerance || scale.Y < UnitlessTolerance || scale.Z < UnitlessTolerance)
                throw new InvalidDataException($"unsupported model transform at phase {phase}");
            var reconstructed = Matrix4x4.CreateScale(scale) * Matrix4x4.CreateFromQuaternion(rotation) *
                Matrix4x4.CreateTranslation(position);
            if (StoreEngine.MaxMatrixDelta(matrix, reconstructed) > UnitlessTolerance)
                throw new InvalidDataException($"non-TRS model transform at phase {phase}");
        }
        float influence = StoreEngine.SampleReferenceInfluence(clip, phase);
        if (!float.IsFinite(influence) || influence < 0f || influence > 1f)
            throw new InvalidDataException($"invalid reference influence at phase {phase}");
        return new Sample(poses, influence);
    }

    private static bool Same(Sample a, Sample b, int voxelsPerUnit)
    {
        if (a.Poses.Length != b.Poses.Length || !Near(a.Influence, b.Influence))
            return false;
        for (int i = 0; i < a.Poses.Length; i++)
        {
            var left = a.Poses[i];
            var right = b.Poses[i];
            // Quaternion sign does not change the represented rotation.
            float rotationDifference = MathF.Min(
                (left.Rotation - right.Rotation).LengthSquared(),
                (left.Rotation + right.Rotation).LengthSquared());
            if (Vector3.Distance(left.Position, right.Position) / voxelsPerUnit > PositionTolerance ||
                Vector3.Distance(left.Scale, right.Scale) > UnitlessTolerance ||
                rotationDifference > UnitlessTolerance * UnitlessTolerance)
                return false;
        }
        return true;
    }

    private static bool Near(float a, float b) => MathF.Abs(a - b) <= UnitlessTolerance;
    private static bool Finite(Vector3 v) => float.IsFinite(v.X) && float.IsFinite(v.Y) && float.IsFinite(v.Z);
}
