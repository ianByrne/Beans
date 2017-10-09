using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Beans
{
    /// <summary>
    /// Interaction logic for HeaderSection.xaml
    /// </summary>
    public partial class HeaderSection : UserControl
    {
        public event EventHandler SettingsClicked;
        public event EventHandler ReportsClicked;

        public HeaderSection()
        {
            InitializeComponent();
        }

        private void BtnSettings_Click(object sender, EventArgs e)
        {
            if (SettingsClicked != null)
            {
                SettingsClicked.Invoke(this, EventArgs.Empty);
            }
        }

        private void BtnReports_Click(object sender, EventArgs e)
        {
            if (ReportsClicked != null)
            {
                ReportsClicked.Invoke(this, EventArgs.Empty);
            }
        }
    }
}
