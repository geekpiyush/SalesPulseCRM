using Microsoft.AspNetCore.Http;
using SalesPulseCRM.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SalesPulseCRM.Application.ServiceContracts
{
    public interface ILeadService
    {
        Task<(bool success, string message)> CreateLeadAsync(CreateLeadDto createLeadDto);

        Task<List<LeadResponseDto>> GetAllLeadsAsync(int userId, string role);

        Task<LeadEditViewModel?> GetLeadByIdAsync(int id);
        Task<bool> UpdateLeadAsync(LeadEditViewModel model, int userId);
        Task<bool> DeleteLeadAsync(int id);

        Task<List<TimelineItemDto>> GetTimeline(int leadId);
        Task<List<UserTaskDto>> GetTodayTasksAsync(int userId, string role);
        Task<TotalLeadsDto> GetTotalLeadCount(int userId, string role);

        Task<UnassignedLeadDto> GetTotalUnassignedLead();
        Task<ConvertedStatsDto> GetConvertedLeads(int userId, string role);
        Task<LeadFunnelDto> GetLeadFunnelAsync(int userId, string role);

        Task<MemoryStream> GetExcelImportFile();
        Task<(int successCount, List<string> errors)> ImportLeadsFromExcel(IFormFile file);
    }
}
