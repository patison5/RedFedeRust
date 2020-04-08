using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Oxide.Core;
using Oxide.Core.Plugins;
using Oxide.Core.Libraries.Covalence;
using Rust;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.AI;

namespace Oxide.Plugins
{
    [Info("PlaneCrash", "k1lly0u", "0.1.103")]
    [Description("Call cargo planes that can be shot down by players to score loot")]
    class PlaneCrash : RustPlugin
    {
        #region Fields
        [PluginReference] Plugin FancyDrop, LustyMap, Kits;

        private static PlaneCrash Instance { get; set; }

        private float mapSize;

        private Timer callTimer;
        private bool initialized;

        private Dictionary<ulong, InventoryData> deadMemberIds = new Dictionary<ulong, InventoryData>();

        // Effects
        const string c4Explosion = "assets/prefabs/tools/c4/effects/c4_explosion.prefab";
        const string heliExplosion = "assets/prefabs/npc/patrol helicopter/effects/heli_explosion.prefab";
        const string debris = "assets/prefabs/npc/patrol helicopter/damage_effect_debris.prefab";
        const string fireball = "assets/bundled/prefabs/oilfireballsmall.prefab";
        const string smokeSignal = "assets/bundled/prefabs/fx/smoke_signal_full.prefab";

        // Prefabs
        const string cargoPlanePrefab = "assets/prefabs/npc/cargo plane/cargo_plane.prefab";
        const string gibs = "assets/prefabs/npc/patrol helicopter/servergibs_patrolhelicopter.prefab";
        const string crates = "assets/prefabs/npc/patrol helicopter/heli_crate.prefab";
        const string supply = "assets/prefabs/misc/supply drop/supply_drop.prefab";
        const string zombiePrefab = "assets/prefabs/npc/murderer/murderer.prefab";
        const string scientistPrefab = "assets/prefabs/npc/scientist/scientist.prefab";
        const string heavyScientistPrefab = "assets/rust.ai/agents/npcplayer/humannpc/heavyscientist/heavyscientist.prefab";
        const string corpsePrefab = "assets/prefabs/player/player_corpse.prefab";
        const string debrisMarker = "assets/prefabs/tools/map/explosionmarker.prefab";
        #endregion

        #region Oxide Hooks
        private void Loaded()
        {           
            mapSize = TerrainMeta.Size.x;
            permission.RegisterPermission("planecrash.cancall", this);
            lang.RegisterMessages(Messages, this);
        }

        private void OnServerInitialized()
        {
            Instance = this;
            initialized = true;

            if (configData.EventTimers.Random)
                StartCrashTimer();
        }

        private void OnEntitySpawned(BaseNetworkable entity)
        {
            if (!initialized || entity == null)
                return;

            if (entity is CargoPlane)
            {
                if (!configData.Plane.ApplyToAll)
                    return;

                NextTick(() =>
                {
                    if (entity == null)
                        return;

                    object success = Interface.Call("isStrikePlane", entity as CargoPlane);
                    if (success is bool && (bool)success)
                        return;

                    object location = (entity as CargoPlane).dropPosition;
                    if (!(location is Vector3))
                        location = Vector3Ex.Range(-(mapSize / 6f), mapSize / 6f);

                    timer.In(2, () => AddCrashComponent(entity as CargoPlane, (Vector3)location, true));
                });
            }

            if (entity is LootableCorpse)
            {
                if (!configData.NPCOptions.ReplaceCorpseLoot)
                    return;

                LootableCorpse corpse = entity as LootableCorpse;              

                InventoryData inventoryData;

                if (!deadMemberIds.TryGetValue(corpse.playerSteamID, out inventoryData))
                    return;

                deadMemberIds.Remove(corpse.playerSteamID);

                timer.In(2, () =>
                {
                    if (inventoryData != null)
                        inventoryData.RestoreItemsTo(corpse);

                    corpse.ResetRemovalTime(configData.NPCOptions.DespawnTime);
                });
            }
        }

        private void OnEntityDeath(BaseCombatEntity entity, HitInfo info)
        {
            if (entity == null || !configData.NPCOptions.ReplaceCorpseLoot)
                return;

            NPCMonitor npcMonitor = entity.GetComponent<NPCMonitor>();
            if (npcMonitor != null)
            {
                if (!deadMemberIds.ContainsKey(npcMonitor.entity.userID))
                    deadMemberIds.Add(npcMonitor.entity.userID, new InventoryData(npcMonitor.entity));

                StripInventory(npcMonitor.entity, true);
                return;
            }
        }

        private void OnEntityKill(BaseNetworkable entity)
        {
            if (entity == null)
                return;

            CrashPlane crashComponent = entity.GetComponent<CrashPlane>();
            if (crashComponent != null)
                UnityEngine.Object.Destroy(crashComponent);
        }

        private void Unload()
        {
            CrashPlane[] planes = UnityEngine.Object.FindObjectsOfType<CrashPlane>();
            if (planes != null)
            {
                foreach (CrashPlane crashPlane in planes)
                    UnityEngine.Object.Destroy(crashPlane);
            }

            Instance = null;
        }
        #endregion

        #region Components
        private enum FlightStatus { Flying, Crashing }

        private class CrashPlane : MonoBehaviour
        {
            private CargoPlane entity;
            public FlightStatus status;

            private FireBall[] engineFires;

            private Vector3 startPos;
            private Vector3 endPos;
            
            private int rocketHits;

            private float speed;
            private float currentSpeed;

            private float crashTimeTaken;
            private float crashTimeToTake;
            private float timeTaken;
            private float timeToTake;

            private bool isDying;
            private bool isSmoking;
            private bool hasDropped;
            private bool isDropPlane;

            private bool isFancyDrop;

            private Type[] interactableTypes = new Type[] { typeof(TreeEntity), typeof(ResourceEntity), typeof(BuildingBlock), typeof(SimpleBuildingBlock), typeof(BaseHelicopter) };

            private ConfigData configData;

            private void Awake()
            {
                entity = GetComponent<CargoPlane>();
                entity.enabled = false;

                gameObject.layer = (int)Layer.Reserved1;
                gameObject.name = "CrashPlane";

                AttachCollider();

                this.configData = Instance.configData;

                speed = currentSpeed = configData.Plane.Speed;

                crashTimeTaken = 0;
                crashTimeToTake = 20;
                status = FlightStatus.Flying;
            }

            private void FixedUpdate()
            {
                if (entity.transform.position.y <= -5)
                {
                    if (!isDying)
                        Die();
                    return;
                }

                if (status == FlightStatus.Crashing)
                {
                    crashTimeTaken = crashTimeTaken + UnityEngine.Time.deltaTime;
                    float single = Mathf.InverseLerp(0, crashTimeToTake, crashTimeTaken);
                    if (single < 1)
                    {
                        currentSpeed = speed + Mathf.Lerp(0, 10, single);
                        entity.transform.transform.eulerAngles = new Vector3(Mathf.Lerp(0, 25f, single), entity.transform.transform.eulerAngles.y, entity.transform.transform.eulerAngles.z);
                    }

                    entity.transform.position = Vector3.MoveTowards(entity.transform.position, entity.transform.position + (entity.transform.forward * 10), currentSpeed * UnityEngine.Time.deltaTime);
                }
                else
                {
                    entity.transform.position = Vector3.MoveTowards(entity.transform.position, endPos, currentSpeed * UnityEngine.Time.deltaTime);

                    if (isDropPlane)
                    {
                        timeTaken = timeTaken + UnityEngine.Time.deltaTime;
                        entity.secondsTaken = timeTaken;
                        float single = Mathf.InverseLerp(0, timeToTake, timeTaken);
                        if (!hasDropped && single >= 0.5f)
                        {
                            hasDropped = true;
                            if (!Instance.FancyDrop || !isFancyDrop)
                            {
                                BaseEntity drop = GameManager.server.CreateEntity(supply, entity.transform.position);
                                drop.globalBroadcast = true;
                                drop.Spawn();
                            }
                        }
                    }
                }
                entity.transform.hasChanged = true;
            }

            private void OnDestroy()
            {
                enabled = false;
                CancelInvoke();

                if (engineFires != null)
                {
                    foreach (FireBall fire in engineFires)
                    {
                        if (fire != null && !fire.IsDestroyed)
                            fire.Extinguish();
                    }
                }

                if (!entity.IsDestroyed)
                    entity.Kill();
            }

            private void OnTriggerEnter(Collider col)
            {
                if (col.gameObject.GetComponentInParent<Terrain>() != null)
                {
                    Die();
                    return;
                }

                if (col.gameObject.GetComponentInParent<BasePlayer>() != null && col.gameObject.layer == 17)
                {
                    col.gameObject.GetComponentInParent<BasePlayer>().Die();
                    return;
                }

                if (col.GetComponentInParent<ServerProjectile>())
                {
                    SmallExplosion();
                    col.GetComponentInParent<TimedExplosive>()?.Explode();
                    rocketHits++;

                    if (configData.Loot.CrateHit > 0)
                        ServerMgr.Instance.StartCoroutine(SpawnLoot(configData.Loot.CrateHit, false));

                    if (configData.Loot.SupplyHit > 0)
                        ServerMgr.Instance.StartCoroutine(SpawnLoot(configData.Loot.SupplyHit, true));

                    if (rocketHits == 1)
                        AddFire();

                    if (rocketHits >= configData.Plane.DestroyHits)
                    {
                        string attackerName = col.GetComponent<BaseEntity>()?.creatorEntity?.ToPlayer()?.displayName ?? "";

                        if (configData.Messages.DisplayAttacker && !string.IsNullOrEmpty(attackerName))
                            SendChatMessage("AttackerMessage2", attackerName);

                        Die();
                        return;
                    }

                    if (rocketHits >= configData.Plane.DownHits && status == FlightStatus.Flying)
                    {                        
                        string attackerName = col.GetComponent<BaseEntity>()?.creatorEntity?.ToPlayer()?.displayName ?? "";

                        if (configData.Messages.DisplayAttacker && !string.IsNullOrEmpty(attackerName))
                            SendChatMessage("AttackerMessage1", attackerName);
                        BeginCrash();
                    }
                }

                if (configData.Plane.Destruction)
                {
                    foreach (Type type in interactableTypes)
                    {
                        if (col.GetComponentInParent(type) != null)
                        {
                            BaseCombatEntity combatEntity = col.GetComponentInParent<BaseCombatEntity>();
                            if (combatEntity != null)
                            {
                                combatEntity.Die();
                                return;
                            }
                            BaseEntity baseEntity = col.GetComponentInParent<BaseEntity>();
                            if (baseEntity != null)
                            {
                                baseEntity.Kill(BaseNetworkable.DestroyMode.Gib);
                                return;
                            }
                        }
                    }            
                } 
            }
            
            public void SetFlightPath(Vector3 newDropPosition, bool isStandard = false)
            {
                if (entity == null)
                    print("[ERROR] Error in SetFlightPath, the plane is null. Something has destroyed it...");

                if (!isStandard)
                {
                    float size = TerrainMeta.Size.x;
                    startPos = Vector3Ex.Range(-1f, 1f);
                    startPos.y = 0f;
                    startPos.Normalize();
                    startPos = startPos * (size * 2f);
                    endPos = startPos * -1f;
                    startPos = startPos + newDropPosition;
                    endPos = endPos + newDropPosition;
                    startPos.y = 150 + configData.Plane.Height;
                    endPos.y = startPos.y;

                    startPos.x = Mathf.Clamp(startPos.x + (UnityEngine.Random.Range(size / 3, -(size / 3))), -(size + 1000), size + 1000);
                    startPos.z = Mathf.Clamp(startPos.z + (UnityEngine.Random.Range(size / 3, -(size / 3))), -(size + 1000), size + 1000);
                    endPos.x = Mathf.Clamp(endPos.x + (UnityEngine.Random.Range(size / 3, -(size / 3))), -(size + 1000), size + 1000);
                    endPos.z = Mathf.Clamp(endPos.z + (UnityEngine.Random.Range(size / 3, -(size / 3))), -(size + 1000), size + 1000);
                    
                    if (configData.Plane.SmokeTrail)
                        RunEffect(smokeSignal, entity, new Vector3(), Vector3.up * 3);
                }
                else
                {
                    startPos = entity.startPos;
                    endPos = entity.endPos;
                    isDropPlane = true;
                }
                
                entity.transform.position = startPos;
                entity.transform.rotation = Quaternion.LookRotation(endPos - startPos);

                timeToTake = Vector3.Distance(startPos, endPos) / speed;
                entity.secondsToTake = timeToTake;

                isFancyDrop = Instance.FancyDrop ? (bool)Instance.FancyDrop?.CallHook("IsFancyDrop", entity) : false;
                if (isDropPlane && isFancyDrop)
                    Instance.FancyDrop?.CallHook("OverrideDropTime", entity, timeToTake);

                Destroy(this, timeToTake);
            }

            private void AttachCollider()
            {
                Rigidbody rigidbody = gameObject.AddComponent<Rigidbody>();
                rigidbody.useGravity = false;
                rigidbody.isKinematic = true;
                rigidbody.detectCollisions = true;
                rigidbody.collisionDetectionMode = CollisionDetectionMode.Discrete;

                BoxCollider collider = gameObject.AddComponent<BoxCollider>();
                collider.isTrigger = true;
                collider.size = new Vector3(50, 10, 50);
                collider.transform.localPosition = Vector3.up * 6;
            }

            public void BeginCrash()
            {
                endPos = new Vector3(endPos.x, 0, endPos.z);
                status = FlightStatus.Crashing;

                SmallExplosion();
                AddFire();
            }

            private void BigExplosion()
            {
                RunEffect(heliExplosion, null, entity.transform.position);
                RunEffect(debris, null, entity.transform.position);
            }

            private void SmallExplosion()
            {
                RunEffect(c4Explosion, null, entity.transform.position);
                RunEffect(debris, null, entity.transform.position);
            }

            private void AddFire()
            {
                if (engineFires == null)
                {
                    engineFires = new FireBall[]
                    {
                        SpawnFireball(entity, new Vector3(-10, 6, 10), 600),
                        SpawnFireball(entity, new Vector3(10, 6, 10), 600)
                    };
                }
            }

            private void Die()
            {
                if (isDying)
                    return;

                isDying = true;
                BigExplosion();
                CreateLootSpawns();

                if (configData.NPCOptions.Enabled)
                    ServerMgr.Instance.StartCoroutine(CreateNPCs(entity.transform.position));

                if (configData.LustyOptions.CrashIcon)
                    InvokeHandler.Invoke(this, UpdateCrashMarker, 1.5f);
                if (configData.MapOptions.CrashIcon)
                    InvokeHandler.Invoke(this, UpdateMapMarker, 1.5f);

                InvokeHandler.Invoke(this, SmallExplosion, 0.25f);
                InvokeHandler.Invoke(this, SmallExplosion, 0.5f);
                InvokeHandler.Invoke(this, BigExplosion, 1.25f);
                InvokeHandler.Invoke(this, SmallExplosion, 1.75f);
                InvokeHandler.Invoke(this, BigExplosion, 2.25f);

                Destroy(this, 2.5f);
            }

            private void CreateLootSpawns()
            {
                List<ServerGib> serverGibs = ServerGib.CreateGibs(gibs, gameObject, entity.gameObject, entity.transform.forward * 2, 5f);
                for (int i = 0; i < 12; i++)
                {
                    BaseEntity fireBall = GameManager.server.CreateEntity(fireball, entity.transform.position, entity.transform.rotation, true);
                    if (fireBall)
                    {
                        Vector3 randsphere = UnityEngine.Random.onUnitSphere;
                        fireBall.transform.position = (entity.transform.position + new Vector3(0f, 1.5f, 0f)) + (randsphere * UnityEngine.Random.Range(-4f, 4f));

                        Collider collider = fireBall.GetComponent<Collider>();
                        fireBall.Spawn();
                        fireBall.SetVelocity(entity.transform.forward + (randsphere * UnityEngine.Random.Range(3, 10f)));

                        foreach (ServerGib serverGib in serverGibs)
                            Physics.IgnoreCollision(collider, serverGib.GetCollider(), true);
                    }
                }

                if (configData.Loot.CrateCrash > 0)
                    ServerMgr.Instance.StartCoroutine(SpawnLoot(configData.Loot.CrateCrash, false, true));

                if (configData.Loot.SupplyCrash > 0)
                    ServerMgr.Instance.StartCoroutine(SpawnLoot(configData.Loot.SupplyCrash, true, true));

                if (configData.Messages.DisplayDestroy)
                    SendChatMessage("DestroyMessage", entity.transform.position.x, entity.transform.position.z);                
            }

            private IEnumerator SpawnLoot(int amount, bool isDrop, bool isCrashing = false)
            {
                for (int j = 0; j < amount; j++)
                {
                    Vector3 randsphere = UnityEngine.Random.onUnitSphere;
                    Vector3 entpos = (entity.transform.position + new Vector3(0f, 1.5f, 0f)) + (randsphere * UnityEngine.Random.Range(-2f, 3f));

                    string ent = isDrop ? supply : crates;
                    BaseEntity crate = GameManager.server.CreateEntity(ent, entpos, Quaternion.LookRotation(randsphere), true);
                    crate.Spawn();

                    if (j == 0 && configData.Plane.Smoke && isCrashing && !isSmoking)
                    {
                        RunEffect(smokeSignal, crate);
                        isSmoking = true;
                    }

                    Rigidbody rigidbody;
                    if (!isDrop)
                        rigidbody = crate.gameObject.AddComponent<Rigidbody>();
                    else
                    {
                        crate.GetComponent<SupplyDrop>().RemoveParachute();
                        rigidbody = crate.GetComponent<Rigidbody>();
                    }

                    if (rigidbody != null)
                    {
                        rigidbody.isKinematic = false;
                        rigidbody.useGravity = true;
                        rigidbody.mass = 1.25f;
                        rigidbody.interpolation = RigidbodyInterpolation.Interpolate;
                        rigidbody.drag = 0.25f;
                        rigidbody.angularDrag = 0.1f;
                        rigidbody.AddForce((entity.transform.forward + (entity.transform.right * UnityEngine.Random.Range(-10f, 10f))) * 100);
                    }

                    FireBall fireBall = SpawnFireball(crate, null);

                    InvokeHandler.Invoke(crate, () => Instance.FillLootContainer(crate, isDrop), 2f);

                    yield return CoroutineEx.waitForEndOfFrame;
                }
            }

            private FireBall SpawnFireball(BaseEntity parent, object offset = null, float lifetime = 0)
            {
                FireBall fireBall = GameManager.server.CreateEntity(fireball, parent.transform.position, new Quaternion(), true) as FireBall;
                if (fireBall)
                {        
                    if (offset is Vector3)
                        fireBall.transform.localPosition = (Vector3)offset;

                    fireBall.Spawn();
                    fireBall.SetParent(parent, false, true);

                    fireBall.GetComponent<Rigidbody>().isKinematic = true;
                    fireBall.GetComponent<Collider>().enabled = false;

                    fireBall.Invoke(fireBall.Extinguish, lifetime == 0 ? configData.Loot.FireLife : lifetime);
                    return fireBall;
                }
                return null;
            }

            private IEnumerator CreateNPCs(Vector3 position)
            {
                string type = configData.NPCOptions.Type.ToLower() == "murderer" ? zombiePrefab : configData.NPCOptions.Type.ToLower() == "heavyscientist" ? heavyScientistPrefab : scientistPrefab;

                int amount = configData.NPCOptions.Amount + (configData.NPCOptions.CorpseEnabled ? configData.NPCOptions.CorpseAmount : 0);

                for (int i = 0; i < amount; i++)
                {
                    Vector3 newPosition = position + (UnityEngine.Random.onUnitSphere * 20);

                    object point = Instance.FindPointOnNavmesh(newPosition);
                    if (point is Vector3)
                        newPosition = (Vector3)point;
                    else newPosition.y = TerrainMeta.HeightMap.GetHeight(newPosition);

                    NPCPlayer entity = InstantiateEntity(type, newPosition);
                    entity.enableSaving = false;
                    entity.Spawn();

                    entity.NavAgent.areaMask = 1;
                    entity.NavAgent.agentTypeID = -1372625422;

                    if (entity is NPCPlayerApex)
                        (entity as NPCPlayerApex).CommunicationRadius = -1f;                   

                    entity.displayName = configData.NPCOptions.Names?.Length > 0 ? configData.NPCOptions.Names.GetRandom() : entity.ShortPrefabName;
                    entity.InitializeHealth(configData.NPCOptions.Health, configData.NPCOptions.Health);

                    entity.gameObject.AddComponent<NPCMonitor>();                    

                    bool isCorpse = i >= configData.NPCOptions.Amount;

                    if (!isCorpse)
                        InvokeHandler.Invoke(entity, () => DespawnNPC(entity), configData.NPCOptions.DespawnTime);

                    string kit = isCorpse ? configData.NPCOptions.CorpseKit : configData.NPCOptions.Kit;

                    if (entity is NPCMurderer && configData.NPCOptions.ReplaceCorpseLoot)
                        (entity as NPCMurderer).LootSpawnSlots = new LootContainer.LootSpawnSlot[0];

                    if (entity is HumanNPC && configData.NPCOptions.ReplaceCorpseLoot)
                        (entity as HumanNPC).LootSpawnSlots = new LootContainer.LootSpawnSlot[0];

                    if (!string.IsNullOrEmpty(kit))
                    {
                        if (entity.IsInvoking(entity.EquipTest))
                            entity.CancelInvoke(entity.EquipTest);

                        Instance.StripInventory(entity);

                        Instance.NextTick(() =>
                        {
                            Instance.Kits?.Call("GiveKit", entity, kit);

                            if (isCorpse)
                                InvokeHandler.Invoke(entity, entity.DieInstantly, UnityEngine.Random.Range(1f, 3f));
                            else Instance.NextTick(() => entity.EquipTest());
                        });
                    }
                    else
                    {
                        if (isCorpse)
                            InvokeHandler.Invoke(entity, entity.DieInstantly, UnityEngine.Random.Range(1f, 3f));
                    }

                    yield return CoroutineEx.waitForEndOfFrame;
                }
            }

            private void DespawnNPC(NPCPlayer npcPlayer)
            {
                if (npcPlayer == null || npcPlayer.IsDestroyed || npcPlayer.IsDead())
                    return;

                npcPlayer.inventory.Strip();
                npcPlayer.DieInstantly();
            }

            private NPCPlayer InstantiateEntity(string type, Vector3 position)
            {
                GameObject gameObject = Facepunch.Instantiate.GameObject(GameManager.server.FindPrefab(type), position, new Quaternion());
                gameObject.name = type;

                UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(gameObject, Rust.Server.EntityScene);

                Destroy(gameObject.GetComponent<Spawnable>());

                if (!gameObject.activeSelf)
                    gameObject.SetActive(true);

                NPCPlayer component = gameObject.GetComponent<NPCPlayer>();
                return component;
            }

            private void UpdateCrashMarker()
            {
                if (!Instance.LustyMap || entity == null)
                    return;

                Instance.LustyMap.Call("AddTemporaryMarker", entity.transform.position.x, entity.transform.position.z, "Crashed Plane", configData.LustyOptions.IconURL, 0);
                Instance.timer.In(configData.LustyOptions.CrashIconTime, () => Instance.LustyMap.Call("RemoveTemporaryMarker", "Crashed Plane"));
            }

            private void UpdateMapMarker()
            {
                if (entity == null)
                    return;

                BaseEntity baseEntity = GameManager.server.CreateEntity(debrisMarker, entity.transform.position, Quaternion.identity, true);
                baseEntity.Spawn();
                baseEntity.SendMessage("SetDuration", configData.MapOptions.CrashIconTime, SendMessageOptions.DontRequireReceiver);
            }
        }

        public class NPCMonitor : MonoBehaviour
        {
            public NPCPlayer entity;
            private Vector3 homePosition;

            private void Awake()
            {
                entity = GetComponent<NPCPlayer>();
                enabled = false;
                homePosition = entity.transform.position;

                InvokeHandler.InvokeRandomized(this, CheckNPCLocation, 1f, 5f, 1f);
            }

            public void CheckNPCLocation()
            {
                if (entity == null || entity.IsDestroyed)
                {
                    Destroy(this);
                    return;
                }

                if (Vector3.Distance(entity.transform.position, homePosition) > 30)                
                    ResetPosition(); 
            }
            
            private void ResetPosition() => entity.SetDestination(homePosition);            
        }
        #endregion

        #region Functions
        private void StartCrashTimer()
        {
            callTimer = timer.In(UnityEngine.Random.Range(configData.EventTimers.Min, configData.EventTimers.Max) * 60, () =>
            {
                CargoPlane plane = (CargoPlane)GameManager.server.CreateEntity(cargoPlanePrefab, new Vector3(), new Quaternion(), true);
                plane.enableSaving = false;
                plane.Spawn();

                CrashPlane crash = plane.gameObject.AddComponent<CrashPlane>();
                crash.SetFlightPath(Vector3Ex.Range(-(mapSize / 6f), mapSize / 6f));

                if (configData.Messages.DisplayIncoming)
                    SendChatMessage("IncomingMessage");

                StartCrashTimer();
            });
        }

        private static void RunEffect(string name, BaseEntity entity = null, Vector3 position = new Vector3(), Vector3 offset = new Vector3())
        {
            if (entity != null)
                Effect.server.Run(name, entity, 0, offset, position, null, true);
            else Effect.server.Run(name, position, Vector3.up, null, true);
        }

        private void AddCrashComponent(CargoPlane plane, Vector3 location, bool isStandard = false)
        {
            if (plane.GetComponent<CrashPlane>())
                return;

            CrashPlane crash = plane.gameObject.AddComponent<CrashPlane>();
            crash.SetFlightPath(location, isStandard);

            if (!isStandard && configData.Messages.DisplayIncoming)
                SendChatMessage("IncomingMessage");
        }

        private void FillLootContainer(BaseEntity entity, bool isDrop)
        {
            if (entity == null)
                return;

            ItemContainer container = isDrop ? entity.GetComponent<SupplyDrop>()?.inventory : entity.GetComponentInParent<LootContainer>()?.inventory;
            ConfigData.LootSettings.LootTables lootTable = isDrop ? configData.Loot.SupplyLoot : configData.Loot.CrateLoot;
           
            if (container == null || lootTable == null)
                return;

            if (lootTable.Enabled)
            {
                while (container.itemList.Count > 0)
                {
                    Item item = container.itemList[0];
                    item.RemoveFromContainer();
                    item.Remove(0f);
                }
                int count = UnityEngine.Random.Range(lootTable.Minimum, lootTable.Maximum);
                for (int i = 0; i < count; i++)
                {
                    ConfigData.LootSettings.LootTables.LootItem lootItem = lootTable.Items.GetRandom();
                    if (lootItem == null)
                        continue;

                    Item item = ItemManager.CreateByName(lootItem.Shortname, UnityEngine.Random.Range(lootItem.Min, lootItem.Max));
                    if (item != null)
                        item.MoveToContainer(container);
                }
            }
            if (configData.Loot.LockCrates)
            {
                entity.SetFlag(BaseEntity.Flags.Locked, true, false);
                InvokeHandler.Invoke(entity, () => { entity.SetFlag(BaseEntity.Flags.Locked, false, false); }, configData.Loot.LockTimer);                
            }
        }

        private object FindPointOnNavmesh(Vector3 targetPosition)
        {
            NavMeshHit navHit;
            if (NavMesh.SamplePosition(targetPosition, out navHit, 100, 1))
                return navHit.position;
            return null;
        }

        private bool IsCrashPlane(CargoPlane plane) => plane.GetComponent<CrashPlane>();

        private void StripInventory(BasePlayer player, bool skipWear = false)
        {
            Item[] allItems = player.inventory.AllItems();

            for (int i = allItems.Length - 1; i >= 0; i--)
            {
                Item item = allItems[i];
                if (skipWear && item?.parent == player.inventory.containerWear)
                    continue;

                item.RemoveFromContainer();
                item.Remove();
            }
        }

        private static void SendChatMessage(string key, params object[] args)
        {
            foreach (var player in BasePlayer.activePlayerList)
            {
                player.ChatMessage(args != null
                    ? string.Format(msg(key, player.UserIDString), args)
                    : msg(key, player.UserIDString));
            }
        }
        
        #endregion

        #region Commands
        [ConsoleCommand("callcrash")]
        void ccmdSendCrash(ConsoleSystem.Arg arg)
        {
            if (arg.Connection != null)
                return;

            Vector3 location = Vector3.zero;
            if (arg.Args != null)
            {
                if (arg.Args.Length == 1 && arg.Args[0].ToLower() == "help")
                {
                    SendReply(arg, $"{Title}  v{Version} - k1lly0u @ chaoscode.io");
                    SendReply(arg, "callcrash - Send a random crash plane");
                    SendReply(arg, "callcrash \"X\" \"Z\" - Send a crash plane towards the specified X and Z co-ordinates");
                    SendReply(arg, "callcrash \"playername\" - Send a crash plane towards the specified player's position");
                    SendReply(arg, "callcrash crashall - Force crash any active planes");
                    return;
                }

                if (arg.Args.Length > 0)
                {
                    if (arg.Args.Length == 2)
                    {
                        float x;
                        float z;
                        if (float.TryParse(arg.Args[0], out x) && float.TryParse(arg.Args[1], out z))
                        {
                            location = new Vector3(x, 0, z);
                            SendReply(arg, $"Crash plane sent to X: {x}, Z: {z}");
                        }
                    }
                    if (arg.Args.Length == 1)
                    {
                        if (arg.Args[0].ToLower() == "crashall")
                        {
                            CrashPlane[] crashPlanes = UnityEngine.Object.FindObjectsOfType<CrashPlane>();
                            if (crashPlanes == null || crashPlanes.Length == 0)
                            {
                                SendReply(arg, "There are no planes currently active");
                                return;
                            }

                            for (int i = 0; i < crashPlanes.Length; i++)
                            {
                                CrashPlane crashPlane = crashPlanes[i];
                                if (crashPlane.status == FlightStatus.Flying)
                                    crashPlane.BeginCrash();
                            }
                            
                            SendReply(arg, $"Force crashing {crashPlanes.Length} planes!");
                            return;
                        }
                        else
                        {
                            IPlayer targetPlayer = covalence.Players.FindPlayer(arg.Args[0]);
                            if (targetPlayer != null && targetPlayer.IsConnected)
                            {
                                BasePlayer target = targetPlayer?.Object as BasePlayer;
                                if (target != null)
                                {
                                    location = target.transform.position;
                                    SendReply(arg, $"Crash plane sent towards {target.displayName}'s current position");
                                }
                            }
                            else
                            {
                                SendReply(arg, "Could not locate the specified player");
                                return;
                            }
                        }
                    }
                    else
                    {
                        location = Vector3Ex.Range(-(mapSize / 6f), mapSize / 6f);
                        SendReply(arg, "Crash plane sent to random location");
                    }
                }
            }
            else
            {
                location = Vector3Ex.Range(-(mapSize / 6f), mapSize / 6f);
                SendReply(arg, "Crash plane sent to random location");
            }

            CargoPlane plane = (CargoPlane)GameManager.server.CreateEntity(cargoPlanePrefab, new Vector3(), new Quaternion(), true);
            plane.Spawn();
            AddCrashComponent(plane, location);
        }

        [ChatCommand("callcrash")]
        void cmdSendCrash(BasePlayer player, string command, string[] args)
        {
            if (!player.IsAdmin && !permission.UserHasPermission(player.UserIDString, "planecrash.cancall"))
                return;

            if (args.Length == 1 && args[0].ToLower() == "help")
            {
                SendReply(player, $"<color=#ce422b>{Title}</color>  <color=#939393>v</color><color=#ce422b>{Version}</color> <color=#939393>-</color> <color=#ce422b>k1lly0u</color><color=#939393> @</color> <color=#ce422b>chaoscode.io</color>");
                SendReply(player, "<color=#ce422b>/callcrash</color><color=#939393> - Send a random crash plane</color>");
                SendReply(player, "<color=#ce422b>/callcrash \"X\" \"Z\" </color><color=#939393>- Send a crash plane towards the specified X and Z co-ordinates</color>");
                SendReply(player, "<color=#ce422b>/callcrash \"playername\" </color><color=#939393>- Send a crash plane towards the specified player's position</color>");
                SendReply(player, "<color=#ce422b>/callcrash crashall</color><color=#939393> - Force all active planes to crash</color>");
                return;
            }
            Vector3 location = Vector3.zero;
            if (args.Length > 0)
            {
                if (args.Length == 2)
                {
                    float x;
                    float z;
                    if (float.TryParse(args[0], out x) && float.TryParse(args[1], out z))
                    {
                        location = new Vector3(x, 0, z);
                        SendReply(player, $"<color=#939393>Crash plane sent to</color> <color=#ce422b>X: {x}, Z: {z}</color>");
                    }
                }
                if (args.Length == 1)
                {
                    if (args[0].ToLower() == "crashall")
                    {
                        CrashPlane[] crashPlanes = UnityEngine.Object.FindObjectsOfType<CrashPlane>();
                        if (crashPlanes == null || crashPlanes.Length == 0)
                        {
                            SendReply(player, "There are no planes currently active");
                            return;
                        }

                        for (int i = 0; i < crashPlanes.Length; i++)
                        {
                            CrashPlane crashPlane = crashPlanes[i];
                            if (crashPlane.status == FlightStatus.Flying)
                                crashPlane.BeginCrash();
                        }

                        SendReply(player, $"Force crashing {crashPlanes.Length} planes!");
                        return;
                    }
                    else
                    {
                        IPlayer targetPlayer = covalence.Players.FindPlayer(args[0]);
                        if (targetPlayer != null && targetPlayer.IsConnected)
                        {
                            BasePlayer target = targetPlayer?.Object as BasePlayer;
                            if (target != null)
                            {
                                location = target.transform.position;
                                SendReply(player, $"<color=#939393>Crash plane sent towards </color><color=#ce422b>{target.displayName}'s</color><color=#939393> current position</color>");
                            }
                        }
                        else
                        {
                            SendReply(player, "<color=#ce422b>Could not locate the specified player</color>");
                            return;
                        }
                    }
                }
            }
            else
            {
                location = Vector3Ex.Range(-(mapSize / 6f), mapSize / 6f);
                SendReply(player, "<color=#ce422b>Crash plane sent to random location</color>");
            }

            CargoPlane plane = (CargoPlane)GameManager.server.CreateEntity(cargoPlanePrefab, new Vector3(), new Quaternion(), true);
            plane.Spawn();
            AddCrashComponent(plane, location);
        }
        #endregion

        #region Config        
        private ConfigData configData;

        private class ConfigData
        {
            [JsonProperty(PropertyName = "Plane Settings")]
            public PlaneSettings Plane { get; set; }
            [JsonProperty(PropertyName = "Loot Settings")]
            public LootSettings Loot { get; set; }
            [JsonProperty(PropertyName = "Message Options")]
            public Messaging Messages { get; set; }
            [JsonProperty(PropertyName = "Timer Settings")]
            public Timers EventTimers { get; set; }
            [JsonProperty(PropertyName = "LustyMap Integration")]
            public Lusty LustyOptions { get; set; }
            [JsonProperty(PropertyName = "Ingame Map Integration")]
            public Map MapOptions { get; set; }
            [JsonProperty(PropertyName = "NPC Options")]
            public Bots NPCOptions { get; set; }

            public class PlaneSettings
            {
                [JsonProperty(PropertyName = "Apply crash mechanics to all spawned planes")]
                public bool ApplyToAll { get; set; }
                [JsonProperty(PropertyName = "Flight speed")]
                public float Speed { get; set; }
                [JsonProperty(PropertyName = "Show smoke on crash site")]
                public bool Smoke { get; set; }
                [JsonProperty(PropertyName = "Height modifier to default flight height")]
                public float Height { get; set; }
                [JsonProperty(PropertyName = "Amount of rocket hits to destroy mid-flight")]
                public int DestroyHits { get; set; }
                [JsonProperty(PropertyName = "Amount of rocket hits to make the plane crash")]
                public int DownHits { get; set; }
                [JsonProperty(PropertyName = "Show smoke trail behind plane")]
                public bool SmokeTrail { get; set; }
                [JsonProperty(PropertyName = "Destroy objects that get in the way of a crashing plane")]
                public bool Destruction { get; set; }
            }
            public class LootSettings
            {
                [JsonProperty(PropertyName = "Fireball lifetime (seconds)")]
                public int FireLife { get; set; }
                [JsonProperty(PropertyName = "Crate amount (Crash)")]
                public int CrateCrash { get; set; }
                [JsonProperty(PropertyName = "Supply drop amount (Crash)")]
                public int SupplyCrash { get; set; }
                [JsonProperty(PropertyName = "Crate amount (Rocket hit)")]
                public int CrateHit { get; set; }
                [JsonProperty(PropertyName = "Supply drop amount (Rocket hit)")]
                public int SupplyHit { get; set; }
                [JsonProperty(PropertyName = "Supply drop loot table")]
                public LootTables SupplyLoot { get; set; }
                [JsonProperty(PropertyName = "Crate loot table")]
                public LootTables CrateLoot { get; set; }

                [JsonProperty(PropertyName = "Lock dropped crates and supply drops")]
                public bool LockCrates { get; set; }
                [JsonProperty(PropertyName = "Locked crates and supply drop timer (seconds)")]
                public int LockTimer { get; set; }

                public class LootTables
                {
                    [JsonProperty(PropertyName = "Use this loot table")]
                    public bool Enabled { get; set; }
                    [JsonProperty(PropertyName = "Minimum amount of items to drop")]
                    public int Minimum { get; set; }
                    [JsonProperty(PropertyName = "Maximum amount of items to drop")]
                    public int Maximum { get; set; }
                    [JsonProperty(PropertyName = "Item list")]
                    public List<LootItem> Items { get; set; }

                    public class LootItem
                    {
                        [JsonProperty(PropertyName = "Item shortname")]
                        public string Shortname { get; set; }
                        [JsonProperty(PropertyName = "Minimum amount")]
                        public int Min { get; set; }
                        [JsonProperty(PropertyName = "Maximum amount")]
                        public int Max { get; set; }
                    }
                }
            }   
            public class Lusty
            {
                [JsonProperty(PropertyName = "Show icon on crash site")]
                public bool CrashIcon { get; set; }
                [JsonProperty(PropertyName = "Amount of time the crash icon will be displayed on LustyMap (seconds)")]
                public int CrashIconTime { get; set; }
                [JsonProperty(PropertyName = "Crash icon URL")]
                public string IconURL { get; set; }               
            }
            public class Map
            {
                [JsonProperty(PropertyName = "Show ingame map marker on crash site")]
                public bool CrashIcon { get; set; }
                [JsonProperty(PropertyName = "Amount of time the crash icon will be displayed on the ingame map (minutes)")]
                public int CrashIconTime { get; set; }               
            }
            public class Messaging
            {
                [JsonProperty(PropertyName = "Display incoming crash plane message")]
                public bool DisplayIncoming { get; set; }
                [JsonProperty(PropertyName = "Display destroyed crash plane message")]
                public bool DisplayDestroy { get; set; }
                [JsonProperty(PropertyName = "Display message stating who shot down the plane")]
                public bool DisplayAttacker { get; set; }               
            }
            public class Timers
            {
                [JsonProperty(PropertyName = "Autospawn crash planes with a random spawn timer")]
                public bool Random { get; set; }
                [JsonProperty(PropertyName = "Minimum time between autospawned planes (minutes)")]
                public int Min { get; set; }
                [JsonProperty(PropertyName = "Maximum time between autospawned planes (minutes)")]
                public int Max { get; set; }
            }
            public class Bots
            {
                [JsonProperty(PropertyName = "Spawn NPCs at the crash site")]
                public bool Enabled { get; set; }
                [JsonProperty(PropertyName = "Amount of NPCs to spawn")]
                public int Amount { get; set; }
                [JsonProperty(PropertyName = "Spawn corpses at the crash site")]
                public bool CorpseEnabled { get; set; }
                [JsonProperty(PropertyName = "Amount of corpses to spawn")]
                public int CorpseAmount { get; set; }
                [JsonProperty(PropertyName = "Custom kit for the corpses (Requires 'Replace corpse loot' set to true)")]
                public string CorpseKit { get; set; }
                [JsonProperty(PropertyName = "Type of NPCs to spawn (Murderer / Scientist / HeavyScientist)")]
                public string Type { get; set; }
                [JsonProperty(PropertyName = "Custom kit for the NPC")]
                public string Kit { get; set; }
                [JsonProperty(PropertyName = "Replace corpse loot with current items")]
                public bool ReplaceCorpseLoot { get; set; }
                [JsonProperty(PropertyName = "Initial health for the NPC")]
                public float Health { get; set; }
                [JsonProperty(PropertyName = "Despawn time for NPCs (seconds)")]
                public int DespawnTime { get; set; }
                [JsonProperty(PropertyName = "NPC Names (Chosen at random)")]
                public string[] Names { get; set; }
            }

            public Oxide.Core.VersionNumber Version { get; set; }
        }

        protected override void LoadConfig()
        {
            base.LoadConfig();
            configData = Config.ReadObject<ConfigData>();

            if (configData.Version < Version)
                UpdateConfigValues();

            Config.WriteObject(configData, true);
        }

        protected override void LoadDefaultConfig() => configData = GetBaseConfig();

        private ConfigData GetBaseConfig()
        {
            return new ConfigData
            {
                EventTimers = new ConfigData.Timers
                {
                    Max = 60,
                    Min = 45,
                    Random = true
                },
                Plane = new ConfigData.PlaneSettings
                {
                    ApplyToAll = false,
                    Smoke = true,
                    Speed = 35f,
                    Height = 0f,
                    DestroyHits = 3,
                    Destruction = true,
                    DownHits = 1,
                    SmokeTrail = true
                },
                Loot = new ConfigData.LootSettings
                {
                    CrateCrash = 3,
                    SupplyCrash = 3,
                    FireLife = 300,
                    CrateHit = 1,
                    SupplyHit = 1,
                    LockCrates = true,
                    LockTimer = 120,
                    CrateLoot = new ConfigData.LootSettings.LootTables
                    {
                        Maximum = 4,
                        Minimum = 1,
                        Items = new List<ConfigData.LootSettings.LootTables.LootItem>
                        {
                            new ConfigData.LootSettings.LootTables.LootItem {Shortname = "metal.refined", Max = 100, Min = 10 },
                            new ConfigData.LootSettings.LootTables.LootItem {Shortname = "explosive.timed", Max = 2, Min = 1 },
                            new ConfigData.LootSettings.LootTables.LootItem {Shortname = "grenade.f1", Max = 3, Min = 1 },
                            new ConfigData.LootSettings.LootTables.LootItem {Shortname = "supply.signal", Max = 1, Min = 1 },
                            new ConfigData.LootSettings.LootTables.LootItem {Shortname = "cctv.camera", Max = 1, Min = 1 },
                            new ConfigData.LootSettings.LootTables.LootItem {Shortname = "targeting.computer", Max = 1, Min = 1 },
                            new ConfigData.LootSettings.LootTables.LootItem {Shortname = "ammo.rifle", Max = 60, Min = 20 },
                            new ConfigData.LootSettings.LootTables.LootItem {Shortname = "ammo.pistol", Max = 60, Min = 20 }
                        },
                        Enabled = false
                    },
                    SupplyLoot = new ConfigData.LootSettings.LootTables
                    {
                        Maximum = 6,
                        Minimum = 2,
                        Items = new List<ConfigData.LootSettings.LootTables.LootItem>
                        {
                            new ConfigData.LootSettings.LootTables.LootItem {Shortname = "rifle.ak", Max = 1, Min = 1 },
                            new ConfigData.LootSettings.LootTables.LootItem {Shortname = "pistol.m92", Max = 1, Min = 1 },
                            new ConfigData.LootSettings.LootTables.LootItem {Shortname = "pistol.semiauto", Max = 1, Min = 1 },
                            new ConfigData.LootSettings.LootTables.LootItem {Shortname = "shotgun.double", Max = 1, Min = 1 },
                            new ConfigData.LootSettings.LootTables.LootItem {Shortname = "smg.thompson", Max = 1, Min = 1 },
                            new ConfigData.LootSettings.LootTables.LootItem {Shortname = "rifle.bolt", Max = 1, Min = 1 }
                        },
                        Enabled = false
                    }
                },
                LustyOptions = new ConfigData.Lusty
                {
                    CrashIcon = true,
                    CrashIconTime = 300,
                    IconURL = "http://www.chaoscode.io/oxide/Images/crashicon.png"
                },
                MapOptions = new ConfigData.Map
                {
                    CrashIcon = true,
                    CrashIconTime = 5
                },
                Messages = new ConfigData.Messaging
                {
                    DisplayDestroy = true,
                    DisplayIncoming = true,
                    DisplayAttacker = true,
                },
                NPCOptions = new ConfigData.Bots
                {
                    Amount = 5,
                    Enabled = true,
                    Type = "Murderer",
                    Kit = "",
                    Health = 100,
                    DespawnTime = 300,
                    Names = new string[0],
                    CorpseAmount = 5,
                    CorpseEnabled = false,
                    CorpseKit = "",
                    ReplaceCorpseLoot = true,
                },
                Version = Version
            };            
        }

        protected override void SaveConfig() => Config.WriteObject(configData, true);

        private void UpdateConfigValues()
        {
            PrintWarning("Config update detected! Updating config values...");

            ConfigData baseConfig = GetBaseConfig();
            if (configData.Version < new VersionNumber(0, 1, 9))
            {
                configData.Messages.DisplayAttacker = baseConfig.Messages.DisplayAttacker;                
                configData.NPCOptions = baseConfig.NPCOptions;
            }

            if (configData.Version < new VersionNumber(0, 1, 94))            
                configData.Plane.Destruction = baseConfig.Plane.Destruction;

            if (configData.Version < new VersionNumber(0, 1, 97))
                configData.MapOptions = baseConfig.MapOptions;

            if (configData.Version < new VersionNumber(0, 1, 99))
                configData.NPCOptions.DespawnTime = baseConfig.NPCOptions.DespawnTime;

            if (configData.Version < new VersionNumber(0, 1, 100))
                configData.NPCOptions.Names = new string[0];

            if (configData.Version < new VersionNumber(0, 1, 101))
            {
                configData.NPCOptions.Type = "murderer";
                configData.NPCOptions.ReplaceCorpseLoot = false;
                configData.NPCOptions.CorpseAmount = 5;
                configData.NPCOptions.CorpseEnabled = false;
                configData.NPCOptions.CorpseKit = string.Empty;

            }

            configData.Version = Version;
            PrintWarning("Config update completed!");
        }
        #endregion

        #region Corpse Inventory Population
        public class InventoryData
        {
            public List<InventoryItem> items = new List<InventoryItem>();

            public InventoryData(NPCPlayer player)
            {
                items = player.inventory.AllItems().Select(item => new InventoryItem
                {
                    itemid = item.info.itemid,
                    amount = item.amount > 1 ? UnityEngine.Random.Range(1, item.amount) : item.amount,
                    ammo = (item.GetHeldEntity() as BaseProjectile)?.primaryMagazine.contents ?? 0,
                    ammotype = (item.GetHeldEntity() as BaseProjectile)?.primaryMagazine.ammoType.shortname ?? null,
                    skin = item.skin,
                    condition = UnityEngine.Random.Range(1, item.condition),
                    instanceData = new InventoryItem.InstanceData(item),
                    contents = item.contents?.itemList.Select(item1 => new InventoryItem
                    {
                        itemid = item1.info.itemid,
                        amount = item1.amount,
                        condition = UnityEngine.Random.Range(1, item1.condition)
                    }).ToArray()
                }).ToList();
            }

            public void RestoreItemsTo(LootableCorpse corpse)
            {
                for (int i = 0; i < items.Count; i++)
                {
                    Item item = CreateItem(items[i]);
                    item.MoveToContainer(corpse.containers[0]);
                }
            }

            private Item CreateItem(InventoryItem itemData)
            {
                Item item = ItemManager.CreateByItemID(itemData.itemid, itemData.amount, itemData.skin);
                item.condition = itemData.condition;

                if (itemData.instanceData != null)
                    itemData.instanceData.Restore(item);

                BaseProjectile weapon = item.GetHeldEntity() as BaseProjectile;
                if (weapon != null)
                {
                    if (!string.IsNullOrEmpty(itemData.ammotype))
                        weapon.primaryMagazine.ammoType = ItemManager.FindItemDefinition(itemData.ammotype);
                    weapon.primaryMagazine.contents = itemData.ammo;
                }
                if (itemData.contents != null)
                {
                    foreach (InventoryItem contentData in itemData.contents)
                    {
                        Item newContent = ItemManager.CreateByItemID(contentData.itemid, contentData.amount);
                        if (newContent != null)
                        {
                            newContent.condition = contentData.condition;
                            newContent.MoveToContainer(item.contents);
                        }
                    }
                }
                return item;
            }

            public class InventoryItem
            {
                public int itemid;
                public ulong skin;
                public int amount;
                public float condition;
                public int ammo;
                public string ammotype;
                public InstanceData instanceData;
                public InventoryItem[] contents;

                public class InstanceData
                {
                    public int dataInt;
                    public int blueprintTarget;
                    public int blueprintAmount;

                    public InstanceData() { }
                    public InstanceData(Item item)
                    {
                        if (item.instanceData == null)
                            return;

                        dataInt = item.instanceData.dataInt;
                        blueprintAmount = item.instanceData.blueprintAmount;
                        blueprintTarget = item.instanceData.blueprintTarget;
                    }

                    public void Restore(Item item)
                    {
                        item.instanceData = new ProtoBuf.Item.InstanceData();
                        item.instanceData.blueprintAmount = blueprintAmount;
                        item.instanceData.blueprintTarget = blueprintTarget;
                        item.instanceData.dataInt = dataInt;
                    }
                }
            }
        }
        #endregion

        #region Localization
        private static string msg(string key, string playerId = null) => Instance.lang.GetMessage(key, Instance, playerId);

        private Dictionary<string, string> Messages = new Dictionary<string, string>
        {
            ["DestroyMessage"] = "<color=#939393>A plane carrying cargo has just crashed at co-ordinates </color><color=#ce422b>X: {0}, Z: {1}</color>",
            ["IncomingMessage"] = "<color=#ce422b>A low flying plane carrying cargo is about to fly over!</color><color=#939393>\nIf you are skilled enough you can shoot it down with a rocket launcher!</color>",
            ["AttackerMessage1"] = "<color=#ce422b>{0}</color><color=#939393> has shot down the plane!</color>",
            ["AttackerMessage2"] = "<color=#ce422b>{0}</color><color=#939393> has shot the plane out of the sky!</color>",           
        };
        #endregion
    }
}
