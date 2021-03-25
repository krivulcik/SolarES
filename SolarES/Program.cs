using Nest;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;

namespace SolarES
{
    class Program
    {
        static void Main(string[] args)
        {
            ESClient.Server = "http://localhost:9200/";
            ESClient.RegisterMapping(typeof(SolarLog), "solar");

            foreach (var filename in args)
            {
                Console.WriteLine(filename);
                var position = 0;

                var logs = ParseGzippedFile(filename);

                var bulkAllObservable = ESClient.Client.BulkAll(logs, b => b
                    .BackOffTime("30s")
                    .BackOffRetries(2)
                    .RefreshOnCompleted()
                    .MaxDegreeOfParallelism(Environment.ProcessorCount)
                    .Size(1000)
                )
                .Wait(TimeSpan.FromMinutes(15), next =>
                {
                    // do something e.g. write number of pages to console
                });


                    //ESClient.Client.Bulk(b =>
                    //{
                    //    while (!reader.EndOfStream)
                    //    {
                    //        ++position;
                    //        var line = reader.ReadLine();
                    //        var item = ParseLog(line);
                    //        if (position % 100000 == 0)
                    //        {
                    //            Console.WriteLine(item.Date.ToString("dd.MM.yyyy HH:mm"));
                    //        }
                    //        b.Index<SolarLog>(i => i.Document(item));
                    //    }

                    //    return b;
                    //});
                //}
                Console.WriteLine("Done " + position);
            }
        }

        private static IEnumerable<SolarLog> ParseGzippedFile(string filename)
        {
            using (FileStream originalFileStream = File.OpenRead(filename))
            using (GZipStream decompressionStream = new GZipStream(originalFileStream, CompressionMode.Decompress))
            using (var reader = new StreamReader(decompressionStream))
            {
                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine();
                    yield return ParseLog(line);
                }
            }
        }

        private static SolarLog ParseLog(string line)
        {
            var items = line.Split(' ');
            var date = DateTimeOffset.FromUnixTimeMilliseconds(long.Parse(items[0]));

            return new SolarLog
            {
                Id = "sl-" + date.ToUnixTimeMilliseconds(),
                Date = date.UtcDateTime,
                DurationMs = int.Parse(items[1]),
                ProductionW = double.Parse(items[9], CultureInfo.InvariantCulture),
                ProductionWh = double.Parse(items[10], CultureInfo.InvariantCulture),
                LoadW = double.Parse(items[13], CultureInfo.InvariantCulture),
                LoadWh = double.Parse(items[14], CultureInfo.InvariantCulture),
            };
        }
    }
}
