> Scope: this reference only covers the uploaded files (`00_Main.cs`–`06_Physic.cs`). A few referenced types (e.g., `PathNet`, `SpaceSearch`, `PreCalMap`, `PhysicalSystem_BSim`, `MiniWatch`, `Agent_Doc`, `Rules_Selection`) are **external** and must be provided by the host project.

---

## 1. High-level architecture

BSim is organized as a lightweight **agent-based simulation loop** (`BSim.ABM`) backed by a **particle-based solver** (`BSim_Physic.Physic` / `IPhysic`). The typical call flow per iteration is:

1. **Agent decision**: each `Agent` evaluates its `Behavior` list and emits physics goals (`List<IPhysic>`).
2. **Agent–Event interaction**: each `Event` may **activate** and **affect** agents (global events or radar-scoped events).
3. **Agent–Agent interaction**: each `Communicate` can create interaction goals between nearby agents.
4. **Agent movement**: `Agent.attr.IMove` emits movement goals for each agent.
5. **Physics step**: `ABM.PS.pip(...)` applies the goals to particles and updates positions.
6. **Documentation**: `Agent.DocMemory(...)` snapshots / logs agent states into `ABM.Document`.

Core modules in the uploaded code:

- **Config & constants**: `BSim.Config`, `BSim.Agent_state`, `BSim.Main`
- **Simulation orchestrator**: `BSim.ABM`
- **Agents & state**: `BSim.Agent`, `Agent_Attr`, `Agent_Bhav`, `Agent_Temp`, `Memory`, `Extensible`
- **Environment**: `BSim.ISpace`
- **Events**: `BSim.Event` (abstract), `EventType`, `EventTypeLib`
- **Behavior & communication**: `BSim.Behavior`, `BSim.Communicate`
- **Physics kernel**: `BSim_Physic.IPhysic`, `Physic`, `_particle`, `IPhysic_template`

---

## 2. Configuration & global constants (`00_Main.cs`)

### `static class BSim.Config`
Holds globally shared simulation parameters.

**Fields**
- `double PersonStrCoeff` – coefficient used to convert `Agent_Attr.StR` to `Agent_Attr.Inst_StR`.
- `double SearchRange` – radius used for path searching (agent start/end snapping in a network).
- `double SocialDistance` – neighborhood radius for agent–agent interactions.
- `double SoomthRate` – smoothing parameter forwarded to `PathNet.PathSearch(...)`.
- `double LevelTol` – tolerance used in spatial queries (e.g., event radar / scope checks).
- `double tolerance` – `RhinoDoc.ActiveDoc.ModelAbsoluteTolerance`.
- `bool debug` – global debug toggle.

### `enum BSim.Agent_state`
Agent finite states: `Busy`, `Walk`, `Talk`, `Free`, `Wait`, `Gone`.

### `static class BSim.Main`
- `string version` – hard-coded version tag included in `ABM.Simulate(...)` reports.

---

## 3. Simulation orchestrator (`05_ABM.cs`)

### `class BSim.ABM`
Represents one simulation instance and owns the **agent list**, **event list**, **space**, and **physics system**.

**Key fields**
- `PhysicalSystem_BSim PS` – external physics wrapper (not included in uploads).
- `int Iteration` – synced from `PS.iteration`.
- `ISpace Space` – environment container (obstacles + path graph).
- `List<Event> Events` – all events / POIs.
- `List<Agent> PPs` – **active** agents in the simulation.
- `List<Agent> PPs_Wait` – agents not yet emerged.
- `List<Agent> PPs_Sink` – expired agents collected for inspection.
- `List<Communicate> Communis` – agent–agent interaction rules.
- `Func<ABM, bool> EndCondition` – end condition (default: no active and no waiting agents).
- `HashSet<Agent_Doc> Document` – documentation store (type not included).

**Constructors**
- `ABM(ISpace Space, List<Event> POIs, List<Agent> PPs, List<Communicate> Communis)`  
  Creates a fresh simulation; agents start in `PPs_Wait`.
- `ABM(ABM abm)`  
  *Copy-like* constructor: resets `Iteration`, reuses `Space` and `Events` (after `Event.Reset()`), deep-copies `PPs_Wait` via `Agent.DeepCopy()`, and clears `PPs`.

> Note: `ABM(ABM abm)` does **not** deep-copy `Space`, `Events`, or `Communis`. It assumes they are immutable or externally managed.

**Main methods**
- `string Simulate(bool Reset, int tab, out string TimeRep)`  
  Runs one full iteration:
  - `PPs_Emerge()` – moves agents from `PPs_Wait` to `PPs` if `Agent.meta.IfEmerge(...)` returns true.
  - `PPs_Expire()` – removes agents from `PPs` to `PPs_Sink` if `Agent.meta.IfExpire(...)` returns true.
  - `InteractSystem(...)` – collects physics goals for decision/event/social/movement.
  - `PS.pip(Reset, goals)` – advances physics (external).
  - `Agent.DocMemory(...)` – pushes agent snapshots into `Document`.
- `List<IPhysic> InteractSystem(out string TimeRep)`  
  Returns the aggregated goal list for the physics system.

**Emergence / expiry**
- `void PPs_Emerge()` calls `Agent.meta.IfEmerge(agent, this)` on each waiting agent.
- `void PPs_Expire()` calls `Agent.meta.IfExpire(agent, this)` on each active agent.

---

## 4. Agents (`01_Agent.cs`)

### 4.1 `class BSim.Agent`
`Agent` is a composition of:
- `Agent_Attr attr` – static-ish attributes + movement controller.
- `Agent_Bhav bhav` – behaviors + to-do list.
- `Agent_View view` – view cone construction helpers.
- `Agent_Meta meta` – lifecycle hooks (emerge/expire/log).
- `Agent_Temp temp` – per-iteration mutable state (target, path, memory, state).

#### Logging (`Agent.log`)
- Internally stores a rolling list `_log` with max length `max = 20`.
- Setting `log` **appends** lines; it does not overwrite.

#### Copying
- `Agent(Agent a)` copy constructor:
  - deep-copies `attr`, `bhav`, `view`
  - **shares** `meta` reference
  - initializes a fresh `temp`
- `Agent DeepCopy()`:
  - uses `new Agent(this)`
  - then deep-copies `temp` via `new Agent_Temp(temp)`

#### Movement / direction helpers
- `Vector3d GetFvec()`  
  Returns a direction vector based on:
  1) current path waypoint (`temp.Path[temp.step]`), else  
  2) current target location (`temp.Target.locate`), else  
  3) cached `attr.Fvec`.
- `Vector3d GetSpdVec()`  
  Returns a unit direction * speed if `temp.State == Walk` and path is valid.
- `int GetPoss()`  
  Returns event posture index (`Event.Poss`) when busy on a target.

#### Documentation hook
- `void DocMemory(ref HashSet<Agent_Doc> docs, int tab)`  
  Inserts/updates an `Agent_Doc` record for this agent (type not included).

---

### 4.2 `class BSim.Agent_Attr`
Defines an agent’s **identity, geometry, movement controller**, and rendering attributes.

**Fields**
- `Behavior IMove` – movement behavior controller (must implement `Behavior.Act(...)`).
- `Point3d Locate` – current location.
- `string ID` – unique agent id.
- `double StR` – strength parameter (user-defined meaning).
- `double Inst_StR` – instantaneous strength used in simulation (`StR * Config.PersonStrCoeff`).
- `double Size` – radius used for `CircleBound`.
- `Mesh[] Postures` – posture meshes (indexed by `Event.Poss`).
- `Color Col` – display color.
- `Vector3d Fvec` – cached facing direction.
- `double Speed` – walking speed.
- `Extensible Ext` – custom key-value attributes.

**Property**
- `Curve CircleBound` (getter)  
  Creates a `Circle(Plane.WorldXY, Locate, Size)` and returns its Nurbs curve.

**Constructors**
- `Agent_Attr(Behavior IMove, double Speed, Point3d Locate, string ID, double StR, double Size, Mesh[] postures, Color col)`
- `Agent_Attr(double Speed, double ATol, Point3d Locate, string ID, double StR, double Size, Mesh[] postures, Color col)`  
  Creates `IMove = new IBehav_MakeMove(ATol, Speed, StR)` (**external** type).
- Copy constructor: `Agent_Attr(Agent_Attr attr)`.

---

### 4.3 `class BSim.Agent_Bhav`
Defines behavior templates and a simple **plan** list.

- `List<Behavior> Behaviors` – executed each iteration by `ABM.InteractSystem(...)`.
- `List<string> ToDoList` – encoded plan items (often interpreted by events).

---

### 4.4 `class BSim.Agent_Temp`
Per-agent mutable runtime state.

**Fields**
- `Agent_state State` – state machine flag.
- `Event Target` – current target event (nullable).
- `List<Memory> Brain` – memory records referencing visited events.
- `List<Point3d> Path` – path waypoints (from `PathNet.PathSearch(...)`).
- `int step` – current waypoint index.
- `Vector3d Fvec` – temporary facing direction.
- `Extensible Ext` – custom runtime attributes.

**Methods**
- `void ClearPath()` – resets `Path` and `step`.
- `bool IfAllMemoExpired()` – true if all `Brain` memories are finished.
- `Memory[] TheLastUnFinishedMemo()` – returns the last unfinished memory (or empty array).
- `Memory[] GetUnFinishedMemo()` – returns all unfinished memories.

---

### 4.5 `class BSim.Memory`
Tracks a “visit” to an `Event` as a `(Step / MaxStep)` counter.

- `Event Event`
- `int Step`
- `int MaxStep`
- `bool IfFinished()` – true if `Step == MaxStep`.
- `bool BeforeExecution()` – true if `Step == 0`.

---

### 4.6 `interface BSim.Agent_Meta`
Lifecycle and reporting hooks for an agent.

- `bool IfEmerge(Agent P, ABM ABM)`
- `bool IfExpire(Agent P, ABM ABM)`
- `object log(Agent P, ABM ABM)`

`ABM` calls `IfEmerge` / `IfExpire` every iteration to move agents between `PPs_Wait`, `PPs`, and `PPs_Sink`.

---

### 4.7 `class BSim.Extensible`
A small dynamic attribute bag to attach arbitrary metadata to objects.

**API**
- `void Set<T>(string name, T value)` – add or update.
- `bool TryGet(string name, out object value)` – non-generic get.
- `bool TryGet<T>(string name, out T value)` – generic get with safe casting / `Convert.ChangeType`.
- `bool Contains(string name)`
- `bool Remove(string name)`

**Copying**
- `Extensible(Extensible other)` deep-copies values:
  - value types / strings: copied directly
  - `ICloneable`: uses `Clone()`
  - serializable objects: uses binary serialization (legacy `BinaryFormatter`)
  - otherwise: shallow fallback

---

## 5. Space & navigation (`02_Space.cs`)

### `class BSim.ISpace`
Provides:
1) **obstacle proximity** queries via an `RTree`, and  
2) a **precomputed graph** handle for path search.

**Fields**
- `List<Curve> Obstacles` – obstacle curves (boundaries).
- `List<Point3d> Obsta_Pts` – sampled points on obstacles.
- `RTree Obsta_Pts_tree` – spatial index of `Obsta_Pts`.
- `PreCalMap GraphMap` – external graph container used by `PathNet.PathSearch(...)`.
- `List<Point3d> GraphNodes` – nodes of the walking graph.

**Constructor**
- `ISpace(List<Curve> Obstacles, double Obs_divLen, PreCalMap prepcMap, List<Point3d> WalkSpaceNodes)`  
  Samples obstacle curves at approximately `Obs_divLen` spacing and builds `Obsta_Pts_tree`.

**Methods**
- `Point3d? Obs_ClosetPt(Point3d pt, double R)`  
  Returns the closest obstacle sample point within radius `R` (via external `SpaceSearch.RT_ClosestPt(...)`).

---

## 6. Events (`03_Event.cs`)

### 6.1 `static class BSim.EventTypeLib`
A global registry of event type names.

- Internal default list includes: `"Default"`, `"Leave"`, `"Talk"`, `"Rest"`.
- Setting `EventTypeLib.types` adds missing names into the internal dictionary.

### 6.2 `class BSim.EventType`
Defines the **semantic type** of an event and its selection rule.

**Fields**
- `string Name`
- `Func<List<Event>, Agent, ABM, List<Event>> SectionRule` – selection rule callback.

**Constructors**
- `EventType(string type)`  
  Registers `type` and uses `Rules_Selection.TryGet("Default")` (**external**) as selection rule.
- `EventType(string type, Func<List<Event>, Agent, ABM, List<Event>> SelFunc)`

---

### 6.3 `abstract class BSim.Event`
Base class for all events/POIs. An `Event` can be:
- **Global** (`IsGlobal == true`): checked for *every* agent each iteration.
- **Radar-scoped** (`IsRadar == true`): only checked for agents inside a spatial scope (`RT`).

**Identity & placement**
- `EventType Type`
- `string ID`
- `Point3d locate` – main location (can be `null` to imply global logic).
- `List<Point3d> sub_locates` – queue positions / sub-locations.
- `List<string> sub_locates_userID` – occupancy list aligned to `sub_locates`.

**User lists**
- `List<Agent> users` – currently executing the event.
- `List<Agent> beTarget` – agents that set this event as target (approach/wait).
- `List<Agent> waitlist` – agents waiting for access.

**Timing & posture**
- `int timeCons` – default time consumption for a memory record.
- `int Poss` – posture index for `Agent_Attr.Postures`.

**Scope**
- `double R` – radius / scale parameter (usage depends on `RT` implementation).
- `object RT` – radar geometry / range object (type depends on `SpaceSearch.RT_PtInScope_Auto(...)`).
- `bool IsGlobal`
- `bool IsRadar`

#### Initialization helper
- `void Default_Construct(string id, string Etype, Func<List<Event>, Agent, ABM, List<Event>> SelFunc, Point3d poi, int TimeConseume, List<Point3d> sub_pts, object Range, double Ra, int poss)`
  - Sets core fields and initializes lists.
  - If `sub_pts` is empty: defaults to `{ poi }`.
  - Creates an internal `Extensible` attribute bag.

#### Activation condition helper
- `bool[] Default_ActivateConditions(Agent P, ABM ABM)`
  Returns a 4-flag array:
  - `[0]` agent has a non-null `temp.Target` and location
  - `[1]` `P.temp.Target` matches this event (type + id)
  - `[2]` agent has related `Memory` records
  - `[3]` set to `false` **only** when the last related `Memory` is finished

> In practice, derived events typically define their own `Activate(...)` logic, but this helper is useful for consistent checks.

#### Abstract methods (must implement)
- `void Affect(Agent P, ABM ABM)` – apply effects when active.
- `bool Activate(Agent P, ABM ABM)` – returns true if this event should affect the agent this step.
- `void Approach(Agent P, ABM ABM)` – approach behavior (e.g., update path, move, queue).
- `bool CanProceed(Agent P, ABM ABM)` – whether agent may start/continue using the event.
- `bool CanBeVisit(Agent P, ABM ABM)` – whether agent is allowed to visit this event at all.

#### Core utilities
- `void Reset()` – clears `users`, `beTarget`, `waitlist`, and empties queue occupancy.
- `void TryApproach(Agent P, ABM EN)`  
  Computes `P.temp.Path` via `PathNet.PathSearch(EN.Space.GraphMap, P.attr.Locate, target, Config.SearchRange, Config.SoomthRate)` (**external**).
- `void Register_Target(Agent P)` – adds to `beTarget` and sets `P.temp.Target`.
- `void Register_Waiter(Agent P)` – adds to `waitlist`.
- `void Register_User(Agent P)` – adds to `users`.

#### Memory registration
- `void Register_Memory(Agent P)`
  - Reads the *next* plan item from `P.bhav.ToDoList[0]`.
  - If the plan string contains `[time:<int>]`, it overrides `timeCons`.
  - Appends a `Memory(this, t)` to `P.temp.Brain`.

#### Queue management
- `int IndexInQueue(Agent P)` – index in `sub_locates_userID` or `-1`.
- `void Organize_Queue()` – compacts `"null"` slots toward the end.
- `int InertQueue_InOrder(Agent P)` – reserves the first available slot.
- `int InertQueue_Random(Agent P, int seed)` – reserves a random available slot.

Return codes:
- `-2`: agent already in queue
- `-1`: no vacancy
- `>= 0`: reserved slot index

#### Expiry helpers
- `void Expire(Agent P)` calls:
  - `Expire_WaiterList`, `Expire_TargetList`, `Expire_UserList`, `Expire_QueueList`

#### Encoding helpers (for `ToDoList` strings)
- `string[] Decode(string input)`  
  Parses strings like `Prefix[key:value][key2:value2]` into an array:
  `["Prefix", "key:value", "key2:value2"]`.
- `string GetEncodeInfor(string[] decoded, string title)`  
  Extracts the value for a given `title`, e.g. `"time"`.

---

## 7. Behavior & communication (`04_Behavior.cs`)

### `abstract class BSim.Behavior`
A behavior is executed each iteration and returns a list of physics goals.

- `abstract List<IPhysic> Act(Agent P, ABM EN)`

Helpers:
- `List<Event> Visitable_Events(List<Event> query_events, Agent P, ABM abm)`  
  Filters by `Event.CanBeVisit(...)`.
- `List<Event> MatchedType_Events(string query_type, ABM abm)`  
  Filters `abm.Events` by `Event.IsType(query_type)`.

### `abstract class BSim.Communicate`
Defines an agent–agent interaction rule.

- `abstract List<IPhysic> Interact(ABM EN, Agent P0, Agent P1)`

---

## 8. Physics kernel (`06_Physic.cs` in namespace `BSim_Physic`)

### 8.1 `interface BSim_Physic.IPhysic`
A “goal” that can compute movement vectors for a subset of particles.

**Required properties**
- `Point3d[] PPos` – reference positions (used to map to particles).
- `Plane[] InitialOrientation` – optional orientation anchors.
- `int[] PIndex` – assigned particle indices (filled by `Physic.AssignPIndex(...)`).
- `Vector3d[] Move` – per-index movement vectors.
- `double[] Weighting` – per-index weights (higher means stronger influence).
- `string Name`

**Required methods**
- `void Calculate(List<_particle> P)` – fill `Move` / `Weighting` based on current particle states.
- `IPhysic Clone()` – cloning support.
- `object Output(List<_particle> P)` – optional output for visualization / debugging.

---

### 8.2 `class BSim_Physic._particle`
Internal particle state used by the solver.

- `Point3d Position`, `Point3d StartPosition`
- `Vector3d MoveSum`, `Vector3d Velocity`
- `double WeightSum`, `double Mass`
- `Plane Orientation`, `Plane StartOrientation`
- `void ClearForces()` – resets `MoveSum` and `WeightSum`.

---

### 8.3 `class BSim_Physic.Physic`
A simple weighted-average particle integrator.

**Particle management**
- `void AddParticle(Point3d p, double m)`
- `void DeleteParticle(int i)`
- `void SetParticleList(List<Point3d> p)`
- `void Restart()` – resets current to start states.
- `void ClearParticles()`
- `int ParticleCount()`

**Queries**
- `Point3d GetPosition(int index)`
- `IEnumerable<Point3d> GetPositions()`
- `Point3d[] GetPositionArray()`
- `Point3d[] GetSomePositions(int[] indexes)`
- `int FindParticleIndex(Point3d Pos, double tol, bool ByCurrent)`
- `int FindOrientedParticleIndex(Plane P, double tol, bool ByCurrent)`

**Index assignment**
- `void AssignPIndex(IPhysic Goal, double Tolerance, bool ByCurrent=false)`  
  Ensures each `Goal.PPos[i]` maps to a particle. If no match within tolerance, a new particle is created.

**Main step**
- `void BSimStep(List<IPhysic> goals)`
  1. `Parallel.ForEach(goals)` calls `goal.Calculate(m_particles)`  
     *(Implementations must be thread-safe.)*
  2. Accumulates `MoveSum += Move[i] * Weighting[i]` and `WeightSum += Weighting[i]`.
  3. Updates each particle position by `move = MoveSum / WeightSum`.
  4. Clears forces and increments iteration counter.

**Diagnostics**
- `List<List<Vector3d>> GetAllMoves(List<IPhysic> goals)`
- `List<List<double>> GetAllWeightings(List<IPhysic> goals)`
- `List<object> GetOutput(List<IPhysic> goals)`

---

### 8.4 `abstract class BSim_Physic.IPhysic_template`
A convenience base class implementing most `IPhysic` boilerplate.

- Provides `Clone()` via `MemberwiseClone()`.
- `Point3d[] GetCurrentPositions(List<_particle> p)` helper.

---

## 9. Minimal integration checklist

To run a complete simulation, the host project typically needs to provide:

1. **Path & spatial utilities**
   - `PathNet.PathSearch(...)`
   - `SpaceSearch.RT_ClosestPt(...)`
   - `SpaceSearch.RT_PtInScope_Auto(...)`
   - `PreCalMap` graph container

2. **Physics wrapper**
   - `PhysicalSystem_BSim` with `pip(bool, List<IPhysic>)` and `iteration`

3. **Event selection rules**
   - `Rules_Selection.TryGet(string)`

4. **Documentation record type**
   - `Agent_Doc` with compatible `IDCompare` semantics and a `record(Agent)` method

---

## 10. Typical usage (pseudo-code)

```csharp
// 1) Build space (obstacles + precomputed graph)
var space = new ISpace(obstacles, obsDivLen, preCalMap, graphNodes);

// 2) Define events (derive from Event and implement abstract methods)
var events = new List<Event> { /* new MyRestEvent(...), new MyLeaveEvent(...), ... */ };

// 3) Define behaviors and movement behavior (derive from Behavior)
var behaviors = new List<Behavior> { /* decision behaviors */ };
var move = /* Behavior instance used as Agent_Attr.IMove */;

// 4) Create agents
var agentAttr = new Agent_Attr(move, speed, startPt, id, strength, size, postures, color);
var agentView = new Agent_View(viewLen, viewAngle);
var agentBhav = new Agent_Bhav(behaviors, toDoList);
var agentMeta = /* Agent_Meta implementation */;
var agent = new Agent(agentAttr, agentView, agentBhav, agentMeta);

// 5) Create ABM
var abm = new ABM(space, events, new List<Agent> { agent }, communicators);

// 6) Step loop
while (!abm.EndCondition(abm))
{
    string timeReport;
    string report = abm.Simulate(Reset:false, tab:0, out timeReport);
    // read agent positions from abm.PPs[*].attr.Locate (updated by physics wrapper)
}

