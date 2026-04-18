async function loadUnassignedLeads() {
    try {
        const response = await fetch('/api/leadapi/unassigned-leads');
        const data = await response.json();

        document.getElementById('totalUnassigned').innerText = data.totalUnassigned;
        document.getElementById('todayUnassigned').innerText = data.today;
        document.getElementById('oldUnassigned').innerText = data.onePlusDays;
    } catch (err) {
        console.log("unassigned Leads Error:", err);
    }
}
loadUnassignedLeads();