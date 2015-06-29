using System.ComponentModel;
using System.Configuration.Install;

namespace SPU.WinService.FraudDetection
{
    [RunInstaller(true)]
    public partial class ProjectInstaller : Installer
    {
        public ProjectInstaller()
        {
            InitializeComponent();
        }
    }
}