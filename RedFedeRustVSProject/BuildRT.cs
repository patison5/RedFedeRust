using Facepunch;
using Oxide.Core;
using Oxide.Core.Configuration;
using Oxide.Core.Plugins;
using Oxide.Game.Rust.Cui;
using Rust;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
namespace Oxide.Plugins
{
    [Info("RadHouse", "BuildRT.ru", "1.2.0")]

    class BuildRT : RustPlugin
    {
        private static readonly int playerLayer = LayerMask.GetMask("Player (Server)");
        private static readonly Collider[] colBuffer = Vis.colBuffer;
        private BaseEntity LootBox;


        public List<BaseEntity> BaseEntityList = new List<BaseEntity>();
        public List<ulong> PlayerAuth = new List<ulong>();
        public object success;
        public string ChatPrefix = "<color=#ffe100>BuildRT:</color>";

        public int GradeNum = 1;


        [ChatCommand("bbrt")]
        void createRtCommand(BasePlayer player, string cmd, string[] Args)
        {
             SendReply(player, $"{ChatPrefix} Заебенил, проверяй..");

             createRt(player);
        }

        [ChatCommand("ddrt")]
        void deleteRtCommand(BasePlayer player, string cmd, string[] Args)
        {
            SendReply(player, $"{ChatPrefix} И правда, ну нахер..");

            DestroyRadHouse();
        }


        void createFoundationEntity(float x, float y, float z, BasePlayer player, string entType) {
            Vector3 pos;

            pos = (Vector3)player.transform.position; 
            pos.x = pos.x + x; 
            pos.y = pos.y + y; 
            pos.z = pos.z + z;

            if (entType == "foundation"){
                BaseEntity wallEntity = GameManager.server.CreateEntity("assets/prefabs/building core/foundation/foundation.prefab", pos, new Quaternion(), true);

                wallEntity.Spawn();
                BaseEntityList.Add(wallEntity);
            }
            else if (entType == "wall1") {

                BaseEntity wallEntity = GameManager.server.CreateEntity("assets/prefabs/building core/wall/wall.prefab", pos, new Quaternion(), true);
                wallEntity.transform.localEulerAngles = new Vector3(0f, 180f, 0f);

                wallEntity.Spawn();
                BaseEntityList.Add(wallEntity);
            }

            else if (entType == "wall2") {

                BaseEntity wallEntity = GameManager.server.CreateEntity("assets/prefabs/building core/wall/wall.prefab", pos, new Quaternion(), true);
                wallEntity.transform.localEulerAngles = new Vector3(0f, 270f, 0);

                wallEntity.Spawn();
                BaseEntityList.Add(wallEntity);
            }
            
        }


        void createRt(BasePlayer player)
        {
            if (BaseEntityList.Count > 0) DestroyRadHouse();
            Vector3 pos;
            pos.x = 0;
            pos.y = 0;
            pos.z = 0;
            success = player.transform.position;
            pos = (Vector3)success;
            Puts(pos.ToString());
            // pos.x = pos.x + 0f; pos.y = pos.y + 1f; pos.z = pos.z + 0f;
            // BaseEntity Foundation1 = GameManager.server.CreateEntity("assets/prefabs/building core/foundation/foundation.prefab", pos, new Quaternion(), true);

            // pos.x = pos.x - 1.5f;
            // BaseEntity Wall = GameManager.server.CreateEntity("assets/prefabs/building core/wall/wall.prefab", pos, new Quaternion(), true);
            // Wall.transform.localEulerAngles = new Vector3(0f, 180f, 0f);

            

            // Foundation1.Spawn();
            // Foundation2.Spawn();
            // Wall.Spawn();

            // BaseEntityList.Add(Foundation1);
            // BaseEntityList.Add(Foundation2);
            // BaseEntityList.Add(Wall);

            createFoundationEntity(0f, 1f, 3f, player, "foundation"); // foundation
            createFoundationEntity(3f, 1f, 3f, player, "foundation"); // foundation
            createFoundationEntity(6f, 1f, 3f, player, "foundation"); // foundation
            createFoundationEntity(9f, 1f, 3f, player, "foundation"); // foundation
            createFoundationEntity(-1.5f, 1f, 3f, player, "wall1"); // foundation
            createFoundationEntity(0f, 1f, 4.5f, player, "wall2"); // foundation
            createFoundationEntity(3f, 1f, 4.5f, player, "wall2"); // foundation
            createFoundationEntity(6f, 1f, 4.5f, player, "wall2"); // foundation
            createFoundationEntity(9f, 1f, 4.5f, player, "wall2"); // foundation



            var buildingID = BuildingManager.server.NewBuildingID();
            try
            {
                foreach (var entity in BaseEntityList)
                {
                    DecayEntity decayEntity = entity.GetComponentInParent<DecayEntity>();
                    decayEntity.AttachToBuilding(buildingID);
                    if (entity.name.Contains("assets/prefabs/deployable/large wood storage/box.wooden.large.prefab") && entity.name.Contains("assets/prefabs/deployable/tool cupboard/cupboard.tool.deployed.prefab") && entity.name.Contains("assets/prefabs/building/wall.window.bars/wall.window.bars.metal.prefab")) break;
                    BuildingBlock buildingBlock = entity.GetComponent<BuildingBlock>();
                    buildingBlock.SetGrade((BuildingGrade.Enum)GradeNum);
                    buildingBlock.UpdateSkin();
                    buildingBlock.SetHealthToMax();
                    if (!entity.name.Contains("assets/prefabs/building core/foundation/foundation.prefab") && !entity.name.Contains("assets/prefabs/building core/foundation.steps/foundation.steps.prefab")) buildingBlock.grounded = true;
                    buildingBlock.SendNetworkUpdate(BasePlayer.NetworkQueue.Update);
                }
            }
            catch { }
        }


        void DestroyRadHouse()
        {
            if (BaseEntityList != null)
            {
                foreach (BaseEntity entity in BaseEntityList)
                {
                    if (!entity.IsDestroyed)
                    entity.Kill();
                }

                BaseEntityList.Clear();

            }
        }


    }
}
                                