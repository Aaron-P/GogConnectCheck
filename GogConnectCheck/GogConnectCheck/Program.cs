using GogConnectCheck.DB;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GogConnectCheck
{
    internal class Program
    {
        private const string UserAgent = "Mozilla/5.0 (compatible; SteamConnectBot/1.0) like Gecko";
        private static readonly Uri ConnectUrl = new Uri("https://www.gog.com/connect", UriKind.Absolute);
        private static readonly Regex Numeric = new Regex(@"^\d+$", RegexOptions.Compiled | RegexOptions.CultureInvariant);

        [STAThread]
        private static void Main()
        {
            MainAsync().Wait();
        }

        private static async Task MainAsync()
        {
            var dbPath = default(string);
            using (var isoStore = IsolatedStorageFile.GetStore(IsolatedStorageScope.User | IsolatedStorageScope.Assembly, null, null))
            using (var isoStream = new IsolatedStorageFileStream("db.sqlite", FileMode.OpenOrCreate, FileAccess.Write, isoStore))
                dbPath = isoStream.GetType().GetField("m_FullPath", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(isoStream).ToString(); //gross

            if (String.IsNullOrEmpty(dbPath))
                return;

            using (var dbContext = new SQLiteContext(dbPath))
            {
                var html = default(string);
                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.UserAgent.ParseAdd(ConfigurationManager.AppSettings["UserAgent"] ?? UserAgent);

                    var response = await client.GetAsync(ConnectUrl);
                    if (!response.IsSuccessStatusCode)
                        return;

                    html = await response.Content.ReadAsStringAsync();
                }

                if (String.IsNullOrEmpty(html))
                    return;

                var document = new HtmlAgilityPack.HtmlDocument();
                document.LoadHtml(html);

                var stateHolders = document.DocumentNode
                    .SelectNodes("//div[@gog-product]")
                    .Where(_ => Numeric.IsMatch(_.Attributes["gog-product"].Value));

                var products = new List<Product>();

                foreach (var stateHolder in stateHolders)
                {
                    int id;
                    if (!int.TryParse(stateHolder.Attributes["gog-product"].Value, out id))
                        continue;

                    var nameNode = stateHolder.SelectSingleNode(".//span[contains(@class,'product-title__text')]");
                    if (nameNode == null)//Error out?
                        continue;

                    var name = nameNode.InnerText.Trim();
                    if (String.IsNullOrEmpty(name))
                        continue;

                    var counterNode = stateHolder.SelectSingleNode(".//span[@gog-counter]");
                    if (counterNode == null)//Error out?
                        continue;

                    int counter;
                    if (!int.TryParse(counterNode.Attributes["gog-counter"].Value, out counter))
                        continue;

                    var product = new Product { Id = id, Name = name, Timestamp = counter };
                    products.Add(product);
                }

                if (!products.Any())
                    return;

                var newProducts = new List<Product>();
                foreach (var product in products)
                {
                    var dbProduct = dbContext.Products.SingleOrDefault(_ => _.Id == product.Id);
                    if (dbProduct == null)
                    {
                        dbContext.Products.Add(product);
                        await dbContext.SaveChangesAsync();
                        newProducts.Add(product);
                    }
                    else if (dbProduct.Expiration < product.Expiration || dbProduct.Name != product.Name)
                    {
                        dbProduct.Timestamp = product.Timestamp;
                        dbProduct.Name = product.Name;
                        await dbContext.SaveChangesAsync();
                        newProducts.Add(product);
                    }
                }

                if (!newProducts.Any())
                    return;

                var message = new StringBuilder();
                message.Append("The following new games are available on GOG connect:\n\n");
                foreach (var newProduct in newProducts)
                {
                    message.AppendFormat("{0}\n", newProduct.Name);
                }
                message.AppendFormat("\nWould you like to go to {0}?", ConnectUrl.AbsoluteUri);

                if (MessageBox.Show(message.ToString(), "New games available!", MessageBoxButtons.YesNo, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly) == DialogResult.Yes)
                    Process.Start(ConnectUrl.AbsoluteUri);
            }
        }
    }
}
