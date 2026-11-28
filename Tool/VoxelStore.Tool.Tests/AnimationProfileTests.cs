using Hero.Voxel.Tool;
using VoxelStore.Tool;

internal static class AnimationProfileTests
{
    private const string PartName = "Body";

    internal static void Run(string root)
    {
        var policy = AnimationProfilePolicy.Load(root);
        foreach (var (id, profile) in policy.Profiles)
        {
            var asset = new AssetManifest
            {
                Id = id, Kind = profile.Kind, Form = profile.Forms.FirstOrDefault(), AnimationProfile = id
            };
            var clips = profile.Required.SelectMany(family => FourTakes(policy, family)).ToArray();
            var result = ProfileAnimationCoverage.Evaluate(policy, asset, clips);
            Require(result.Complete && result.RequiredClips.Count == profile.Required.Count * 4, id + " has four takes per family");
            var missing = ProfileAnimationCoverage.Evaluate(policy, asset, clips.SkipLast(1).ToArray());
            Require(!missing.Complete && missing.MissingClips.SequenceEqual(new[] { clips[^1].Name }), id + " identifies missing V3");
        }

        var equipment = new AssetManifest { Id = "fixture", Kind = "equipment", Form = "axe", AnimationProfile = "equipment.heavy" };
        var baseline = policy.Resolve(equipment).Required.SelectMany(f => FourTakes(policy, f)).ToArray();
        var forbidden = ProfileAnimationCoverage.Evaluate(policy, equipment,
            baseline.Concat(new[] { Clip("Hit", false, 0), Clip("Death", false, 0), Clip("WalkForward", true, 0) }).ToArray());
        Require(forbidden.InvalidClips.Count == 3 && !forbidden.Complete, "irrelevant equipment motions rejected");
        var malformed = ProfileAnimationCoverage.Evaluate(policy, equipment,
            baseline.Append(Clip("Attack.V01", false, 0)).ToArray());
        Require(malformed.InvalidClips.Count == 1 && !malformed.Complete, "malformed qualifier rejected");
        var wrongLoop = ProfileAnimationCoverage.Evaluate(policy, equipment,
            Replace(baseline, "Idle.V2", clip => clip with { Loop = false }));
        Require(wrongLoop.InvalidClips.Single().Contains("loop policy"), "profile determines looping");
        var side = ProfileAnimationCoverage.Evaluate(policy, equipment, baseline.Append(Clip("Attack.Left.V1", false, 1)).ToArray());
        Require(side.MissingClips.SequenceEqual(new[] { "Attack.Left", "Attack.Left.V2", "Attack.Left.V3" }), "side family requires its own four takes");

        var ranged = new AssetManifest { Id = "ranged", Kind = "equipment", Form = "bow", AnimationProfile = "equipment.ranged" };
        var optional = ProfileAnimationCoverage.Evaluate(policy, ranged, baseline.Append(Clip("Aim", true, 0)).ToArray());
        Require(optional.MissingClips.SequenceEqual(new[] { "Aim.V1", "Aim.V2", "Aim.V3" }), "optional motion is complete or absent");
        Require(ProfileAnimationCoverage.Evaluate(policy, ranged, baseline.Concat(FourTakes(policy, "Aim")).ToArray()).Complete,
            "complete allowed optional motion accepted");
        Require(!ProfileAnimationCoverage.Evaluate(policy, equipment, baseline.Concat(FourTakes(policy, "Aim")).ToArray()).Complete,
            "optional family not allowed on unrelated form");

        var part = new CompiledPart(PartName, null, "opaque",
            new CompiledTransform(new Int3(0, 0, 0)), new Float3(0, 0, 0),
            new Int3(1, 1, 1), new[] { new VoxelCell(0, 0, 0, 0) });
        var model = new VoxelCompilation("AnimationFixture", 8, Array.Empty<PaletteEntry>(), new[] { part },
            Array.Empty<CompiledSocket>(), baseline) { AssemblyOrigin = new CompiledAssemblyOrigin(PartName) };

        ProfileAnimationQuality Quality(CompiledAnimationClip[] clips) =>
            ProfileAnimationQuality.Evaluate(policy, equipment, model with { Animations = clips });

        Require(Quality(baseline).Complete, "distinct finite sampled trajectories and reference channels pass mechanical checks");
        var attack = baseline.Single(c => c.Name == "Attack");
        var durationOnly = Quality(Replace(baseline, "Attack.V1", _ => attack with { Name = "Attack.V1", Duration = 2f }));
        Require(durationOnly.DuplicateTakes.Any(x => x.Contains("Attack.V1 duplicates Attack")), "duration-only variants rejected");
        var rekeyed = attack with
        {
            Name = "Attack.V1",
            Tracks = new[] { attack.Tracks[0] with { Position = Channel(
                (0, new Float3(0, 0, 0)), (.25f, new Float3(1, 0, 0)), (.5f, new Float3(2, 0, 0)),
                (.75f, new Float3(1, 0, 0)), (1, new Float3(0, 0, 0))) } }
        };
        Require(Quality(Replace(baseline, "Attack.V1", _ => rekeyed)).DuplicateTakes.Any(x => x.Contains("Attack.V1 duplicates Attack")),
            "equivalent rekeyed motion rejected");
        var variant = baseline.Single(c => c.Name == "Attack.V2");
        Require(Quality(Replace(baseline, "Attack.V3", _ => variant with { Name = "Attack.V3" })).DuplicateTakes
            .Any(x => x.Contains("Attack.V3 duplicates Attack.V2")), "non-base duplicates rejected");
        var noOp = Quality(Replace(baseline, "Attack.V2", clip => clip with { Tracks = Array.Empty<CompiledAnimationTrack>() }));
        Require(noOp.InvalidTakes.Any(x => x.Contains("no authored motion")), "no-op rejected");
        var brokenSeam = Quality(Replace(baseline, "Idle.V1", clip => clip with
        {
            Tracks = new[] { clip.Tracks[0] with { Position = Channel((0, new Float3(0, 0, 0)), (1, new Float3(2, 0, 0))) } }
        }));
        Require(brokenSeam.InvalidTakes.Any(x => x.Contains("loop seam")), "unclosed loop rejected");
        var zeroScale = Quality(Replace(baseline, "Drop.V1", clip => clip with
        {
            Tracks = new[] { clip.Tracks[0] with { Scale = Channel((0, new Float3(1, 1, 1)), (1, new Float3(0, 0, 0))) } }
        }));
        Require(zeroScale.InvalidTakes.Any(x => x.Contains("unsupported")), "singular pose rejected");
        Require(Quality(Replace(baseline, "Drop.V1", clip => clip with { ReferenceInfluence = null })).InvalidTakes
            .Any(x => x.Contains("Drop must release")), "terminal Drop must release");
        var detached = attack with
        {
            Name = "Attack.V1",
            ReferenceInfluence = Influence((0, 1), (.25f, 0), (.75f, 0), (1, 1))
        };
        Require(Quality(Replace(baseline, "Attack.V1", _ => detached)).Complete,
            "authored release and return distinguishes otherwise matching local motion");
        Require(Quality(Replace(baseline, "Attack.V1", _ => detached with
        {
            ReferenceInfluence = Influence((0, 1), (.25f, 0), (1, 0))
        })).InvalidTakes.Any(x => x.Contains("Attack must start and finish")), "nonterminal release returns");

        var avatarAsset = new AssetManifest
        {
            Id = "avatar-fixture",
            Kind = "avatar",
            AnimationProfile = "avatar.grounded"
        };
        CompiledAnimationClip[] avatarClips = policy.Resolve(avatarAsset).Required
            .SelectMany(f => FourTakes(policy, f))
            .ToArray();
        VoxelCompilation avatarModel = model with
        {
            Name = "AvatarAnimationFixture",
            Animations = avatarClips
        };
        IReadOnlyList<StoreEngine.CompatibilitySample> samples =
            StoreEngine.BuildCompatibilitySamples(avatarModel, model);
        Require(
            avatarClips.All(clip => samples.Any(sample => sample.BodyClip == clip.Name)),
            "every authored body take receives compatibility coverage");
        Require(
            baseline.All(clip => samples.Any(sample => sample.EquipmentClip == clip.Name)),
            "every authored equipment take receives compatibility coverage");
        Require(
            samples.Any(sample =>
                sample.BodyClip == "Death.V2" &&
                sample.EquipmentClip == "Drop.V2"),
            "terminal body/equipment motions are paired independently");
        Require(
            samples.Count < avatarClips.Length * baseline.Length,
            "compatibility planning avoids a Cartesian product of authored takes");
        Require(
            samples.Any(sample =>
                sample.BodyPhase != sample.EquipmentPhase),
            "independent clip phases are represented");
        try
        {
            _ = StoreEngine.BuildCompatibilitySamples(
                avatarModel,
                model with
                {
                    Animations = baseline
                        .Where(clip => clip.Name != "Idle")
                        .ToArray()
                });
            throw new InvalidOperationException(
                "missing exact Idle must fail compatibility planning");
        }
        catch (InvalidDataException ex)
        {
            Require(
                ex.Message.Contains("Idle", StringComparison.Ordinal),
                "missing exact clip reports its identity");
        }
    }

    private static IEnumerable<CompiledAnimationClip> FourTakes(AnimationProfilePolicy policy, string family)
    {
        for (int i = 0; i <= 3; i++)
            yield return Clip(i == 0 ? family : family + ".V" + i, policy.LoopFor(family), i);
    }

    private static CompiledAnimationClip Clip(string name, bool loop, int variant)
    {
        Float3[] directions = { new(2, 0, 0), new(0, 2, 0), new(0, 0, 2), new(-2, 0, 0) };
        bool drop = name == "Drop" || name.StartsWith("Drop.", StringComparison.Ordinal);
        return new CompiledAnimationClip(name, 1f, loop,
            new[] { new CompiledAnimationTrack(PartName, Channel(
                (0, new Float3(0, 0, 0)), (.5f, directions[variant]), (1, drop ? new Float3(0, 0, -3) : new Float3(0, 0, 0))), null, null) })
        { ReferenceInfluence = drop ? Influence((0, 1), (.5f, 0), (1, 0)) : null };
    }

    private static CompiledAnimationVectorChannel Channel(params (float Time, Float3 Value)[] keys) =>
        new(CompiledAnimationInterpolation.Linear, keys.Select(key => new CompiledAnimationVectorKey(key.Time, key.Value)).ToArray());
    private static CompiledAnimationScalarChannel Influence(params (float Time, float Value)[] keys) =>
        new(CompiledAnimationInterpolation.Linear, keys.Select(key => new CompiledAnimationScalarKey(key.Time, key.Value)).ToArray());
    private static CompiledAnimationClip[] Replace(CompiledAnimationClip[] source, string name, Func<CompiledAnimationClip, CompiledAnimationClip> map) =>
        source.Select(clip => clip.Name == name ? map(clip) : clip).ToArray();
    private static void Require(bool condition, string message)
    {
        if (!condition)
            throw new InvalidOperationException(message);
    }
}
