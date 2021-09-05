using System;
using System.Collections.Generic;

    using System.IO;
    using System.Net;
    using System.Threading;
    using System.Threading.Tasks;
using Loxone.Net;

namespace WebApplication1.Services
{
    public class LoxoneService
    {
        public List<Room> GetRoms()
        {
return null;
        }

        private const string _miniserverAddress = "http://testminiserver.loxone.com:7778/";

        static async Task Get(string[] args)
        {
            using (var connection = new MiniserverConnection(new Uri(_miniserverAddress)))
            {
                // Specify Miniserver username and password.
                connection.Credentials = new NetworkCredential("web", "web");

                Console.WriteLine($"Opening connection to miniserver at {connection.Address}...");
                await connection.OpenAsync(CancellationToken.None).ConfigureAwait(false);

                // Load cached structure file or download a fresh one if the local file does not exist or is outdated.
                StructureFile structureFile = null;
                if (File.Exists("LoxAPP3.json"))
                {
                    structureFile = await StructureFile.LoadAsync("LoxAPP3.json", CancellationToken.None).ConfigureAwait(false);
                    var lastModified = await connection.GetStructureFileLastModifiedDateAsync(CancellationToken.None).ConfigureAwait(false);
                    if (lastModified > structureFile.LastModified)
                    {
                        // Structure file cached locally is outdated, throw it away.
                        structureFile = null;
                    }
                }

                if (structureFile == null)
                {
                    // The structure file either did not exist on disk or was outdated. Download a fresh copy from
                    // miniserver right now.
                    Console.WriteLine("Downloading structure file...");
                    structureFile = await connection.DownloadStructureFileAsync(CancellationToken.None).ConfigureAwait(false);

                    // Save it locally on disk.
                    await structureFile.SaveAsync("LoxAPP3.json", CancellationToken.None).ConfigureAwait(false);
                }

                Console.WriteLine($"Structure file loaded, culture {structureFile.Localization.Culture}.");

                Console.WriteLine("Enabling status updates...");
                await connection.EnableStatusUpdatesAsync(CancellationToken.None).ConfigureAwait(false);
            }
        }

        private static void StructureFileRoundTrip()
        {
            var structureFile = StructureFile.LoadAsync("LoxAPP3.json", CancellationToken.None).GetAwaiter().GetResult();
            structureFile.SaveAsync("LoxAPP3.roundtripped.json", CancellationToken.None).GetAwaiter().GetResult();
        }

        static void Main(string[] args)
        {
            ////StructureFileRoundTrip();

            try
            {
                Get(args).GetAwaiter().GetResult();
            }
            catch (Exception ex) when (!System.Diagnostics.Debugger.IsAttached)
            {
                Console.WriteLine(ex);
            }
        }
    }
    }
}
