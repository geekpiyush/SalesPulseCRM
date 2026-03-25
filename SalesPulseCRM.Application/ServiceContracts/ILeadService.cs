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

        Task<List<LeadResponseDto>> GetAllLeadsAsync();

        Task<LeadResponseDto?> GetLeadByIdAsync(int id);

        Task<bool> DeleteLeadAsync(int id);
    }
}
