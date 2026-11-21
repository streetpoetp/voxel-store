using Hero.Config.Runtime;

namespace VoxelStore.Preview.Editor
{
    [DefaultConfigVersion]
    public sealed class VoxelStorePreviewConfigVersion : IConfigVersion
    {
        public string VersionName => "voxel-store-preview";

        public void BuildConfigs(EditorConfigBuilder builder)
        {
            // Preview-only consumer: no gameplay/config domain is authored here.
        }
    }
}
