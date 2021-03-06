﻿using IPA.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace BeatSaberCustomCampaigns.Custom_Trackers
{
    public class SaberTimeInWallMissionObjectiveChecker : SimpleValueMissionObjectiveChecker, CustomTracker
    {
        protected ObstacleController.Pool obstaclePool;
        protected PlayerController playerController;
        protected Saber[] sabers;
        protected float currentValue;
        bool loaded = false;

        protected override void Init()
        {
            StartCoroutine(WaitForLoad());
            if (_missionObjective.referenceValueComparisonType == MissionObjective.ReferenceValueComparisonType.Min || _missionObjective.referenceValueComparisonType == MissionObjective.ReferenceValueComparisonType.Equal)
            {
                base.status = Status.NotClearedYet;
            }
            else
            {
                base.status = Status.NotFailedYet;
            }
        }

        IEnumerator WaitForLoad()
        {
            while (!loaded)
            {
                obstaclePool = Resources.FindObjectsOfTypeAll<PlayerHeadAndObstacleInteraction>().FirstOrDefault()?.GetField<ObstacleController.Pool, PlayerHeadAndObstacleInteraction>("_obstaclePool");
                playerController = Resources.FindObjectsOfTypeAll<PlayerController>().FirstOrDefault();
                if (obstaclePool == null || playerController == null)
                    yield return new WaitForSeconds(0.1f);
                else
                    loaded = true;
            }

            yield return new WaitForSeconds(0.1f);
            sabers = new Saber[2];
            sabers[0] = playerController.leftSaber;
            sabers[1] = playerController.rightSaber;
        }

        public void Update()
        {
            if (!loaded) return;
            for (int i = 0; i < 2; i++)
            {
                foreach (ObstacleController activeObstacleController in obstaclePool.activeItems)
                {
                    Bounds bounds = activeObstacleController.bounds;
                    if (sabers[i].isActiveAndEnabled && isSaberInWall(bounds, activeObstacleController.transform, sabers[i].saberBladeBottomPos, sabers[i].saberBladeTopPos))
                    {
                        currentValue += Time.deltaTime;
                        base.checkedValue = (int)(currentValue * 1000);
                        CheckAndUpdateStatus();
                        break;
                    }
                }
            }
        }
        
        public bool isSaberInWall(Bounds bounds, Transform transform, Vector3 bladeBottomPos, Vector3 bladeTopPos)
        {
            bladeBottomPos = transform.InverseTransformPoint(bladeBottomPos);
            bladeTopPos = transform.InverseTransformPoint(bladeTopPos);
            float num = Vector3.Distance(bladeBottomPos, bladeTopPos);
            Vector3 vector = bladeTopPos - bladeBottomPos;
            vector.Normalize();
            if (bounds.IntersectRay(new Ray(bladeBottomPos, vector), out float distance) && distance <= num)
            {
                return true;
            }
            return false;
        }

        public string GetMissionObjectiveTypeName()
        {
            return ChallengeRequirement.GetObjectiveName("saberInWall");
        }
    }
}
