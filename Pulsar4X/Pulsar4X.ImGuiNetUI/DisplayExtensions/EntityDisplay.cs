using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using ImGuiNET;
using Pulsar4X.ECSLib;

namespace Pulsar4X.SDL2UI
{
    public static class EntityDisplay
    {
        public static void DisplaySummary(this Entity entity, EntityState entityState, GlobalUIState uiState)
        {
            Vector2 windowContentSize = ImGui.GetContentRegionAvail();
            var firstChildSize = new Vector2(windowContentSize.X * 0.33f, windowContentSize.Y);
            var secondChildSize = new Vector2(windowContentSize.X * 0.33f, windowContentSize.Y);
            var thirdChildSize = new Vector2(windowContentSize.X * 0.33f - (windowContentSize.X * 0.01f), windowContentSize.Y);
            if(ImGui.BeginChild("ColonySummary1", firstChildSize, true))
            {
                if(ImGui.CollapsingHeader("Parent Information", ImGuiTreeNodeFlags.DefaultOpen))
                {
                    var colonyInfoDb = entity.GetDataBlob<ColonyInfoDB>();
                    var bodyInfoDb = colonyInfoDb.PlanetEntity.GetDataBlob<SystemBodyInfoDB>();

                    ImGui.Columns(2);
                    ImGui.PushStyleColor(ImGuiCol.Text, Styles.DescriptiveColor);
                    ImGui.Text("Name");
                    ImGui.PopStyleColor();
                    ImGui.NextColumn();
                    if(ImGui.SmallButton(colonyInfoDb.PlanetEntity.GetDefaultName()))
                    {
                        uiState.EntityClicked(colonyInfoDb.PlanetEntity.Guid, uiState.SelectedStarSysGuid, MouseButtons.Primary);
                    }
                    ImGui.NextColumn();
                    ImGui.Separator();
                    DisplayHelpers.PrintRow("Type", bodyInfoDb.BodyType.ToDescription());
                    DisplayHelpers.PrintRow("Tectonic Activity", bodyInfoDb.Tectonics.ToDescription());
                    DisplayHelpers.PrintRow("Gravity", bodyInfoDb.Gravity.ToString("#"));
                    DisplayHelpers.PrintRow("Temperature", bodyInfoDb.BaseTemperature.ToString("#.#") + " C");
                    DisplayHelpers.PrintRow("Length of Day", bodyInfoDb.LengthOfDay.ToString("hh") + " hours");
                    DisplayHelpers.PrintRow("Tilt", bodyInfoDb.AxialTilt.ToString("#"));
                    DisplayHelpers.PrintRow("Magnetic Field", bodyInfoDb.MagneticField.ToString("#"));
                    DisplayHelpers.PrintRow("Radiation Level", bodyInfoDb.RadiationLevel.ToString("#"));
                    DisplayHelpers.PrintRow("Atmospheric Dust", bodyInfoDb.AtmosphericDust.ToString("#"), separator: false);
                }
                ImGui.Columns(1);
                entity.GetDataBlob<ColonyInfoDB>().Display(entityState, uiState);
                ImGui.EndChild();
            }
            ImGui.SameLine();
            if(ImGui.BeginChild("ColonySummary2", secondChildSize, true))
            {
                if(ImGui.CollapsingHeader("Installations", ImGuiTreeNodeFlags.DefaultOpen))
                {
                    if(entity.TryGetDatablob<ComponentInstancesDB>(out var componentInstances))
                    {
                        componentInstances.Display(entityState, uiState);
                    }
                }

                ImGui.EndChild();
            }
            ImGui.SameLine();
            if(ImGui.BeginChild("ColonySummary3", thirdChildSize, true))
            {
                if(ImGui.CollapsingHeader("Stockpile", ImGuiTreeNodeFlags.DefaultOpen))
                {
                    if(entity.TryGetDatablob<VolumeStorageDB>(out var storage))
                    {
                        ImGui.Columns(2);
                        DisplayHelpers.PrintRow("Total Mass in Storage", Stringify.Mass(storage.TotalStoredMass));
                        DisplayHelpers.PrintRow("Transfer Rate", storage.TransferRateInKgHr.ToString() + " kg/hr");
                        DisplayHelpers.PrintRow("Transfer Range", storage.TransferRangeDv_mps.ToString("0.#") + " dV m/s", tooltipOne: "This is confusing as hell :D", separator: false);
                        ImGui.Columns(1);
                        storage.Display(entityState, uiState, ImGuiTreeNodeFlags.None);
                    }
                }
                ImGui.EndChild();
            }
        }

        public static void DisplayIndustry(this Entity entity, EntityState entityState, GlobalUIState uiState)
        {
            IndustryDisplay.GetInstance(entityState).Display(uiState);
        }
        public static void DisplayMining(this Entity entity, GlobalUIState uiState)
        {
            var mineralStaticInfo = uiState.Game.StaticData.CargoGoods.GetMineralsList();
            var minerals = entity.GetDataBlob<ColonyInfoDB>().PlanetEntity.GetDataBlob<MineralsDB>()?.Minerals;
            var miningRates = entity.GetDataBlob<MiningDB>()?.ActualMiningRate;
            var storage = entity.GetDataBlob<VolumeStorageDB>()?.TypeStores;

            Vector2 topSize = ImGui.GetContentRegionAvail();
            if(ImGui.BeginChild("NumberOfMines" + entity.Guid, new Vector2(topSize.X, 28f), true, ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse))
            {
                if(entity.TryGetDatablob<MiningDB>(out var miningDB))
                {
                    ImGui.Text("Number of Mines:");
                    if(ImGui.IsItemHovered())
                        ImGui.SetTooltip("You can build more mines on this colony using the Production tab.");
                    ImGui.SameLine();
                    ImGui.PushStyleColor(ImGuiCol.Text, Styles.HighlightColor);
                    ImGui.Text(miningDB.NumberOfMines.ToString());
                    ImGui.PopStyleColor();

                }
                else
                {
                    ImGui.Text("Number of Mines: 0");
                }
                ImGui.EndChild();
            }

            if(ImGui.BeginTable("###MineralTable" + entity.Guid, 6, ImGuiTableFlags.BordersV | ImGuiTableFlags.BordersOuterH | ImGuiTableFlags.RowBg))
            {
                ImGui.TableSetupColumn("Mineral");
                ImGui.TableSetupColumn("Stockpile");
                ImGui.TableSetupColumn("Available to Mine");
                ImGui.TableSetupColumn("Accessibility");
                ImGui.TableSetupColumn("Annual Production");
                ImGui.TableSetupColumn("Years to Depletion");
                ImGui.TableHeadersRow();

                if(minerals == null) minerals = new Dictionary<Guid, MineralDeposit>();

                foreach(var (id, mineral) in minerals)
                {
                    var mineralData = mineralStaticInfo.FirstOrDefault(x => x.ID == id);

                    if(mineralData == null) continue;

                    var stockpileData = storage?.FirstOrDefault(x => x.Value.CurrentStoreInUnits.ContainsKey(id)).Value;
                    var stockpileUnits = stockpileData?.CurrentStoreInUnits;
                    var annualProduction = miningRates.ContainsKey(id) ? 365 * miningRates[id] : 0;

                    ImGui.TableNextRow();
                    ImGui.TableNextColumn();
                    ImGui.Text(mineralData.Name);
                    if(ImGui.IsItemHovered())
                        ImGui.SetTooltip(mineralData.Description);
                    ImGui.TableNextColumn();
                    if(stockpileData != null)
                    {
                        ImGui.Text(stockpileUnits[id].ToString("#,###,###,###,###,###,##0"));
                    }
                    else
                    {
                        if(storage == null)
                            ImGui.Text("Unavailable");
                        else
                            ImGui.Text("0");
                    }
                    if(ImGui.IsItemHovered())
                        ImGui.SetTooltip("Amount of " + mineralData.Name + " (in tons) available for use in the colonies stockpile.");

                    ImGui.TableNextColumn();
                    ImGui.Text(mineral.Amount.ToString("#,###,###,###,###,###,##0"));
                    if(ImGui.IsItemHovered())
                        ImGui.SetTooltip("Amount of " + mineralData.Name + " (in tons) available that can be mined from this colony.");
                    ImGui.TableNextColumn();
                    ImGui.Text(mineral.Accessibility.ToString("0.00"));
                    if(ImGui.IsItemHovered())
                        ImGui.SetTooltip("How easy it is to mine " + mineralData.Name + " from this colony.\n\n1.0 = easiest\n0.0 = hardest");
                    ImGui.TableNextColumn();
                    if(miningRates.ContainsKey(id))
                    {
                        ImGui.Text(annualProduction.ToString("#,###,###"));
                        if(ImGui.IsItemHovered())
                            ImGui.SetTooltip("Annual production of " + mineralData.Name + " (in tons) from this colony.");
                    }
                    else
                    {
                        ImGui.Text("-");
                        if(ImGui.IsItemHovered())
                            ImGui.SetTooltip("This colony is currently unable to mine " + mineralData.Name + ".");
                    }
                    ImGui.TableNextColumn();
                    if(annualProduction > 0)
                    {
                        string yearsToDepletion = Math.Round((double)mineral.Amount / (double)annualProduction, 4).ToString("#.0");
                        ImGui.Text(yearsToDepletion);
                        if(ImGui.IsItemHovered())
                            ImGui.SetTooltip("The colony will exhaust the available " + mineralData.Name + " in " + yearsToDepletion + " years.");
                    }
                    else
                    {
                        ImGui.Text("-");
                    }
                }

                ImGui.EndTable();

                if(minerals.Count == 0)
                {
                    ImGui.Text("No minerals available.");
                }
            }
        }

        public static void DisplayResearch(this Entity entity, EntityState entityState, GlobalUIState uiState)
        {
            if(!entity.TryGetDatablob<EntityResearchDB>(out var researchDB)) return;

            Vector2 topSize = ImGui.GetContentRegionAvail();
            if(ImGui.BeginChild("NumberOfResearchLabs" + entity.Guid, new Vector2(topSize.X, 28f), true, ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse))
            {
                ImGui.Text("Universities:");
                ImGui.SameLine();
                ImGui.PushStyleColor(ImGuiCol.Text, Styles.HighlightColor);
                ImGui.Text(researchDB.Labs.Count.ToString("0"));
                ImGui.PopStyleColor();
                ImGui.SameLine();
                ImGui.Text("Research Points:");
                ImGui.SameLine();
                ImGui.PushStyleColor(ImGuiCol.Text, Styles.HighlightColor);
                ImGui.Text(researchDB.Labs.Values.Sum().ToString());
                ImGui.PopStyleColor();

                ImGui.EndChild();
            }

            Vector2 sizeAvailable = ImGui.GetContentRegionAvail();
            if(ImGui.BeginChild("UniversityList", sizeAvailable, true))
            {
                foreach(var (instance, value) in researchDB.Labs)
                {
                    ImGui.Text(instance.Name);
                    ImGui.Text(value.ToString());
                }
                ImGui.EndChild();
            }
        }

        public static void DisplayLogistics(this Entity entity, EntityState entityState, GlobalUIState uiState)
        {
            ColonyLogisticsDisplay.GetInstance(StaticRefLib.StaticData, entityState).Display();
        }
    }
}