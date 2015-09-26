﻿using System;
using System.Collections.Generic;

namespace Pulsar4X.ECSLib
{
    internal static class MineProcessor
    {

        private const int _timeBetweenRuns = 68400; //one terran day.

        public static void Initialize()
        {
        }

        public static void Process(Game game, List<StarSystem> systems, int deltaSeconds)
        {
            foreach (var system in systems)
            {
                system.EconLastTickRun += deltaSeconds;
                if (system.EconLastTickRun >= _timeBetweenRuns)
                {
                    foreach (Entity colonyEntity in system.SystemManager.GetAllEntitiesWithDataBlob<ColonyInfoDB>())
                    {
                        PerEconTic(game, colonyEntity);
                    }
                    system.EconLastTickRun -= _timeBetweenRuns;
                }
            }
        }

        public static void PerEconTic(Game game, Entity colonyEntity)
        {

        }


        internal static void MineResources(Entity colonyEntity)
        {
            JDictionary<Guid, int> mineRates = colonyEntity.GetDataBlob<ColonyMinesDB>().MineingRate;
            JDictionary<Guid,MineralDepositInfo> planetMinerals = colonyEntity.GetDataBlob<SystemBodyDB>().Minerals;
            JDictionary<Guid, float> colonyMineralStockpile = colonyEntity.GetDataBlob<ColonyInfoDB>().MineralStockpile;
            float mineBonuses = colonyEntity.GetDataBlob<ColonyBonusesDB>().GetBonus(AbilityType.Mine);
            foreach (var kvp in mineRates)
            {                
                double accessability = planetMinerals[kvp.Key].Accessibility;
                double actualRate = kvp.Value * mineBonuses * accessability;
                int mineralsMined = (int)Math.Min(actualRate, planetMinerals[kvp.Key].Amount);

                colonyMineralStockpile.SafeValueAdd<Guid>(kvp.Key, mineralsMined);
                MineralDepositInfo mineralDeposit = planetMinerals[kvp.Key];
                int newAmount = mineralDeposit.Amount -= mineralsMined;
                
                accessability = Math.Pow((float)mineralDeposit.Amount / mineralDeposit.HalfOriginalAmount, 3) * mineralDeposit.Accessibility;
                double newAccess = GMath.Clamp(accessability, 0.1, mineralDeposit.Accessibility);

                MineralDepositInfo newDepositInfo = new MineralDepositInfo
                {
                    Amount = newAmount, 
                    HalfOriginalAmount = mineralDeposit.HalfOriginalAmount, 
                    Accessibility = newAccess
                };
                planetMinerals[kvp.Key] = newDepositInfo;
            }
        }

        //this needs to be run when an entity with MineResources is put on planet.
        internal static void CalcMaxRate(Entity colonyEntity)
        {
            List<Entity> installations = colonyEntity.GetDataBlob<ColonyInfoDB>().Installations;
            List<Entity> mines = new List<Entity>();
            foreach (var inst in installations)
            {
                if(inst.HasDataBlob<MineResourcesDB>())
                    mines.Add(inst);
            }
            JDictionary<Guid,int> rates = new JDictionary<Guid, int>();
            foreach (var mine in mines)
            {
                foreach (var kvp in mine.GetDataBlob<MineResourcesDB>().ResourcesPerEconTick)
                {
                    rates.SafeValueAdd(kvp.Key,kvp.Value);
                }                
            }
            colonyEntity.GetDataBlob<ColonyMinesDB>().MineingRate = rates;
        }
    }
}