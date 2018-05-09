using System.Collections.Generic;
using Contracts.Models;

namespace Contracts.Services
{
    public interface IFlowInfoService
    {
        string Url { get; set; }

        void AddFlowInfo(FlowData flowData);
        string GetUrl(string projectId, string codename);
    }
}
