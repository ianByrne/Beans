using Google.Apis.Auth.OAuth2;
using Google.Apis.Sheets.v4;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Beans
{
    public class GoogleSheet
    {
        private readonly string[] _scopes = { SheetsService.Scope.Spreadsheets };
        private readonly string _applicationName;
        private readonly string _clientSecretPath;
        private readonly string _credentialsPath;
        public string SpreadsheetId { get; private set; }
        public SheetsService Service { get; private set; }

        public GoogleSheet(string applicationName, string spreadsheetId, string clientSecretPath, string credentialsPath)
        {
            _applicationName = applicationName;
            _clientSecretPath = clientSecretPath;
            SpreadsheetId = spreadsheetId;

            string credPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal);
            _credentialsPath = Path.Combine(credPath, credentialsPath);
        }

        public async Task Connect()
        {
            UserCredential credential;

            using (var stream = new FileStream(_clientSecretPath, FileMode.Open, FileAccess.Read))
            {
                credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
                    GoogleClientSecrets.Load(stream).Secrets,
                    _scopes,
                    "user",
                    CancellationToken.None,
                    new FileDataStore(_credentialsPath, true));
            }

            // Create Google Sheets API service.
            Service = new SheetsService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = _applicationName,
            });
        }
    }
}
