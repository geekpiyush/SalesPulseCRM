async function loadConvertedLeads() {
    const res = await fetch('/api/leadapi/converted-leads');
    const data = await res.json();

    document.getElementById('convertedTotal').innerText = data.total;
    document.getElementById('convertedToday').innerText = data.today;
    document.getElementById('convertedWeek').innerText = data.thisWeek;
    document.getElementById('convertedMonth').innerText = data.thisMonth;
}