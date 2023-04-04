using MvvmCross.ViewModels;
using MvxStarter.Core.ViewModels;

namespace MvxStarter.Core
{
    public class App : MvxApplication
    {
        public override void Initialize()
        {
            RegisterAppStart<SearchPanelViewModel>();
        }
    }
}
