using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SalesPulseCRM.Domain.Entities;
using SalesPulseCRM.Infrastructure.DB;

namespace SalesPulseCRM.WEB.Controllers
{
    public class TeamController : Controller
    {
        private readonly CrmDbContext _db;
        public TeamController(CrmDbContext crmDbContext)
        {
            _db = crmDbContext;
        }

        [HttpGet]
        public async Task<IActionResult> CreateTeamAsync()
        {

            ViewBag.Managers = await _db.Users
            .Where(x => x.Role == "Manager")
            .ToListAsync();

            var usedManagers = await _db.Teams
            .Select(x => x.ManagerId)
            .ToListAsync();

            ViewBag.UsedManagers = usedManagers;
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> CreateTeam(string teamName, int managerId)
        {
            if(string.IsNullOrWhiteSpace(teamName))
            {
                TempData["Error"] = "Team name is required";
                return RedirectToAction("CreateTeam");
            }

            if (managerId == 0)
            {
                TempData["Error"] = "Manager is required";
                return RedirectToAction("CreateTeam");
            }
            var exists = await _db.Teams
            .AnyAsync(x => x.TeamName == teamName);

            if (exists)
            {
                TempData["Error"] = "Team already exists";
                return RedirectToAction("CreateTeam");
            }

            var existingManager = await _db.Teams.AnyAsync(x => x.ManagerId == managerId);
            if(existingManager)
            {
                TempData["Error"] = "Manager already assign to another team";
                return RedirectToAction("CreateTeam");
            }

            var team = new Team
            {
                TeamName = teamName,
                ManagerId = managerId,

            };
            _db.Teams.Add(team);
            await _db.SaveChangesAsync();

            _db.TeamMembers.Add(new TeamMember
            {
                TeamId = team.TeamId,
                UserId = team.ManagerId
               
            });

            await _db.SaveChangesAsync();

            TempData["Success"] = "Team created successfully";

            return RedirectToAction("CreateTeam");
        }

        [HttpGet]
        public async Task<IActionResult> ViewTeam()
        {
            var teams = await _db.Teams
                .Select(t => new TeamViewModel
                {
                    TeamId = t.TeamId,
                    TeamName = t.TeamName,
                    ManagerName = _db.Users
                        .Where(u => u.UserId == t.ManagerId)
                        .Select(u => u.Name)
                        .FirstOrDefault(),

                    MemberCount = _db.TeamMembers
                        .Count(tm => tm.TeamId == t.TeamId)
                })
                .ToListAsync();

            return View(teams);
        }



        [HttpGet]
        public async Task<IActionResult> ManageMembers(int teamId)
        {
            var team = await _db.Teams
                .Include(t => t.Manager)
                .FirstOrDefaultAsync(t => t.TeamId == teamId);

            if (team == null)
            {
                return NotFound();
            }

            var allUsers = await _db.Users
            .Where(u => u.ManagerId == team.ManagerId || u.UserId == team.ManagerId)
            .ToListAsync();

            var existingMembers = await _db.TeamMembers
                .Where(x => x.TeamId == teamId)
                .Select(x => x.UserId)
                .ToListAsync();



            ViewBag.Team = team;
            ViewBag.Users = allUsers;
            ViewBag.Members = existingMembers; // 🔥 THIS WAS MISSING

            return View();
        }


        [HttpPost]
        public async Task<IActionResult> AddMember(int teamId, List<int> userIds)
        {
            var team = await _db.Teams.FindAsync(teamId);
            if (team == null)
            {
                TempData["Error"] = "Team not found";
                return RedirectToAction("ManageMembers", new { teamId });
            }

            if (userIds == null)
                userIds = new List<int>();

            // manager always included
            if (!userIds.Contains(team.ManagerId))
                userIds.Add(team.ManagerId);

            var existingMembers = await _db.TeamMembers
                .Where(x => x.TeamId == teamId)
                .ToListAsync();

            var existingIds = existingMembers.Select(x => x.UserId).ToList();

            // 🔥 REMOVE unchecked users
            var toRemove = existingMembers
                .Where(x => !userIds.Contains(x.UserId))
                .ToList();

            _db.TeamMembers.RemoveRange(toRemove);

            // 🔥 ADD new users only
            var toAdd = userIds
                .Where(id => !existingIds.Contains(id))
                .Select(id => new TeamMember
                {
                    TeamId = teamId,
                    UserId = id
                });

            await _db.TeamMembers.AddRangeAsync(toAdd);
            await _db.SaveChangesAsync();

            TempData["Success"] = "Team updated successfully";

            return RedirectToAction("ManageMembers", new { teamId });
        }
    }
}
