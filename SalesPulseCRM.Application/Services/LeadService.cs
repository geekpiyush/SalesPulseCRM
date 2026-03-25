using Microsoft.EntityFrameworkCore;
using SalesPulseCRM.Application.DTOs;
using SalesPulseCRM.Application.ServiceContracts;
using SalesPulseCRM.Domain.Entities;
using SalesPulseCRM.Infrastructure.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SalesPulseCRM.Application.Services
{
    public class LeadService : ILeadService
    {
        private readonly CrmDbContext _db;
        public LeadService(CrmDbContext crmDbContext)
        {
            _db = crmDbContext;
        }
        public async Task<(bool success, string message)> CreateLeadAsync(CreateLeadDto createLeadDto)
        {
            //check duplicateLead
             var existingLead = await _db.Leads.AnyAsync(x => x.Phone ==  createLeadDto.Phone);
            if(existingLead)
            {
                return (false, "Lead already exist");
            }

            var lead = new Lead
            {
                CustomerName = createLeadDto.CustomerName,
                Phone = createLeadDto.Phone,
                Email = createLeadDto.Email,
                StateId = createLeadDto.StateId,
                CityId = createLeadDto.CityId,
                ProjectId = createLeadDto.ProjectId,
                LeadSourceId = createLeadDto.LeadSourceId,
                CreatedDate = DateTime.Now
            };
            _db.Add(lead);
            await _db.SaveChangesAsync();
           return (true, "Lead created successfully");
        }

        public async Task<bool> DeleteLeadAsync(int id)
        {
            var lead = await _db.Leads.FindAsync(id);
            if (lead == null)
            {
                return false;
            }
            _db.Leads.Remove(lead);
            await _db.SaveChangesAsync();

            return true;
        }

        public async Task<List<LeadResponseDto>> GetAllLeadsAsync()
        {
            return await _db.Leads.Select(x => new LeadResponseDto
            {
                LeadId = x.LeadId,
                CustomerName = x.CustomerName,
                Phone = x.Phone,
                Email = x.Email,
                CreatedDate = x.CreatedDate
            }).ToListAsync();
        }

        public async Task<LeadResponseDto?> GetLeadByIdAsync(int id)
        {
            return await _db.Leads.Where(x => x.LeadId == id)
                .Select(x => new LeadResponseDto
                {
                    LeadId = x.LeadId,
                    CustomerName = x.CustomerName,
                    Phone = x.Phone,
                    Email = x.Email,
                    CreatedDate = x.CreatedDate
                }).FirstOrDefaultAsync();
        }
    }
}
