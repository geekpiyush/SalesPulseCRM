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
        public async Task<IActionResult> ManageMembers(int teamId)
        {
            var team = await _db.Teams
                .Include(t => t.Manager)
                .FirstOrDefaultAsync(t => t.TeamId == teamId);

            if (team == null)
            {
                return NotFound();
            }

            var allUsers = await _db.Users.ToListAsync();

            var existingMembers = await _db.TeamMembers
                .Where(x => x.TeamId == teamId)
                .Select(x => x.UserId)
                .ToListAsync();

            ViewBag.Team = team;
            ViewBag.Users = allUsers;
            ViewBag.Members = existingMembers; // 🔥 THIS WAS MISSING

            return View();
        }
    }
}
