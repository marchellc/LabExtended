using PluginAPI.Core;
using PluginAPI.Core.Factories;
using PluginAPI.Core.Interfaces;

namespace LabExtended.API
{
    public class ExFactory : PlayerFactory
    {
        public override Type BaseType { get; } = typeof(ExPlayer);

        public override Player Create(IGameComponent component)
        {
            if (component is ReferenceHub referenceHub)
                return new ExPlayer(referenceHub);

            throw new InvalidOperationException($"Unknown IGameComponent type: {component.GetType().FullName}");
        }
    }
}