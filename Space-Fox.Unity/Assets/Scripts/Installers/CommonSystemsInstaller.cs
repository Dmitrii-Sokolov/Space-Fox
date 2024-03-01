using UnityEngine;
using Zenject;

namespace SpaceFox
{
    public class CommonSystemsInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            Container.BindInterfacesAndSelfTo<UpdateProxy>().FromNewComponentOnNewGameObject().AsSingle();
        }
    }
}
