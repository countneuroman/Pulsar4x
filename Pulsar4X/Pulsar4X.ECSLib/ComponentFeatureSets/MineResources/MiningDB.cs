﻿using System;
using System.Collections.Generic;

namespace Pulsar4X.ECSLib
{
    public class MiningDB : BaseDataBlob, IAbilityDescription
    {
        public Dictionary<Guid, long> BaseMiningRate { get; set; }
        public Dictionary<Guid, long> ActualMiningRate { get; set; }

        public int NumberOfMines { get; set;} = 0;

        public Dictionary<Guid, MineralDeposit> MineralDeposit => OwningEntity.GetDataBlob<ColonyInfoDB>().PlanetEntity.GetDataBlob<MineralsDB>().Minerals;

        public MiningDB()
        {
            BaseMiningRate = new Dictionary<Guid, long>();
            ActualMiningRate = new Dictionary<Guid, long>();
        }

        public MiningDB(MiningDB db)
        {
            
        }

        public override object Clone()
        {
            return new MiningDB(this);
        }

        public string AbilityName()
        {
            return "Resource Mining";
        }

        public string AbilityDescription()
        {
            string time = StaticRefLib.Game.Settings.EconomyCycleTime.ToString();
            string desc = "Mines Resources at Rates of: \n";
            foreach (var kvp in BaseMiningRate)
            {
                string resourceName = StaticRefLib.StaticData.CargoGoods.GetMineral(kvp.Key).Name;
                desc += resourceName + "\t" + Stringify.Number(kvp.Value) + "\n";
            }

            return desc + "per " + time;
        }
    }
}