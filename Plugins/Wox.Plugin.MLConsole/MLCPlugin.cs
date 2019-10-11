using System;
using Wox.Plugin;
using Wox.Plugin.Features;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using System.IO;
using CsvHelper;

namespace Wox.Plugin.MLConsole
{
    public class MLCPlugin : IPlugin
    {
        public PluginInitContext Context { get; set; }
        StreamReader reader;
        public void Init(PluginInitContext context)
        {
            Context = context;
        }
        public List<Result> Query(Query query)
        {
            string last = query.Terms.Last();
            if (last.Equals("ml", StringComparison.CurrentCultureIgnoreCase)) last = string.Empty;
            return GetData().GetRecords<SearchData>().Where(d => d.Ref == last).Select(d =>
            {
                return new Result()
                {
                    Title = d.Title,
                    SubTitle = d.SubTitle,
                    IcoPath = "icon.png",
                    Action = (a) =>
                    {
                        try
                        {
                            if (d.SearchAhead)
                                Context.API.ChangeQuery(query.RawQuery + " " + d.Title, true);
                            else
                                Process.Start("https://www.google.com/search?q=site%3AWikipedia.com+" + d.Title);

                            return !d.SearchAhead;
                        }
                        catch (Exception ex)
                        {
                            Context.API.ShowMsg(ex.Message);
                            return false;
                        }
                    }
                };
            }).ToList();
        }

        public CsvReader GetData()
        {
            if (reader != null)
            {
                reader.Close();
                reader.Dispose();
                reader = null;
            }
            reader = new StreamReader(Context.CurrentPluginMetadata.PluginDirectory + "\\mlc.csv");
            return new CsvReader(reader);
        }
    }

    public class SearchData
    {
        public string Title { get; set; }
        public string SubTitle { get; set; }
        public string Ref { get; set; }
        public bool SearchAhead { get; set; }
    }
}
