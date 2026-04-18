async function loadLeadsCount() {
    try {
        const res = await fetch('/api/leadapi/leads-count');
        const data = await res.json();


        document.getElementById("activeLeads").innerText = data.totalActiveLeads;
        document.getElementById("deadLeads").innerText = data.lostLeads;

    } catch (err) {
        console.error("Error loading leads count:", err);
    }
}