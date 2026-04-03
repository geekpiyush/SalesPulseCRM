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


    }
}
