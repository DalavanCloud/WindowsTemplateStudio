﻿// ******************************************************************
// Copyright (c) Microsoft. All rights reserved.
// This code is licensed under the MIT License (MIT).
// THE CODE IS PROVIDED “AS IS”, WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED,
// INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.
// IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM,
// DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT,
// TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH
// THE CODE OR THE USE OR OTHER DEALINGS IN THE CODE.
// ******************************************************************

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.TemplateEngine.Abstractions;
using Microsoft.TemplateEngine.Edge.Template;
using Microsoft.Templates.Core.Diagnostics;
using Microsoft.Templates.Core.Locations;

using Newtonsoft.Json;
using System.Text.RegularExpressions;

namespace Microsoft.Templates.Core
{
    public class TemplatesRepository
    {
        private const string Catalog = "_catalog";
        private static readonly string[] SupportedIconTypes = new string[] { ".jpg", ".jpeg", ".png", ".xaml" };

        public TemplatesSynchronization Sync { get; private set; }

        public string CurrentContentFolder { get => Sync?.CurrentContentFolder; }

        public TemplatesRepository(TemplatesSource source)
        {
            Sync = new TemplatesSynchronization(source);
        }

         
        public string GetTemplatesVersion()
        {
            return Sync.CurrentContentVersion?.ToString();
        }

        public async Task SynchronizeAsync(bool force = false)
        {
            await Sync.Do(force);
        }

        public IEnumerable<ITemplateInfo> GetAll()
        {
            var queryResult = CodeGen.Instance.Cache.List(false, WellKnownSearchFilters.LanguageFilter("C#"));

            return queryResult
                        .Where(r => r.IsMatch)
                        .Select(r => r.Info)
                        .ToList();
        }

        public IEnumerable<ITemplateInfo> Get(Func<ITemplateInfo, bool> predicate)
        {
            return GetAll()
                        .Where(predicate);
        }

        public IEnumerable<ITemplateInfo> GetDependencies(ITemplateInfo ti)
        {
            return ti.GetDependencyList().Select(d => GetAll().FirstOrDefault(t => t.Identity == d));
        }


        public ITemplateInfo Find(Func<ITemplateInfo, bool> predicate)
        {
            return GetAll()
                        .FirstOrDefault(predicate);
        }


        public IEnumerable<MetadataInfo> GetProjectTypes()
        {
            return GetMetadataInfo("projectTypes");
        }

        public IEnumerable<MetadataInfo> GetFrameworks()
        {
            return GetMetadataInfo("frameworks");
        }

        private IEnumerable<MetadataInfo> GetMetadataInfo(string type)
        {
            var folderName = Path.Combine(Sync.CurrentContentFolder, Catalog);
            if (!Directory.Exists(folderName))
            {
                return null;
            }

            var metadataFile = Path.Combine(folderName, $"{type}.json");
            var metadata = JsonConvert.DeserializeObject<List<MetadataInfo>>(File.ReadAllText(metadataFile));

            metadata.ForEach(m => SetMetadataDescription(m, folderName, type));
            metadata.ForEach(m => SetMetadataIcon(m, folderName, type));
            metadata.ForEach(m => m.MetadataType = type);
            metadata.ForEach(m => SetLicenceTerms(m));

            return metadata.OrderBy(m => m.Order);
        }

        private const string Separator = "|";
        private const string LicencesPattern = @"\[(?<text>.*?)\]\((?<url>.*?)\)\" + Separator + "?";

        private void SetLicenceTerms(MetadataInfo metadataInfo)
        {            
            var result = new List<(string text, string url)>();

            var licencesMatches = Regex.Matches(metadataInfo.Licences, LicencesPattern);
            for (int i = 0; i < licencesMatches.Count; i++)
            {
                var m = licencesMatches[i];
                if (m.Success)
                {
                    result.Add((m.Groups["text"].Value, m.Groups["url"].Value));
                }

            }
            metadataInfo.LicenceTerms = result;
        }

        private static void SetMetadataDescription(MetadataInfo mInfo, string folderName, string type)
        {
            var descriptionFile = Path.Combine(folderName, type, $"{mInfo.Name}.md");
            if (File.Exists(descriptionFile))
            {
                mInfo.Description = File.ReadAllText(descriptionFile);
            }
        }

        private static void SetMetadataIcon(MetadataInfo mInfo, string folderName, string type)
        {
            var iconFile = Directory
                            .EnumerateFiles(Path.Combine(folderName, type))
                            .Where(f => SupportedIconTypes.Contains(Path.GetExtension(f)))
                            .FirstOrDefault(f => Path.GetFileNameWithoutExtension(f).Equals(mInfo.Name, StringComparison.OrdinalIgnoreCase));

            if (File.Exists(iconFile))
            {
                mInfo.Icon = iconFile;
            }
        }

        public ITemplateInfo GetLayoutTemplate(LayoutItem item, string framework)
        {
            return Find(t => t.GroupIdentity == item.templateGroupIdentity && t.GetFrameworkList().Any(f => f.Equals(framework, StringComparison.OrdinalIgnoreCase)));
        }
    }
}
