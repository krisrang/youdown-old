using System;
using System.Diagnostics;
using MEFedMVVM.ViewModelLocator;

namespace Lemon
{
    public static class Resolver
    {
        public static T GetInstance<T>()
        {
            return GetInstance<T>(null);
        }

        public static T GetInstance<T>(string name)
        {
            return ViewModelRepository.Instance.Resolver.Container.GetExportedValue<T>(name);
        }
    }
}