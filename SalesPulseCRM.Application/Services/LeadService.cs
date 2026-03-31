using Microsoft.EntityFrameworkCore;
using SalesPulseCRM.Application.DTOs;
using SalesPulseCRM.Application.ServiceContracts;
using SalesPulseCRM.Domain.Entities;
using SalesPulseCRM.Domain.Enum;
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

        public async Task<List<LeadResponseDto>> GetAllLeadsAsync(int userId, string role)
        {
            IQueryable<Lead> query = _db.Leads;

            // 🔐 ROLE BASED FILTERING
            if (role == "Admin")
            {
                // no filter → full access
            }
            else if (role == "Manager")
            {
                var teamIds = await _db.Users
                    .Where(u => u.ManagerId == userId)
                    .Select(u => u.UserId)
                    .ToListAsync();

                teamIds.Add(userId); // include manager's own leads

                query = query.Where(l =>
                    (l.CurrentAssignedTo != null && teamIds.Contains(l.CurrentAssignedTo.Value))
                    
                );
            }
            else if (role == "Employee")
            {
                query = query.Where(l => l.CurrentAssignedTo == userId);
            }

            // 🔥 FINAL PROJECTION
            return await (from l in query

                          join u in _db.Users
                          on l.CurrentAssignedTo equals u.UserId into userGroup
                          from u in userGroup.DefaultIfEmpty()

                          select new LeadResponseDto
                          {
                              LeadId = l.LeadId,
                              CustomerName = l.CustomerName,
                              Phone = l.Phone,
                              Email = l.Email,

                              LeadType = l.LeadStatus != null ? l.LeadStatus.StatusName : null,

                              ProjectName = l.Project != null ? l.Project.ProjectName : null,
                              CityName = l.City != null ? l.City.CityName : null,
                              StateName = l.State != null ? l.State.StateName : null,
                              SourceName = l.LeadSource != null ? l.LeadSource.SourceName : null,

                              CurrentAssignTo = l.CurrentAssignedTo,
                              CurrentAssignToName = u != null ? u.Name : "Unassigned",

                              CreatedDate = l.CreatedDate
                          })
                          .OrderByDescending(l => l.CreatedDate) // 🔥 important for UX
                          .ToListAsync();
        }
        public async Task<LeadEditViewModel?> GetLeadByIdAsync(int id)
        {
            var lead = await _db.Leads
               .Include(l => l.Project)
                .Include(l => l.City)
                .Include(l => l.LeadStatus)
                .Include(l => l.Followups)
                .Include(l => l.Notes)
                .FirstOrDefaultAsync(l => l.LeadId == id);

            if (lead == null) return null;

            var user = await _db.Users
                .Where(u => u.UserId == lead.CurrentAssignedTo)
                .Select(u => u.Name)
                .FirstOrDefaultAsync();

            // STEP 1: Fetch separately (DB level)
            var notes = await (from n in _db.LeadNotes
                               join u in _db.Users on n.UserId equals u.UserId into ug
                               from u in ug.DefaultIfEmpty()
                               where n.LeadId == id
                               select new TimelineItemDto
                               {
                                   Type = "Note",
                                   Title = "Note Added",
                                   Description = n.NoteText,
                                   Date = n.CreatedDate,
                                   UserName = u != null ? u.Name : "System"
                               }).ToListAsync();

                    var now = DateTime.Now;

                    var followupsRaw = await (from f in _db.Followups
                                              join u in _db.Users on f.UserId equals u.UserId into ug
                                              from u in ug.DefaultIfEmpty()
                                              where f.LeadId == id
                                              select new
                                              {
                                                  f.Status,
                                                  f.FollowupDateTime,
                                                  UserName = u != null ? u.Name : "System"
                                              }).ToListAsync();

                    var followups = followupsRaw.Select(f => new TimelineItemDto
                    {
                        Type = "Followup",
                        Title = "Follow-up",

                        Description =
                            f.Status == FollowupStatus.Completed
                                ? $"✅ Completed on {f.FollowupDateTime:dd MMM yyyy hh:mm tt}"
                                : f.FollowupDateTime < now
                                    ? $"❌ Missed on {f.FollowupDateTime:dd MMM yyyy hh:mm tt}"
                                    : $"⏳ Scheduled for {f.FollowupDateTime:dd MMM yyyy hh:mm tt}",

                        Date = f.FollowupDateTime,
                        UserName = f.UserName
                    }).ToList();


            var assignments = await (from a in _db.LeadAssignments
                                     join u in _db.Users on a.AssignedTo equals u.UserId into ug
                                     from u in ug.DefaultIfEmpty()
                                     where a.LeadId == id
                                     select new TimelineItemDto
                                     {
                                         Type = "Assignment",
                                         Title = "Lead Assigned",
                                         Description = "Assigned to " + (u != null ? u.Name : "Unknown"),
                                         Date = a.AssignedDate,
                                         UserName = u != null ? u.Name : "System"
                                     }).ToListAsync();


            // STEP 2: Combine in memory (SAFE)
            var timeline = notes
                .Concat(followups)
                .Concat(assignments)
                .OrderByDescending(x => x.Date)
                .ToList();

            return new LeadEditViewModel
            {
                // DISPLAY
                CustomerName = lead.CustomerName,
                Phone = lead.Phone,
                Email = lead.Email,
                ProjectName = lead.Project?.ProjectName,
                CityName = lead.City?.CityName,
                LeadType = lead.LeadStatus?.StatusName,
                CurrentAssignToName = user ?? "Unassigned",
                CreatedDate = lead.CreatedDate,

                // 🔥 THIS WAS YOUR MAIN BUG
                Timeline = timeline,

                // UPDATE
                Lead = new UpdateLeadDto
                {
                    LeadId = lead.LeadId,
                    LeadStatusId = lead.LeadStatusId,
                    ProjectId = lead.ProjectId,

                    Budget = lead.Budget,
                    NextAction = lead.NextAction,
                    CustomerInterest = lead.CustomerInterest,

                    MeetingStatus = lead.MeetingStatus,
                    MeetingDateTime = lead.MeetingDateTime,

                    FollowupDate = lead.Followups?
                        .OrderByDescending(f => f.FollowupDateTime)
                        .Select(f => (DateTime?)f.FollowupDateTime)
                        .FirstOrDefault(),

                    NoteText = lead.Notes?
                        .OrderByDescending(n => n.CreatedDate)
                        .Select(n => n.NoteText)
                        .FirstOrDefault()
                }
            };
        }

        public async Task<List<TimelineItemDto>> GetTimeline(int leadId)
        {
            var now = DateTime.Now;

            // NOTES
            var notes = await (from n in _db.LeadNotes
                               join u in _db.Users on n.UserId equals u.UserId into ug
                               from u in ug.DefaultIfEmpty()
                               where n.LeadId == leadId
                               select new TimelineItemDto
                               {
                                   Type = "Note",
                                   Title = "Note Added",
                                   Description = n.NoteText,
                                   Date = n.CreatedDate,
                                   UserName = u != null ? u.Name : "System"
                               }).ToListAsync();

            // FOLLOWUPS
            var followupsRaw = await (from f in _db.Followups
                                      join u in _db.Users on f.UserId equals u.UserId into ug
                                      from u in ug.DefaultIfEmpty()
                                      where f.LeadId == leadId
                                      select new
                                      {
                                          f.Status,
                                          f.FollowupDateTime,
                                          UserName = u != null ? u.Name : "System"
                                      }).ToListAsync();

            var followups = followupsRaw.Select(f => new TimelineItemDto
            {
                Type = "Followup",
                Title = "Follow-up",
                Description =
                    f.Status == FollowupStatus.Completed
                        ? $"✅ Completed on {f.FollowupDateTime:dd MMM yyyy hh:mm tt}"
                        : f.FollowupDateTime < now
                            ? $"❌ Missed on {f.FollowupDateTime:dd MMM yyyy hh:mm tt}"
                            : $"⏳ Scheduled for {f.FollowupDateTime:dd MMM yyyy hh:mm tt}",
                Date = f.FollowupDateTime,
                UserName = f.UserName
            }).ToList();

            // ASSIGNMENTS
            var assignments = await (from a in _db.LeadAssignments
                                     join u in _db.Users on a.AssignedTo equals u.UserId into ug
                                     from u in ug.DefaultIfEmpty()
                                     where a.LeadId == leadId
                                     select new TimelineItemDto
                                     {
                                         Type = "Assignment",
                                         Title = "Lead Assigned",
                                         Description = "Assigned to " + (u != null ? u.Name : "Unknown"),
                                         Date = a.AssignedDate,
                                         UserName = u != null ? u.Name : "System"
                                     }).ToListAsync();

            // FINAL MERGE
            return notes
                .Concat(followups)
                .Concat(assignments)
                .OrderByDescending(x => x.Date)
                .ToList();
        }

        public async Task<bool> UpdateLeadAsync(LeadEditViewModel model, int userId)
        {
            var lead = await _db.Leads
                .Include(l => l.Followups)
                .Include(l => l.Notes)
                .FirstOrDefaultAsync(l => l.LeadId == model.Lead.LeadId);

            if (lead == null)
                return false;

            // 🔥 UPDATE MAIN LEAD
            lead.LeadStatusId = model.Lead.LeadStatusId;
            lead.ProjectId = model.Lead.ProjectId;
            lead.Budget = model.Lead.Budget;
            lead.NextAction = model.Lead.NextAction;
            lead.CustomerInterest = model.Lead.CustomerInterest;
            lead.MeetingStatus = model.Lead.MeetingStatus;
            lead.MeetingDateTime = model.Lead.MeetingDateTime;

            // 🔥 NOTE ADD
            if (!string.IsNullOrWhiteSpace(model.Lead.NoteText))
            {
                _db.LeadNotes.Add(new LeadNote
                {
                    LeadId = lead.LeadId,
                    NoteText = model.Lead.NoteText,
                    CreatedDate = DateTime.Now,
                    UserId = userId
                });
            }

            // 🔥 FOLLOWUP ADD
            if (model.Lead.FollowupDate.HasValue)
            {
                _db.Followups.Add(new Followup
                {
                    LeadId = lead.LeadId,
                    FollowupDateTime = model.Lead.FollowupDate.Value,
                    Status = FollowupStatus.Pending,
                    UserId = userId
                });
            }

            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<TodayTaskDto> GetTodayTasksAsync()
        {
            var result = new TodayTaskDto();

            using (var conn = _db.Database.GetDbConnection())
            {
                await conn.OpenAsync();

                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                SELECT 
                    COUNT(CASE 
                        WHEN CAST(CurrentAssignedDate AS DATE) = CAST(GETDATE() AS DATE)
                        AND NextAction IS NOT NULL 
                        THEN 1 END) AS Followups,

                    COUNT(CASE 
                        WHEN CAST(CurrentAssignedDate AS DATE) < CAST(GETDATE() AS DATE)
                        AND NextAction IS NOT NULL 
                        THEN 1 END) AS Missed,

                    COUNT(CASE 
                        WHEN CAST(MeetingDateTime AS DATE) = CAST(GETDATE() AS DATE)
                        AND MeetingStatus IS NOT NULL
                        THEN 1 END) AS Meetings

                FROM Leads
                WHERE IsDeleted = 0
            ";

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            result.Followups = reader.GetInt32(0);
                            result.Missed = reader.GetInt32(1);
                            result.Meetings = reader.GetInt32(2);
                        }
                    }
                }
            }

            return result;
        }
    }
}
