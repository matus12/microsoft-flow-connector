using System.Collections.Generic;
using Contracts.Models;
using Contracts.Services;

namespace Services.Services
{
    internal class FlowInfoService : IFlowInfoService
    {
        public string Str => "cus";
        public string Url { get; set; }

        private Dictionary<string, Dictionary<string, string>> _projectsData;

        public FlowInfoService()
        {
            _projectsData = new Dictionary<string, Dictionary<string, string>>();
        }

        public void AddFlowInfo(FlowData flowData)
        {
            if (_projectsData.TryGetValue(flowData.ProjectId, out var project))
            {
                if (!project.ContainsKey(flowData.Codename))
                {
                    project.Add(flowData.Codename, flowData.Url);
                }
                else
                {
                    project[flowData.Codename] = flowData.Url;
                }

                _projectsData[flowData.ProjectId] = project;
            }
            else
            {
                _projectsData.Add(flowData.ProjectId, new Dictionary<string, string>
                {
                    {
                        flowData.Codename,
                        flowData.Url
                    }
                });
            }
        }

        public string GetUrl(string projectId, string codename)
        {
            if (!_projectsData.TryGetValue(projectId, out var project)) return null;

            return project.TryGetValue(codename, out var url) ? url : null;
        }
    }
}
