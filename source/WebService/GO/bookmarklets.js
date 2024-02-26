// minifier used: https://www.toptal.com/developers/javascript-minifier


// GO2NGU/loadouts
// javascript:fetch("http://localhost:8088/ngu/go2ngu/loadouts",{method:"POST",body:JSON.stringify(appState.savedequip)});
function go2ngu_loadouts2() {
    fetch("http://localhost:8088/ngu/go2ngu/loadouts",
        {
            method: "POST",
            body: JSON.stringify(appState.savedequip)
        });
}


// NGU2GO/equipped
// javascript:fetch("http://localhost:8088/ngu/ngu2go/equipped").then(e=>e.json()).then(e=>{let n=appState.savedequip;Object.assign(n.find(e=>"current"==e.name),e),appHandlers.handleSettings("savedequip",n)});
function ngu2go_equipped2() {
    fetch("http://localhost:8088/ngu/ngu2go/equipped")
        .then(resp => resp.json())
        .then(gear => {
            let data = appState.savedequip;
            let slot = data.find(sl => sl.name == "current");
            Object.assign(slot, gear);
            appHandlers.handleSettings('savedequip', data);
        });
}


// NGU2GO/augstats
// javascript:fetch("http://localhost:8088/ngu/NGU2GO/augstats").then(t=>t.json()).then(t=>{let s=appState.augstats;Object.assign(s,t),appHandlers.handleSettings("augstats",s)});
function ngu2go_augstats() {
    fetch("http://localhost:8088/ngu/NGU2GO/augstats")
        .then(resp => resp.json())
        .then(data => {
            let augStats = appState.augstats;
            Object.assign(augStats, data);
            appHandlers.handleSettings("augstats", augStats);
        });
}


// NGU2GO/ngustats
// javascript:fetch("http://localhost:8088/ngu/NGU2GO/ngustats").then(t=>t.json()).then(t=>{let s=appState.ngustats;Object.assign(s,t),appHandlers.handleSettings("ngustats",s)});
function ngu2go_ngustats() {
    fetch("http://localhost:8088/ngu/NGU2GO/ngustats")
        .then(resp => resp.json())
        .then(data => {
            let nguStats = appState.ngustats;
            Object.assign(nguStats, data);
            appHandlers.handleSettings("ngustats", nguStats);
        });
}


// NGU2GO/hacks
// javascript:fetch("http://localhost:8088/ngu/ngu2go/hacks").then(e=>e.json()).then(e=>{let a=appState.hackstats;a.rpow=e.rpow,a.rcap=e.rcap,a.hackspeed=e.hackspeed;for(let c=0;c<15;c++)a.hacks[c].goal=a.hacks[c].level=e.hacks[c].level,a.hacks[c].reducer=e.hacks[c].reducer;appHandlers.handleSettings("hackstats",a)});
function ngu2go_hacks2() {
    fetch("http://localhost:8088/ngu/ngu2go/hacks")
        .then(r => r.json())
        .then(o => {
            let h = appState.hackstats;
            h.rpow = o.rpow;
            h.rcap = o.rcap;
            h.hackspeed = o.hackspeed;
            for (let x = 0; x < 15; x++) {
                h.hacks[x].goal = h.hacks[x].level = o.hacks[x].level;
                h.hacks[x].reducer = o.hacks[x].reducer;
            }
            appHandlers.handleSettings('hackstats', h);
        });
}


// GO2NGU/hacks
// javascript:fetch("http://localhost:8088/ngu/go2ngu/hacks",{method:"POST",body:JSON.stringify(appState.hackstats.hacks.map(a=>a.goal))});
function go2ngu_hacks2() {
    fetch("http://localhost:8088/ngu/go2ngu/hacks", {
        method: "POST",
        body: JSON.stringify(appState.hackstats.hacks.map(h => h.goal))
    });
}
