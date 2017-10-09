using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Beans
{
    public static class Utils
    {
        public static IBeansDatabase BeansDatabase;

        public static MainWindow BeansMainWindow;
        public static IBeansReportsView ReportsView;
        public static IBeansSettingsView SettingsView;

        public static DateTime StartDate;
        public static DateTime EndDate;

        private static Dictionary<string, string> _config;

        public static void Init(MainWindow mainWindow)
        {
            LoadConfig();

            GoogleSheet googleSheet = new GoogleSheet(
                _config["GoogleSheetApplicationName"],
                _config["GoogleSheetSpreadsheetId"],
                _config["GoogleSheetClientSecretPath"],
                _config["GoogleSheetCredentialsPath"]);

            BeansDatabase = new BeansDatabaseGoogle(googleSheet);

            BeansMainWindow = mainWindow;

            // Default dates (this month)
            EndDate = DateTime.Now;
            StartDate = new DateTime(EndDate.Year, EndDate.Month, 1);

            // Reports View
            ReportsView = new BeansReportsPage();
            BeansReportsPresenter reportsPresenter = new BeansReportsPresenter(ReportsView);

            // Settings View
            SettingsView = new BeansSettingsPage();
            BeansSettingsPresenter settingsPresenter = new BeansSettingsPresenter(SettingsView);

            BeansMainWindow.NavigationService.Navigate(ReportsView);
        }

        private static void LoadConfig()
        {
            string configPath = System.Configuration.ConfigurationManager.AppSettings["ConfigFilePath"];

            using (StreamReader reader = new StreamReader(configPath))
            {
                string json = reader.ReadToEnd();

                _config = JsonConvert.DeserializeObject<Dictionary<string,string>>(json);
            }
        }
    }
}
