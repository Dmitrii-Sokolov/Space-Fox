using Zenject;

namespace SpaceFox
{
    public class PlaneTerrainInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            Container.BindInterfacesAndSelfTo<PlaneHeightProvider>().FromNew().AsSingle();
        }
    }
}
