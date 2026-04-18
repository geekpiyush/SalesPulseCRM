async function loadLeadFunnel() {
    try {
        const res = await fetch('/api/leadapi/lead-funnel');
        const data = await res.json();

        // 🔥 CENTER
        document.getElementById("totalActive").innerText = data.totalActive;

        document.getElementById("attemptedContact").innerText = data.attemptedContact;
        document.getElementById("needsFollowUp").innerText = data.needsFollowUp;
        document.getElementById("callbackScheduled").innerText = data.callbackScheduled;
        document.getElementById("onHold").innerText = data.onHold;
        document.getElementById("interested").innerText = data.interested;

        // 🔥 POSITIVE
        document.getElementById("contacted").innerText = data.contacted;
        document.getElementById("qualified").innerText = data.qualified;
        document.getElementById("converted").innerText = data.converted;

        // 🔥 NEGATIVE
        document.getElementById("noResponse").innerText = data.noResponse;
        document.getElementById("notInterested").innerText = data.notInterested;
        document.getElementById("lost").innerText = data.lost;

    } catch (err) {
        console.error("Lead Funnel Load Failed:", err);
    }
}