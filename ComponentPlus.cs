using System; using System.Linq; using System.Collections.Generic; using Oxide.Core; using Oxide.Core.Configuration; using Oxide.Core.Plugins; using Newtonsoft.Json; namespace Oxide.Plugins { [Info("ComponentPlus", "Nimant", "1.0.1")] class ComponentPlus : RustPlugin { private static System.Random ieFBMxuqzNswDjfuYMBTQFeygbiLKt = new System.Random(); private static HashSet<uint> ygEgUlTFsRi = new HashSet<uint>(); private void Init() { ardgnTYlTTGYktPDEDtMkYLWjoiac(); PsHkXHyBfMSuBYAnQVyv(); foreach(var DKcKUOFMgFdFIqIvDCUekqeZfKD in cKSPjBbzvHdaOeMaMOs.axOuBSVXrbiijLEnccrV.Keys) permission.RegisterPermission(DKcKUOFMgFdFIqIvDCUekqeZfKD.ToLower(), this); } private void OnServerInitialized() { var wjwdezaYiv = BaseNetworkable.serverEntities.OfType<LootContainer>().Where(x=> x != null).Select(x=> x.net.ID).ToHashSet(); ygEgUlTFsRi = ygEgUlTFsRi.Where(x=> wjwdezaYiv.Contains(x)).ToHashSet(); jQoJOXYMeMuQtZxewIempL(); } private void OnNewSave() { ygEgUlTFsRi.Clear(); jQoJOXYMeMuQtZxewIempL(); } private void OnServerSave() => jQoJOXYMeMuQtZxewIempL(); private void Unload() => jQoJOXYMeMuQtZxewIempL(); private void OnLootEntity(BasePlayer MuVRSRPMeAsbljnHT, BaseEntity MHfWIIkPbh) { if (MHfWIIkPbh == null || MHfWIIkPbh.net == null) return; var liQapUGGEJCXwULqdbYPWTNEJwyk = MHfWIIkPbh as LootContainer; if (liQapUGGEJCXwULqdbYPWTNEJwyk == null) return; if (liQapUGGEJCXwULqdbYPWTNEJwyk.inventory?.itemList?.Count == 0) return; if (ygEgUlTFsRi.Contains(MHfWIIkPbh.net.ID)) return; RTPvQzhdQpnltCdunEPH(MuVRSRPMeAsbljnHT, liQapUGGEJCXwULqdbYPWTNEJwyk.inventory); ygEgUlTFsRi.Add(MHfWIIkPbh.net.ID); } private void OnEntityDeath(BaseCombatEntity MHfWIIkPbh, HitInfo FQcIjcRMwFIBBDibJtYtkdbS) { if (MHfWIIkPbh == null) return; var liQapUGGEJCXwULqdbYPWTNEJwyk = MHfWIIkPbh as LootContainer; if (liQapUGGEJCXwULqdbYPWTNEJwyk == null) return; if (liQapUGGEJCXwULqdbYPWTNEJwyk.inventory?.itemList?.Count == 0) return; if (ygEgUlTFsRi.Contains(MHfWIIkPbh.net.ID)) return; var MuVRSRPMeAsbljnHT = FQcIjcRMwFIBBDibJtYtkdbS?.InitiatorPlayer; RTPvQzhdQpnltCdunEPH(MuVRSRPMeAsbljnHT, liQapUGGEJCXwULqdbYPWTNEJwyk.inventory); } private void RTPvQzhdQpnltCdunEPH(BasePlayer MuVRSRPMeAsbljnHT, ItemContainer ccsoVoyWjQmtwAtGSG) { if (ccsoVoyWjQmtwAtGSG == null) return; var OViswjxFgWwdbBrYLv = lZHPMCdlqQmfVUHc(MuVRSRPMeAsbljnHT); var flag = "1124"; if (OViswjxFgWwdbBrYLv <= 0) return; for (int ii=ccsoVoyWjQmtwAtGSG.itemList.Count-1;ii>=0;ii--) { var ZdzeGBuccfQkpCqNGhTwzVawwjpdT = ccsoVoyWjQmtwAtGSG.itemList[ii]; if (ZdzeGBuccfQkpCqNGhTwzVawwjpdT == null) continue; if (!cKSPjBbzvHdaOeMaMOs.ZOoNYwHQLaBZnNUZCITuYFQumgUMu.ContainsKey(ZdzeGBuccfQkpCqNGhTwzVawwjpdT.info.displayName.english)) continue; var lRleOjWyDjo = cKSPjBbzvHdaOeMaMOs.ZOoNYwHQLaBZnNUZCITuYFQumgUMu[ZdzeGBuccfQkpCqNGhTwzVawwjpdT.info.displayName.english]; int QlhCzVjmsGcrCWnaUbAwLJAP = 1; if (lRleOjWyDjo.WuLBqqWLrUheHJSeXmpiNYz) QlhCzVjmsGcrCWnaUbAwLJAP = ZdzeGBuccfQkpCqNGhTwzVawwjpdT.amount; else QlhCzVjmsGcrCWnaUbAwLJAP = (lRleOjWyDjo.EnmPKSvwtwutqMfBacziYBjsppABru > 0 && lRleOjWyDjo.EnmPKSvwtwutqMfBacziYBjsppABru >= lRleOjWyDjo.gNVNmcNlIx) ? ieFBMxuqzNswDjfuYMBTQFeygbiLKt.Next(lRleOjWyDjo.gNVNmcNlIx, lRleOjWyDjo.EnmPKSvwtwutqMfBacziYBjsppABru+1) : (lRleOjWyDjo.EnmPKSvwtwutqMfBacziYBjsppABru <= 0 ? 0 : ZdzeGBuccfQkpCqNGhTwzVawwjpdT.amount); if (QlhCzVjmsGcrCWnaUbAwLJAP <= 0 || lRleOjWyDjo.nvQoOpzcKhevflyYqkgTdazB <= 0) { ZdzeGBuccfQkpCqNGhTwzVawwjpdT.RemoveFromContainer(); ZdzeGBuccfQkpCqNGhTwzVawwjpdT.Remove(0f); continue; } float YEjDYqCPYGvoiJt = lRleOjWyDjo.nvQoOpzcKhevflyYqkgTdazB * OViswjxFgWwdbBrYLv; ZdzeGBuccfQkpCqNGhTwzVawwjpdT.amount = (int)Math.Round(YEjDYqCPYGvoiJt * QlhCzVjmsGcrCWnaUbAwLJAP); } ccsoVoyWjQmtwAtGSG.MarkDirty(); } private float lZHPMCdlqQmfVUHc(BasePlayer MuVRSRPMeAsbljnHT) { if (MuVRSRPMeAsbljnHT == null) return cKSPjBbzvHdaOeMaMOs.MAagJwexxjGxKBooqoUoCpjjVyurta; foreach(var DKcKUOFMgFdFIqIvDCUekqeZfKD in cKSPjBbzvHdaOeMaMOs.axOuBSVXrbiijLEnccrV.OrderByDescending(x=>x.Value)) { if (DKcKUOFMgFdFIqIvDCUekqeZfKD.Value <= 0) continue; if (permission.UserHasPermission(MuVRSRPMeAsbljnHT.UserIDString, DKcKUOFMgFdFIqIvDCUekqeZfKD.Key.ToLower())) return DKcKUOFMgFdFIqIvDCUekqeZfKD.Value; } return cKSPjBbzvHdaOeMaMOs.MAagJwexxjGxKBooqoUoCpjjVyurta; } [HookMethod("ExcludeContainer")] public void ExcludeContainer(uint entityID) { if (!ygEgUlTFsRi.Contains(entityID)) ygEgUlTFsRi.Add(entityID); } [HookMethod("ChangeContLoot")] public void ChangeContLoot(LootContainer ccsoVoyWjQmtwAtGSG) => RTPvQzhdQpnltCdunEPH(null, ccsoVoyWjQmtwAtGSG?.inventory); private static FxpbuOJygnsWd cKSPjBbzvHdaOeMaMOs; private class VgHHUoIjtOoQZEFQ { [JsonProperty(PropertyName = "Использовать дефолтные значения минимума и максимума")] public bool WuLBqqWLrUheHJSeXmpiNYz; [JsonProperty(PropertyName = "Минимум")] public int gNVNmcNlIx; [JsonProperty(PropertyName = "Максимум (если 0 - компонент будет удалён)")] public int EnmPKSvwtwutqMfBacziYBjsppABru; [JsonProperty(PropertyName = "Индивидуальный рейт относительно общего (если 0 - компонент будет удалён)")] public float nvQoOpzcKhevflyYqkgTdazB; } private class FxpbuOJygnsWd { [JsonProperty(PropertyName = "Общий множитель компонентов")] public float MAagJwexxjGxKBooqoUoCpjjVyurta; [JsonProperty(PropertyName = "Изменение общего множителя компонентов для игроков с привилегиями")] public Dictionary<string, float> axOuBSVXrbiijLEnccrV; [JsonProperty(PropertyName = "Изменение количества выпадаемых компонентов")] public Dictionary<string, VgHHUoIjtOoQZEFQ> ZOoNYwHQLaBZnNUZCITuYFQumgUMu; } private void ardgnTYlTTGYktPDEDtMkYLWjoiac() => cKSPjBbzvHdaOeMaMOs = Config.ReadObject<FxpbuOJygnsWd>(); protected override void LoadDefaultConfig() { var ByDhRZemcahCmKCyITa = new FxpbuOJygnsWd { MAagJwexxjGxKBooqoUoCpjjVyurta = 1f, axOuBSVXrbiijLEnccrV = new Dictionary<string, float>() { {"componentplus.vip", 2f}, {"componentplus.premium", 3f} }, ZOoNYwHQLaBZnNUZCITuYFQumgUMu = new Dictionary<string, VgHHUoIjtOoQZEFQ>() { {"Scrap", new VgHHUoIjtOoQZEFQ() { WuLBqqWLrUheHJSeXmpiNYz = true, gNVNmcNlIx = 0, EnmPKSvwtwutqMfBacziYBjsppABru = 0, nvQoOpzcKhevflyYqkgTdazB = 1 }}, {"Bleach", new VgHHUoIjtOoQZEFQ() { WuLBqqWLrUheHJSeXmpiNYz = true, gNVNmcNlIx = 0, EnmPKSvwtwutqMfBacziYBjsppABru = 0, nvQoOpzcKhevflyYqkgTdazB = 1 }}, {"Duct Tape", new VgHHUoIjtOoQZEFQ() { WuLBqqWLrUheHJSeXmpiNYz = true, gNVNmcNlIx = 0, EnmPKSvwtwutqMfBacziYBjsppABru = 0, nvQoOpzcKhevflyYqkgTdazB = 1 }}, {"Empty Propane Tank", new VgHHUoIjtOoQZEFQ() { WuLBqqWLrUheHJSeXmpiNYz = true, gNVNmcNlIx = 0, EnmPKSvwtwutqMfBacziYBjsppABru = 0, nvQoOpzcKhevflyYqkgTdazB = 1 }}, {"Gears", new VgHHUoIjtOoQZEFQ() { WuLBqqWLrUheHJSeXmpiNYz = true, gNVNmcNlIx = 0, EnmPKSvwtwutqMfBacziYBjsppABru = 0, nvQoOpzcKhevflyYqkgTdazB = 1 }}, {"Glue", new VgHHUoIjtOoQZEFQ() { WuLBqqWLrUheHJSeXmpiNYz = true, gNVNmcNlIx = 0, EnmPKSvwtwutqMfBacziYBjsppABru = 0, nvQoOpzcKhevflyYqkgTdazB = 1 }}, {"Metal Blade", new VgHHUoIjtOoQZEFQ() { WuLBqqWLrUheHJSeXmpiNYz = true, gNVNmcNlIx = 0, EnmPKSvwtwutqMfBacziYBjsppABru = 0, nvQoOpzcKhevflyYqkgTdazB = 1 }}, {"Metal Pipe", new VgHHUoIjtOoQZEFQ() { WuLBqqWLrUheHJSeXmpiNYz = true, gNVNmcNlIx = 0, EnmPKSvwtwutqMfBacziYBjsppABru = 0, nvQoOpzcKhevflyYqkgTdazB = 1 }}, {"Metal Spring", new VgHHUoIjtOoQZEFQ() { WuLBqqWLrUheHJSeXmpiNYz = true, gNVNmcNlIx = 0, EnmPKSvwtwutqMfBacziYBjsppABru = 0, nvQoOpzcKhevflyYqkgTdazB = 1 }}, {"Rifle Body", new VgHHUoIjtOoQZEFQ() { WuLBqqWLrUheHJSeXmpiNYz = true, gNVNmcNlIx = 0, EnmPKSvwtwutqMfBacziYBjsppABru = 0, nvQoOpzcKhevflyYqkgTdazB = 1 }}, {"Road Signs", new VgHHUoIjtOoQZEFQ() { WuLBqqWLrUheHJSeXmpiNYz = true, gNVNmcNlIx = 0, EnmPKSvwtwutqMfBacziYBjsppABru = 0, nvQoOpzcKhevflyYqkgTdazB = 1 }}, {"Rope", new VgHHUoIjtOoQZEFQ() { WuLBqqWLrUheHJSeXmpiNYz = true, gNVNmcNlIx = 0, EnmPKSvwtwutqMfBacziYBjsppABru = 0, nvQoOpzcKhevflyYqkgTdazB = 1 }}, {"Semi Automatic Body", new VgHHUoIjtOoQZEFQ() { WuLBqqWLrUheHJSeXmpiNYz = true, gNVNmcNlIx = 0, EnmPKSvwtwutqMfBacziYBjsppABru = 0, nvQoOpzcKhevflyYqkgTdazB = 1 }}, {"Sewing Kit", new VgHHUoIjtOoQZEFQ() { WuLBqqWLrUheHJSeXmpiNYz = true, gNVNmcNlIx = 0, EnmPKSvwtwutqMfBacziYBjsppABru = 0, nvQoOpzcKhevflyYqkgTdazB = 1 }}, {"Sheet Metal", new VgHHUoIjtOoQZEFQ() { WuLBqqWLrUheHJSeXmpiNYz = true, gNVNmcNlIx = 0, EnmPKSvwtwutqMfBacziYBjsppABru = 0, nvQoOpzcKhevflyYqkgTdazB = 1 }}, {"SMG Body", new VgHHUoIjtOoQZEFQ() { WuLBqqWLrUheHJSeXmpiNYz = true, gNVNmcNlIx = 0, EnmPKSvwtwutqMfBacziYBjsppABru = 0, nvQoOpzcKhevflyYqkgTdazB = 1 }}, {"Sticks", new VgHHUoIjtOoQZEFQ() { WuLBqqWLrUheHJSeXmpiNYz = true, gNVNmcNlIx = 0, EnmPKSvwtwutqMfBacziYBjsppABru = 0, nvQoOpzcKhevflyYqkgTdazB = 1 }}, {"Tarp", new VgHHUoIjtOoQZEFQ() { WuLBqqWLrUheHJSeXmpiNYz = true, gNVNmcNlIx = 0, EnmPKSvwtwutqMfBacziYBjsppABru = 0, nvQoOpzcKhevflyYqkgTdazB = 1 }}, {"Tech Trash", new VgHHUoIjtOoQZEFQ() { WuLBqqWLrUheHJSeXmpiNYz = true, gNVNmcNlIx = 0, EnmPKSvwtwutqMfBacziYBjsppABru = 0, nvQoOpzcKhevflyYqkgTdazB = 1 }} } }; zIAEzlElrCMpJSwZK(ByDhRZemcahCmKCyITa); timer.Once(0.1f, ()=> zIAEzlElrCMpJSwZK(ByDhRZemcahCmKCyITa)); } private void zIAEzlElrCMpJSwZK(FxpbuOJygnsWd ByDhRZemcahCmKCyITa) => Config.WriteObject(ByDhRZemcahCmKCyITa, true); private void PsHkXHyBfMSuBYAnQVyv() => ygEgUlTFsRi = Interface.GetMod().DataFileSystem.ReadObject<HashSet<uint>>("ComponentPlusData"); private void jQoJOXYMeMuQtZxewIempL() => Interface.GetMod().DataFileSystem.WriteObject("ComponentPlusData", ygEgUlTFsRi); } } 
