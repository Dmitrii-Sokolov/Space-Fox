using VContainer;
using VContainer.Unity;

namespace SpaceFox
{
    public class GravityLifetimeScope : LifetimeScope
    {
        protected override void Configure(IContainerBuilder builder)
        {
            builder.Register<GravitySystem>(Lifetime.Singleton);
        }
    }
}
