async function loadTimeline() {
    const res = await fetch('/Lead/GetTimelinePartial?leadId=0');
    const html = await res.text();
    document.getElementById('dashboardTimeline').innerHTML = html;
}
loadTimeline();