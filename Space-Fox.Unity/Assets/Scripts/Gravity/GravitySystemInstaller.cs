using Zenject;

namespace SpaceFox
{
    public class GravitySystemInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            Container.BindInterfacesAndSelfTo<GravitySystem>().AsSingle();
        }
    }
}
