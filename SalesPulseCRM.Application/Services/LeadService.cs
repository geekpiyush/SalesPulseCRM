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

            if (role == "Admin")
            {
                // ✅ Full access (no filter)
            }
            else if (role == "Manager")
            {
                var teamIds = await _db.Users
                    .Where(u => u.ManagerId == userId)
                    .Select(u => u.UserId)
                    .ToListAsync();

                teamIds.Add(userId); // include manager

                query = query.Where(l =>
                    l.CurrentAssignedTo != null &&
                    teamIds.Contains(l.CurrentAssignedTo.Value)
                );
            }
            else if (role == "Employee")
            {
                query = query.Where(l => l.CurrentAssignedTo == userId);
            }

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
                          .OrderByDescending(l => l.CreatedDate)
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

                                     join toUser in _db.Users
                                     on a.AssignedTo equals toUser.UserId into toGroup
                                     from toUser in toGroup.DefaultIfEmpty()

                                     join byUser in _db.Users
                                     on a.AssignedBy equals byUser.UserId into byGroup
                                     from byUser in byGroup.DefaultIfEmpty()

                                     where a.LeadId == leadId

                                     select new TimelineItemDto
                                     {
                                         Type = "Assignment",
                                         Title = "Lead Assigned",

                                         Description = $"Assigned to {toUser.Name} by {byUser.Name}",

                                         Date = a.AssignedDate,

                                         UserName = byUser != null ? byUser.Name : "System"
                                     })
                           .ToListAsync();

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
            lead.LastUpdatedDate  = DateTime.Now;

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

        //public async Task<List<UserTaskDto>> GetTodayTasksAsync(int userId, string role)
        //{
        //    var result = new List<UserTaskDto>();

        //    using (var conn = _db.Database.GetDbConnection())
        //    {
        //        await conn.OpenAsync();

        //        using (var cmd = conn.CreateCommand())
        //        {
        //            cmd.CommandText = @"
        //                SELECT 
        //                    u.UserId,
        //                    u.Name,
        //                    u.Role,

        //                    -- 🔥 FOLLOWUPS (Today)
        //                    COUNT(CASE 
        //                        WHEN f.FollowupDateTime >= CAST(GETDATE() AS DATE)
        //                        AND f.FollowupDateTime < DATEADD(DAY, 1, CAST(GETDATE() AS DATE))
        //                        THEN 1 END) AS Followups,

        //                    -- 🔥 MISSED (Overdue)
        //                    COUNT(CASE 
        //                        WHEN f.FollowupDateTime < CAST(GETDATE() AS DATE)
        //                        THEN 1 END) AS Missed,

        //                    -- 🔥 MEETINGS (Today)
        //                    COUNT(CASE 
        //                        WHEN l.MeetingDateTime >= CAST(GETDATE() AS DATE)
        //                        AND l.MeetingDateTime < DATEADD(DAY, 1, CAST(GETDATE() AS DATE))
        //                        THEN 1 END) AS Meetings,

        //                    COUNT(DISTINCT l.LeadId) AS TotalLeads

        //                FROM Users u

        //                LEFT JOIN Leads l 
        //                    ON l.CurrentAssignedTo = u.UserId
        //                    AND l.IsDeleted = 0

        //                LEFT JOIN Followups f 
        //                    ON f.LeadId = l.LeadId
        //                    AND f.UserId = u.UserId

        //                WHERE
        //                (
        //                    @Role = 'Admin'
        //                    OR (@Role = 'Manager' AND (u.ManagerId = @UserId OR u.UserId = @UserId))
        //                    OR (@Role = 'Employee' AND u.UserId = @UserId)
        //                )

        //                GROUP BY u.UserId, u.Name, u.Role
        //                ORDER BY u.Name
        //                ";

        //            var p1 = cmd.CreateParameter();
        //            p1.ParameterName = "@UserId";
        //            p1.Value = userId;
        //            cmd.Parameters.Add(p1);

        //            var p2 = cmd.CreateParameter();
        //            p2.ParameterName = "@Role";
        //            p2.Value = role;
        //            cmd.Parameters.Add(p2);

        //            using (var reader = await cmd.ExecuteReaderAsync())
        //            {
        //                while (await reader.ReadAsync())
        //                {
        //                    result.Add(new UserTaskDto
        //                    {
        //                        UserId = reader["UserId"] == DBNull.Value ? 0 : Convert.ToInt32(reader["UserId"]),
        //                        Name = reader["Name"]?.ToString(),
        //                        Role = reader["Role"]?.ToString(),

        //                        Followups = reader["Followups"] == DBNull.Value ? 0 : Convert.ToInt32(reader["Followups"]),
        //                        Missed = reader["Missed"] == DBNull.Value ? 0 : Convert.ToInt32(reader["Missed"]),
        //                        Meetings = reader["Meetings"] == DBNull.Value ? 0 : Convert.ToInt32(reader["Meetings"]),

        //                        TotalLeads = reader["TotalLeads"] == DBNull.Value ? 0 : Convert.ToInt32(reader["TotalLeads"])
        //                    });
        //                }
        //            }
        //        }
        //    }

        //    return result;
        //}

        public async Task<TotalLeadsDto> GetTotalLeadCount(int userId, string role)
        {
            var result = new TotalLeadsDto();

            using (var conn = _db.Database.GetDbConnection())
            {
                await conn.OpenAsync();

                using (var cmd = conn.CreateCommand())
                {
                    string roleFilter = "";

                    if (role == "Employee")
                    {
                        roleFilter = "AND l.CurrentAssignedTo = @UserId";
                    }
                    else if (role == "Manager")
                    {
                        // Manager: own + team
                        roleFilter = @"
                    AND l.CurrentAssignedTo IN (
                        SELECT UserId FROM Users WHERE ManagerId = @UserId
                        UNION
                        SELECT @UserId
                    )";
                    }
                    // Admin = no filter

                    cmd.CommandText = $@"
                SELECT 
                    COUNT(CASE 
                        WHEN ls.StatusName NOT IN (
                            'Blacklisted','Lost','Not Interested',
                            'Invalid Lead','Duplicate Lead',
                            'Do Not Contact','Unqualified'
                        )
                        THEN 1 
                    END) AS ActiveLeads,

                    COUNT(CASE 
                        WHEN ls.StatusName IN (
                            'Blacklisted','Lost','Not Interested',
                            'Invalid Lead','Duplicate Lead',
                            'Do Not Contact','Unqualified'
                        )
                        THEN 1 
                    END) AS DeadLeads

                FROM Leads l
                JOIN LeadStatus ls ON l.LeadStatusId = ls.LeadStatusId
                WHERE l.IsDeleted = 0
                {roleFilter};
            ";

                    var param = cmd.CreateParameter();
                    param.ParameterName = "@UserId";
                    param.Value = userId;
                    cmd.Parameters.Add(param);

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            result.TotalActiveLeads = reader.GetInt32(0);
                            result.LostLeads = reader.GetInt32(1);
                        }
                    }
                }
            }

            return result;
        }

        public async Task<UnassignedLeadDto> GetTotalUnassignedLead()
        {
            var result = new UnassignedLeadDto();

            using (var conn = _db.Database.GetDbConnection())
            {
                await conn.OpenAsync();

                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                SELECT 
                    COUNT(*) AS TotalUnassigned,

                    COUNT(CASE 
                        WHEN CAST(CreatedDate AS DATE) = CAST(GETDATE() AS DATE) 
                        THEN 1 END) AS Today,

                    COUNT(CASE 
                        WHEN CAST(CreatedDate AS DATE) < CAST(GETDATE() AS DATE) 
                        THEN 1 END) AS OnePlusDay

                FROM Leads
                WHERE 
                    IsDeleted = 0
                    AND (CurrentAssignedTo IS NULL OR CurrentAssignedTo = 0)
            ";

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            result.TotalUnassigned = Convert.ToInt32(reader["TotalUnassigned"]);
                            result.Today = Convert.ToInt32(reader["Today"]);
                            result.OnePlusDays = Convert.ToInt32(reader["OnePlusDay"]);
                        }
                    }
                }
            }

            return result;
        }
        public async Task<ConvertedStatsDto> GetConvertedLeads(int userId, string role)
        {
            var result = new ConvertedStatsDto();

            using (var conn = _db.Database.GetDbConnection())
            {
                await conn.OpenAsync();

                using (var cmd = conn.CreateCommand())
                {
                    var query = @"
                SELECT 
                    COUNT(*) AS TotalConverted,

                    COUNT(CASE 
                        WHEN CAST(LastUpdatedDate AS DATE) = CAST(GETDATE() AS DATE)
                        THEN 1 END) AS Today,

                    COUNT(CASE 
                        WHEN DATEPART(WEEK, LastUpdatedDate) = DATEPART(WEEK, GETDATE())
                             AND YEAR(LastUpdatedDate) = YEAR(GETDATE())
                        THEN 1 END) AS ThisWeek,

                    COUNT(CASE 
                        WHEN MONTH(LastUpdatedDate) = MONTH(GETDATE())
                             AND YEAR(LastUpdatedDate) = YEAR(GETDATE())
                        THEN 1 END) AS ThisMonth

                FROM Leads
                WHERE 
                    IsDeleted = 0
                    AND LeadStatusId = 18
            ";


                    if (role == "Employee")
                    {
                        query += " AND CurrentAssignedTo = @UserId";
                    }
                    else if (role == "Manager")
                    {
                        query += @" AND CurrentAssignedTo IN (
                                SELECT UserId FROM Users WHERE ManagerId = @UserId
                                UNION SELECT @UserId
                            )";
                    }

                    cmd.CommandText = query;

                    var param = cmd.CreateParameter();
                    param.ParameterName = "@UserId";
                    param.Value = userId;
                    cmd.Parameters.Add(param);

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            result.Total = Convert.ToInt32(reader["TotalConverted"]);
                            result.Today = Convert.ToInt32(reader["Today"]);
                            result.ThisWeek = Convert.ToInt32(reader["ThisWeek"]);
                            result.ThisMonth = Convert.ToInt32(reader["ThisMonth"]);
                        }
                    }
                }
            }

            return result;
        }

        public async Task<LeadFunnelDto> GetLeadFunnelAsync(int userId, string role)
        {
            var result = new LeadFunnelDto();

            using (var conn = _db.Database.GetDbConnection())
            {
                await conn.OpenAsync();

                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                SELECT 
                    COUNT(*) AS TotalActive,

                    COUNT(CASE WHEN ls.StatusName = 'Attempted Contact' THEN 1 END) AS AttemptedContact,
                    COUNT(CASE WHEN ls.StatusName = 'Needs Follow-up' THEN 1 END) AS NeedsFollowUp,
                    COUNT(CASE WHEN ls.StatusName = 'Callback Scheduled' THEN 1 END) AS CallbackScheduled,
                    COUNT(CASE WHEN ls.StatusName = 'On Hold' THEN 1 END) AS OnHold,
                    COUNT(CASE WHEN ls.StatusName = 'Interested' THEN 1 END) AS Interested,

                    COUNT(CASE WHEN ls.StatusName = 'Contacted' THEN 1 END) AS Contacted,
                    COUNT(CASE WHEN ls.StatusName = 'Qualified' THEN 1 END) AS Qualified,
                    COUNT(CASE WHEN ls.StatusName = 'Converted' THEN 1 END) AS Converted,

                    COUNT(CASE WHEN ls.StatusName = 'No Response' THEN 1 END) AS NoResponse,
                    COUNT(CASE WHEN ls.StatusName = 'Not Interested' THEN 1 END) AS NotInterested,
                    COUNT(CASE WHEN ls.StatusName = 'Lost' THEN 1 END) AS Lost

                FROM Leads l
                LEFT JOIN LeadStatus ls ON ls.LeadStatusId = l.LeadStatusId

                WHERE 
                    l.IsDeleted = 0

                    AND (
                        @Role = 'Admin'

                        OR (
                            @Role = 'Manager' 
                            AND (
                                l.CurrentAssignedTo = @UserId
                                OR l.CurrentAssignedTo IN (
                                    SELECT UserId FROM Users WHERE ManagerId = @UserId
                                )
                            )
                        )

                        OR (
                            @Role = 'Employee' 
                            AND l.CurrentAssignedTo = @UserId
                        )
                    )
            ";

                    var p1 = cmd.CreateParameter();
                    p1.ParameterName = "@UserId";
                    p1.Value = userId;
                    cmd.Parameters.Add(p1);

                    var p2 = cmd.CreateParameter();
                    p2.ParameterName = "@Role";
                    p2.Value = role;
                    cmd.Parameters.Add(p2);

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            result.TotalActive = Convert.ToInt32(reader["TotalActive"]);

                            result.AttemptedContact = Convert.ToInt32(reader["AttemptedContact"]);
                            result.NeedsFollowUp = Convert.ToInt32(reader["NeedsFollowUp"]);
                            result.CallbackScheduled = Convert.ToInt32(reader["CallbackScheduled"]);
                            result.OnHold = Convert.ToInt32(reader["OnHold"]);
                            result.Interested = Convert.ToInt32(reader["Interested"]);

                            result.Contacted = Convert.ToInt32(reader["Contacted"]);
                            result.Qualified = Convert.ToInt32(reader["Qualified"]);
                            result.Converted = Convert.ToInt32(reader["Converted"]);

                            result.NoResponse = Convert.ToInt32(reader["NoResponse"]);
                            result.NotInterested = Convert.ToInt32(reader["NotInterested"]);
                            result.Lost = Convert.ToInt32(reader["Lost"]);
                        }
                    }
                }
            }

            return result;
        }

        public Task<List<UserTaskDto>> GetTodayTasksAsync(int userId, string role)
        {
            throw new NotImplementedException();
        }
    }
}
