using System.Text.Json;
using System.Text.Json.Serialization;
using ImanSoftware.DepLens.Abstractions.Models;
using ImanSoftware.DepLens.Abstractions.Services;
using ImanSoftware.Outcomes;

namespace ImanSoftware.DepLens.Core.Implementation;

internal sealed class HtmlGraphReportService : IHtmlGraphReportService
{
    public async Task<Outcome<string>> GenerateAsync(
        IReadOnlyList<ProjectDependencyReport> reports,
        string outputDirectory,
        string fileName = "dependency-graph.html")
    {
        try
        {
            var json = JsonSerializer.Serialize(reports, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                Converters = { new JsonStringEnumConverter() } 
            });

            var html = HtmlTemplate.Replace("__REPORT_DATA__", json);

            Directory.CreateDirectory(outputDirectory);
            var outputPath = Path.Combine(outputDirectory, fileName);
            await File.WriteAllTextAsync(outputPath, html);

            return Outcome.Successful(outputPath);
        }
        catch (Exception ex)
        {
            return Outcome.Failure<string>(new OutcomeError(
                $"Failed to generate HTML report: {ex.Message}",
                "HTML_REPORT_FAILED",
                OutcomeErrorType.Failure));
        }
    }

    private const string HtmlTemplate = """
<!DOCTYPE html>
<html lang="en">
<head>
<meta charset="UTF-8">
<meta name="viewport" content="width=device-width, initial-scale=1, viewport-fit=cover">
<title>DepLens — Dependency Map</title>
<link rel="preconnect" href="https://fonts.googleapis.com">
<link rel="preconnect" href="https://fonts.gstatic.com" crossorigin>
<link href="https://fonts.googleapis.com/css2?family=JetBrains+Mono:wght@400;500;600&family=Inter:wght@400;500;600;700&display=swap" rel="stylesheet">
<script src="https://cdnjs.cloudflare.com/ajax/libs/d3/7.9.0/d3.min.js"></script>
<style>
  :root {
    --bg: #11151a; --surface: #1a1f26; --ink: #e7eaee; --ink-soft: #8b95a1; --line: #2a3038;
    --project: #5fb8ae; --project-glow: #8fd8cf; --package: #4a5560; --package-border: #6b7684;
    --accent: #e3a15c; --accent-soft: #3a2f1f; --link: #5b6673; --panel-w: 300px;
  }
  * { box-sizing: border-box; }
  html, body { height: 100%; margin: 0; background: var(--bg); color: var(--ink);
    font-family: 'Inter', 'Segoe UI', sans-serif; overflow: hidden; }
  #app { display: flex; flex-direction: column; height: 100%; }

  header { padding: 12px 18px; background: var(--surface); border-bottom: 1px solid var(--line);
    display: flex; align-items: center; gap: 14px; flex-wrap: wrap; z-index: 6; }
  h1 { font-size: 15px; font-weight: 700; margin: 0; white-space: nowrap; color: var(--ink); }
  h1 span { color: var(--ink-soft); font-weight: 500; font-size: 12px; display: block; margin-top: 2px; }

  .view-toggle { display: flex; border: 1px solid var(--line); border-radius: 8px; overflow: hidden; }
  .view-toggle button { border: none; background: var(--bg); color: var(--ink-soft); padding: 7px 12px;
    font-size: 12px; font-family: inherit; cursor: pointer; }
  .view-toggle button.active { background: var(--project); color: #0d1113; font-weight: 600; }

  #search { flex: 1; min-width: 140px; max-width: 260px; padding: 7px 12px; border-radius: 8px;
    border: 1px solid var(--line); background: var(--bg); color: var(--ink); font-family: inherit; font-size: 13px; }
  #search::placeholder { color: var(--ink-soft); }

  .legend { display: flex; gap: 14px; font-size: 11.5px; color: var(--ink-soft); margin-inline-start: auto; }
  .legend-item { display: flex; align-items: center; gap: 5px; }
  .dot { width: 9px; height: 9px; border-radius: 50%; }
  .dot.proj { background: var(--project); }
  .dot.pkg { background: var(--package); border: 1px solid var(--package-border); }
  .dot.arrow { width: 14px; height: 2px; background: var(--link); }

  .body-row { flex: 1; display: flex; min-height: 0; position: relative; }

  #sidebar { position: relative; width: var(--panel-w); flex-shrink: 0; background: var(--surface);
    border-inline-end: 1px solid var(--line); display: flex; flex-direction: column;
    transition: margin-inline-start .2s ease; overflow: hidden; }
  #app.sidebar-collapsed #sidebar { margin-inline-start: calc(-1 * var(--panel-w)); }
  #sidebar-resizer { position: absolute; top: 0; bottom: 0; inset-inline-end: -3px; width: 6px;
    cursor: col-resize; z-index: 8; }
  #sidebar-resizer:hover, #sidebar-resizer.active { background: var(--project-glow); opacity: .45; }
  #sidebar-toggle { position: absolute; top: 10px; inset-inline-start: 10px; z-index: 7; width: 30px; height: 30px;
    border-radius: 8px; border: 1px solid var(--line); background: var(--surface); color: var(--ink); cursor: pointer; font-size: 14px; }
  #app:not(.sidebar-collapsed) #sidebar-toggle { display: none; }

  .sidebar-head { padding: 10px 12px; border-bottom: 1px solid var(--line); display: flex; align-items: center;
    justify-content: space-between; }
  .sidebar-head b { font-size: 12.5px; color: var(--ink); }
  .sidebar-head button { border: none; background: none; color: var(--ink-soft); cursor: pointer; font-size: 13px; }

  #tree { flex: 1; overflow-y: auto; padding: 6px; font-size: 12.5px; }
  .tree-row { display: flex; align-items: center; gap: 6px; padding: 5px 8px; border-radius: 6px; cursor: pointer;
    white-space: nowrap; color: var(--ink); }
  .tree-row:not(.leaf) { overflow: hidden; text-overflow: ellipsis; }
  .tree-row:hover { background: var(--bg); }
  .tree-row.group { color: var(--ink-soft); font-size: 11px; text-transform: uppercase; letter-spacing: .03em; cursor: default; }
  .tree-row.group:hover { background: none; }
  .tree-row .chev { width: 12px; flex-shrink: 0; font-size: 9px; color: var(--ink-soft); transition: transform .15s; }
  .tree-row.leaf { padding-inline-start: 14px; color: var(--ink-soft); }
  .tree-row.leaf .name { flex: 1; min-width: 0; overflow: hidden; text-overflow: ellipsis; white-space: nowrap; }
  .tree-row.leaf .ver { flex-shrink: 0; color: var(--accent); font-family: 'JetBrains Mono', monospace; font-size: 10.5px; }
  .tree-row.selected { background: var(--accent-soft); }
  li.collapsed > .tree-children { display: none; }
  li.collapsed > .tree-row .chev { transform: rotate(-90deg); }
  ul.tree, ul.tree-children { list-style: none; margin: 0; padding-inline-start: 14px; }
  ul.tree { padding-inline-start: 0; }
  .tree-row.flash { animation: flash 1s ease 2; }
  @keyframes flash { 0%,100% { background: none; } 50% { background: var(--accent-soft); } }

  #details { border-top: 1px solid var(--line); padding: 12px; font-size: 12.5px; max-height: 42%; overflow-y: auto; }
  #details h3 { margin: 0 0 4px; font-size: 13px; font-family: 'JetBrains Mono', monospace; color: var(--ink); word-break: break-word; }
  #details .path { color: var(--ink-soft); font-size: 11px; word-break: break-all; margin-bottom: 8px; }
  #details .row { display: flex; justify-content: space-between; gap: 8px; padding: 3px 0; border-bottom: 1px dashed var(--line); }
  #details .row .n { font-family: 'JetBrains Mono', monospace; color: var(--ink); word-break: break-word; }
  #details .row .v { color: var(--accent); font-family: 'JetBrains Mono', monospace; font-size: 11px; flex-shrink: 0; }
  #details .empty { color: var(--ink-soft); font-style: italic; }
  #details .warn { color: #e3785c; font-size: 11px; margin-top: 6px; }

  main { position: relative; flex: 1; min-height: 0; background: var(--bg); }
  svg { width: 100%; height: 100%; display: block; cursor: grab; }
  .cluster-hull { fill-opacity: 1; stroke-width: 1.6; }
  .cluster-label { font-size: 11px; font-weight: 700; font-family: 'Inter', sans-serif; }

  .link { stroke: var(--link); stroke-width: 1.6; opacity: 1; transition: opacity .2s, stroke .15s, stroke-width .15s;
    marker-end: url(#arrow); }
  .link.dim { opacity: .08; }
  .link.hi { stroke: var(--accent); stroke-width: 2.4; }
  .node circle { stroke: var(--surface); stroke-width: 2px; transition: opacity .2s, filter .2s; cursor: pointer; }
  .node.external circle { stroke-dasharray: 3 2; }
  .node.dim { opacity: .15; }
  .node text { font-family: 'JetBrains Mono', monospace; font-size: 10px; fill: var(--ink); pointer-events: none;
    paint-order: stroke; stroke: var(--bg); stroke-width: 3px; }
  .node.dim text { opacity: .12; }
  .node.hi circle { filter: drop-shadow(0 0 7px var(--project-glow)); }

  #tooltip { position: fixed; pointer-events: none; background: var(--surface); border: 1px solid var(--line);
    border-radius: 10px; padding: 9px 13px; font-size: 12px; box-shadow: 0 6px 20px rgba(0,0,0,.4);
    max-width: 260px; opacity: 0; transition: opacity .12s; z-index: 20; word-break: break-word; }
  #tooltip.show { opacity: 1; }
  #tooltip .tt-name { font-weight: 700; font-family: 'JetBrains Mono', monospace; color: var(--ink); }
  #tooltip .tt-meta { color: var(--ink-soft); margin-top: 3px; display: flex; justify-content: space-between; gap: 10px; }
  #tooltip .tt-version { color: var(--accent); font-family: 'JetBrains Mono', monospace; flex-shrink: 0; }
  #tooltip .tt-solutions { color: var(--ink-soft); margin-top: 3px; font-size: 11px; }

  @media (max-width: 720px) {
    :root { --panel-w: 80vw; }
    .legend { display: none; }
    #sidebar-resizer { display: none; }
  }
</style>
</head>
<body>
<div id="app">
  <header>
    <h1>Dependency Map<span id="subtitle"></span></h1>
    <div class="view-toggle">
      <button id="view-pkg" class="active">Dependency Graph</button>
      <button id="view-arch">Solution Architecture</button>
    </div>
    <input id="search" type="text" placeholder="Search project or package…" autocomplete="off">
    <div class="legend">
      <div class="legend-item"><span class="dot proj"></span> Project</div>
      <div class="legend-item"><span class="dot pkg"></span> NuGet package</div>
      <div class="legend-item"><span class="dot arrow"></span> depends on →</div>
    </div>
  </header>
  <div class="body-row">
    <button id="sidebar-toggle" title="Show sidebar">☰</button>
    <aside id="sidebar">
      <div class="sidebar-head"><b>Analysis Tree</b><button id="sidebar-close" title="Collapse">⟨⟨</button></div>
      <div id="tree"></div>
      <div id="details"><div class="empty">Click a node or a tree item to see details.</div></div>
      <div id="sidebar-resizer" title="Drag to resize"></div>
    </aside>
    <main>
      <svg id="graph">
        <defs>
          <marker id="arrow" viewBox="0 0 10 10" refX="17" refY="5" markerWidth="6.5" markerHeight="6.5" orient="auto-start-reverse">
            <path d="M0,0 L10,5 L0,10 z" fill="context-stroke"></path>
          </marker>
        </defs>
      </svg>
    </main>
  </div>
</div>
<div id="tooltip"></div>

<script>
const reports = __REPORT_DATA__;

function baseName(p) { return (p || "").split(/[\\/]/).pop().replace(/\.(csproj|sln|slnx)$/i, ""); }

// --- Identity scheme -------------------------------------------------------
// هر نوع node یک namespace جدا برای کلید داره تا هیچ‌وقت تصادفی قاطی نشن،
// حتی اگه دو پروژه/رفرنس متفاوت اسم فایل یکسانی داشته باشن.
const projKey = (fullPath) => "proj:" + fullPath;
const extKey  = (rawPath)  => "ext:" + rawPath;
const pkgKey  = (name)     => "pkg:" + name;

function depKey(d) {
  if (d.type === "Package") return { key: pkgKey(d.name), label: d.name, kind: "package" };
  if (d.targetFullPath) return { key: projKey(d.targetFullPath), label: baseName(d.targetFullPath), kind: "project" };
  return { key: extKey(d.name), label: baseName(d.name), kind: "project" }; // External: بدون مسیر واقعی
}

const projects = reports.map(r => ({
  key: projKey(r.projectFullPath),
  id: baseName(r.projectFullPath),
  fullPath: r.projectFullPath,
  solutionPaths: r.solutionPaths,
  solutions: r.solutionPaths.map(baseName),
  dependencies: r.dependencies
}));
const projectByKey = Object.fromEntries(projects.map(p => [p.key, p]));

const realSolutionNames = [...new Set(projects.flatMap(p => p.solutions))];
const orphanProjects = projects.filter(p => p.solutions.length === 0);
const solutions = realSolutionNames.map(name => ({
  name,
  path: projects.find(p => p.solutions.includes(name))?.solutionPaths.find(sp => baseName(sp) === name) ?? null,
  projects: projects.filter(p => p.solutions.includes(name))
}));
if (orphanProjects.length) solutions.push({ name: "No Solution", path: null, projects: orphanProjects });

document.getElementById("subtitle").textContent =
  `${realSolutionNames.length} solution${realSolutionNames.length !== 1 ? "s" : ""} · ${projects.length} project${projects.length !== 1 ? "s" : ""}`;

const ropePalette = [
  { fill: "rgba(95,184,174,.10)", stroke: "rgba(95,184,174,.6)" },
  { fill: "rgba(227,161,92,.10)", stroke: "rgba(227,161,92,.6)" },
  { fill: "rgba(122,148,227,.10)", stroke: "rgba(122,148,227,.6)" },
  { fill: "rgba(196,122,214,.10)", stroke: "rgba(196,122,214,.6)" },
  { fill: "rgba(214,122,140,.10)", stroke: "rgba(214,122,140,.6)" },
  { fill: "rgba(140,196,122,.10)", stroke: "rgba(140,196,122,.6)" }
];

// --- Package solution-membership (Rule #3) ---------------------------------
// یک پکیج فقط وقتی عضو یک طناب می‌شه که *فقط یک* سلوشن مستقیماً بهش وابسته باشه.
// اگه بیشتر از یک سلوشن رفرنسش کرده، در فضای آزاد قرار می‌گیره تا طناب‌ها هم‌پوشانی مصنوعی پیدا نکنن.
function computePackageSolutionMap() {
  const map = {}; // pkgKey -> Set(solutionName)
  projects.forEach(p => {
    p.dependencies.filter(d => d.scope === "Direct" && d.type === "Package").forEach(d => {
      const k = pkgKey(d.name);
      if (!map[k]) map[k] = new Set();
      p.solutions.forEach(s => map[k].add(s));
    });
  });
  return map;
}

// --- Graph builders ---------------------------------------------------------
function buildPackageGraph() {
  const packageSolutionMap = computePackageSolutionMap();
  const nodes = new Map(), links = [], seen = new Set();

  projects.forEach(p => {
    if (!nodes.has(p.key)) nodes.set(p.key, { id: p.key, label: p.id, type: "project", solutions: new Set(p.solutions), resolved: true });

    p.dependencies.filter(d => d.scope === "Direct").forEach(d => {
      const { key, label, kind } = depKey(d);
      const isExternal = kind === "project" && d.isResolved === false;

      if (!nodes.has(key)) {
        let nodeSolutions;
        if (kind === "package") {
          const owners = packageSolutionMap[key];
          nodeSolutions = owners && owners.size === 1 ? owners : new Set(); // Rule #3
        } else {
          nodeSolutions = new Set(isExternal ? [] : p.solutions);
        }
        nodes.set(key, { id: key, label, type: kind === "package" ? "package" : "project", version: d.version, resolved: d.isResolved, solutions: nodeSolutions });
      } else if (kind === "project" && !isExternal) {
        p.solutions.forEach(s => nodes.get(key).solutions.add(s));
      }

      const linkId = p.key + "→" + key;
      if (!seen.has(linkId)) { seen.add(linkId); links.push({ source: p.key, target: key }); }
    });
  });

  const arr = [...nodes.values()];
  arr.forEach(n => n.solutions = [...n.solutions]);
  return { nodes: arr, links };
}

function buildArchitectureGraph() {
  const nodes = new Map();
  projects.forEach(p => nodes.set(p.key, { id: p.key, label: p.id, type: "project", solutions: [...p.solutions], resolved: true }));
  const links = [];
  projects.forEach(p => {
    p.dependencies.filter(d => d.scope === "Direct" && d.type === "Project").forEach(d => {
      const { key, label } = depKey(d);
      if (!nodes.has(key)) nodes.set(key, { id: key, label, type: "project", solutions: d.isResolved === false ? [] : [...p.solutions], resolved: d.isResolved });
      links.push({ source: p.key, target: key });
    });
  });
  return { nodes: [...nodes.values()], links };
}

// --- Rendering ---------------------------------------------------------------
const svg = d3.select("#graph");
const g = svg.append("g");
const bgLayer = g.append("g");
const linkLayer = g.append("g");
const nodeLayer = g.append("g");
const tooltip = d3.select("#tooltip");

let width, height, simulation, currentMode = "pkg", anchors = {};

function measure() {
  const rect = document.querySelector("main").getBoundingClientRect();
  width = rect.width; height = rect.height;
  svg.attr("viewBox", [0, 0, width, height]);
}
measure();

function computeAnchors() {
  const map = {};
  if (realSolutionNames.length === 1) {
    map[realSolutionNames[0]] = { x: width / 2, y: height / 2 };
  } else if (realSolutionNames.length > 1) {
    const R = Math.min(width, height) * 0.32;
    realSolutionNames.forEach((name, i) => {
      const a = (i / realSolutionNames.length) * 2 * Math.PI - Math.PI / 2;
      map[name] = { x: width / 2 + R * Math.cos(a), y: height / 2 + R * Math.sin(a) };
    });
  }
  return map;
}

function anchorCentroid(nodeSolutions) {
  const rel = nodeSolutions.map(s => anchors[s]).filter(Boolean);
  if (!rel.length) return { x: width / 2, y: height / 2 };
  return { x: d3.mean(rel, p => p.x), y: d3.mean(rel, p => p.y) };
}

function foreignRepelForce() {
  let nodesRef;
  const strength = 0.28, minDist = 170;
  function force(alpha) {
    for (const n of nodesRef) {
      for (const name of realSolutionNames) {
        if (n.solutions && n.solutions.includes(name)) continue;
        const a = anchors[name];
        if (!a) continue;
        const dx = n.x - a.x, dy = n.y - a.y;
        const dist = Math.hypot(dx, dy) || 0.01;
        if (dist < minDist) {
          const push = (minDist - dist) / dist * strength * alpha;
          n.vx += dx * push; n.vy += dy * push;
        }
      }
    }
  }
  force.initialize = (_) => nodesRef = _;
  return force;
}

function applyClusterForces() {
  simulation
    .force("clusterX", d3.forceX(d => d.solutions && d.solutions.length ? anchorCentroid(d.solutions).x : width / 2)
      .strength(d => d.solutions && d.solutions.length ? 0.12 : 0.02))
    .force("clusterY", d3.forceY(d => d.solutions && d.solutions.length ? anchorCentroid(d.solutions).y : height / 2)
      .strength(d => d.solutions && d.solutions.length ? 0.12 : 0.02))
    .force("foreignRepel", foreignRepelForce());
}

function circlePts(cx, cy, r, n) {
  return d3.range(n).map(i => { const a = (i / n) * 2 * Math.PI; return [cx + r * Math.cos(a), cy + r * Math.sin(a)]; });
}

function hullPathFor(nodePts) {
  if (!nodePts.length) return null;
  const samples = [];
  nodePts.forEach(([x, y]) => samples.push(...circlePts(x, y, 16, 8)));
  const hull = d3.polygonHull(samples);
  if (!hull) return null;
  const c = d3.polygonCentroid(hull);
  const padded = hull.map(([x, y]) => {
    const dx = x - c[0], dy = y - c[1], len = Math.hypot(dx, dy) || 1;
    return [x + dx / len * 26, y + dy / len * 26];
  });
  return d3.line().curve(d3.curveCatmullRomClosed.alpha(0.6))(padded);
}

function render(mode) {
  currentMode = mode;
  linkLayer.selectAll("*").remove();
  nodeLayer.selectAll("*").remove();
  bgLayer.selectAll("*").remove();
  if (simulation) simulation.stop();

  const { nodes, links } = mode === "pkg" ? buildPackageGraph() : buildArchitectureGraph();
  anchors = computeAnchors();

  const hullGroups = realSolutionNames.map((name, i) => ({
    name,
    path: bgLayer.append("path").attr("class", "cluster-hull")
      .attr("fill", ropePalette[i % ropePalette.length].fill)
      .attr("stroke", ropePalette[i % ropePalette.length].stroke),
    label: bgLayer.append("text").attr("class", "cluster-label")
      .attr("fill", ropePalette[i % ropePalette.length].stroke).text(name)
  }));

  simulation = d3.forceSimulation(nodes)
    .force("link", d3.forceLink(links).id(d => d.id).distance(80).strength(0.85))
    .force("charge", d3.forceManyBody().strength(-260))
    .force("collide", d3.forceCollide().radius(d => d.type === "project" ? 32 : 20));
  applyClusterForces();

  const linkSel = linkLayer.selectAll("line").data(links).join("line").attr("class", "link");
  const nodeSel = nodeLayer.selectAll("g").data(nodes, d => d.id).join("g")
    .attr("class", d => "node" + (d.resolved === false ? " external" : ""))
    .call(d3.drag()
      .on("start", (e, d) => { if (!e.active) simulation.alphaTarget(0.3).restart(); d.fx = d.x; d.fy = d.y; })
      .on("drag", (e, d) => { d.fx = e.x; d.fy = e.y; })
      .on("end", (e, d) => { if (!e.active) simulation.alphaTarget(0); d.fx = null; d.fy = null; }));

  nodeSel.append("circle")
    .attr("r", d => d.type === "project" ? 15 : 8)
    .attr("fill", d => d.type === "project" ? "var(--project)" : "var(--package)");
  nodeSel.append("text").attr("x", d => d.type === "project" ? 20 : 12).attr("dy", "0.32em").text(d => d.label);

  function neighborsOf(id) {
    const set = new Set([id]);
    links.forEach(l => { const s = l.source.id ?? l.source, t = l.target.id ?? l.target;
      if (s === id) set.add(t); if (t === id) set.add(s); });
    return set;
  }

  nodeSel.on("mouseenter", (event, d) => {
      const active = neighborsOf(d.id);
      nodeSel.classed("dim", n => !active.has(n.id));
      nodeSel.classed("hi", n => n.id === d.id);
      linkSel.classed("dim", l => (l.source.id ?? l.source) !== d.id && (l.target.id ?? l.target) !== d.id);
      linkSel.classed("hi", l => (l.source.id ?? l.source) === d.id || (l.target.id ?? l.target) === d.id);
      const solText = d.solutions && d.solutions.length
        ? `<div class="tt-solutions">${d.solutions.length > 1 ? "shared by: " : "in: "}${d.solutions.join(", ")}</div>`
        : (d.type === "package" ? `<div class="tt-solutions">shared across multiple solutions</div>` : "");
      tooltip.classed("show", true).html(
        `<div class="tt-name">${d.label}</div><div class="tt-meta"><span>${d.type === "project" ? "Project" : "NuGet package"}${d.resolved === false ? " · external" : ""}</span>` +
        (d.version ? `<span class="tt-version">v${d.version}</span>` : "") + `</div>${solText}`);
    })
    .on("mousemove", (event) => tooltip.style("left", (event.clientX + 16) + "px").style("top", (event.clientY + 16) + "px"))
    .on("mouseleave", () => {
      nodeSel.classed("dim", false).classed("hi", false);
      linkSel.classed("dim", false).classed("hi", false);
      tooltip.classed("show", false);
    })
    .on("click", (event, d) => selectEntity(d.id, d.type, { fromGraph: true }));

  simulation.on("tick", () => {
    linkSel.attr("x1", d => d.source.x).attr("y1", d => d.source.y).attr("x2", d => d.target.x).attr("y2", d => d.target.y);
    nodeSel.attr("transform", d => `translate(${d.x},${d.y})`);

    hullGroups.forEach(hg => {
      const pts = nodes.filter(n => n.solutions && n.solutions.includes(hg.name)).map(n => [n.x, n.y]);
      const d3path = hullPathFor(pts);
      if (d3path) {
        hg.path.attr("d", d3path).style("display", null);
        const xs = pts.map(p => p[0]), ys = pts.map(p => p[1]);
        hg.label.attr("x", Math.min(...xs) - 10).attr("y", Math.min(...ys) - 34);
      } else { hg.path.style("display", "none"); }
    });
  });

  window._currentNodeSel = nodeSel;
  window._currentLinkSel = linkSel;
}

function relayout() {
  if (!simulation) return;
  measure();
  anchors = computeAnchors();
  applyClusterForces();
  simulation.alpha(0.4).restart();
}

svg.call(d3.zoom().scaleExtent([0.3, 3]).on("zoom", (e) => g.attr("transform", e.transform)));

document.getElementById("view-pkg").addEventListener("click", () => {
  document.getElementById("view-pkg").classList.add("active");
  document.getElementById("view-arch").classList.remove("active");
  render("pkg");
});
document.getElementById("view-arch").addEventListener("click", () => {
  document.getElementById("view-arch").classList.add("active");
  document.getElementById("view-pkg").classList.remove("active");
  render("arch");
});

document.getElementById("search").addEventListener("input", (e) => {
  const q = e.target.value.trim().toLowerCase();
  const nodeSel = window._currentNodeSel, linkSel = window._currentLinkSel;
  if (!q) { nodeSel.classed("dim", false); linkSel.classed("dim", false); return; }
  nodeSel.classed("dim", n => !n.label.toLowerCase().includes(q));
  linkSel.classed("dim", true);
});

window.addEventListener("resize", relayout);

(function setupSidebarResize() {
  const resizer = document.getElementById("sidebar-resizer");
  const root = document.documentElement;
  let dragging = false;
  resizer.addEventListener("mousedown", (e) => { dragging = true; resizer.classList.add("active"); document.body.style.userSelect = "none"; e.preventDefault(); });
  window.addEventListener("mousemove", (e) => {
    if (!dragging) return;
    const max = window.innerWidth / 3, min = 240;
    root.style.setProperty("--panel-w", Math.min(max, Math.max(min, e.clientX)) + "px");
    relayout();
  });
  window.addEventListener("mouseup", () => { if (!dragging) return; dragging = false; resizer.classList.remove("active"); document.body.style.userSelect = ""; });
})();

// --- Sidebar tree ---------------------------------------------------------
function renderTree() {
  const root = document.createElement("ul");
  root.className = "tree";
  solutions.forEach(sol => {
    const solLi = document.createElement("li");
    solLi.innerHTML = `<div class="tree-row" data-type="solution" data-id="${sol.name}"><span class="chev">▾</span>📁 ${sol.name}</div>`;
    const projUl = document.createElement("ul");
    projUl.className = "tree-children";
    sol.projects.forEach(p => {
      const pkgRefs = p.dependencies.filter(d => d.scope === "Direct" && d.type === "Package");
      const projRefs = p.dependencies.filter(d => d.scope === "Direct" && d.type === "Project");
      const pLi = document.createElement("li");
      pLi.className = "collapsed";
      pLi.innerHTML = `<div class="tree-row" data-type="project" data-id="${p.key}"><span class="chev">▾</span>📦 ${p.id}</div>`;
      const inner = document.createElement("ul");
      inner.className = "tree-children";
      const pkgGroupLi = document.createElement("li");
      pkgGroupLi.innerHTML = `<div class="tree-row group">Package references (${pkgRefs.length})</div>`;
      const pkgUl = document.createElement("ul");
      pkgRefs.forEach(d => {
        const { key } = depKey(d);
        const li = document.createElement("li");
        li.innerHTML = `<div class="tree-row leaf" data-type="package" data-id="${key}"><span class="name" title="${d.name}">${d.name}</span><span class="ver">${d.version ?? "?"}</span></div>`;
        pkgUl.appendChild(li);
      });
      pkgGroupLi.appendChild(pkgUl);
      const projGroupLi = document.createElement("li");
      projGroupLi.innerHTML = `<div class="tree-row group">Project references (${projRefs.length})</div>`;
      const projRefUl = document.createElement("ul");
      projRefs.forEach(d => {
        const { key, label } = depKey(d);
        const li = document.createElement("li");
        li.innerHTML = `<div class="tree-row leaf" data-type="project-ref" data-id="${key}"><span class="name">${label}${d.isResolved ? "" : " (external)"}</span></div>`;
        projRefUl.appendChild(li);
      });
      projGroupLi.appendChild(projRefUl);
      inner.appendChild(pkgGroupLi); inner.appendChild(projGroupLi);
      pLi.appendChild(inner); projUl.appendChild(pLi);
    });
    solLi.appendChild(projUl); root.appendChild(solLi);
  });
  const container = document.getElementById("tree");
  container.innerHTML = ""; container.appendChild(root);
}
renderTree();

document.getElementById("tree").addEventListener("click", (e) => {
  const row = e.target.closest(".tree-row");
  if (!row || row.classList.contains("group")) return;
  const li = row.parentElement;
  if (li.querySelector(":scope > .tree-children")) li.classList.toggle("collapsed");
  selectEntity(row.dataset.id, row.dataset.type, { fromTree: true });
});

function expandAncestors(li) { let el = li; while (el) { el.classList.remove("collapsed"); el = el.parentElement.closest("li"); } }

function revealInTree(id) {
  document.getElementById("app").classList.remove("sidebar-collapsed");
  document.querySelectorAll(".tree-row.selected").forEach(r => r.classList.remove("selected"));
  const row = document.querySelector(`.tree-row[data-type="project"][data-id="${CSS.escape(id)}"]`) ||
              document.querySelector(`.tree-row[data-id="${CSS.escape(id)}"]`);
  if (!row) return;
  row.classList.add("selected");
  expandAncestors(row.closest("li"));
  row.scrollIntoView({ block: "center", behavior: "smooth" });
  row.classList.add("flash");
  setTimeout(() => row.classList.remove("flash"), 2000);
}

function highlightInGraph(id) {
  const nodeSel = window._currentNodeSel, linkSel = window._currentLinkSel;
  if (!nodeSel || !nodeSel.data().some(n => n.id === id)) return;
  const links = linkSel.data();
  const active = new Set([id]);
  links.forEach(l => { const s = l.source.id ?? l.source, t = l.target.id ?? l.target; if (s === id) active.add(t); if (t === id) active.add(s); });
  nodeSel.classed("dim", n => !active.has(n.id));
  nodeSel.classed("hi", n => n.id === id);
  linkSel.classed("dim", l => (l.source.id ?? l.source) !== id && (l.target.id ?? l.target) !== id);
  linkSel.classed("hi", l => (l.source.id ?? l.source) === id || (l.target.id ?? l.target) === id);
  setTimeout(() => { nodeSel.classed("dim", false).classed("hi", false); linkSel.classed("dim", false).classed("hi", false); }, 3000);
}

function renderDetails(html) { document.getElementById("details").innerHTML = html; }

function selectEntity(id, type, opts = {}) {
  if (type === "solution") {
    const sol = solutions.find(s => s.name === id);
    renderDetails(`<h3>📁 ${sol.name}</h3><div class="path">${sol.path ?? "(no .sln/.slnx found)"}</div>
      <div class="row"><span class="n">Projects</span><span class="v">${sol.projects.length}</span></div>`);
  } else if (type === "project" || type === "project-ref") {
    const p = projectByKey[id];
    if (!p) {
      const label = id.startsWith("ext:") ? id.slice(4) : id;
      renderDetails(`<h3>📦 ${baseName(label)}</h3><div class="path">${label}</div><div class="empty">External reference — outside the scanned directory, not analyzed.</div>`);
    } else {
      const pkgRefs = p.dependencies.filter(d => d.scope === "Direct" && d.type === "Package");
      const projRefs = p.dependencies.filter(d => d.scope === "Direct" && d.type === "Project");
      renderDetails(`<h3>📦 ${p.id}</h3><div class="path">${p.fullPath}</div>
        <div class="row"><span class="n">Solution(s)</span><span class="v">${p.solutions.join(", ") || "—"}</span></div>
        ${pkgRefs.map(d => `<div class="row"><span class="n">${d.name}</span><span class="v">${d.version ?? "?"}</span></div>`).join("")}
        ${projRefs.map(d => `<div class="row"><span class="n">→ ${depKey(d).label}</span></div>`).join("")}`);
    }
    if (opts.fromTree) highlightInGraph(id);
  } else if (type === "package") {
    const name = id.startsWith("pkg:") ? id.slice(4) : id;
    const owner = projects.find(p => p.dependencies.some(d => d.type === "Package" && d.name === name));
    const dep = owner?.dependencies.find(d => d.type === "Package" && d.name === name);
    renderDetails(`<h3>📄 ${name}</h3><div class="row"><span class="n">Version</span><span class="v">${dep?.version ?? "?"}</span></div>
      <div class="row"><span class="n">Source</span><span class="v">${dep?.versionSource ?? "?"}</span></div>`);
    if (opts.fromTree) highlightInGraph(id);
  }
  if (opts.fromGraph) revealInTree(id);
}

document.getElementById("sidebar-close").addEventListener("click", () => document.getElementById("app").classList.add("sidebar-collapsed"));
document.getElementById("sidebar-toggle").addEventListener("click", () => document.getElementById("app").classList.remove("sidebar-collapsed"));

render("pkg");
</script>
</body>
</html>

""";
}
